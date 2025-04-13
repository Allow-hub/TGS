using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        private class GuardState : ImtStateMachine<CharacterState>.State
        {
            private int gardAnim = Animator.StringToHash("IsGarding");
            protected internal override void Enter()
            {
                base.Enter();
                Context.anim.SetBool(gardAnim, true);
            }

            protected internal override void Update()
            {
                base.Update();
                //if (!Context.playerInputManager.IsGarding)
                //    Context.stateMachine.SendEvent((int)StateEventId.Idle);
            }

            protected internal override void Exit()
            {
                base.Exit();
                Context.anim.SetBool(gardAnim, false);
            }
        }
    }
}
