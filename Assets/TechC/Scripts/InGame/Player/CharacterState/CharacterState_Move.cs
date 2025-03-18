using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        private class MoveState : ImtStateMachine<CharacterState>.State
        {
            protected internal override void Enter()
            {
                base.Enter();
            }

            protected internal override void Update()
            {
                base.Update();
                if (Context.playerInputManager.MoveInput == Vector2.zero)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Idle);
                    Debug.Log("Zero");
                }
                else
                {
                    Context.MoveCharacter(Context.playerData.MoveSpeed);
                }
            }
            
           

            protected internal override void Exit() 
            {
                base.Exit();
            }
        }

    }
}
