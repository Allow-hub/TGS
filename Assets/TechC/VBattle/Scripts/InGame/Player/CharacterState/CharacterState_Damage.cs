using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {

        /// <summary>
        /// ダメージを喰らったときのステート
        /// </summary>
        private class DamageState : ImtStateMachine<CharacterState>.State
        {
            private int hitAnim = Animator.StringToHash("IsHitting");
            private float elapsedTime = 0f;
            private float duration = 1;
            protected internal override void Enter()
            {
                base.Enter();
                Context.isHitting = true;
                Context.anim.SetBool(hitAnim, true);
            }

            protected internal override void Update()
            {
                base.Update();
                elapsedTime += Time.deltaTime;
                if (elapsedTime > duration)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Idle);
                }
            }

            protected internal override void Exit()
            {
                base.Exit();
                Context.isHitting = false;
                elapsedTime = 0f;
                Context.anim.SetBool(hitAnim, false);
            }
        }
    }
}
