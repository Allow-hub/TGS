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
        /// <summary>
        /// ゴムベラを前に振る、前方への軽い攻撃。3回目で派生
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

        }

        /// <summary>
        /// 前方をロールして相手を蹴る
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();
        }

        /// <summary>
        /// 高速で後ろに下がる
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
