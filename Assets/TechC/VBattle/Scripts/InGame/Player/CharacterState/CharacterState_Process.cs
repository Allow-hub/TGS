using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        private class ProcessState : ImtStateMachine<CharacterState>.State
        {
            protected internal override void Enter()
            {
            }
            protected internal override void Update()
            {
                Debug.Log("Process");
            }
            protected internal override void Exit() 
            {
            }
        }
    }
}
