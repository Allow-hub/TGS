using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
            public GameObject playerObject;
            public int stockCount = 1;           // 残機数
            public int playerID;                 // プレイヤーID
            public bool isAlive = true;          // 生存状態
            public Vector3 respawnPosition;      // リスポーン位置
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

        #region Unity ライフサイクル
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

            //制限時間の表記
            if(isTimeLimitEnabled){
                timerText.text = GetRemainingTime().ToString();
            }else{
                timerText.text = "∞";
            }
            // プレイヤーの初期化
            for (int i = 0; i < players.Count; i++)
            {
                players[i].playerID = i;
                players[i].isAlive = true;
                
                // リスポーン位置を記録
                if (players[i].playerObject != null)
                {
                    players[i].respawnPosition = players[i].playerObject.transform.position;
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
             if(isTimeLimitEnabled){
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
                
                
                // リスポーンイベント発火
                OnPlayerRespawn?.Invoke(player);
            }
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