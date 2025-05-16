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
        public override void NeutralAttack()
        {
            base.NeutralAttack();

        }

        public override void LeftAttack()
        {
            base.LeftAttack();
        }

        public override void RightAttack()
        {
            base.RightAttack();
        }

        public override void DownAttack()
        {
            base.DownAttack();
        }

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
