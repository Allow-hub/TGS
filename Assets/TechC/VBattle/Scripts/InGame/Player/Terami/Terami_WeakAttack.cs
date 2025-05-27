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
        [SerializeField] private GameObject cookie;
        //     [SerializeField] private GameObject ;
        //     [SerializeField] private GameObject ;
        //    [SerializeField] private GameObject ;

        [Header("ニュートラルアタックの設定")]
        private float returnNeutralEffectTime = 3f;

        [Header("下弱")]
        private float returnRightEffectTime = 3.0f;




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
            Debug.Log("クッキーエフェクトが出ました");
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnRightEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(cookieEffect);
            });
        }

        /// <summary>
        /// 相手の上部からお菓子を落として攻撃
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
            Debug.Log("オーバーライド," + attackData.damage);
        }

    }
}
