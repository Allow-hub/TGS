using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState : MonoBehaviour
    {
        // ステートマシンの入力（イベント）を判り易くするために列挙型で定義
        public enum StateEventId
        {
            Start,  //開始前の待機モーション
            Idle,   //何もしていないとき
            Damage,　//被ダメージ時
            Move,
            Jump,
            Gard,
            Dead,
            Appeal,
            WeakAttack,
            StrongAttack
        }
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Animator anim;
        [SerializeField] private float rayLength = 0.1f;

        private ImtStateMachine<CharacterState> stateMachine;
        Rigidbody rb;
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            // ステートマシンのインスタンスを生成して遷移テーブルを構築
            stateMachine = new ImtStateMachine<CharacterState>(this); // 自身がコンテキストになるので自身のインスタンスを渡す
            stateMachine.AddTransition<StartState, IdleState>((int)StateEventId.Start);
            stateMachine.AddTransition<MoveState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<DamageState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<JumpState, IdleState>((int)StateEventId.Idle);
            stateMachine.AddTransition<GardState, IdleState>((int)StateEventId.Idle);

            stateMachine.AddTransition<IdleState, MoveState>((int)StateEventId.Move);

            // 起動ステートを設定（起動ステートは StartState）
            stateMachine.SetStartState<StartState>();

        }

        private void Start()
        {
            stateMachine.Update();
        }
        private void Update()
        {
            stateMachine.Update();
            Debug.Log(stateMachine.CurrentStateName);
            Debug.Log(IsGrounded());
        }

        private void MoveCharacter(float moveSpeed)
        {
            Vector2 moveInput = playerInputManager.MoveInput;
            Vector3 moveVelocity = new Vector3(moveInput.x, 0, 0) * moveSpeed;

            if (rb != null)
            {
                rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, rb.velocity.z);
            }
        }

        private bool IsGrounded()
        {
            Vector3 rayOrigin = transform.position + Vector3.up;
            RaycastHit hit;

            // 下方向にレイを飛ばし、"Ground" レイヤーと接触しているか判定
            return Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, LayerMask.GetMask("Ground"));
        }
    }
}
