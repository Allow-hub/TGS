using IceMilkTea.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

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

        [SerializeField] private bool canDebugLog;

        private Animator anim;

        private Queue<ICommand> commandQueue = new Queue<ICommand>();
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

            //通常ステートからの移行
            stateMachine.AddTransition<NeutralState, AirState>((int)StateEventId.Air);
            stateMachine.AddTransition<NeutralState, GuardState>((int)StateEventId.Guard);
            stateMachine.AddTransition<NeutralState, AttackState>((int)StateEventId.Attack);
            stateMachine.AddTransition<NeutralState, DamageState>((int)StateEventId.Damage);

            //空中ステートからの移行
            stateMachine.AddTransition<AirState, DamageState>((int)StateEventId.Damage);
            stateMachine.AddTransition<AirState, AttackState>((int)StateEventId.Attack);


            //stateMachine.AddAnyTransition<DamageState>((int)StateEventId.Damage);
            //どのステートからでも移行できる
            stateMachine.AddAnyTransition<DeadState>((int)StateEventId.Dead);

            //開始時のステート設定
            stateMachine.SetStartState<StartState>();
        }

        public void OnUpdate()
        {
            stateMachine.Update();
            if (canDebugLog)
                Debug.Log(stateMachine.CurrentStateName);

            // MoveCommand だけ残さないようにフィルタリング
            var newQueue = new Queue<ICommand>();
            foreach (var cmd in commandQueue)
            {
                if (!(cmd is MoveCommand) && !(cmd is JumpCommand))
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
        public void ChangeNeutralState() => stateMachine.SendEvent((int)StateEventId.Neutral);
        public void ChangeAirState() => stateMachine.SendEvent((int)StateEventId.Air);
        public void ChangeGuardrState() => stateMachine.SendEvent((int)StateEventId.Guard);
        public void ChangeAttackState() => stateMachine.SendEvent((int)StateEventId.Attack);

        public void ChangeDamageState() => stateMachine.SendEvent((int)StateEventId.Damage);
        public void ChangeDeadState() => stateMachine.SendEvent((int)StateEventId.Dead);

        public bool IsAttackState() => stateMachine.CurrentStateName == "AttackState";
    }
}

