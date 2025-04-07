using IceMilkTea.StateMachine;
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
    public partial class CharacterState : MonoBehaviour
    {
        public enum StateEventId
        {
            Start,
            Idle,
            Damage,
            Crouch,
            Move,
            Jump,
            Guard,
            Dead,
            Appeal,
            Attack,
        }

        [Header("Reference")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private CharacterData playerData;
        [SerializeField] private Player.CharacterController playerController;
        [SerializeField] private Animator anim;

        [Header("攻撃設定")]
        [SerializeField] private AttackManager attackManager;

        private bool isGrounded;                                    // 地面判定

        public ImtStateMachine<CharacterState> StateMachine => stateMachine;

        private ImtStateMachine<CharacterState> stateMachine;

        public bool IsHitting => isHitting;
        private bool isHitting = false;

        Rigidbody rb;


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            stateMachine = new ImtStateMachine<CharacterState>(this);

            //Idleへの遷移
            stateMachine.AddTransition<StartState, IdleState>((int)StateEventId.Start);
            stateMachine.AddTransition<MoveState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<DamageState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<JumpState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<GuardState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<AppealState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<CrouchState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<AttackState, IdleState>((int)StateEventId.Idle);

            //Idleからの遷移
            stateMachine.AddTransition<IdleState, MoveState>((int)StateEventId.Move);
            stateMachine.AddTransition<IdleState, GuardState>((int)StateEventId.Guard);
            stateMachine.AddTransition<IdleState, AppealState>((int)StateEventId.Appeal);
            stateMachine.AddTransition<IdleState, CrouchState>((int)StateEventId.Crouch);
            stateMachine.AddTransition<IdleState, JumpState>((int)StateEventId.Jump);
            stateMachine.AddTransition<IdleState, AttackState>((int)StateEventId.Attack);

            stateMachine.AddTransition<MoveState, AttackState>((int)StateEventId.Attack);
            stateMachine.AddTransition<CrouchState, AttackState>((int)StateEventId.Attack);


            //ダメージステートはどのステートからでも移行できる
            stateMachine.AddAnyTransition<DamageState>((int)StateEventId.Damage);
            stateMachine.AddAnyTransition<DeadState>((int)StateEventId.Dead);

            stateMachine.SetStartState<StartState>();
        }

        private void Update()
        {
            stateMachine.Update();
            Debug.Log(stateMachine.CurrentStateName);

            isGrounded = playerController.IsGrounded();

            //Hpが0になったとき
            if (playerController.GetHp() <= 0)
                stateMachine.SendEvent((int)StateEventId.Dead);
        }

    }
}

