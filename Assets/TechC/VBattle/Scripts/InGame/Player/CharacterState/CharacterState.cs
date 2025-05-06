using IceMilkTea.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Extensions;
using TechC.Player;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static TechC.AttackManager;

namespace TechC
{
    /// <summary>
    /// プレイヤーの行動状態を管理する
    /// またpartialで分離してそれぞれのStateの中身を実装している
    /// </summary>

    [Serializable]
    public partial class CharacterState
    {
        public enum StateEventId
        {
            Start,        // 初期状態
            Neutral,      // 通常状態（立ち/移動/しゃがみなど通常操作を受け付ける状態）
            Air,          // 空中状態
            Attack,       // 攻撃中状態
            Appeal,       // アピール状態
            Damage,       // 被ダメージ状態
            Guard,        // ガード状態
            Dead          // 死亡状態

        }

        [Header("Reference")]
        private BaseInputManager playerInputManager;
        private Player.CharacterController characterController;
        private CommandHistory commandHistory;

        private string stateLogId = "state";

        private Animator anim;

        private Queue<ICommand> commandQueue = new Queue<ICommand>();
        private ICommand currentCommand = null;

        [Header("攻撃設定")]
        private AttackManager attackManager;


        [Header("アニメーション")]
        //private int moveAnim = Animator.StringToHash("IsMoving");
        private int jumpAnim = Animator.StringToHash("IsJumping");
        private int doubleJumpAnim = Animator.StringToHash("IsDoubleJumping");
        private int crouchAnim = Animator.StringToHash("IsCrouching");

        // コマンドの優先順位を定義（数値が大きいほど優先度が高い）
        private readonly Dictionary<System.Type, int> commandPriority = new Dictionary<System.Type, int>
        {
            { typeof(MoveCommand), 1 },      // 移動は優先度低め
            { typeof(CrouchCommand), 2 },    // しゃがみは移動より優先
            { typeof(JumpCommand), 3 } ,      // ジャンプ
            { typeof(GuardCommand), 4 },       // ガードは最優先
            {typeof(AttackCommand), 5 }
        };
        public ImtStateMachine<CharacterState> StateMachine => stateMachine;

        private ImtStateMachine<CharacterState> stateMachine;

        public bool IsHitting => isHitting;
        private bool isHitting = false;

        /// <summary>
        /// キャラクターのステートのコンストラクタ
        /// </summary>
        /// <param name="playerInputManager"></param>
        /// <param name="characterController"></param>
        /// <param name="attackManager"></param>
        /// <param name="anim"></param>
        /// <param name="commandHistory"></param>
        public CharacterState(BaseInputManager playerInputManager,
                              Player.CharacterController characterController,
                              AttackManager attackManager,
                              Animator anim,
                              CommandHistory commandHistory)
        {
            this.playerInputManager = playerInputManager;
            this.characterController = characterController;
            this.attackManager = attackManager;
            this.anim = anim;
            Init();
            this.commandHistory = commandHistory;
        }

        private void Init()
        {

            stateMachine = new ImtStateMachine<CharacterState>(this);

            //通常ステートへの移行
            stateMachine.AddTransition<StartState, NeutralState>((int)StateEventId.Start);
            stateMachine.AddTransition<AirState, NeutralState>((int)StateEventId.Neutral);
            stateMachine.AddTransition<AttackState, NeutralState>((int)StateEventId.Neutral);
            stateMachine.AddTransition<GuardState, NeutralState>((int)StateEventId.Neutral);
            stateMachine.AddTransition<DamageState, NeutralState>((int)StateEventId.Neutral);
            //stateMachine.AddTransition<AppealState, NeutralState>((int)StateEventId.Neutral);

            //通常ステートからの移行
            stateMachine.AddTransition<NeutralState, AirState>((int)StateEventId.Air);
            stateMachine.AddTransition<NeutralState, GuardState>((int)StateEventId.Guard);
            stateMachine.AddTransition<NeutralState, AttackState>((int)StateEventId.Attack);
            stateMachine.AddTransition<NeutralState, DamageState>((int)StateEventId.Damage);
            //stateMachine.AddTransition<NeutralState, AppealState>((int)StateEventId.Appeal);

            //空中ステートからの移行
            stateMachine.AddTransition<AirState, DamageState>((int)StateEventId.Damage);
            stateMachine.AddTransition<AirState, AttackState>((int)StateEventId.Attack);

            //攻撃ステートからの移行
            stateMachine.AddTransition<AttackState, DamageState>((int)StateEventId.Damage);

            //アピールステートからの移行
            //stateMachine.AddTransition<AppealState, DamageState>((int)StateEventId.Damage);

            //どのステートからでも移行できる
            stateMachine.AddAnyTransition<DeadState>((int)StateEventId.Dead);

            //開始時のステート設定
            stateMachine.SetStartState<StartState>();
        }

        public void OnUpdate()
        {
            stateMachine.Update();
            CustomLogger.Info(stateMachine.CurrentStateName, stateLogId);
            // 重複しないようにフィルタリング
            var newQueue = new Queue<ICommand>();
            foreach (var cmd in commandQueue)
            {
                if (!(cmd is MoveCommand) && !(cmd is JumpCommand) && !(cmd is AttackCommand))
                {
                    newQueue.Enqueue(cmd);
                }
            }
            commandQueue = newQueue;

            // 死亡チェック
            if (characterController.GetHp() <= 0)
                ChangeDeadState();
        }

        public void EnqueueCommand(ICommand command)
        {
            commandQueue.Enqueue(command);
        }

