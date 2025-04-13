using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// NPC用入力マネージャー（UIボタンやデバッグ用）
    /// </summary>
    public class NpcInputManager : BaseInputManager
    {
        [Header("デバッグUI")]
        [SerializeField] private bool enableDebugUI = true;
        [SerializeField] private float buttonWidth = 100f;
        [SerializeField] private float buttonHeight = 40f;
        [SerializeField] private float startX = 10f;
        [SerializeField] private float startY = 10f;
        [SerializeField] private float padding = 5f;

        [Header("NPCの移動設定")]
        [SerializeField] private float inputSmoothness = 0.1f; // 入力の滑らかさ
        private Vector2 targetMoveInput; // 目標とする移動入力

        private void OnGUI()
        {
            if (!enableDebugUI) return;

            float currentY = startY;

            // 移動ボタン（左）
            if (GUI.Button(new Rect(startX, currentY, buttonWidth, buttonHeight), "左移動"))
            {
                OnMove(new Vector2(-1, 0), true, false);
            }

            // 移動ボタン（右）
            if (GUI.Button(new Rect(startX + buttonWidth + padding, currentY, buttonWidth, buttonHeight), "右移動"))
            {
                OnMove(new Vector2(1, 0), true, false);
            }

            // 移動停止ボタン
            if (GUI.Button(new Rect(startX + (buttonWidth + padding) * 2, currentY, buttonWidth, buttonHeight), "移動停止"))
            {
                OnMove(Vector2.zero, false, true);
            }

            currentY += buttonHeight + padding;

            // ジャンプボタン
            if (GUI.Button(new Rect(startX, currentY, buttonWidth, buttonHeight), "ジャンプ"))
            {
                OnJump(true, false);
                StartCoroutine(AutoRelease(ActionType.Jump));
            }

            // しゃがみボタン
            if (GUI.Button(new Rect(startX + buttonWidth + padding, currentY, buttonWidth, buttonHeight), isCrouching ? "しゃがみ解除" : "しゃがみ"))
            {
                if (isCrouching)
                    OnCrouch(false, true);
                else
                    OnCrouch(true, false);
            }

            currentY += buttonHeight + padding;

            // 弱攻撃ボタン
            if (GUI.Button(new Rect(startX, currentY, buttonWidth, buttonHeight), "弱攻撃"))
            {
                OnWeakAttack(true, false);
                StartCoroutine(AutoRelease(ActionType.WeakAttack));
            }

            // 強攻撃ボタン
            if (GUI.Button(new Rect(startX + buttonWidth + padding, currentY, buttonWidth, buttonHeight), "強攻撃"))
            {
                OnStrongAttack(true, false);
                StartCoroutine(AutoRelease(ActionType.StrongAttack));
            }

            currentY += buttonHeight + padding;

            // ガードボタン
            if (GUI.Button(new Rect(startX, currentY, buttonWidth, buttonHeight), isGuarding ? "ガード解除" : "ガード"))
            {
                if (isGuarding)
                    OnGuard(false, true);
                else
                    OnGuard(true, false);
            }

            // アピールボタン
            if (GUI.Button(new Rect(startX + buttonWidth + padding, currentY, buttonWidth, buttonHeight), "アピール"))
            {
                OnAppeal(true, false);
                StartCoroutine(AutoRelease(ActionType.Appeal));
            }

            currentY += buttonHeight + padding;
        }

        private enum ActionType
        {
            Move, Jump, Crouch, Guard, WeakAttack, StrongAttack, Appeal
        }

        // 一定時間後に自動的にボタンを離す処理
        private IEnumerator AutoRelease(ActionType actionType, float delay = 0.2f)
        {
            yield return new WaitForSeconds(delay);

            switch (actionType)
            {
                case ActionType.Jump:
                    OnJump(false, true);
                    break;
                case ActionType.WeakAttack:
                    OnWeakAttack(false, true);
                    break;
                case ActionType.StrongAttack:
                    OnStrongAttack(false, true);
                    break;
                case ActionType.Appeal:
                    OnAppeal(false, true);
                    break;
            }
        }

        protected override void Update()
        {
            // 移動入力を滑らかに変化させる
            // ここが重要：targetMoveInputから直接moveInputを更新
            moveInput = Vector2.Lerp(moveInput, targetMoveInput, inputSmoothness);

            // 移動状態の更新
            isMoving = moveInput.magnitude > 0.1f;

            // 基底クラスのUpdateを呼び出し、CheckDashなどを実行
            base.Update();
        }

        // --- 基底クラスの抽象メソッド実装 ---

        public override void OnMove(Vector2 inputValue, bool started, bool canceled)
        {
            targetMoveInput = inputValue;
            // 重要：moveInputも直接更新して即座に反映させる
            moveInput = inputValue;

            if (started)
            {
                isMoving = true;
            }
            else if (canceled)
            {
                targetMoveInput = Vector2.zero;
                moveInput = Vector2.zero;
                isMoving = false;
                isDashing = false;
            }

            // PlayerInputManagerと同様に、MoveCommandはCheckDash内で発行されるため
            // ここでは発行しません
        }

        public override void OnJump(bool started, bool canceled)
        {
            isJumping = started;

            if (started)
            {
                characterState.EnqueueCommand(commands[jumpCommand]);
            }
        }

        public override void OnCrouch(bool started, bool canceled)
        {
            isCrouching = started;

            if (started)
            {
                characterState.EnqueueCommand(commands[crouchCommand]);
            }
        }

        public override void OnGuard(bool started, bool canceled)
        {
            isGuarding = started;

            if (started)
            {
                characterState.EnqueueCommand(commands[guardCommand]);
            }
        }

        public override void OnWeakAttack(bool started, bool canceled)
        {
            isWeakAttacking = started;

            if (started)
            {
                characterState.EnqueueCommand(commands[attackCommand]);
            }
        }

        public override void OnStrongAttack(bool started, bool canceled)
        {
            isStrongAttacking = started;

            if (started)
            {
                characterState.EnqueueCommand(commands[attackCommand]);
            }
        }

        public override void OnAppeal(bool started, bool canceled)
        {
            isAppealing = started;

            // アピールコマンドがあれば実行
        }
    }
}