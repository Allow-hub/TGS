using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめの弱攻撃の実装
    /// </summary>
    public class Ame_WeakAttack : WeakAttack
    {
        [Header("エフェクトのプレハブや参照")]
        [SerializeField] private GameObject sword;
        [SerializeField] private GameObject slash;
        [SerializeField] private GameObject flyingSlash;
        [SerializeField] private GameObject flower;

        [Header("ニュートラルアタックの設定")]
        [SerializeField] private float slashEffectDistance = 2f;
        [SerializeField] private Vector3 n1Angle;
        [SerializeField] private Vector3 n2Angle;
        [SerializeField] private Vector3 n3Angle;
        private float returnNeutralEffectTime = 3f;
        private Vector3 currentSlashAngle;
        [Header("左弱")]
        [SerializeField] private AttackData leftAttackCounter;
        private bool isCountering = false;

        [Header("右弱")]
        [SerializeField] private float xOffset = 3f;
        [SerializeField] private float flyingSlashSpeed = 5f;
        private float returnRightEffectTime = 3f;

        [Header("下弱")]
        //スライディング時の変化後の自分の当たり判定
        [SerializeField] private Vector3 changeHitBox;
        [SerializeField] private float chageColliderSpeed = 10f;
        [SerializeField] private float slidingSpeed = 5f;
        private float returnDownEffectTime = 2f;


        [Header("上弱")]
        [SerializeField] private float xFlowerOffset;
        [SerializeField] private float yFlowerOffset;
        private float returnUpEffectTime = 3f;

        /// <summary>
        /// 剣を振る、前方への軽い攻撃。３回まで派生
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

            //ニュートラルが何段階目かを確かめる
            if (currentNeutral == neutralAttackData_1)
                currentSlashAngle = n1Angle;
            else if (currentNeutral == neutralAttackData_2)
                currentSlashAngle = n2Angle;
            else if (currentNeutral == neutralAttackData_3)
                currentSlashAngle = n3Angle;
            var slObjPos = transform.position.AddY(slashEffectDistance);
            // 向きに応じて回転反転
            if (transform.forward.x < 0)
            {
                currentSlashAngle.y = 180 - currentSlashAngle.y;
            }
            var rotation = Quaternion.Euler(currentSlashAngle);

            //slashEffectの取得。各段階の回転を反映
            var slObj = CharaEffectFactory.I.GetEffectObj(slash, slObjPos, rotation);
            RegisterEffect(slObj);
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedActionWithPause(this, returnNeutralEffectTime, BattleJudge.I.GetPauseStateFunc, () =>
            {
                UnregisterEffect(slObj);
                CharaEffectFactory.I.ReturnEffectObj(slObj);
            });
            AudioManager.I.PlayCharacterSE(CharacterType.Ame, CharacterSEType.WeakNormalAttack_1);
        }

        /// <summary>
        /// カウンター
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();  
            characterController.SetCanCounter(true);
            characterController.SetCounterAction(CounterAttack);
            isCountering = false;
            //カウンター待機時に攻撃を受けなかった場合戻す
            DelayUtility.StartDelayedActionWithPause(this, leftAttackData.attackDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                if (isCountering) return;
                characterController.SetCanCounter(false);
                characterController.SetCounterAction(null);
            });
        }

        private void CounterAttack()
        {
            isAttacking = false;
            isCountering = true;
            // 前方にレイを飛ばして相手をチェック
            float rayDistance = 30f;
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                var opponentController = hit.collider.GetComponentInParent<Player.CharacterController>();
                //相手の方向を向く
                if (opponentController != null && opponentController.PlayerID != characterController.PlayerID)
                {
                }
                else
                {
                    transform.forward = -transform.forward;
                }
            }
            // カウンター攻撃を実行
            ExecuteAttack(leftAttackCounter);
            DelayUtility.StartDelayedActionWithPause(this, 0.1f, BattleJudge.I.GetPauseStateFunc, () =>
            {
                characterController.GetAnim().SetBool(leftAttackData.animHash, false);
            });

            DelayUtility.StartDelayedActionWithPause(this, leftAttackCounter.attackDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                isCountering = false;
                characterController.SetCanCounter(false);
                characterController.SetCounterAction(null);
            });
        }


        /// <summary>
        /// 氷の斬撃を飛ばす、飛び道具側の攻撃はCharaEffectが行う
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();
            GameObject slObj = null;
            DelayUtility.StartDelayedActionWithPause(this, rightAttackData.hitTiming, BattleJudge.I.GetPauseStateFunc, () =>
            {
                //飛び道具の処理
                var pos = transform.position.AddX(xOffset);
                slObj = CharaEffectFactory.I.GetEffectObj(flyingSlash, pos, Quaternion.identity);
                RegisterEffect(slObj);
                var effectSetting = slObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID);
                var rb = slObj.GetComponent<Rigidbody>();
                //斬撃をrbで飛ばす
                rb.velocity = transform.forward * flyingSlashSpeed;
            });

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedActionWithPause(this, returnRightEffectTime, BattleJudge.I.GetPauseStateFunc, () =>
            {
                UnregisterEffect(slObj);
                CharaEffectFactory.I.ReturnEffectObj(slObj);
            });
        }

        /// <summary>
        /// スライディング攻撃、canRepeatがtrueにしてあるので多段ヒット
        /// スライディング中は敵との物理衝突をなくし貫通させる
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();
            characterController.StopVelocity();
            characterController.AddForcePlayer(transform.forward, slidingSpeed, ForceMode.Impulse);
            characterController.ChangeHitCollider(changeHitBox, chageColliderSpeed);
            characterController.ChangeColliderTrigger(true);
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedActionWithPause(this, returnDownEffectTime, BattleJudge.I.GetPauseStateFunc, () =>
            {
                characterController.ResetHitCollider(chageColliderSpeed);
                characterController.ChangeColliderTrigger(false);
            });
        }

        /// <summary>
        /// 軽く剣を振り上げる、エフェクトの中身未実装
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();
            GameObject obj = null;
            var basePos = transform.position + transform.forward * xFlowerOffset + Vector3.up * yFlowerOffset;
            obj = CharaEffectFactory.I.GetEffectObj(flower, basePos, Quaternion.identity);
            RegisterEffect(obj);
            if (transform.forward.x < 0)
            {
                obj.transform.Rotate(0, 180, 0);
            }
            else if (transform.forward.x > 0)
            {
                obj.transform.Rotate(Vector3.zero);
            }


            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedActionWithPause(this, returnUpEffectTime, BattleJudge.I.GetPauseStateFunc, () =>
            {
                UnregisterEffect(obj);
                CharaEffectFactory.I.ReturnEffectObj(obj);
            });
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
            sword.SetActive(true);

            DelayUtility.StartDelayedActionWithPause(this, attackData.attackDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                sword.SetActive(false);
            });
        }
    }
}