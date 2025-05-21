using Cysharp.Threading.Tasks;
using System;
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
        [SerializeField] private ObjectPool effectPool;
        [SerializeField] private CharacterType characterType;
        [Header("攻撃コンポーネント")]
        [SerializeField] private WeakAttack weakAttack;
        [SerializeField] private StrongAttack strongAttack;
        [SerializeField] private AppealBase appealBase;
        [Header("反発設定")]
        [SerializeField] private float bounceStopTime = 0.5f;
        [SerializeField] private float maxBounceForce = 30f;

        [SerializeField] private float addY = 1;
        [SerializeField] private float wallBounceMultiplier = 1.5f; // 壁からの反発倍率
        [SerializeField] private bool enableWallBounce = true; // 壁反発機能の有効/無効
        [Header("プレイヤー設定")]
        [SerializeField] private int playerID = 1; // 1Pか2Pかを識別するID
        [SerializeField] private CapsuleCollider hitCollider;


        [Header("HP設定")]
        [SerializeField] private HPPresenter hpPresenter;
        [Header("ガード設定")]
        [SerializeField] private float defaultAnimSpeed = 1.0f;

        [Header("必殺技設定")]
        [SerializeField] private GaugePresenter gaugePresenter;
        [SerializeField] private float maxGauge = 100f;

        [Header("移動・ジャンプ設定")]
        [SerializeField] private float jumpInputThreshold = 0.7f; // ジャンプ入力のしきい値
        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private bool isDrawingRay;

        [Header("エフェクトのPrefab")]
        [SerializeField] private GameObject debrisPrefab;
        #endregion

        #region プライベート変数
        // ガード関連
        private float currentGuardPower;
        private float lastGuardTime;
        private Vector3 lastVelocity;

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
        private HitData lastHitData;
        private Coroutine sizeChangeRoutine;
        private Vector3 defaultSize;     // x=radius, y=height
        private Vector3 defaultCenter;   // centerを戻すために保存


        #endregion

        #region プロパティ
        public float DefaultAnimSpeed => defaultAnimSpeed;
        public CharacterType CharacterType => characterType;
        public int PlayerID => playerID; // PlayerIDのゲッター
        #endregion

        #region 初期化メソッド
        private void Awake()
        {
            effectPool = GameObject.FindGameObjectWithTag("EffectPool").GetComponent<ObjectPool>();
            // アタックマネージャーの初期化
            var attackManager = new AttackManager();
            characterState = new CharacterState(playerInputManager, this, attackManager, anim, commandHistory);
            attackManager?.Initialize(weakAttack, strongAttack, appealBase, playerInputManager, this);

            // アニメーション設定
            anim.speed = defaultAnimSpeed;

            // パラメータ初期化
            currentGuardPower = characterData.GuardPower;
            defaultSize = new Vector3(hitCollider.radius, hitCollider.height, 0f);
            defaultCenter = hitCollider.center;

        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();

            // HPPresenterがnullでないか確認してから購読
            if (hpPresenter != null)
            {
                hpPresenter.OnDeath += Des;
            }
            else
            {
                Debug.LogError($"Player {playerID}: HPPresenterが見つかりません。");
            }
        }

        /// <summary>
        /// プレイヤーIDに基づいて適切なHPPresenterとGaugePresenterを検索して取得
        /// </summary>
        private void FindPresenters()
        {
            string presenterTag = $"Presenter_P{playerID}";

            // タグで検索
            GameObject preObj = GameObject.FindWithTag(presenterTag);
            gaugePresenter = preObj.GetComponent<GaugePresenter>();

            hpPresenter = preObj.GetComponent<HPPresenter>();

            // デバッグログ
            // Debug.Log($"Player {playerID}: HPPresenter {(hpPresenter != null ? "見つかりました" : "見つかりませんでした")}");
            // Debug.Log($"Player {playerID}: GaugePresenter {(gaugePresenter != null ? "見つかりました" : "見つかりませんでした")}");
        }

        /// <summary>
        /// プレイヤーIDを設定する（生成時に呼び出す）
        /// </summary>
        public void SetPlayerID(int id)
        {
            playerID = id;
            if (id == 1)
                gameObject.transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            else if (id == 2)
                gameObject.transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
            // IDが変更された場合は、対応するPresenterを再取得
            FindPresenters();
        }

        #endregion

        #region 更新メソッド
        private void FixedUpdate()
        {
            // プレイヤーのステート管理
            characterState.OnUpdate();

            // ステート遷移制御
            UpdateStateTransitions();

            lastVelocity = rb.velocity;
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

        public void ChangeColliderTrigger(bool b) => hitCollider.isTrigger = b;

        /// <summary>
        /// 当たり判定を変化させる
        /// </summary>
        /// <param name="newSize">x=radius, y=height, z=未使用 or 将来拡張</param>
        /// <param name="transitionSpeed">補間速度（1以上で推奨）</param>
        public void ChangeHitCollider(Vector3 newSize, float transitionSpeed, Vector3? newCenter = null)
        {
            if (sizeChangeRoutine != null)
                StopCoroutine(sizeChangeRoutine);

            Vector3 targetCenter = newCenter ?? new Vector3(0, newSize.y / 2f, 0);
            sizeChangeRoutine = StartCoroutine(ResizeColliderRoutine(newSize, transitionSpeed, targetCenter));
        }
        public void ResetHitCollider(float transitionSpeed = 5f)
        {
            ChangeHitCollider(defaultSize, transitionSpeed, defaultCenter);
        }


        private IEnumerator ResizeColliderRoutine(Vector3 targetSize, float speed, Vector3 targetCenter)
        {
            float t = 0f;

            float startRadius = hitCollider.radius;
            float startHeight = hitCollider.height;
            Vector3 startCenter = hitCollider.center;

            float targetRadius = targetSize.x;
            float targetHeight = targetSize.y;

            while (t < 1f)
            {
                t += Time.deltaTime * speed;

                hitCollider.radius = Mathf.Lerp(startRadius, targetRadius, t);
                hitCollider.height = Mathf.Lerp(startHeight, targetHeight, t);
                hitCollider.center = Vector3.Lerp(startCenter, targetCenter, t);

                yield return null;
            }

            // 最終補正
            hitCollider.radius = targetRadius;
            hitCollider.height = targetHeight;
            hitCollider.center = targetCenter;
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

        //IDamageable用メソッド
        public void TakeDamage(float damage)
        {
            PresenterTakeDamage(damage);
        }
        public void Des()
        {
            HandleDeath();
        }

        /// <summary>
        /// HP値を取得する
        /// </summary>
        public float GetHp()
        {
            // hpPresenterがnullでないことを確認
            if (hpPresenter != null)
            {
                return hpPresenter.GetCurrentValue();
            }
            Debug.LogWarning($"Player {playerID}: HPPresenterが見つかりません。デフォルト値を返します。");
            return 0f;
        }


        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        public void PresenterTakeDamage(float damage)
        {
            // hpPresenterがnullでないことを確認
            if (hpPresenter != null)
            {
                hpPresenter.TakeDamage(damage * GetMultipiler(BuffType.Attack));
            }
            else
            {
                Debug.LogError($"Player {playerID}: HPPresenterがnullのため、ダメージ処理ができません");
            }
        }

        /// <summary>
        /// キャラクター死亡時の処理
        /// </summary>
        private void HandleDeath()
        {
            // 死亡時の処理を実装
            Debug.Log($"Player {playerID}のキャラクターが死亡しました");
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
            if (gaugePresenter != null)
            {
                gaugePresenter.AddGauge(amount);
            }
            else
            {
                Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、ゲージ追加ができません");
            }
        }
        /// <summary>
        /// 必殺技ゲージを増加させる、bool値を問わず
        /// </summary>
        public void NotBoolAddSpecialGauge(float amount)
        {
            if (gaugePresenter != null)
            {
                gaugePresenter.NotBoolAddGauge(amount);
            }
            else
            {
                Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、ゲージ追加ができません");
            }
        }
        /// <summary>
        /// 必殺技を使用する（使用可能な場合のみ成功）
        /// </summary>
        public bool TryUseSpecialAttack(float cost)
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.TryUseSpecialAttack(cost);
            }
            Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、必殺技使用ができません");
            return false;
        }

        public void ResetSpecial()=>gaugePresenter.ResetGauge();

        /// <summary>
        /// 必殺技ゲージの割合を取得（UI表示用など）
        /// </summary>
        public float GetSpecialGaugePercentage()
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.GetGaugePercentage();
            }
            Debug.LogWarning($"Player {playerID}: GaugePresenterがnullのため、ゲージ割合が取得できません");
            return 0f;
        }

        /// <summary>
        /// 必殺技が使用可能かどうか
        /// </summary>
        public bool CanSpecialAttack()
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.CanSpecialAttack();
            }
            Debug.LogWarning($"Player {playerID}: GaugePresenterがnullのため、必殺技準備状態が確認できません");
            return false;
        }

        /// <summary>
        /// 必殺技がチャージ可能かどうかを切り替える
        /// </summary>
        public void ChangeCanCharge(bool value)
        {
            if (gaugePresenter != null)
            {
                gaugePresenter.SetCanCharge(value);
            }
            else
            {
                Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、チャージ状態を変更できません");
            }
        }

        /// <summary>
        /// チャージ可能状態かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsChargeEnabled()
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.GetCanCharge();
            }
            Debug.LogWarning($"Player {playerID}: GaugePresenterがnullのため、チャージ状態が確認できません");
            return false;
        }

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

        public Collider GetCollider() => hitCollider;
        #endregion

        #region Unity内部コールバック
        private void OnCollisionEnter(Collision collision)
        {
            // 地面に着地した時の処理
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                ResetJump();
            }

            // 壁に衝突しかつダメージステート中なら反発する
            if (collision.gameObject.CompareTag("Wall") && enableWallBounce)
            {
                if (collision.contacts.Length > 0)
                {
                    Vector3 contactPoint = collision.contacts[0].point;
                    ApplyWallBounce(collision, contactPoint).Forget();

                }
            }
        }


        /// <summary>
        /// 壁に衝突した時の反発処理
        /// </summary>
        /// <param name="collision">衝突情報</param>
        private async UniTask ApplyWallBounce(Collision collision, Vector3 hitPos)
        {

            // 入射ベクトルを取得（ぶつかった直前の速度）
            Vector3 inDirection = lastVelocity;

            // 衝突面の法線ベクトル（接触点から取得）
            Vector3 wallNormal = collision.contacts[0].normal;

            // 反射ベクトルを物理法則に基づいて計算
            Vector3 reflected = Vector3.Reflect(inDirection.normalized, wallNormal);

            // 跳ね返り力（速度ベースで自然な力に）
            float bounceForce = inDirection.magnitude * wallBounceMultiplier;
            bounceForce = Mathf.Clamp(bounceForce, 0f, maxBounceForce); // ← クランプ

            // 破片
            var debris = effectPool.GetObject(debrisPrefab, hitPos, Quaternion.identity);
            debris.GetComponent<ExplosionDebris>()?.Explode();

            await UniTask.Delay(TimeSpan.FromSeconds(bounceStopTime));

            // 速度をゼロにしてから反発力を加える
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(reflected * bounceForce, ForceMode.Impulse);

            // 破片のリターン
            await UniTask.Delay(TimeSpan.FromSeconds(3f));
            debris.GetComponent<ExplosionDebris>()?.ResetExplosion();
            effectPool.ReturnObject(debris);

            // Debug: 反射方向の確認
            // Debug.DrawRay(hitPos, wallNormal, Color.red, 2f);      // 法線
            // Debug.DrawRay(hitPos, inDirection.normalized, Color.blue, 2f); // 入射
            // Debug.DrawRay(hitPos, reflected, Color.green, 2f);     // 反射
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