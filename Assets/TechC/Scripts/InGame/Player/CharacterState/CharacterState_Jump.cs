using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        [Header("ジャンプ設定")]
        [SerializeField] private float jumpForce = 4;
  
        private class JumpState : ImtStateMachine<CharacterState>.State 
        {
            private int jumpAnim = Animator.StringToHash("IsJumping");
            private float elapsedTime = 0;
            private float jumpCoolTime = 0.5f;
            protected internal override void Enter()
            {
                base.Enter();
                Context.anim.SetBool(jumpAnim, true);
                Context.playerController.AddForcePlayer(Vector3.up,Context.jumpForce,ForceMode.Impulse);
            }

            protected internal override void Update()
            {
                base.Update();
                elapsedTime += Time.deltaTime;
                //ジャンプが二回連続で入らないように
                if (elapsedTime > jumpCoolTime)
                {
                    if (Context.playerController.IsGrounded())
                        Context.stateMachine.SendEvent((int)StateEventId.Idle);
                }

                Context.MoveCharacter(
                            Context.playerInputManager.MoveInput,
                            Context.isGrounded,
                            Context.groundAcceleration,
                            Context.airAcceleration,
                            Context.maxSpeed,
                            Context.groundFriction,
                            Context.airFriction,
                            Context.turnSpeed
                            );
            }

            protected internal override void Exit()
            {
                base.Exit();
                elapsedTime = 0;
                Context.anim.SetBool(jumpAnim, false);
            }
        }

    }
}
