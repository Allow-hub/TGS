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
            private int guardAnim = Animator.StringToHash("IsGuarding");
            protected internal override void Enter()
            {
                base.Enter();
            }

            protected internal override void Update()
            {
                base.Update();
                Context.characterController.DecreaseGuardPower();
                Context.HandleCommand<INeutralUsableCommand>(ref Context.currentCommand);
            }

            protected internal override void Exit()
            {
                base.Exit();
            }
        }
    }
}
