using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using TechC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

namespace TechC.Player
{
    public class PlayerInputManager : BaseInputManager
    {
        // 入力判定設定
        [Header("入力判定設定")]
        [SerializeField] private float intentionalMoveThreshold = 0.7f; // 意図的な移動と判断する閾値
        [SerializeField] private float jumpButtonPriority = 0.9f; // ジャンプボタン優先度（高いほどボタン入力優先）
        [SerializeField] private float crouchButtonPriority = 0.9f; // しゃがみボタン優先度
        [SerializeField] private float inputDebounceDuration = 0.12f; // 入力の無効時間（秒）
        
        // スナップ方向入力用の設定
        [Header("スナップ方向設定")]
        [SerializeField] private float directionDeadzone = 0.3f; // 方向入力のデッドゾーン
        [SerializeField] private bool restrictJumpToPureUp = true; // ジャンプを純粋な上方向のみに制限するか
        [SerializeField] private bool restrictCrouchToPureDown = true; // しゃがみを純粋な下方向のみに制限するか

        // 内部状態管理
        private Vector2 lastMoveInput;
        private float lastJumpTime = -10f;
        private float lastCrouchTime = -10f;
        private bool isJumpButtonPressed = false;
        private bool isCrouchButtonPressed = false;
        private InputDirection currentDirection = InputDirection.None;

        // 移動入力処理
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
                currentDirection = InputDirection.None;
            }

            // スナップされた方向を取得
            currentDirection = DirectionInputHandler.GetSnappedDirection(moveInput, directionDeadzone);

            // 入力を保存
            lastMoveInput = moveInput;

            // 基底クラスメソッドに転送
            OnMove(moveInput, started, canceled);
        }

        // ジャンプ入力処理
        public void OnJump(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            // ボタン状態を更新
            if (started)
                isJumpButtonPressed = true;
            else if (canceled)
                isJumpButtonPressed = false;

            // 誤操作防止ロジック
            if (started)
            {
                float currentTime = Time.time;
                
                // 次の条件のいずれかを満たす場合のみジャンプを実行
                bool shouldJump = false;
                
                // 1. 明示的なボタン入力（スティック以外）の場合は常に許可
                if (context.control.name != "stick")
                {
                    shouldJump = true;
                }
                // 2. スティック入力の場合
                else
                {
                    // スナップ方向による判定
                    if (restrictJumpToPureUp)
                    {
                        // 真上方向の場合のみジャンプを許可
                        shouldJump = DirectionInputHandler.IsJumpDirection(currentDirection);
                    }
                    else
                    {
                        // 従来のロジック：上方向への強い入力があればジャンプを許可
                        if (moveInput.y > jumpButtonPriority)
                        {
                            shouldJump = true;
                        }
                    }
                }
                
                // 前回のジャンプからの時間間隔を考慮
                if (currentTime - lastJumpTime <= inputDebounceDuration)
                {
                    shouldJump = false;
                }

                if (shouldJump)
                {
                    isJumping = true;
                    lastJumpTime = currentTime;
                    OnJump(true, false);
                }
            }
            else if (canceled)
            {
                isJumping = false;
                OnJump(false, true);
            }
        }

        // しゃがみ入力処理
        public void OnCrouch(InputAction.CallbackContext context)
        {
            bool started = context.started;
            bool canceled = context.canceled;

            // ボタン状態を更新
            if (started)
                isCrouchButtonPressed = true;
            else if (canceled)
                isCrouchButtonPressed = false;

            // 誤操作防止ロジック
            if (started)
            {
                float currentTime = Time.time;
                
                // 次の条件のいずれかを満たす場合のみしゃがみを実行
                bool shouldCrouch = false;
                
                // 1. 明示的なボタン入力（スティック以外）の場合は常に許可
                if (context.control.name != "stick")
                {
                    shouldCrouch = true;
                }
                // 2. スティック入力の場合
                else
                {
                    // スナップ方向による判定
                    if (restrictCrouchToPureDown)
                    {
                        // 真下方向の場合のみしゃがみを許可
                        shouldCrouch = DirectionInputHandler.IsCrouchDirection(currentDirection);
                    }
                    else
                    {
                        // 従来のロジック：下方向への強い入力があればしゃがみを許可
                        if (moveInput.y < -crouchButtonPriority)
                        {
                            shouldCrouch = true;
                        }
                    }
                }
                
                // 前回のしゃがみからの時間間隔を考慮
                if (currentTime - lastCrouchTime <= inputDebounceDuration)
                {
                    shouldCrouch = false;
                }

                if (shouldCrouch)
                {
                    isCrouching = true;
                    lastCrouchTime = currentTime;
                    OnCrouch(true, false);
                }
            }
            else if (canceled)
            {
                isCrouching = false;
                OnCrouch(false, true);
            }
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