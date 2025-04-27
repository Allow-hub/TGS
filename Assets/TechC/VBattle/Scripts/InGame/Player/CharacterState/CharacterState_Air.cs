using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        private class AirState : ImtStateMachine<CharacterState>.State
        {



            protected internal override void Enter()
            {
                base.Enter(); 
                Context.currentCommand = null;

            }

            protected internal override void Update()
            {
                base.Update();
                Context.HandleCommand<IAirUsableCommand>(ref Context.currentCommand);

            }

            protected internal override void Exit()
            {
                base.Exit();
                Context.currentCommand = null;
            }

        }
    }
}
