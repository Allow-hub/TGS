using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {

        private class AppealState : ImtStateMachine<CharacterState>.State
        {
            [Header("アピール設定")]
            private float duration;
            private float elapsedTime;
            protected internal override void Enter()
            {
                base.Enter();
            }

            protected internal override void Update()
            {
                base.Update();
                elapsedTime += Time.deltaTime;
                if(elapsedTime>duration)
                {
                    //Context.stateMachine.SendEvent((int)StateEventId.Idle);
                    elapsedTime = 0;
                }
            }

            protected internal override void Exit()
            {
                base.Exit();
            }
        }
    }
}
