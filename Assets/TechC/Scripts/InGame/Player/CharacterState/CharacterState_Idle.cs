using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        /// <summary>
        /// 何も押していない状態を表すステート
        /// </summary>
        private class IdleState : ImtStateMachine<CharacterState>.State
        {
            private GameObject playerObj;
            protected internal override void Enter()
            {
                if (playerObj == null) 
                    playerObj = Context.gameObject.transform.GetChild(0).gameObject;
                playerObj.transform.localRotation = Quaternion.identity;

            }
            protected internal override void Update()
            {
                if (Context.playerInputManager.MoveInput.x != 0)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Move);
                }

                //プレイヤーのインプットによってステートを変更する
                if (Context.playerInputManager.IsGarding)
                    Context.stateMachine.SendEvent((int)StateEventId.Guard);
                if (Context.playerInputManager.IsCrouching)
                    Context.stateMachine.SendEvent((int)StateEventId.Crouch);
                if (Context.playerInputManager.IsJumping && Context.playerController.IsGrounded())
                    Context.stateMachine.SendEvent((int)StateEventId.Jump);
                if (Context.playerInputManager.IsAppealing && Context.playerController.IsGrounded())
                    Context.stateMachine.SendEvent((int)StateEventId.Appeal);

                if (Context.playerInputManager.IsWeakAttacking && Context.playerController.IsGrounded())
                    Context.stateMachine.SendEvent((int)StateEventId.WeakAttack);
                if (Context.playerInputManager.IsStrongAttacking && Context.playerController.IsGrounded())
                    Context.stateMachine.SendEvent((int)StateEventId.StrongAttack);
            }

            protected internal override void Exit()
            {
            }
        }
    }
}
