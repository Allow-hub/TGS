using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

namespace TechC.Player
{
    public class PlayerInputManager : MonoBehaviour
    {
        [SerializeField] private Player.CharacterController characterController;
        private CharacterState characterState;
        [SerializeField] private WeakAttack weakAttack;
        private Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
        public Vector2 MoveInput => moveInput;
        public bool IsMoving => isMoving;
        public bool IsCrouching => isCrouching;
        public bool IsJumping => isJumping;
        public bool IsGuarding => isGuarding;
        public bool IsAppealing => isAppealing;
        public bool IsWeakAttacking => isWeakAttacking;
        public bool IsStrongAttacking => isStrongAttacking;
        public bool IsDashing => isDashing;
        private Vector2 moveInput;

        private bool isMoving;
        private bool lastIsMoving;  // 前回の移動状態を保持する変数
        private bool isCrouching;
        private bool isGuarding;
        private bool isJumping;
        private bool isAppealing;
        private bool isWeakAttacking;
        private bool isStrongAttacking;
        private bool isDashing;

        private string moveCommand = "Move";
        private string jumpCommand = "Jump";
        private string attackCommand = "Attack";
        private string crouchCommand = "Crouch";
        private string guardCommand = "Guard";

        private float lastMoveEndTime = 0f; // 前回の移動が終了した時間
        private float dashTimeWindow = 0.3f; // ダッシュ判定の有効時間（秒）

        private void Start()
        {
            characterState = characterController.GetCharacterState();
            // コマンドを登録
            commands[moveCommand] = new MoveCommand(characterController, this);
            commands[jumpCommand] = new JumpCommand(characterController);
            commands[attackCommand] = new AttackCommand(characterState);
            commands[crouchCommand] = new CrouchCommand(characterController, this);
            commands[guardCommand] =new GuardCommand(characterController,this);
        }


        private bool hasMoved = false; // 一度でも移動したかどうかのフラグ

        private int lastMoveDirection = 0; // 前回の移動方向 (-1: 左, 0: なし, 1: 右)

        private void Update()
        {
            CheckDash();
        }
        private void CheckDash()
        {
                // 移動入力がある場合
            if (moveInput.x != 0)
            {
                characterState.EnqueueCommand(commands[moveCommand]);

                // 現在の移動方向を取得
                int currentDirection = moveInput.x > 0 ? 1 : -1;

                // 以前に移動したことがあり、かつ前回の移動終了から一定時間内、かつ同じ方向であればダッシュ
                if (hasMoved &&
                    Time.time - lastMoveEndTime <= dashTimeWindow &&
                    currentDirection == lastMoveDirection)
                {
                    isDashing = true;
                    hasMoved = false; // ダッシュ後はフラグをリセット
                }

                // 現在の方向を保存
                lastMoveDirection = currentDirection;
            }
            // 移動入力がない場合
            else
            {
                if (lastIsMoving && !isMoving) // 移動が終了した瞬間
                {
                    lastMoveEndTime = Time.time; // 移動終了時間を記録
                    hasMoved = true; // 移動履歴フラグを立てる
                                     // lastMoveDirection はリセットせず保持する（方向の記憶）
                }

                isDashing = false; // 移動していない場合はダッシュ解除
            }

            lastIsMoving = isMoving; // 現在の移動状態を保存
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            if (context.started)
                isMoving = true;
            else if (context.canceled)
            {
                isMoving = false;
                isDashing = false ;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
                characterState.EnqueueCommand(commands[jumpCommand]);
        }

        public void OnAppeal(InputAction.CallbackContext context)
        {
            if (context.started)
                isAppealing = true;
            else if (context.canceled)
                isAppealing = false;
        }

        public void OnGuard(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isGuarding = true;
                characterState.EnqueueCommand(commands[guardCommand]);
            }
            else if (context.canceled)
                isGuarding = false;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                characterState.EnqueueCommand(commands[crouchCommand]);
                isCrouching = true;
            }
            else if (context.canceled)
                isCrouching = false;
        }

        public void OnWeakAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                characterState.EnqueueCommand(commands[attackCommand]);
                isWeakAttacking = true;
            }else if (context.canceled)
                isWeakAttacking = false;
        }

        public void OnStrongAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                characterState.EnqueueCommand(commands[attackCommand]);
                isStrongAttacking = true;
            }
            else if (context.canceled)
                isStrongAttacking = false;
        }
    }

}