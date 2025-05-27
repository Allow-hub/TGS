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
        [SerializeField] private GameObject cookie; /* 下弱用 */
        [SerializeField] private GameObject chocolate; /* 上弱用(CharaEffect,BoxColliderをつけること) */
        //     [SerializeField] private GameObject ;
        //    [SerializeField] private GameObject ;

        [Header("ニュートラルアタックの設定")]
        private float returnNeutralEffectTime = 3f;

        [Header("左弱")]

        [Header("右弱")]


        [Header("下弱")]
        private float returnDownEffectTime = 3.0f;

        [Header("上弱")]
        [SerializeField] private float yOffset = 10f;
        [SerializeField] private float chocolateFallSpeed = 3f;
        private float returnUpEffectTime = 100.0f;





        /// <summary>
        /// ゴムベラを前に振る、前方への軽い攻撃。3回目で派生
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();
            // if (currentNeutral == neutralAttackData_1)
        }

        /// <summary>
        /// 前方をロールして相手を蹴る
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();
        }

        /// <summary>
        /// 未定
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

            var cookieObjPos = characterController.transform.position;

            var cookieEffect = CharaEffectFactory.I.GetEffectObj(cookie, cookieObjPos, Quaternion.identity);
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnUpEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(cookieEffect);
            });
        }

        /// <summary>
        /// 相手の上部からお菓子を落とす、お菓子側の攻撃はCharaEffectが行う
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();

            /* お菓子を上から降らせる処理 */
            GameObject chocolateObj = null;
            var otherPlayer = BattleJudge.Instance.GetOtherPlayerObjects(characterController.PlayerID)[0];

            DelayUtility.StartDelayedAction(this, rightAttackData.hitTiming, () =>
            {
                /* お菓子の処理 */
                Debug.Log($"相手の座標の位置：{otherPlayer.transform.position}");
                var otherPlayerPos = transform.position.AddY(yOffset);
                Debug.Log($"変更後の座標：{otherPlayer.transform.position}");

                chocolateObj = CharaEffectFactory.I.GetEffectObj(chocolate, otherPlayerPos, Quaternion.identity);
                var effectSetting = chocolateObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID);
                var rb = chocolateObj.GetComponent<Rigidbody>();
                //斬撃をrbで飛ばす
                rb.velocity = transform.forward * chocolateFallSpeed;

                Debug.Log("チョコレートのエフェクトが出ました");

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
