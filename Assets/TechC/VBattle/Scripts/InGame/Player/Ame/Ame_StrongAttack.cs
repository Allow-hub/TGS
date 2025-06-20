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
        [SerializeField] private float wallOffsetX = 1.5f;
        [SerializeField] private float wallOffsetY = 1.5f;

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
                RegisterEffect(currentIceObj);

                var rb = currentIceObj.GetComponent<Rigidbody>();
                rb.velocity = transform.forward * leftStrongVelocity;
                DelayUtility.StartDelayedActionWithPause(this, explosionDuration, BattleJudge.I.GetPauseStateFunc, () =>
                {
                    if (currentIceObj != null)
                    {
                        UnregisterEffect(currentIceObj);
                        CharaEffectFactory.I.ReturnEffectObj(currentIceObj);
                        currentCount = 0;
                        currentIceObj = null; // 明示的にクリア
                    }
                });
            }
            else
            {
                var createPos = currentIceObj.transform.position;
                UnregisterEffect(currentIceObj);
                CharaEffectFactory.I.ReturnEffectObj(currentIceObj);

                var explosionObj = CharaEffectFactory.I.GetEffectObj(iceExplosionPrefab, createPos, Quaternion.identity);
                RegisterEffect(explosionObj);

                var charaEffectSetting = explosionObj.GetComponent<CharaEffect>();
                charaEffectSetting.SetOwnerId(characterController.PlayerID);
                charaEffectSetting.SetAttackProcessor(attackProcessor);

                DelayUtility.StartDelayedActionWithPause(this, explosionDuration, BattleJudge.I.GetPauseStateFunc, () =>
                {
                    UnregisterEffect(explosionObj);
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
            GameObject iceWallObj = null;

            DelayUtility.StartDelayedActionWithPause(this, rightAttackData.hitTiming, BattleJudge.I.GetPauseStateFunc, () =>
            {
                var wallPos = transform.position.AddX(wallOffsetX).AddY(wallOffsetY);
                iceWallObj = CharaEffectFactory.I.GetEffectObj(iceWallPrefab, wallPos, Quaternion.identity);
                RegisterEffect(iceWallObj);

                var charaEffect = iceWallObj.GetComponent<CharaEffect>();
                charaEffect?.SetOwnerId(characterController.PlayerID);
                charaEffect?.SetAttackProcessor(attackProcessor);
            });

            DelayUtility.StartDelayedActionWithPause(this, iceWallDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                if (iceWallObj != null)
                {
                    UnregisterEffect(iceWallObj);
                    CharaEffectFactory.I.ReturnEffectObj(iceWallObj);
                }
            });
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

            Vector3 spawnPos = transform.position + Vector3.up * yOffset;
            GameObject stormObj = CharaEffectFactory.I.GetEffectObj(bladeStormPrefab, spawnPos, Quaternion.identity);
            RegisterEffect(stormObj);

            float scaleMultiplier = 1f;
            float chance = Random.value;
            if (chance < 0.3f) scaleMultiplier = 1.8f; //30%の確率で大きく
            else if (chance < 0.5f) scaleMultiplier = 0.3f; //20%の確率で小さく

            stormObj.transform.localScale *= scaleMultiplier;

            var rb = stormObj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = transform.up * upwardVelocity;

            var charaEffect = stormObj.GetComponent<CharaEffect>();
            charaEffect?.SetOwnerId(characterController.PlayerID);
            charaEffect?.SetAttackProcessor(attackProcessor);

            DelayUtility.StartDelayedActionWithPause(this, returnStrongUpEffectTime, BattleJudge.I.GetPauseStateFunc, () =>
            {
                UnregisterEffect(stormObj);
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
