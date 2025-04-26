using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    /// <summary>
    ///キャラクターを管理をするクラス
    ///regionを使うとタブ化して見やすくできる
    /// </summary>
    public class CharacterController : MonoBehaviour, IDamageable, IGuardable
    {
        #region シリアライズされたフィールド
        [Header("基本リファレンス")]
        [SerializeField] private BaseInputManager playerInputManager;
        [SerializeField] private CharacterData characterData;
        [SerializeField] private CharacterState characterState;
        [SerializeField] private Animator anim;
        [SerializeField] private CommandHistory commandHistory;

        [Header("攻撃コンポーネント")]
        [SerializeField] private WeakAttack weakAttack;
        [SerializeField] private StrongAttack strongAttack;
        [SerializeField] private AppealBase appealBase;

        [Header("ガード設定")]
        [SerializeField] private float defaultAnimSpeed = 1.0f;

        [Header("必殺技設定")]
        [SerializeField] private GaugePresenter gaugePresenter;
        [SerializeField] private float maxGauge = 100f;

        [Header("移動・ジャンプ設定")]
        [SerializeField] private float jumpInputThreshold = 0.7f; // ジャンプ入力のしきい値
        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private bool isDrawingRay;
        #endregion

        #region プライベート変数
        // ガード関連
        private float currentGuardPower;
        private float lastGuardTime;


        // 移動・物理関連
        private Rigidbody rb;
        private Vector3 velocity = Vector3.zero; // 現在の速度
        private Dictionary<BuffType, float> multipliers = new()
        {
            { BuffType.Speed, 1.0f },
            { BuffType.Attack, 1.0f }
        };
        // ジャンプ関連
        private bool hasDoubleJumped = false;

        // 戦闘関連
        private float currentHp;
        private HitData lastHitData;
        #endregion

        #region プロパティ
        public float DefaultAnimSpeed => defaultAnimSpeed;
        #endregion

        #region 初期化メソッド
        private void Awake()
        {
            // アタックマネージャーの初期化
            var attackManager = new AttackManager();
            characterState = new CharacterState(playerInputManager, this, attackManager, anim, commandHistory);
            attackManager?.Initialize(weakAttack, strongAttack, appealBase, playerInputManager, this);

            // アニメーション設定
            anim.speed = defaultAnimSpeed;

            // パラメータ初期化
            currentHp = characterData.Hp;
            currentGuardPower = characterData.GuardPower;

        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            currentHp = characterData.Hp;
        }
        #endregion

        #region 更新メソッド
        private void FixedUpdate()
        {
            // プレイヤーのステート管理
            characterState.OnUpdate();

            // ステート遷移制御
            UpdateStateTransitions();

            // ガード値回復処理
            if (CanHeal())
                HealGuardPower(characterData.GuardRecoverySpeed);
            //Debug.Log(IsChargeEnabled());
            // 時間経過によるゲージ加算処理
            //characterGauge.AddGaugeOnTime(characterData.GaugeIncreaseInterval, characterData.GaugeIncreaseAmount);
            //Debug.Log(characterGauge.CurrentGauge);
        }

        /// <summary>
        /// ステート遷移の条件をチェックして適切なステートに変更する
        /// </summary>
        private void UpdateStateTransitions()
        {
            // 通常ステートへの遷移条件
            // 1.地上にいる場合
            // 2.ダメージステートでない場合
            // 3.通常ステートでない場合
            // 4.アタックステートでない場合
            // 5.ガードステートでない場合
            if (IsGrounded() &&
                characterState.StateMachine.CurrentStateName != "DamageState" &&
                characterState.StateMachine.CurrentStateName != "NeutralState" &&
                characterState.StateMachine.CurrentStateName != "AttackState" &&
                characterState.StateMachine.CurrentStateName != "GuardState")
            {
                characterState.ChangeNeutralState();
            }
            else if (!IsGrounded() && characterState.StateMachine.CurrentStateName != "DamageState")
            {
                characterState.ChangeAirState();
            }
        }
        #endregion

        #region 物理・移動関連メソッド
        /// <summary>
        /// キャラクターに力を加える
        /// </summary>
        public void AddForcePlayer(Vector3 dir, float force, ForceMode forceMode)
            => rb.AddForce(dir * force, forceMode);

        /// <summary>
        /// 地上にいるかどうかを判定する
        /// </summary>
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

        /// <summary>
        /// 地上での移動処理
        /// </summary>
        private void GroundMovement(Vector3 moveDirection, float horizontalInput, float controlMultiplier)
        {
            // 地上での移動速度
            float groundSpeed = characterData.MoveSpeed * controlMultiplier * GetMultipiler(BuffType.Speed);

            // 入力があれば移動方向に速度を設定
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // X軸方向の向きを設定（キャラクターの向き）
                transform.forward = new Vector3(Mathf.Sign(horizontalInput), 0, 0);

                // 新しい速度を計算（スムーズに変化するためのLerp）
                velocity = Vector3.Lerp(velocity, moveDirection * groundSpeed, characterData.Acceleration * Time.deltaTime);
            }
            else
            {
                // 入力がない場合は減速
                velocity = Vector3.Lerp(velocity, Vector3.zero, characterData.Deceleration * Time.deltaTime);
            }

            // 移動適用（Z軸の速度は常に0）
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, 0);
        }

        /// <summary>
        /// 空中での移動処理
        /// </summary>
        private void AirMovement(Vector3 moveDirection, float horizontalInput)
        {
            // 空中での移動速度（地上より制限される）
            float airSpeed = characterData.MoveSpeed * GetMultipiler(BuffType.Speed) * characterData.AirControlMultiplier;

            // 空中での水平移動（制限付き）
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // キャラクターの向きを変更
                transform.forward = new Vector3(Mathf.Sign(horizontalInput), 0, 0);

                // 空中でも方向転換可能だが、地上より制限される
                float targetVelocityX = horizontalInput * airSpeed;
                float newVelocityX = Mathf.Lerp(rb.velocity.x, targetVelocityX, characterData.AirAcceleration * Time.deltaTime);

                rb.velocity = new Vector3(newVelocityX, rb.velocity.y, 0);
            }

            // 空中でのファストフォール（急降下）- スマブラの特徴的な動き
            // 垂直入力が十分に下向きの場合のみ発動
            if (playerInputManager.MoveInput.y < -jumpInputThreshold && rb.velocity.y < 0)
            {
                rb.AddForce(Vector3.down * characterData.FastFallSpeed, ForceMode.Acceleration);
            }
        }
        #endregion

        #region ジャンプ関連メソッド
        /// <summary>
        /// ジャンプ処理
        /// </summary>
        public void Jump()
        {
            if (IsGrounded())
            {
                rb.AddForce(Vector3.up * characterData.JumpForce, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// 二段ジャンプ処理
        /// </summary>
        public void DoubleJump()
        {
            // 空中での二段ジャンプ 
            if (CanDoubleJump() && !IsGrounded())
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, 0); // 上方向の速度をリセット
                rb.AddForce(Vector3.up * characterData.DoubleJumpForce, ForceMode.Impulse);
                UseDoubleJump();
            }
        }

        /// <summary>
        /// ジャンプ状態をリセット（着地時に呼び出す）
        /// </summary>
        private void ResetJump()
        {
            hasDoubleJumped = false;
        }

        /// <summary>
        /// 二段ジャンプが可能かどうか
        /// </summary>
        private bool CanDoubleJump() => !hasDoubleJumped;

        /// <summary>
        /// 二段ジャンプを使用済みにする
        /// </summary>
        private void UseDoubleJump() => hasDoubleJumped = true;
        #endregion

        #region ダメージ・HP関連メソッド
        /// <summary>
        /// HP値を取得する
        /// </summary>
        public float GetHp() => currentHp;

        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        public void TakeDamage(float damage)
        {
            currentHp -= damage * GetMultipiler(BuffType.Attack);
            if (currentHp > 0) return;
            currentHp = 0;
            Des();
        }

        /// <summary>
        /// キャラクター死亡時の処理
        /// </summary>
        public void Des()
        {
            // 死亡時の処理を実装
        }

        /// <summary>
        /// 最後に受けた攻撃データを設定
        /// </summary>
        public void SetLastHitData(HitData hitData) => lastHitData = hitData;

        /// <summary>
        /// 最後に受けた攻撃データを取得
        /// </summary>
        public HitData GetLastHitData() => lastHitData;
        #endregion

        #region ガード関連メソッド
        /// <summary>
        /// ガードパワーを取得
        /// </summary>
        public float GetGuardPower() => currentGuardPower;

        /// <summary>
        /// ガード中に耐久値減少
        /// </summary>
        public void DecreaseGuardPower() => currentGuardPower -= characterData.GuardDecreasePower;

        /// <summary>
        /// 最後にガードした時間を設定
        /// </summary>
        public void SetLastGuardTime(float time) => lastGuardTime = time;

        /// <summary>
        /// ガード時のダメージ処理
        /// </summary>
        public void GuardDamage(float damage, ICommand guardCommand)
        {
            currentGuardPower -= damage;
            Debug.Log(currentGuardPower);
            if (currentGuardPower > 0) return;
            currentGuardPower = 0;
            GuardBreak(guardCommand);
        }

        /// <summary>
        /// ガードブレイク処理
        /// </summary>
        public void GuardBreak(ICommand guardCommand)
        {
            guardCommand.ForceFinish();
            currentGuardPower = 0; // ガードがマイナスで保存されないように
            Debug.Log("Guardが破壊されました");
        }

        /// <summary>
        /// ガードパワー回復処理
        /// </summary>
        public void HealGuardPower(float value)
        {
            if (currentGuardPower < characterData.GuardPower)
                currentGuardPower += value;
            else
                currentGuardPower = characterData.GuardPower;
            //Debug.Log($"{currentGuardPower}");
        }

        /// <summary>
        /// ガード回復が可能な状態かを判定
        /// </summary>
        public bool CanHeal()
        {
            var lastGuard = Time.time - lastGuardTime;
            return lastGuard >= characterData.GuardRecoveryInterval;
        }
        #endregion

        #region 必殺技関連メソッド
        /// <summary>
        /// 必殺技ゲージを増加させる
        /// </summary>
        public void AddSpecialGauge(float amount)
        {
            gaugePresenter.AddGauge(amount);
        }
        /// <summary>
        /// 必殺技ゲージを増加させる、bool値を問わず
        /// </summary>
        public void NotBoolAddSpecialGauge(float amount)
        {
            gaugePresenter.NotBoolAddGauge(amount);
        }
        /// <summary>
        /// 必殺技を使用する（使用可能な場合のみ成功）
        /// </summary>
        public bool TryUseSpecialAttack(float cost)
        {
            return gaugePresenter.TryUseSpecialAttack(cost);
        }

        /// <summary>
        /// 必殺技ゲージの割合を取得（UI表示用など）
        /// </summary>
        public float GetSpecialGaugePercentage() => gaugePresenter.GetGaugePercentage();

        /// <summary>
        /// 必殺技が使用可能かどうか
        /// </summary>
        public bool IsSpecialAttackReady(float cost) => gaugePresenter.IsSpecialAttackReady(cost);

        /// <summary>
        /// 必殺技がチャージ可能かどうかを切り替える
        /// </summary>
        public void ChangeCanCharge(bool value) => gaugePresenter.SetCanCharge(value);

        /// <summary>
        /// チャージ可能状態かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsChargeEnabled() => gaugePresenter.GetCanCharge();

        #endregion

        #region バフ関連メソッド
    
        /// <summary>
        /// バフの適用（バフの種類,乗算の数値）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddMultiplier(BuffType type, float value)
        {
            if (multipliers.ContainsKey(type))
                multipliers[type] *= value;
        }
        /// <summary>
        /// バフの適用（バフの種類,除算の数値）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void RemoveMultiplier(BuffType type, float value)
        {
            if (multipliers.ContainsKey(type))
                multipliers[type] /= value;
        }

        /// <summary>
        /// multipliers に type が 存在するなら、その値（value）を返す
        /// 存在しないなら、デフォルト値の 1.0f を返す
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public float GetMultipiler(BuffType type) =>
            multipliers.TryGetValue(type, out var value) ? value : 1.0f;
        #endregion

        #region アニメーション関連メソッド
        /// <summary>
        /// アニメーターを取得
        /// </summary>
        public Animator GetAnim() => anim;

        /// <summary>
        /// アニメーションのブールパラメータを設定
        /// </summary>
        public void SetAnim(int hashName, bool value) => anim.SetBool(hashName, value);
        #endregion

        #region ゲッターメソッド
        /// <summary>
        /// キャラクターステートを取得
        /// </summary>
        public CharacterState GetCharacterState() => characterState;

        /// <summary>
        /// キャラクターデータを取得
        /// </summary>
        public CharacterData GetCharacterData() => characterData;
        #endregion

        #region Unity内部コールバック
        private void OnCollisionEnter(Collision collision)
        {
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
        #endregion
    }
}