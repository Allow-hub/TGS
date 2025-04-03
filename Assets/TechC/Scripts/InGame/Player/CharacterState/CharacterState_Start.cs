using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        /// <summary>
        /// 3.2.1...Startまでの
        /// キャラの動き
        /// </summary>
        private class StartState : ImtStateMachine<CharacterState>.State
        {
            private float duration = 0.1f;
            private float elapsedTime = 0f; 
            protected internal override void Enter()
            {
                base.Enter();
            }
            protected internal override void Update()
            {
                base.Update();

                elapsedTime += Time.deltaTime;
                if(elapsedTime >= duration)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Start);
                    elapsedTime = 0f;
                }
            }
            protected internal override void Exit()
            {
                base.Exit();
            }
        }
    }
}
