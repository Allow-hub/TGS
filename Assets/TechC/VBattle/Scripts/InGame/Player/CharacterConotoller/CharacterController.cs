using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TechC.UIs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.Player
{
    /// <summary>
    ///キャラクターを管理をするクラス
    /// partialによる分離済み
    ///regionを使うとタブ化して見やすくできる
    /// </summary>
    public partial class CharacterController : MonoBehaviour, IDamageable, IGuardable
    {
        #region シリアライズされたフィールド
        [Header("基本リファレンス")]
        [SerializeField] private BaseInputManager playerInputManager;
        [SerializeField] private CharacterData characterData;
        [SerializeField] private CharacterState characterState;
        [SerializeField] private Animator anim;
        [SerializeField] private CommandHistory commandHistory;
        [SerializeField] private CharacterType characterType;
        [Header("攻撃コンポーネント")]
        [SerializeField] private WeakAttack weakAttack;
        [SerializeField] private StrongAttack strongAttack;
        [SerializeField] private AppealBase appealBase;
        [SerializeField] private AttackSet attackSet;
        public AttackSet AttackSet => attackSet;
        [Header("反発設定")]
        [SerializeField] private float bounceStopTime = 0.5f;
        [SerializeField] private float maxBounceForce = 30f;

        [SerializeField] private float wallBounceMultiplier = 1.5f; // 壁からの反発倍率
        [SerializeField] private bool enableWallBounce = true; // 壁反発機能の有効/無効
        [Header("プレイヤー設定")]
        [SerializeField] private int playerID = 1; // 1Pか2Pかを識別するID
        [SerializeField] private CapsuleCollider hitCollider;
        [SerializeField] private SkinnedMeshRenderer[] renderers;
        [SerializeField] private Material outlineMat;
        [SerializeField] private Color outlineColor1, outlineColor2;

        [Header("HP設定")]
        [SerializeField] private HPPresenter hpPresenter;
        [Header("ガード設定")]
        [SerializeField] private float defaultAnimSpeed = 1.0f;
        [SerializeField] private float lowFrequency;
        [SerializeField] private float highFrequency;
        [SerializeField] private float duration;

        [Header("必殺技設定")]
        [SerializeField] private GaugePresenter gaugePresenter;

        [Header("移動・ジャンプ設定")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float jumpInputThreshold = 0.7f; // ジャンプ入力のしきい値
        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private bool isDrawingRay;
        [Header("ジャンプ調整")]
        public float gravityScale = 1f;          // 通常上昇重力
        public float fallGravityScale = 2.5f;    // 通常落下重力
        public float shortHopGravityScale = 3.5f; // 可変ジャンプ用
        public float fastFallGravityScale = 5f;   // 急降下用
        public float fastFallInputThreshold = -0.5f;

        // 移動・回転関連の定数
        private const float STOP_THRESHOLD = 0.1f;
        private const float RIGHT_FACING_ANGLE = 90f; // 右向きの目標角度
        private const float LEFT_FACING_ANGLE = -90f; // 左向きの目標角度
        private const float BUFF_DEFAULT_MULTIPLIER = 1.0f; // バフの初期倍率


        private bool canCounter = false;
        public bool CanCounter => canCounter;

        [Header("エフェクトのPrefab")]
        [SerializeField] private GameObject debrisPrefab;

        [Header("文字のPrefab")]
        [SerializeField] private GameObject grass;

        [Header("コメント")]
        [SerializeField] private Transform handPos;
        public bool hasComment;
        #endregion

        #region プライベート変数
        // ガード関連
        private float currentGuardPower;
        private float lastGuardTime;
        private Vector3 lastVelocity;
        private bool isWallHitting = false;

        // 移動・物理関連
        private Rigidbody rb;
        private Dictionary<BuffType, float> multipliers = new()
        {
            { BuffType.Speed, BUFF_DEFAULT_MULTIPLIER },
            { BuffType.Attack, BUFF_DEFAULT_MULTIPLIER }
        };
        private Dictionary<BuffType, Dictionary<int, float>> multiplierEntries = new(); // 各バフに対して複数の倍率を保持する

        // ジャンプ関連
        private bool hasDoubleJumped = false;

        // 戦闘関連
        private HitData lastHitData;
        private Coroutine sizeChangeRoutine;
        private Vector3 defaultSize;     // x=radius, y=height
        private Vector3 defaultCenter;   // centerを戻すために保存
        private bool lastHitCanWallBounce;
        private float lastHitBounceForce;
        private float lastHitBounceUpForce;
        private Player.CharacterController opponentController;

        private Action onCounter;
        private InputDevice inputDevice;
        [SerializeField] private bool isClonePlayer = false;
        #endregion

        #region プロパティ
        public Rigidbody Rb => rb;
        public float DefaultAnimSpeed => defaultAnimSpeed;
        public CharacterType CharacterType => characterType;
        public CharacterData CharacterData => characterData;
        public Player.CharacterController OpponentController => opponentController;
        public int PlayerID => playerID; // PlayerIDのゲッター
        public Action OnCommentEvent;
        public bool IsWallHitting=> isWallHitting;
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

        #region コメント関連メソッド

        /// <summary>
        /// 草のモデルをプレイヤーに持たせる
        /// </summary>
        public void SpawnGrassEffect()
        {
            if (hasComment) return;
            hasComment = true;

            if (grass == null)
            {
                Debug.LogError("grassプレハブがCharacterControllerにセットされていません");
                return;
            }

            GameObject grassInstance = EffectFactory.I.GetEffectObj(grass, handPos.position, Quaternion.identity);
            if (grassInstance == null)
            {
                Debug.LogError("grassInstanceが取得できませんでした。ObjectPool/EffectFactoryの設定を確認してください");
                return;
            }

            var grassController = grassInstance.GetComponent<GrassController>();
            if (grassController == null)
            {
                Debug.LogError("grassInstanceにGrassControllerがアタッチされていません");
                return;
            }

            grassController.Init();
            OnCommentEvent = null;
            OnCommentEvent += grassController.Throw;
            grassInstance.transform.SetParent(handPos);
            grassInstance.transform.localPosition = Vector3.zero;
            grassInstance.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// TODO:要修正、草コメント以外の対応ができない
        /// </summary>
        public void InvokeCommentEvent()
        {
            OnCommentEvent?.Invoke();
            hasComment = false;
        }


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
                AudioManager.I.PlayCharacterSE(characterType, CharacterSEType.Land);
                ResetJump();
            }
            // 壁に衝突しかつダメージステート中なら反発する
            if (lastHitCanWallBounce && collision.gameObject.CompareTag("Wall"))
            {
                HandleWallBounce(collision);
                ResetWallBounceData();
            }
        }

        /// <summary>
        /// 壁バウンド可能攻撃を受けた時に呼ぶ
        /// </summary>
        public void SetWallBounceData(float force, float upForce)
        {
            lastHitCanWallBounce = true;
            lastHitBounceForce = force;
            lastHitBounceUpForce = upForce;
        }

        public void ResetWallBounceData()
        {
            if (!lastHitCanWallBounce) return;
            lastHitCanWallBounce = false;
            lastHitBounceForce = 0f;
            lastHitBounceUpForce = 0f;
        }
        private void HandleWallBounce(Collision collision)
        {
            if (rb == null || collision.contacts.Length == 0) return;

            Vector3 wallNormal = collision.contacts[0].normal;
            Vector3 incoming = lastVelocity;

            Vector3 reflectDir;
            if (incoming.sqrMagnitude > 0.0001f)
                reflectDir = Vector3.Reflect(incoming.normalized, wallNormal).normalized;
            else
                reflectDir = -wallNormal.normalized;

            float forceFromAttack = Mathf.Clamp(lastHitBounceForce, 0f, maxBounceForce);
            float speedContribution = lastVelocity.magnitude * wallBounceMultiplier;
            float totalForce = Mathf.Clamp(forceFromAttack + speedContribution, 0f, maxBounceForce);

            // 1. 衝突直後に一瞬だけ壁に押し付ける
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(-wallNormal * 5f, ForceMode.Impulse); // 壁方向に押し付ける
            
            CameraManager.I.StartShake(intensity: 0.2f, duration: 0.5f);
            HitStopManager.I.DoHitStop(duration: 0.2f, timeScale: 0.05f);
            AudioManager.I.PlaySE(SEID.WallHit);
            isWallHitting = true;
            characterState.ChangeDamageState();
            // 2. 反射を少し遅らせて実行（0.08秒後など）
            DelayUtility.StartDelayedActionWithPause(this, 0.1f, BattleJudge.I.GetPauseStateFunc, () =>
            {
                float bounceForce = totalForce * 0.4f;

                // 上方向の力を強める（例えば2倍）
                Vector3 upBoost = Vector3.up * lastHitBounceUpForce * 2f;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // 反射ベクトルのY成分も少し強めてみる（0.5f分だけ足す）
                Vector3 boostedReflectDir = reflectDir + Vector3.up * 1.5f;
                boostedReflectDir.Normalize();

                rb.AddForce((boostedReflectDir * bounceForce) + upBoost, ForceMode.Impulse);
                isWallHitting = false;
            });


            // 3. 壁衝突エフェクト
            var debris = EffectFactory.I.GetEffectObj(debrisPrefab, collision.contacts[0].point, Quaternion.identity);
            debris.GetComponent<ExplosionDebris>()?.Explode();
            DelayUtility.StartDelayedActionWithPause(this, 2f, BattleJudge.I.GetPauseStateFunc, () =>
            {
                debris.GetComponent<ExplosionDebris>()?.ResetExplosion();
                EffectFactory.I.ReturnEffect(debris);
            });
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