using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TODO:攻撃不能状態などを対応させること
/// </summary>
namespace TechC
{
    /// <summary>
    /// バトルの勝敗を管理する調停者
    /// </summary>
    public class BattleJudge : MonoBehaviour
    {
        #region クラス定義
        [System.Serializable]
        public class PlayerData
        {
            public GameObject playerPrefab;
            [HideInInspector] public GameObject playerObject;
            public int stockCount = 1;           // 残機数
            public int playerID;                 // プレイヤーID
            public bool isAlive = true;          // 生存状態
            public GameObject initialPosition;
            public Vector3 respawnPosition;      // リスポーン位置
            public bool isInvincible = false;    // 無敵状態
            public bool canAttack = true;        // 攻撃可能状態
        }
        #endregion

        #region インスペクター設定項目
        [SerializeField] private TextMeshProUGUI timerText;
        [Header("バトル設定")]
        [SerializeField] private float timeLimit = 180f;  // 制限時間（秒）
        [SerializeField] private bool isTimeLimitEnabled = true;  // 制限時間の有無
        [SerializeField] private float respawnInvincibleTime = 3f;  // リスポーン無敵時間

        [Header("プレイヤー設定")]
        [SerializeField] private List<PlayerData> players = new List<PlayerData>();
        #endregion

        #region イベント
        [Header("イベント")]
        public UnityEvent<PlayerData> OnPlayerDeath;         // プレイヤーが死亡したとき
        public UnityEvent<PlayerData> OnPlayerRespawn;       // プレイヤーがリスポーンしたとき
        public UnityEvent<PlayerData> OnPlayerWin;           // プレイヤーが勝利したとき
        public UnityEvent<PlayerData> OnPlayerLose;          // プレイヤーが敗北したとき
        public UnityEvent OnBattleStart;                     // バトル開始時
        public UnityEvent OnBattleEnd;                       // バトル終了時
        public UnityEvent<float> OnTimeUpdate;               // 時間更新時
        #endregion

        #region プライベート変数
        private float currentTime;              // 現在の経過時間
        private bool isBattleOngoing = false;   // バトル進行中フラグ
        private int alivePlayerCount = 0;       // 生存プレイヤー数
        #endregion

