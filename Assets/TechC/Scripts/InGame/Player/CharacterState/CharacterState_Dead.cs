using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        /// <summary>
        /// HPが０になったときのステート
        /// </summary>
       private class DeadState : ImtStateMachine<CharacterState>.State
        {
            protected internal override void Enter()
            {
                base.Enter();
            }

            protected internal override void Update()
            {
                base.Update();
            }

            protected internal override void Exit()
            {
                base.Exit();
            }
        }
    }
}
