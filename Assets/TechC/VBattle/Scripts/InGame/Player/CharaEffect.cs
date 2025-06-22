using UnityEngine;
using UnityEngine.VFX;

namespace TechC
{
    /// <summary>
    /// キャラのエフェクトはすべてこれがついている必要がある
    /// </summary>
    public class CharaEffect : MonoBehaviour
    {
        [SerializeField] private AttackData attackData;
        [SerializeField] private VisualEffect vfx;
        [SerializeField] private Rigidbody rb;
        /// 自分が所属するオブジェクトプール
        private ObjectPool objectPool;
        private AttackProcessor attackProcessor;
        private int ownerId;

        [SerializeField] private bool canHeal;
        [SerializeField] private bool canSelfReturn;
        private float healAmount = 50f;//回復する場合外から設定する
        private Vector3 lastVelocity;//ポーズ時用の速度記録

        /// <summary>
        /// ファクトリー側で呼ぶ初期化メソッド
        /// </summary>
        /// <param name="objectPool"></param>
        public void Init(ObjectPool objectPool)
        {
            this.objectPool = objectPool;
        }
        private void OnEnable()
        {
            if (BattleJudge.I == null) return;
            // 一時停止イベントの登録
            BattleJudge.I.OnPauseStarted.AddListener(OnPauseStarted);
            BattleJudge.I.OnPauseEnded.AddListener(OnPauseEnded);
        }
        private void ODisable()
        {
            // 一時停止イベントの解除
            if (BattleJudge.I != null)
            {
                BattleJudge.I.OnPauseStarted.RemoveListener(OnPauseStarted);
                BattleJudge.I.OnPauseEnded.RemoveListener(OnPauseEnded);
            }
            if (rb == null) return;
            rb.isKinematic = false;
        }

        /// <summary>
        /// 攻撃側のIDを設定（自キャラの攻撃が自分に当たらないように）
        /// </summary>
        /// <param name="id">Player.CharacterControllerのPlayerId</param>
        public void SetOwnerId(int id) => ownerId = id;
        public void SetAttackProcessor(AttackProcessor attackProcessor) => this.attackProcessor = attackProcessor;

        public void SetHealAmount(float value) => healAmount = value;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            var opponentController = other.gameObject.GetComponentInParent<Player.CharacterController>();
            var opponentId = opponentController.PlayerID;

            if (ownerId == opponentId)
            {
                if (!canHeal) return;
                opponentController.HealHp(healAmount);
            }
            else
            {
                // ドレイン系の攻撃が増える場合、拡張が必要
                if (attackProcessor == null)
                    Debug.LogError("attaclProcesserを追加してください");

                attackProcessor?.HandleAttack(attackData, other);
            }

            if (canSelfReturn)
            {
                CharaEffectFactory.I.ReturnEffectObj(gameObject);
            }
        }

        /// <summary>
        /// ポーズ開始時に呼ばれる
        /// ポーズ中は動きを止めるため、Rigidbodyの速度を保存して0にする
        /// </summary>
        private void OnPauseStarted()
        {
            lastVelocity = rb != null ? rb.velocity : Vector3.zero;
            if (vfx != null)
                vfx.playRate = 0; // エフェクトの再生を停止
            // 一時停止中はエフェクトを無効化
            if (rb != null)
            {
                rb.velocity = Vector3.zero; // 一時停止中は動きを止める
                rb.isKinematic = true;
            }
        }

        /// <summary>
        /// ポーズ終了時に呼ばれる
        /// ポーズ終了後は保存しておいた速度を復元する
        /// </summary>
        private void OnPauseEnded()
        {
            // 一時停止終了後はエフェクトを有効化
            if (vfx != null)
                vfx.playRate = 1; // エフェクトの再生を再開
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = lastVelocity; // ポーズ前の速度を復元
            }
        }
    }
}
