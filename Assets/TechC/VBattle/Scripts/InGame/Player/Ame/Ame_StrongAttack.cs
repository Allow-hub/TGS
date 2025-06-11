using System.Collections;
using System.Collections.Generic;
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


        [Header("エフェクトのプレハブや参照")]
        [SerializeField] private GameObject bladeStormPrefab;

        // [Header("ニュートラル強")]
        [Header("ニュートラル強")]
        [SerializeField] private GameObject iceClonePrefab; // 分身のプレハブ
        [SerializeField] private float echoTimeWindow = 3.0f; // 再現する時間幅
        [SerializeField] private int maxEchoCount = 3;

        [Header("左強")]
        [SerializeField] private float magicDuration = 2f;
        [SerializeField] private float yOffset = 2;
        [SerializeField] private float leftStrongVelocity;
        [SerializeField] private float explosionDuration = 3f;
        private GameObject currentIceObj;
        private float elapsedTime;
        private const int MAXCOUNT = 2;
        private int currentCount;
        private bool OnleftStrong = false;

        // [Header("右強")]
        [Header("右強")]
        [SerializeField] private float iceWallDuration = 5.0f;
        [SerializeField] private float wallOffset = 1.5f;

        // [Header("下強")]
        // [Header("上強")]
        [Header("上強")]

        [SerializeField] private float returnStrongUpEffectTime = 3f;
        [SerializeField] private float upwardVelocity = 2.5f;
        [SerializeField] private float backOffset = 0.5f;
        [SerializeField] private float scaleVariance = 0.3f;



        /// <summary>
        /// 数秒前の自分が氷で実体化し、攻撃も記録通りなぞってくれる
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

            var commandHistory = GetComponent<CommandHistory>();
            if (commandHistory == null)
            {
                Debug.LogWarning("CommandHistoryが見つかりませんでした");
                return;
            }

            List<CommandHistory.CommandRecord> recentAttacks = commandHistory.GetFullHistory()
                .FindAll(r => r.commandInstance is AttackCommand && Time.time - r.executionTime <= echoTimeWindow);

            int echoCount = 0;

            foreach (var record in recentAttacks)
            {
                if (echoCount >= maxEchoCount) break;

                Vector3 spawnPos = record.playerPosition + Vector3.up * 0.1f;
                GameObject clone = Instantiate(iceClonePrefab, spawnPos, Quaternion.identity);

                // 攻撃コマンドの再実行
                if (record.commandInstance is AttackCommand atkCmd)
                {
                    // Clone の Transform や ID を使って攻撃の方向などを上書きしてもOK
                    atkCmd.Execute();  // 通常はクローンに対してやりたい処理
                }

                echoCount++;
            }
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
                DelayUtility.StartDelayedAction(this, magicDuration, () =>
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
                DelayUtility.StartDelayedAction(this, explosionDuration, () =>
                {
                    CharaEffectFactory.I.ReturnEffectObj(explosionObj);
                });
                currentCount = 0;
            }
            AudioManager.I.PlayCharacterSE(CharacterType.Ame,CharacterSEType.StrongLeftAttack);
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
            DelayUtility.StartDelayedAction(this, iceWallDuration, () =>
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

            DelayUtility.StartDelayedAction(this, returnStrongUpEffectTime, () =>
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
            DelayUtility.StartDelayedAction(this, duration, () =>
            {
                swordObj.gameObject.SetActive(false);
            });
        }
    }
}
