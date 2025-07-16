using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

namespace TechC.Player
{
    /// <summary>
    ///キャラクターを管理をするクラス
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
        
        // 移動・回転関連の定数
        private const float STOP_THRESHOLD = 0.1f;
        private const float FINAL_ROTATION_THRESHOLD = 1f; // 回転の最終調整の許容角度（1度）
        private const float MICRO_ROTATION_THRESHOLD = 0.1f; // 微小誤差の許容範囲（0.1度）
        private const float ROTATION_TOLERANCE = 5f; // 回転処理を行う最小角度差（5度）
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
            currentGuardPower = characterData.GuardPower;
            defaultSize = new Vector3(hitCollider.radius, hitCollider.height, 0f);
            defaultCenter = hitCollider.center;
            if (outlineMat != null && renderers != null)
            {
                outlineMat = Instantiate(outlineMat);

                foreach (var smr in renderers)
                {
                    if (smr == null) continue;
                    var mats = smr.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != null && mats[i].name.Contains("Outline"))
                        {
                            mats[i] = outlineMat;
                        }
                    }
                    smr.materials = mats;
                }
            }
            // バフ辞書の初期化
            foreach (var e in Enum.GetValues(typeof(BuffType)))
            {
                multiplierEntries.Add((BuffType)e, new Dictionary<int, float>());
            }
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            opponentController = BattleJudge.I.GetOtherPlayerObjects(PlayerID)[0].GetComponent<Player.CharacterController>();
            if (isClonePlayer) return;
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
        }

        public void SetClonePlayerID(int id) => playerID = id;
        /// <summary>
        /// プレイヤーIDを設定する（生成時に呼び出す）
        /// </summary>
        public void SetPlayerID(int id, InputDevice inputDevice)
        {
            playerID = id;
            if (id == 1)
            {
                if (outlineMat.HasProperty("_OutlineColor"))
                {
                    outlineMat.SetColor("_OutlineColor", outlineColor1);
                }
                gameObject.transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            }
            else if (id == 2)
            {
                if (outlineMat.HasProperty("_OutlineColor"))
                {
                    outlineMat.SetColor("_OutlineColor", outlineColor2);
                }
                gameObject.transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
            }
            this.inputDevice = inputDevice;
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

       
        #region バフ関連メソッド

        /// <summary>
        /// バフの適用（バフの種類,乗算の数値）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddMultiplier(BuffType type, int buffId, float value)
        {
            var dic = multiplierEntries[type];

            if (dic.ContainsKey(buffId))
                dic[buffId] = value;
            else
                dic.Add(buffId, value);

            // multipliersの値を再計算
            UpdateMultiplier(type);
        }

        /// <summary>
        /// バフの解除（バフの種類,除算の数値）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void RemoveMultiplier(BuffType type, int buffId, float value)
        {
            var dic = multiplierEntries[type];

            if (dic.ContainsKey(buffId))
                dic.Remove(buffId);
            else
            {
                Debug.LogWarning($"登録されていないバフを失効しようとしました:{type}/{buffId}");
                return;
            }

            // multipliersの値を再計算
            UpdateMultiplier(type);
        }

        /// <summary>
        /// 指定されたバフタイプの最終倍率を計算して更新
        /// </summary>
        /// <param name="type"></param>
        private void UpdateMultiplier(BuffType type)
        {
            var dic = multiplierEntries[type];

            // すべてのバフを乗算で適用
            float finalMultiplier = 1.0f;
            foreach (var buff in dic.ToArray())
            {
                finalMultiplier *= buff.Value;
            }

            multipliers[type] = finalMultiplier;
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
            var debris = EffectFactory.I.GetEffectObj(debrisPrefab, hitPos, Quaternion.identity);
            debris.GetComponent<ExplosionDebris>()?.Explode();

            await UniTask.Delay(TimeSpan.FromSeconds(bounceStopTime));

            // 速度をゼロにしてから反発力を加える
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(reflected * bounceForce, ForceMode.Impulse);

            // 破片のリターン
            await UniTask.Delay(TimeSpan.FromSeconds(3f));
            debris.GetComponent<ExplosionDebris>()?.ResetExplosion();
            EffectFactory.I.ReturnEffect(debris);
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