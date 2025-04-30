using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{ /// <summary>
  /// キャラクター入力を管理する抽象基底クラス
  /// PlayerとNpcの共通処理を定義します
  /// </summary>
    public abstract class BaseInputManager : MonoBehaviour
    {
        [SerializeField] protected Player.CharacterController characterController;
        [SerializeField, ReadOnly] private string inputLogId = "input";
        protected CharacterState characterState;
        protected CommandHistory commandHistory;

        protected Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        [Header("Guard")]
        [SerializeField] private GameObject guardObj;

        // 入力状態
        protected Vector2 moveInput;
        protected bool isMoving;
        protected bool lastIsMoving;
        protected bool isCrouching;
        protected bool isGuarding;
        protected bool isJumping;
        protected bool isAppealing;
        protected bool isWeakAttacking;
        protected bool isStrongAttacking;
        protected bool isDashing;

        // コマンド名定義
        protected string moveCommand = "Move";
        protected string jumpCommand = "Jump";
        protected string attackCommand = "Attack";
        protected string crouchCommand = "Crouch";
        protected string guardCommand = "Guard";

        // ダッシュ関連変数
        protected float lastMoveEndTime = 0f;
        [SerializeField] protected float dashTimeWindow = 0.3f;
        protected bool hasMoved = false;
        protected int lastMoveDirection = 0;

        // プロパティ
        public Vector2 MoveInput => moveInput;
        public bool IsMoving => isMoving;
        public bool IsCrouching => isCrouching;
        public bool IsJumping => isJumping;
        public bool IsGuarding => isGuarding;
        public bool IsAppealing => isAppealing;
        public bool IsWeakAttacking => isWeakAttacking;
        public bool IsStrongAttacking => isStrongAttacking;
        public bool IsDashing => isDashing;

        protected virtual void Awake()
        {
            // CommandHistoryを取得
            commandHistory = GetComponent<CommandHistory>();
        }

        protected virtual void Start()
        {
            if (characterController != null)
            {
                characterState = characterController.GetCharacterState();

                // 基本コマンドを登録
                RegisterCommands();
            }
            else
            {
                CustomLogger.Error("CharacterControllerが設定されていません", inputLogId);
            }
        }

        /// <summary>
        /// 基本コマンドを登録するメソッド
        /// </summary>
        protected virtual void RegisterCommands()
        {
            commands[moveCommand] = new MoveCommand(characterController, this);
            commands[jumpCommand] = new JumpCommand(characterController, this);
            commands[attackCommand] = new AttackCommand(characterState);
            commands[crouchCommand] = new CrouchCommand(characterController, this);
            commands[guardCommand] = new GuardCommand(characterState, characterController, this, characterController.GetCharacterData(), guardObj);
        }

        protected virtual void Update()
        {
            CheckDash();
        }

        /// <summary>
        /// ダッシュ条件をチェックするメソッド
        /// </summary>
        protected virtual void CheckDash()
        {
            // 移動入力がある場合
            if (moveInput.x != 0)
            {
                // 現在の移動方向を取得
                int currentDirection = moveInput.x > 0 ? 1 : -1;
                characterState.EnqueueCommand(commands[moveCommand]);


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
                }

                isDashing = false; // 移動していない場合はダッシュ解除
            }

            lastIsMoving = isMoving; // 現在の移動状態を保存
        }

        /// <summary>
        /// 全ての入力をリセット
        /// </summary>
        public void ResetInput()
        {
            isMoving = false;
            isJumping = false;
            isGuarding = false;
            isAppealing = false;
            isCrouching = false;
            isDashing = false;
            isWeakAttacking = false;
            isStrongAttacking = false;
        }

        /// <summary>
        /// コマンドのインスタンスを外部から取得する
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public ICommand GetCommandInstance(string commandName) => commands[commandName];

        // 各入力に対する抽象メソッド - 継承先で実装
        public abstract void OnMove(Vector2 inputValue, bool started, bool canceled);
        public abstract void OnJump(bool started, bool canceled);
        public abstract void OnCrouch(bool started, bool canceled);
        public abstract void OnGuard(bool started, bool canceled);
        public abstract void OnWeakAttack(bool started, bool canceled);
        public abstract void OnStrongAttack(bool started, bool canceled);
        public abstract void OnAppeal(bool started, bool canceled);
    }

}
