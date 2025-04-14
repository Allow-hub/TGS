using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    public class CharacterController : MonoBehaviour, IDamageable
    {
        [Header("Reference")]
        [SerializeField] private BaseInputManager playerInputManager;
        [SerializeField] private CharacterData playerData;
        [SerializeField] private CharacterState characterState;
        [SerializeField] private Animator anim;
        [SerializeField] private CommandHistory commandHistory;
        [Header("攻撃コンポーネント")]
        [SerializeField] private WeakAttack weakAttack;
        [SerializeField] private StrongAttack strongAttack;
        //[SerializeField] private AttackSet attackSet; 

        [SerializeField] private float defaultAnimSpeed = 1.0f;
        public float DefaultAnimSpeed => defaultAnimSpeed;
        [SerializeField] private float maxGage = 100;
        [SerializeField] private float jumpInputThreshold = 0.7f; // ジャンプ入力のしきい値
        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private bool isDrawingRay;


        private HitData lastHitData;
        private float speedMultiplier=1.0f;//スピードバフを受け取るための変数
        private Rigidbody rb;
        private float currentHp;

        private bool hasDoubleJumped = false;

        private Vector3 velocity = Vector3.zero;                    // 現在の速度
        private void Awake()
        {
            var attackManager = new AttackManager();
            characterState = new CharacterState(playerInputManager, this, attackManager, anim, commandHistory);
            attackManager?.Initialize(weakAttack, strongAttack, playerInputManager, this);

            anim.speed = defaultAnimSpeed;

            //SetupAttackData();
        }
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            currentHp = playerData.Hp;

        }

        private void FixedUpdate()
        {
            characterState.OnUpdate();
            if (IsGrounded() &&
                characterState.StateMachine.CurrentStateName != "DamageState" &&
                characterState.StateMachine.CurrentStateName != "NeutralState" &&
                characterState.StateMachine.CurrentStateName != "AttackState")
            {
                characterState.ChangeNeutralState();
            }
            else if (!IsGrounded() && characterState.StateMachine.CurrentStateName != "DamageState")
            {
                characterState.ChangeAirState();

            }
        }

        public void AddForcePlayer(Vector3 dir, float force, ForceMode forceMode)
            => rb.AddForce(dir * force, forceMode);

        public float GetHp() => currentHp;


        public void TakeDamage(float damage)
        {
            currentHp -= damage;
            if (currentHp > 0) return;
            currentHp = 0;
            Des();
        }
        public void Des()
        {

        }

        /// <summary>
        /// 地上にいるかどうか
        /// </summary>
        /// <returns></returns>
        public bool IsGrounded()
        {
            Vector3 rayOrigin = transform.position + Vector3.up;
            RaycastHit hit;

            return Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, LayerMask.GetMask("Ground"));
        }

        /// <summary>
        /// ステート側で読み込む移動処理
        /// </summary>
        public void MoveCharacter(float controlMultiplier)
        {
            // X軸のみの移動方向を取得（Z軸は無視）
            float horizontalInput = playerInputManager.MoveInput.x;
            Vector3 moveDirection = new Vector3(horizontalInput, 0, 0).normalized;

            // 接地判定に基づいて異なる挙動を適用
            if (IsGrounded())
            {
                // 着地したらジャンプ状態をリセット
                if (hasDoubleJumped)
                {
                    ResetJump();
                }

                // 地上での移動
                GroundMovement(moveDirection, horizontalInput, controlMultiplier);
            }
            else
            {
                // 空中での移動
                AirMovement(moveDirection, horizontalInput);
            }

        }

        private void GroundMovement(Vector3 moveDirection, float horizontalInput, float controlMultiplier)
        {
            // 地上での移動速度
            float groundSpeed = playerData.MoveSpeed* controlMultiplier * speedMultiplier;

            // 入力があれば移動方向に速度を設定
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // X軸方向の向きを設定（キャラクターの向き）
                transform.forward = new Vector3(Mathf.Sign(horizontalInput), 0, 0);

                // 新しい速度を計算（スムーズに変化するためのLerp）
                velocity = Vector3.Lerp(velocity, moveDirection * groundSpeed, playerData.Acceleration * Time.deltaTime);
            }
            else
            {
                // 入力がない場合は減速
                velocity = Vector3.Lerp(velocity, Vector3.zero, playerData.Deceleration * Time.deltaTime);
            }

            // 移動適用（Z軸の速度は常に0）
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, 0);
        }

        private void AirMovement(Vector3 moveDirection, float horizontalInput)
        {
            // 空中での移動速度（地上より制限される）
            float airSpeed = playerData.MoveSpeed *speedMultiplier* playerData.AirControlMultiplier;

            // 空中での水平移動（制限付き）
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // キャラクターの向きを変更
                transform.forward = new Vector3(Mathf.Sign(horizontalInput), 0, 0);

                // 空中でも方向転換可能だが、地上より制限される
                float targetVelocityX = horizontalInput * airSpeed;
                float newVelocityX = Mathf.Lerp(rb.velocity.x, targetVelocityX, playerData.AirAcceleration * Time.deltaTime);

                rb.velocity = new Vector3(newVelocityX, rb.velocity.y, 0);
            }

            // 空中でのファストフォール（急降下）- スマブラの特徴的な動き
            // 垂直入力が十分に下向きの場合のみ発動
            if (playerInputManager.MoveInput.y < -jumpInputThreshold && rb.velocity.y < 0)
            {
                rb.AddForce(Vector3.down * playerData.FastFallSpeed, ForceMode.Acceleration);
            }
        }

        public void Jump()
        {
            if (IsGrounded())
            {
                rb.AddForce(Vector3.up * playerData.JumpForce, ForceMode.Impulse);
            }
        }

        public void DoubleJump()
        {
            // 空中での二段ジャンプ 
            if (CanDoubleJump() && !IsGrounded())
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, 0); // 上方向の速度をリセット
                rb.AddForce(Vector3.up * playerData.DoubleJumpForce, ForceMode.Impulse);
                UseDoubleJump();
            }
        }


        // ジャンプ状態をリセット（着地時に呼び出す）
        private void ResetJump()
        {
            hasDoubleJumped = false;
        }

        // 二段ジャンプが可能かどうか
        private bool CanDoubleJump() => !hasDoubleJumped;

        // 二段ジャンプを使用
        private void UseDoubleJump() => hasDoubleJumped = true;

        public void SetLastHitData(HitData hitData) => lastHitData = hitData;


        public HitData GetLastHitData() => lastHitData;


        public Animator GetAnim() => anim;
        public void SetAnim(int hashName, bool value) => anim.SetBool(hashName, value);
        public CharacterState GetCharacterState() => characterState;


        /// <summary>
        /// スピードバフを適用
        /// </summary>
        /// <param name="multiplier"></param>
        public void AddSpeedMultiplier(float multiplier) => speedMultiplier *= multiplier;

        /// <summary>
        /// スピードバフを除外
        /// </summary>
        /// <param name="multiplier"></param>
        public void RemoveSpeedMultiplier(float multiplier) => speedMultiplier /= multiplier;

        private void OnCollisionEnter(Collision collision)
        {
            //if (collision.gameObject.CompareTag("Player"))
            //{
            //    if (collision.gameObject.TryGetComponent<CharacterController>(out CharacterController controller))
            //    {
            //    }
            //}

            // 地面に着地した時の処理
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                ResetJump();
            }
        }

        private void OnDrawGizmos()
        {
            if (!isDrawingRay) return;
            // レイの発射位置
            Vector3 rayOrigin = transform.position + Vector3.up;

            // 地面に当たるときは緑、そうでないときは赤
            Gizmos.color = IsGrounded() ? Color.green : Color.red;

            // レイの描画
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayLength);

            // レイの終端に球を表示
            Gizmos.DrawSphere(rayOrigin + Vector3.down * rayLength, 0.05f);
        }
    }
}