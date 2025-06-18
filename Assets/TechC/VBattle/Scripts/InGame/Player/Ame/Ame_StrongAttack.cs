using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめの強攻撃の実装
    /// </summary>
    public class Ame_StrongAttack : StrongAttack
    {
        [Header("プレハブの参照")]
        [SerializeField] private GameObject swordObj;
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private GameObject iceDataPrefab;
        [SerializeField] private GameObject iceExplosionPrefab;
        [SerializeField] private GameObject iceRosePrefab;
        [SerializeField] private GameObject iceWallPrefab;
        [SerializeField] private GameObject bladeStormPrefab;


        [Header("ニュートラル強")]
        [SerializeField] private TransformRecorder transformRecorder;
        [SerializeField] private CommandHistory commandHistory;
        [SerializeField] private GameObject iceCloneObj; // 分身のプレハブ
        [SerializeField] private float echoTimeInterval = 3.0f; // 再現する時間幅
        private bool isCloneAttacking = false;

        [Header("左強")]
        [SerializeField] private float magicDuration = 2f;
        [SerializeField] private float yOffset = 2;
        [SerializeField] private float leftStrongVelocity;
        [SerializeField] private float explosionDuration = 3f;
        private GameObject currentIceObj;
        private const int MAXCOUNT = 2;
        private int currentCount;

        [Header("右強")]
        [SerializeField] private float iceWallDuration = 5.0f;
        [SerializeField] private float wallOffset = 1.5f;

        [Header("上強")]
        [SerializeField] private float returnStrongUpEffectTime = 3f;
        [SerializeField] private float upwardVelocity = 2.5f;



        /// <summary>
        /// 数秒前の自分が氷で実体化し、攻撃も記録通りなぞってくれる
        /// </summary>
        public override void NeutralAttack()
        {
            if (isCloneAttacking) return;
            base.NeutralAttack();
            var cloneObj = Instantiate(iceCloneObj);
            transformRecorder.StartReplayFromSecondsAgo(echoTimeInterval, cloneObj.transform);

            if (commandHistory == null)
            {
                Debug.LogWarning("CommandHistoryが見つかりませんでした");
                return;
            }
            var cloneController = cloneObj.GetComponent<Player.CharacterController>();
            cloneController.SetClonePlayerID(characterController.PlayerID);
            if (characterController.GetCharacterState().AttackManager == null) return;
            commandHistory.ReplayAttackCommandsFromSecondsAgo(echoTimeInterval, cloneController.GetCharacterState().AttackManager);
            isCloneAttacking = true;
            DelayUtility.StartDelayedActionWithPause(this, echoTimeInterval, BattleJudge.I.GetPauseStateFunc, () =>
            {
                Destroy(cloneObj);
                isCloneAttacking = false;
            });
        }


        /// <summary>
        /// 氷の魔法を圧縮データにして飛ばす、二回目の入力で解凍
        /// その場で爆発が起こる
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();
            currentCount++;

            if (currentCount < MAXCOUNT)
            {
                magicCircle.SetActive(true);
                DelayUtility.StartDelayedActionWithPause(this, magicDuration, BattleJudge.I.GetPauseStateFunc, () =>
                {
                    magicCircle.SetActive(false);
                });

                var pos = transform.position.AddY(yOffset);
                currentIceObj = CharaEffectFactory.I.GetEffectObj(iceDataPrefab, pos, Quaternion.identity);
                var rb = currentIceObj.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.velocity = transform.forward * leftStrongVelocity;
            }
            else
            {
                var createPos = currentIceObj.transform.position;
                CharaEffectFactory.I.ReturnEffectObj(currentIceObj);
                var explosionObj = CharaEffectFactory.I.GetEffectObj(iceExplosionPrefab, createPos, Quaternion.identity);
                var charaEffectSetting = explosionObj.GetComponent<CharaEffect>();
                charaEffectSetting.SetOwnerId(characterController.PlayerID);
                charaEffectSetting.SetAttackProcessor(attackProcessor);
                DelayUtility.StartDelayedActionWithPause(this, explosionDuration, BattleJudge.I.GetPauseStateFunc, () =>
                {
                    CharaEffectFactory.I.ReturnEffectObj(explosionObj);
                });
                currentCount = 0;
            }
            AudioManager.I.PlayCharacterSE(CharacterType.Ame, CharacterSEType.StrongLeftAttack);
        }

        /// <summary>
        /// 前方に氷の壁を床から飛び出させる
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();

            // 氷の壁を生成
            Vector3 wallPos = transform.position
                + transform.forward * (wallOffset + 0.5f) // ← 少し前へ（+0.5fなど調整）
                - Vector3.up * 0.5f;                      // ← 少し下へ（-0.5fなど調整）

            Quaternion wallRot = Quaternion.LookRotation(-transform.forward);
            GameObject iceWallObj = CharaEffectFactory.I.GetEffectObj(iceWallPrefab, wallPos, wallRot);
            iceWallObj.transform.localScale = Vector3.one; // スケールリセット（前回の修正）

            // 攻撃設定
            var charaEffect = iceWallObj.GetComponent<CharaEffect>();
            if (charaEffect != null)
            {
                charaEffect.SetOwnerId(characterController.PlayerID);
                charaEffect.SetAttackProcessor(attackProcessor);
            }



            // 打ち上げ処理を一度だけ行う
            TryLaunchEnemy(iceWallObj);

            // 一定時間後にエフェクト削除
            DelayUtility.StartDelayedActionWithPause(this, iceWallDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(iceWallObj);
            });
        }
        private void TryLaunchEnemy(GameObject wallObj)
        {
            Vector3 wallCenter = wallObj.transform.position;
            float centerThreshold = 50f;

            Collider[] hits = Physics.OverlapBox(
                wallCenter,
                wallObj.transform.localScale * 0.5f + Vector3.up * 0.5f,
                wallObj.transform.rotation,
                LayerMask.GetMask("Enemy")
            );

            foreach (var hit in hits)
            {
                Rigidbody targetRb = hit.attachedRigidbody;
                if (targetRb == null) continue;

                Vector3 flatDiff = hit.transform.position - wallCenter;
                flatDiff.y = 0f;

                float distance = flatDiff.magnitude;
                Debug.Log($"[IceWall] Hit Enemy: {hit.name}, Distance from center: {distance}");

                if (distance <= centerThreshold)
                {
                    Vector3 knockDir = rightAttackData.knockbackDirection;
                    knockDir.y = 10f;
                    knockDir.Normalize();

                    Debug.Log($"[IceWall] Central Hit! Knockback Dir: {knockDir}, Force: {rightAttackData.knockback}");

                    targetRb.velocity = Vector3.zero;
                    targetRb.AddForce(knockDir * rightAttackData.knockback, ForceMode.Impulse);
                    break;
                }
            }
        }

        /// <summary>
        /// 下に剣を突き立てて周囲に氷の薔薇を咲かせて範囲攻撃
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();
            ActiveSword(downAttackData.attackDuration);
        }

        /// <summary>
        /// 上に剣を突き出し刃の竜巻を発生させる
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();

            Vector3 spawnPos = transform.position
                             + Vector3.up * yOffset;

            GameObject stormObj = CharaEffectFactory.I.GetEffectObj(bladeStormPrefab, spawnPos, Quaternion.identity);

            // ランダムなスケールを適用（例：0.7〜1.3倍）
            float scaleMultiplier = 1f;
            float chance = Random.value;
            if (chance < 0.3f) scaleMultiplier = 1.8f;     // 30%の確率で大きく
            else if (chance < 0.5f) scaleMultiplier = 0.3f; // 20%の確率で小さく

            // スケールの適用
            stormObj.transform.localScale *= scaleMultiplier;

            var rb = stormObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.velocity = transform.up * upwardVelocity;
            }

            var charaEffect = stormObj.GetComponent<CharaEffect>();
            if (charaEffect != null)
            {
                charaEffect.SetOwnerId(characterController.PlayerID);
                charaEffect.SetAttackProcessor(attackProcessor);
            }

            DelayUtility.StartDelayedActionWithPause(this, returnStrongUpEffectTime, BattleJudge.I.GetPauseStateFunc, () =>
           {
               CharaEffectFactory.I.ReturnEffectObj(stormObj);
           });
        }



        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
            ActiveSword(upAttackData.attackDuration);

        }

        private void ActiveSword(float duration)
        {
            swordObj.gameObject.SetActive(true);
            DelayUtility.StartDelayedActionWithPause(this, duration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                swordObj.gameObject.SetActive(false);
            });
        }
    }
}
