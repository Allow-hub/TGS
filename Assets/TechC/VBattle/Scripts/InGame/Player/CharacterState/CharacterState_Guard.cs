using IceMilkTea.StateMachine;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        private class GuardState : ImtStateMachine<CharacterState>.State
        {
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
