using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        [Header("移動パラメータ")]
        [SerializeField] private float groundAcceleration = 20f;    // 地上の加速率
        [SerializeField] private float airAcceleration = 10f;       // 空中の加速率
        [SerializeField] private float maxSpeed = 8.0f;             // 最大速度
        [SerializeField] private float groundFriction = 25f;        // 地上の減速率
        [SerializeField] private float airFriction = 5f;            // 空中の減速率
        [SerializeField] private float turnSpeed = 35f;             // 方向転換時の減速率

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
                }
                else
                {
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
                if (Context.playerInputManager.IsWeakAttacking && Context.playerController.IsGrounded())
                    Context.stateMachine.SendEvent((int)StateEventId.WeakAttack);
                if (Context.playerInputManager.IsJumping && Context.playerController.IsGrounded())
                    Context.stateMachine.SendEvent((int)StateEventId.Jump);
            }



            protected internal override void Exit()
            {
                base.Exit();
            }

        }

    }
}