        private void HandleCommand<T>(ref ICommand currentCommand) where T : class, ICommand
        {
            // 実行中のコマンドがなければ、新しいコマンドを取り出す
            if (currentCommand == null)
            {
                GetNextCommand<T>(ref currentCommand);
                // コマンド履歴に記録
                if (currentCommand != null)
                {
                    RecordCommandToHistory<T>(ref currentCommand);
                }
            }
            else
            {
                // 割り込み可能かチェック
                var highPriorityCommand = CheckForHighPriorityCommand<T>(ref currentCommand);
                if (highPriorityCommand != null)
                {
                    CustomLogger.Info($"[{stateMachine.CurrentStateName}] コマンド {currentCommand.GetType().Name} を中断し、{highPriorityCommand.GetType().Name} を実行", stateLogId);
                    currentCommand.ForceFinish();
                    currentCommand = highPriorityCommand;
                    currentCommand.Execute();
                    RecordCommandToHistory<T>(ref currentCommand);
                }
                else
                {
                    currentCommand.Execute();

                    // 終了チェック
                    if ((currentCommand is T usableCommand && usableCommand.IsFinished) ||
                        (currentCommand is ICommand && currentCommand.IsFinished))
                    {
                        currentCommand = null;
                        GetNextCommand<T>(ref currentCommand);
                    }
                }
            }
        }

        /// <summary>
        /// 実行したコマンドの履歴を保存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void RecordCommandToHistory<T>(ref ICommand currentCommand) where T : class, ICommand
        {

            if (commandHistory != null && currentCommand != null)
            {
                //CommandがAttackCommandならTypeとStrengthの保存
                if (currentCommand is AttackCommand attackCommand)
                {
                    attackCommand.SetAttackType(CheckAttackType());
                    attackCommand.SetAttackStrength(CheckAttackStrength());

                }
                commandHistory.RecordCommand(
                    currentCommand,
                    GetType().Name,
                    !(currentCommand is T usableCommand) || usableCommand.IsFinished,
                    characterController.transform.position
                );
            }
            //CustomLogger.Info("コマンドを保存"+Time.time);
        }
        /// <summary>
        /// 攻撃種方向の確認
        /// </summary>
        /// <returns></returns>
        private AttackType CheckAttackType()
        {
            if (playerInputManager.MoveInput.x < 0)
                return AttackType.Left;
            if (playerInputManager.MoveInput.x > 0)
                return AttackType.Right;
            if (playerInputManager.MoveInput.y < 0)
                return AttackType.Down;
            if (playerInputManager.MoveInput.y > 0)
                return AttackType.Up;
            return AttackType.Neutral;
        }
        /// <summary>
        /// 攻撃の強さの確認
        /// </summary>
        /// <returns></returns>
        private AttackStrength CheckAttackStrength()
        {

            // 攻撃強度の判定
            if (playerInputManager.IsWeakAttacking)
                return AttackStrength.Weak;
            else if (playerInputManager.IsStrongAttacking)
                return AttackStrength.Strong;
            else if (playerInputManager.IsAppealing)
                return AttackStrength.Appeal;

            return AttackStrength.Weak;
        }
        /// <summary>
        /// 次のコマンドを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void GetNextCommand<T>(ref ICommand currentCommand) where T : class, ICommand
        {
            while (commandQueue.Count > 0)
            {
                var command = commandQueue.Dequeue();
                if (command is T usable)
                {
                    CustomLogger.Info($"[{stateMachine.CurrentStateName}]対応コマンド: {command.GetType().Name}", stateLogId);
                    currentCommand = usable;
                    currentCommand.Execute(); // 最初の1回
                    break;
                }
                else
                {
                    CustomLogger.Info($"[ {stateMachine.CurrentStateName} ]非対応コマンド: {command.GetType().Name}", stateLogId);
                }
            }
        }

        /// <summary>
        /// 優先度の高いコマンドがキューにあるか確認
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T CheckForHighPriorityCommand<T>(ref ICommand currentCommand) where T : class, ICommand
        {
            if (commandQueue.Count == 0 || currentCommand == null)
                return null;

            int currentPriority = GetCommandPriority(currentCommand.GetType());

            foreach (var command in commandQueue)
            {
                if (command is T usable)
                {
                    int commandPriorityValue = GetCommandPriority(command.GetType());
                    if (commandPriorityValue > currentPriority)
                    {
                        // 高優先度コマンドを削除して返す
                        commandQueue = new Queue<ICommand>(
                            System.Linq.Enumerable.Where(commandQueue, c => c != command)
                        );
                        return usable;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// コマンドの優先度を取得
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        private int GetCommandPriority(System.Type commandType)
        {
            if (commandPriority.TryGetValue(commandType, out int priority))
            {
                return priority;
            }
            return 0; // デフォルトの優先度
        }

        public ICommand GetCurrentCommand() => currentCommand;
        public void ChangeNeutralState() => stateMachine.SendEvent((int)StateEventId.Neutral);
        public void ChangeAirState() => stateMachine.SendEvent((int)StateEventId.Air);
        public void ChangeGuardrState() => stateMachine.SendEvent((int)StateEventId.Guard);
        public void ChangeAttackState() => stateMachine.SendEvent((int)StateEventId.Attack);

        public void ChangeDamageState() => stateMachine.SendEvent((int)StateEventId.Damage);
        public void ChangeDeadState() => stateMachine.SendEvent((int)StateEventId.Dead);
        public void ChangeAppealState() => stateMachine.SendEvent((int)StateEventId.Appeal);
        public bool IsAttackState() => stateMachine.CurrentStateName == "AttackState";
        public bool IsGuardState() => stateMachine.CurrentStateName == "GuardState";
        public bool IsDamageState() => stateMachine.CurrentStateName == "DamageState";
    }
}