        #region インスタンス
        // シングルトンインスタンス
        private static BattleJudge _instance;
        public static BattleJudge Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BattleJudge>();
                }
                return _instance;
            }
        }
        #endregion

        #region Unity ライフサイクル
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // バトルの初期化
            InitializeBattle();
        }

        private void Update()
        {
            if (!isBattleOngoing) return;

            // 時間制限の管理
            if (isTimeLimitEnabled)
            {
                currentTime -= Time.deltaTime;
                OnTimeUpdate?.Invoke(currentTime);
                timerText.text = currentTime.ToString("N0");
                if (currentTime <= 0)
                {
                    EndBattleByTimeUp();
                    return;
                }
            }
        }
        #endregion

        #region バトル初期化・終了
        /// <summary>
        /// バトルの初期化
        /// </summary>
        public void InitializeBattle()
        {
            currentTime = timeLimit;
            isBattleOngoing = true;
            alivePlayerCount = players.Count;

            // タイマー表示
            timerText.text = isTimeLimitEnabled ? GetRemainingTime().ToString() : "∞";

            for (int i = 0; i < players.Count; i++)
            {
                players[i].isAlive = true;
                players[i].isInvincible = false;
                players[i].canAttack = true;

                // プレイヤーの生成
                if (players[i].playerPrefab != null)
                {
                    GameObject newPlayer = Instantiate(players[i].playerPrefab, players[i].initialPosition.transform.position, Quaternion.identity);
                    players[i].playerObject = newPlayer;
                    var characterController = newPlayer.GetComponent<Player.CharacterController>();
                    characterController.SetPlayerID(players[i].playerID);
                }
                else
                {
                    Debug.LogWarning($"Player {i} にプレハブが設定されていません。");
                }
            }

            OnBattleStart?.Invoke();
        }


        /// <summary>
        /// 時間切れでバトルを終了
        /// </summary>
        private void EndBattleByTimeUp()
        {
            //終了時時間を0に
            if (isTimeLimitEnabled)
            {
                timerText.text = 0.ToString();
            }
            // 時間切れの場合は残っているプレイヤーを勝利とする
            PlayerData winnerPlayer = null;

            foreach (var player in players)
            {
                if (player.isAlive)
                {
                    winnerPlayer = player;
                    break;
                }
            }

            if (winnerPlayer != null)
            {
                OnPlayerWin?.Invoke(winnerPlayer);
            }

            EndBattle();
        }

        /// <summary>
        /// バトルを終了する
        /// </summary>
        private void EndBattle()
        {
            isBattleOngoing = false;

            // バトル終了時にすべてのプレイヤーの攻撃を禁止
            foreach (var player in players)
            {
                player.canAttack = false;
            }

            OnBattleEnd?.Invoke();
        }
        #endregion

        #region プレイヤー操作

        /// <summary>
        /// プレイヤーの死亡処理
        /// </summary>
        /// <param name="playerID">死亡したプレイヤーID</param>
        public void PlayerDeath(int playerID)
        {
            if (playerID < 0 || playerID >= players.Count) return;

            PlayerData player = players[playerID];
            player.stockCount--;
            player.isAlive = false;
            player.canAttack = false;

            // プレイヤー死亡イベント発火
            OnPlayerDeath?.Invoke(player);

            if (player.stockCount <= 0)
            {
                // 残機がなくなった場合
                PlayerEliminated(player);
            }
            else
            {
                // 残機がある場合はリスポーン
                StartCoroutine(RespawnPlayer(player));
            }
        }

        /// <summary>
        /// プレイヤーのリスポーン処理
        /// </summary>
        /// <param name="player">リスポーンするプレイヤー</param>
        private IEnumerator RespawnPlayer(PlayerData player)
        {
            yield return new WaitForSeconds(2f);  // リスポーン待機時間

            if (player.playerObject != null)
            {
                // プレイヤーをリスポーン位置に戻す
                player.playerObject.transform.position = player.respawnPosition;

                // Rigidbodyがあれば速度をリセット
                Rigidbody rb = player.playerObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                player.isAlive = true;
                player.isInvincible = true;  // 無敵状態を設定

                // リスポーンイベント発火
                OnPlayerRespawn?.Invoke(player);

                // リスポーン直後は攻撃不可
                player.canAttack = false;

                // 無敵時間後に通常状態に戻す
                StartCoroutine(EndInvincibility(player));
            }
        }

        /// <summary>
        /// 無敵時間の終了処理
        /// </summary>
        /// <param name="player">対象プレイヤー</param>
        private IEnumerator EndInvincibility(PlayerData player)
        {
            yield return new WaitForSeconds(respawnInvincibleTime);

            player.isInvincible = false;
            player.canAttack = true;  // 攻撃可能状態に戻す

            // 必要に応じて無敵終了イベントを追加することも可能
        }

        /// <summary>
        /// プレイヤーが完全に敗北した場合の処理
        /// </summary>
        /// <param name="player">敗北したプレイヤー</param>
        private void PlayerEliminated(PlayerData player)
        {
            alivePlayerCount--;

            // プレイヤー敗北イベント発火
            OnPlayerLose?.Invoke(player);

            // 一人だけ生き残っていたら勝利判定
            if (alivePlayerCount == 1)
            {
                foreach (var p in players)
                {
                    if (p.isAlive)
                    {
                        OnPlayerWin?.Invoke(p);
                        EndBattle();
                        break;
                    }
                }
            }
            else if (alivePlayerCount <= 0)
            {
                // 全員敗北した場合（引き分け）
                EndBattle();
            }
        }
        #endregion

        #region 攻撃判定
        /// <summary>
        /// プレイヤーが攻撃可能かどうかを判定
        /// </summary>
        /// <param name="playerID">プレイヤーID</param>
        /// <returns>攻撃可能ならtrue、不可ならfalse</returns>
        public bool CanPlayerAttack(int playerID)
        {
            // バトルが進行中でなければ攻撃不可
            if (!isBattleOngoing) return false;

            // プレイヤーIDが不正なら攻撃不可
            if (playerID < 0 || playerID >= players.Count) return false;

            PlayerData player = players[playerID];

            // 生存状態でなければ攻撃不可
            if (!player.isAlive) return false;

            // 攻撃可能状態でなければ攻撃不可
            if (!player.canAttack) return false;

            // 上記の条件をすべて満たせば攻撃可能
            return true;
        }

        /// <summary>
        /// ターゲットが攻撃対象として有効かどうかを判定
        /// </summary>
        /// <param name="targetPlayerID">ターゲットプレイヤーID</param>
        /// <returns>攻撃対象として有効ならtrue、無効ならfalse</returns>
        public bool IsValidAttackTarget(int targetPlayerID)
        {
            // プレイヤーIDが不正なら無効
            if (targetPlayerID < 0 || targetPlayerID >= players.Count) return false;

            PlayerData targetPlayer = players[targetPlayerID];

            // 生存状態でなければ無効
            if (!targetPlayer.isAlive) return false;

            // 無敵状態なら無効
            if (targetPlayer.isInvincible) return false;

            // 上記の条件をすべて満たせば有効
            return true;
        }

        /// <summary>
        /// プレイヤーの無敵状態を取得
        /// </summary>
        /// <param name="playerID">プレイヤーID</param>
        /// <returns>無敵状態ならtrue、そうでなければfalse</returns>
        public bool IsPlayerInvincible(int playerID)
        {
            if (playerID < 0 || playerID >= players.Count) return false;
            return players[playerID].isInvincible;
        }

        /// <summary>
        /// プレイヤーの攻撃可能状態を設定
        /// </summary>
        /// <param name="playerID">プレイヤーID</param>
        /// <param name="canAttack">攻撃可能状態</param>
        public void SetPlayerAttackState(int playerID, bool canAttack)
        {
            if (playerID < 0 || playerID >= players.Count) return;
            players[playerID].canAttack = canAttack;
        }
        #endregion

        #region ユーティリティメソッド
        /// <summary>
        /// プレイヤーの残機数を取得
        /// </summary>
        /// <param name="playerID">プレイヤーID</param>
        /// <returns>残機数</returns>
        public int GetPlayerStockCount(int playerID)
        {
            if (playerID < 0 || playerID >= players.Count) return 0;
            return players[playerID].stockCount;
        }

        /// <summary>
        /// プレイヤーを追加
        /// </summary>
        /// <param name="playerObject">プレイヤーオブジェクト</param>
        /// <param name="stockCount">初期残機数</param>
        public void AddPlayer(GameObject playerObject, int stockCount = 3)
        {
            PlayerData newPlayer = new PlayerData
            {
                playerObject = playerObject,
                stockCount = stockCount,
                playerID = players.Count,
                isAlive = true,
                canAttack = true,
                isInvincible = false,
                respawnPosition = playerObject.transform.position
            };

            players.Add(newPlayer);
        }

        /// <summary>
        /// 現在のバトル状態を取得
        /// </summary>
        /// <returns>バトルが進行中かどうか</returns>
        public bool IsBattleOngoing()
        {
            return isBattleOngoing;
        }

        /// <summary>
        /// 残り時間を取得
        /// </summary>
        /// <returns>残り時間（秒）</returns>
        public float GetRemainingTime()
        {
            return currentTime;
        }
        #endregion
    }
}