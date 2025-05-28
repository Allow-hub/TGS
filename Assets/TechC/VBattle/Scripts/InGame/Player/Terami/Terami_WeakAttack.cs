using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    ///　キャラ２：照海の弱攻撃の実装
    /// </summary>
    public class Terami_WeakAttack : WeakAttack
    {
        [Header("エフェクトのプレハブや参照")]
        [SerializeField] private GameObject marshmallow; /* 右弱用 */
        [SerializeField] private GameObject cookie; /* 下弱用 */
        [SerializeField] private GameObject chocolate; /* 上弱用(CharaEffect,BoxColliderをつけること) */
        //    [SerializeField] private GameObject ;

        [Header("ニュートラルアタックの設定")]
        private float returnNeutralEffectTime = 3f;

        [Header("左弱")]
        [SerializeField] private float LeftXOffset = 3f;
        [SerializeField] private float LeftYOffset = 1f;

        [SerializeField] private float marshmallowThrowSpeed = 10f;
        [SerializeField] private float returnLeftEffectTime = 3.0f;


        [Header("右弱")]

        [SerializeField] private float returnRightEffectTime = 3.0f;


        [Header("下弱")]
        [SerializeField] private float returnDownEffectTime = 3.0f;

        [Header("上弱")]
        [SerializeField] private float UpYOffset = 5f;
        [SerializeField] private float chocolateFallSpeed = 10f;
        [SerializeField] private float returnUpEffectTime = 5.0f;

        /// <summary>
        /// ゴムベラを前に振る、前方への軽い攻撃。3回目で派生
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();
            // if (currentNeutral == neutralAttackData_1)
        }

        /// <summary>
        /// 未定
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();

            GameObject marshObj = null;
            DelayUtility.StartDelayedAction(this, rightAttackData.hitTiming, () =>
            {
                /* マシュマロの処理 */
                var marshPos = transform.position;
                marshPos = transform.position.AddY(LeftYOffset);
                Debug.Log($"変更前の座標の位置：{marshPos}");
                marshObj = CharaEffectFactory.I.GetEffectObj(marshmallow, marshPos, Quaternion.identity);
                var effectSetting = marshObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID);
                var rb = marshObj.GetComponent<Rigidbody>();
                /* マシュマロをrbで飛ばす */
                rb.velocity = transform.forward * marshmallowThrowSpeed;
            });

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnLeftEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(marshObj);
            });
        }

        /// <summary>
        /// マシュマロを投げて相手に当たったら爆発する、飛び道具側の攻撃はCharaEffectが行う
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();
        }

        /// <summary>
        /// 地面を叩いて、周囲にダメージを与える
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();

            var cookieObjPos = transform.position;

            var cookieEffect = CharaEffectFactory.I.GetEffectObj(cookie, cookieObjPos, Quaternion.identity);
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnDownEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(cookieEffect);
            });
        }

        /// <summary>
        /// 相手の上部からチョコを落とす、チョコの攻撃はCharaEffectが行う
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();

            /* チョコを上から降らせる処理 */
            GameObject chocolateObj = null; /* チョコのPrefabの初期化 */

            DelayUtility.StartDelayedAction(this, rightAttackData.hitTiming, () =>
            {
                /* 相手の座標を取得 */
                var otherPlayerPos = BattleJudge.I.GetOtherPlayerObjects(characterController.PlayerID)[0].transform.position; /* [0]を取得しているのは1vs1限定 */
                otherPlayerPos = otherPlayerPos.AddY(UpYOffset); /* 高さを追加 */

                /* お菓子の処理 */
                chocolateObj = CharaEffectFactory.I.GetEffectObj(chocolate, otherPlayerPos, Quaternion.identity);
                var effectSetting = chocolateObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID);
                var rb = chocolateObj.GetComponent<Rigidbody>();

                rb.velocity = -transform.up * chocolateFallSpeed; /* チョコを落下 */
            });

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnUpEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(chocolateObj);
            });
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
        }

    }
}
