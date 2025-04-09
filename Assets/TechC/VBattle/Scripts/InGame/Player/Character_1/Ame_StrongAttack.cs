using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class Ame_StrongAttack : StrongAttack
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

        // ExecuteAttackメソッドもオーバーライド
        protected override void ExecuteAttack(AttackData attackData)
        {
            Debug.Log("オーバーライド,"+attackData.damage);
        }
    }
}
