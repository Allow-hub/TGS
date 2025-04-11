using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    [Serializable]
    public class Ame_WeakAttack : WeakAttack
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
            Debug.Log("オーバーライド,"+attackData.damage);
        }

        public override void ForceFinish()
        {
            base.ForceFinish();
            Debug.Log("強制終了時");
        }
    }
}