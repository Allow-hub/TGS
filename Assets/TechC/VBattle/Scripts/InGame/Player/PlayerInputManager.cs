using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

namespace TechC.Player
{
    public class PlayerInputManager : BaseInputManager
    {
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            bool started = context.started;
            bool canceled = context.canceled;

            if (started)
                isMoving = true;
            else if (canceled)
            {
                isMoving = false;
                isDashing = false;
            }

            // 基底クラスメソッドに転送
            OnMove(moveInput, started, canceled);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            if (started)
                isJumping = true;
            else if (canceled)
                isJumping = false;

            OnJump(started, canceled);
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            if (started)
                isCrouching = true;
            else if (canceled)
                isCrouching = false;

            OnCrouch(started, canceled);
        }

        public void OnGuard(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            if (started)
                isGuarding = true;
            else if (canceled)
                isGuarding = false;

            OnGuard(started, canceled);
        }

        public void OnWeakAttack(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            if (started)
                isWeakAttacking = true;
            else if (canceled)
                isWeakAttacking = false;

            OnWeakAttack(started, canceled);
        }

        public void OnStrongAttack(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            if (started)
                isStrongAttacking = true;
            else if (canceled)
                isStrongAttacking = false;

            OnStrongAttack(started, canceled);
        }

        public void OnAppeal(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            if (started)
                isAppealing = true;
            else if (canceled)
                isAppealing = false;

            OnAppeal(started, canceled);
        }

        // 基底クラスの抽象メソッド実装
        public override void OnMove(Vector2 inputValue, bool started, bool canceled)
        {
            // 移動コマンドは CheckDash 内で発行されるため、ここでは何もしない
        }

        public override void OnJump(bool started, bool canceled)
        {
            if (started && commands.ContainsKey(jumpCommand))
            {
                characterState.EnqueueCommand(commands[jumpCommand]);
            }
        }

        public override void OnCrouch(bool started, bool canceled)
        {
            if (started && commands.ContainsKey(crouchCommand))
            {
                characterState.EnqueueCommand(commands[crouchCommand]);
            }
        }

        public override void OnGuard(bool started, bool canceled)
        {
            if (started && commands.ContainsKey(guardCommand))
            {
                characterState.EnqueueCommand(commands[guardCommand]);
            }
        }

        public override void OnWeakAttack(bool started, bool canceled)
        {
            if (started && commands.ContainsKey(attackCommand))
            {
                characterState.EnqueueCommand(commands[attackCommand]);
            }
        }

        public override void OnStrongAttack(bool started, bool canceled)
        {
            if (started && commands.ContainsKey(attackCommand))
            {
                characterState.EnqueueCommand(commands[attackCommand]);
            }
        }

        public override void OnAppeal(bool started, bool canceled)
        {
            if (started && commands.ContainsKey(attackCommand))
            {
                characterState.EnqueueCommand(commands[attackCommand]);
            }
        }
    }
}