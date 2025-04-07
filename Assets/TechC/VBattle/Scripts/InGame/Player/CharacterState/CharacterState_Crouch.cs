using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        /// <summary>
        /// しゃがみのステート
        /// </summary>
        private class CrouchState : ImtStateMachine<CharacterState>.State
        {
            private int crouchAnim = Animator.StringToHash("IsCrouching");
            protected internal override void Enter()
            {
                base.Enter();
                Context.anim.SetBool(crouchAnim, true);
            }
            protected internal override void Update()
            {
                base.Update();
                if(!Context.playerInputManager.IsCrouching)
                    Context.stateMachine.SendEvent((int)StateEventId.Idle);
                if (Context.playerInputManager.IsWeakAttacking && Context.playerController.IsGrounded())
                    Context.stateMachine.SendEvent((int)StateEventId.Attack);
            }
            protected internal override void Exit()
            {
                base.Exit();
                Context.anim.SetBool(crouchAnim, false);

            }
        }
    }
}
