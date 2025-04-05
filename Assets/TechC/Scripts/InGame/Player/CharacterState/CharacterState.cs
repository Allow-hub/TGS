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
            WeakAttack,
            StrongAttack,
        }

        [Header("Reference")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Animator anim;

        [Header("攻撃設定")]
        [SerializeField] private AttackManager attackManager;
       
        private Vector3 velocity = Vector3.zero;                    // 現在の速度
        private bool isGrounded;                                    // 地面判定

        private ImtStateMachine<CharacterState> stateMachine;
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

            isGrounded =playerController.IsGrounded();

            //Hpが0になったとき
            if (playerController.GetHp() <= 0)
                stateMachine.SendEvent((int)StateEventId.Dead);
        }
        public void MoveCharacter(
                Vector2 moveInput,
                bool isGrounded,
                float groundAccel,
                float airAccel,
                float maxSpd,
                float groundFrict,
                float airFrict,
                float turnSpd
            )
        {
            float acceleration = isGrounded ? groundAccel : airAccel;
            float friction = isGrounded ? groundFrict : airFrict;

            // 移動方向
            Vector3 moveDir = new Vector3(moveInput.x, 0, 0);

            if (moveDir != Vector3.zero)
            {
                // 方向転換時は素早く減速
                if (Vector3.Dot(velocity, moveDir) < 0)
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, 0, turnSpd * Time.deltaTime);
                }

                // 加速処理
                velocity += moveDir * acceleration * Time.deltaTime;

                // 最大速度制限
                velocity.x = Mathf.Clamp(velocity.x, -maxSpd, maxSpd);
            }
            else
            {
                // 慣性で滑る
                velocity.x = Mathf.MoveTowards(velocity.x, 0, friction * Time.deltaTime);
            }

            // 実際に移動
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, rb.velocity.z);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                if (isHitting) return;
                stateMachine.SendEvent((int)StateEventId.Damage);
            }
        }
    }
}

