using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        private class IdleState : ImtStateMachine<CharacterState>.State
        {
            private int idleAnim = Animator.StringToHash("IsIdling");
            protected internal override void Enter()
            {
                Context.anim.SetBool(idleAnim, true);
            }
            protected internal override void Update()
            {
                if (Context.playerInputManager.MoveInput.x != 0)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Move);
                }
            }

            protected internal override void Exit()
            {
                Context.anim.SetBool(idleAnim, false);
            }
        }
    }
}
