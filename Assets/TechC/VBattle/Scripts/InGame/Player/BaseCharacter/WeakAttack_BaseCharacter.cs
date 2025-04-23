using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace TechC
{
    [Serializable]
    public class WeakAttack_BaseCharacter : WeakAttack
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
        }


        public override void ForceFinish()
        {
            base.ForceFinish();
            StopAllCoroutines();
        }

     
    }
}
