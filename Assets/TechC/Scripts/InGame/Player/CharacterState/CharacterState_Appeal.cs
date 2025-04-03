using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        [Header("アピール設定")]
        [SerializeField] private float duration;

        private class AppealState : ImtStateMachine<CharacterState>.State
        {
            private float elapsedTime;
            protected internal override void Enter()
            {
                base.Enter();
            }

            protected internal override void Update()
            {
                base.Update();
                elapsedTime += Time.deltaTime;
                if(elapsedTime>Context.duration)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Idle);
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
