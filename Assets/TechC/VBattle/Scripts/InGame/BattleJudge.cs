using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// バトルの勝敗を管理する調停者
    /// </summary>
    public class BattleJudge : Singleton<BattleJudge>
    {
        #region クラス定義
        [Serializable]
        public class PlayerData
        {
            public GameObject playerPrefab;
            [HideInInspector] public GameObject playerObject;
            public int stockCount = 1;           // 残機数
            public int playerID;                 // プレイヤーID
            public bool isAlive = true;          // 生存状態
            public GameObject initialPosition;
            public bool isInvincible = false;    // 無敵状態
            public bool canAttack = true;        // 攻撃可能状態
            public InputDevice inputDevice;
        }
        #endregion

        #region インスペクター設定項目
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Slider timerSlider;
        [Header("バトル設定")]
        [SerializeField] private float timeLimit = 180f;  // 制限時間（秒）
        [SerializeField] private bool isTimeLimitEnabled = true;  // 制限時間の有無
        [SerializeField] private GameObject ultMap;//必殺技後のステージ
        [SerializeField] private GameObject normalMap;//2dのステージ

        [SerializeField] private TextMeshProUGUI countDownText; // カウントダウン表示用

        [SerializeField] private bool isDebug = true;
        [Header("プレイヤー設定")]
        [SerializeField] private GameObject p1InitialPosition;
        [SerializeField] private GameObject p2InitialPosition;
        [SerializeField] private List<PlayerData> players = new List<PlayerData>();
        public List<PlayerData> Players => players;
        #endregion

        #region イベント
        [Header("イベント")]
        public UnityEvent<PlayerData> OnPlayerDeath;         // プレイヤーが死亡したとき
        public UnityEvent<PlayerData> OnPlayerWin;           // プレイヤーが勝利したとき
        public UnityEvent<PlayerData> OnPlayerLose;          // プレイヤーが敗北したとき
        public UnityEvent OnBattleStart;                     // バトル開始時
        public UnityEvent<PlayerData> OnBattleEnd;
        public UnityEvent OnUltStart;
        public UnityEvent<float> OnTimeUpdate;               // 時間更新時
        [Header("ポーズイベント")]
        public UnityEvent OnPauseStarted;
        public UnityEvent OnPauseEnded;
        #endregion

        #region プライベート変数
        private bool isUlting = false;
        public bool IsUlting => isUlting;
        private float currentTime;              // 現在の経過時間
        private bool isBattleOngoing = false;   // バトル進行中フラグ
        private int alivePlayerCount = 0;       // 生存プレイヤー数
        protected override bool UseDontDestroyOnLoad => false;

        // プレイヤーごとの一時保存用
        private Dictionary<int, Vector3> lastVelocities = new Dictionary<int, Vector3>();
        private Dictionary<int, Vector3> lastAngularVelocities = new Dictionary<int, Vector3>();
        private Dictionary<int, float> lastAnimSpeeds = new Dictionary<int, float>();

        private bool isPaused = false;          // ポーズ状態フラグ
        public bool IsPaused => isPaused;       // 読み取り専用プロパティ
        public Func<bool> GetPauseStateFunc => () => isPaused;  // Funcデリゲート
        #endregion


        #region Unity ライフサイクル
        protected override void Init()
        {
            base.Init();
            // バトルの初期化
            InitializeBattle();
        }

        private void Update()
        {
            if (!isBattleOngoing) return;
            if (isTimeLimitEnabled)
            {
                currentTime -= Time.deltaTime;
                timerSlider.value = currentTime / timeLimit; // スライダーの更新
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
            alivePlayerCount = players.Count;
            // タイマー表示
            timerText.text = isTimeLimitEnabled ? GetRemainingTime().ToString() : "∞";

            //デバッグ状態でないときセレクトで選択した情報を取得
            if (!isDebug)
            {
                ResetPlayer();
                foreach (var info in GameManager.I.GetPlayerInfo())
                    AddPlayer(info.prefab, info.playerId, info.inputDevice);
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].isAlive = true;
                    players[i].isInvincible = true;
                    players[i].canAttack = true;

                    // プレイヤーの生成
                    if (players[i].playerPrefab != null)
                    {
                        GameObject newPlayer = Instantiate(players[i].playerPrefab, players[i].initialPosition.transform.position, Quaternion.identity);
                        if (players[i].inputDevice != null)
                        {
                            var playerInput = newPlayer.GetComponent<PlayerInput>();
                            string controlScheme = players[i].inputDevice is Gamepad ? "PadScheme" : "KeyboardScheme";
                            playerInput.SwitchCurrentControlScheme(controlScheme, players[i].inputDevice);
                        }
                        else
                        {
                            Debug.LogError("InputDeviceがnull");
                        }
                        players[i].playerObject = newPlayer;
                        var characterController = newPlayer.GetComponent<Player.CharacterController>();
                        var inputManager = newPlayer.GetComponent<BaseInputManager>();
                        inputManager.enabled = false;
                        characterController.SetPlayerID(players[i].playerID, players[i].inputDevice);
                    }
                    else
                    {
                        Debug.LogWarning($"Player {i} にプレハブが設定されていません。");
                    }
                }


                float startTime = 5f;
                float repeatTime = 1f;
                StartCountDown(startTime, repeatTime);
            }
            else
            {
                isBattleOngoing = true;
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
                        characterController.SetPlayerID(players[i].playerID, players[i].inputDevice);
                    }
                    else
                    {
                        Debug.LogWarning($"Player {i} にプレハブが設定されていません。");
                    }
                }
                countDownText.gameObject.SetActive(false);
                OnBattleStart?.Invoke();
            }

            SetIsUlting(false);
        }

        private void StartCountDown(float startTime, float repeatTime)
        {
            float count = startTime - 2;//Start!!を出す用の調整
            countDownText.gameObject.SetActive(true);

            OnBattleStart?.Invoke();
            DelayUtility.StartRepeatedActionWithPause(this, startTime, repeatTime, GetPauseStateFunc, () =>
            {
                countDownText.text = count.ToString();
                if (count <= 0)
                {
                    countDownText.text = "Start!!";
                    DelayUtility.StartDelayedActionWithPause(this, 1f, GetPauseStateFunc, () =>
                    {
                        countDownText.gameObject.SetActive(false);
                    });
                    foreach (var player in players)
                    {
                        var input = player.playerObject.GetComponent<BaseInputManager>();
                        input.enabled = true;
                        isBattleOngoing = true;
                        player.isInvincible = false;
                    }
                }
                
                count--;
            });
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

            EndBattle(winnerPlayer);
        }

        /// <summary>
        /// バトルを終了する
        /// </summary>
        private void EndBattle(PlayerData winnerPlayer)
        {
            isBattleOngoing = false;

            // バトル終了時にすべてのプレイヤーの攻撃を禁止
            foreach (var player in players)
            {
                player.canAttack = false;
            }
            SetPause(true);
            OnBattleEnd?.Invoke(winnerPlayer);
        }
        #endregion

        #region プレイヤー操作

        /// <summary>
        /// プレイヤーの死亡処理
        /// </summary>
        /// <param name="playerID">死亡したプレイヤーID</param>
        public void PlayerDeath(int playerID)
        {
            playerID--;
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
                        EndBattle(p);
                        break;
                    }
                }
            }
            else if (alivePlayerCount <= 0)
            {
                // 全員敗北した場合（引き分け）
                EndBattle(null);
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
            playerID--;
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
            targetPlayerID--;
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
        /// ポーズ状態を設定する
        /// </summary>
        /// <param name="pause">trueでポーズ、falseで再開</param>
        public void SetPause(bool pause)
        {
            if (isPaused == pause) return; // 既に同じ状態の場合は何もしない

            isPaused = pause;

            if (isPaused)
            {
                PausePlayers();
            }
            else
            {
                ResumePlayers();
            }
        }


        public void PausePlayers()
        {
            foreach (var player in players)
            {
                if (player.playerObject != null)
                {
                    // PlayerInputを無効化
                    var input = player.playerObject.GetComponent<PlayerInput>();
                    if (input != null)
                        input.enabled = false;

                    // Rigidbodyをフリーズ＆速度保存
                    var rb = player.playerObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        lastVelocities[player.playerID] = rb.velocity;
                        lastAngularVelocities[player.playerID] = rb.angularVelocity;
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = true;
                    }

                    // アニメーション速度保存
                    var characterController = player.playerObject.GetComponent<Player.CharacterController>();
                    if (characterController != null)
                    {
                        var anim = characterController.GetAnim();
                        lastAnimSpeeds[player.playerID] = anim.speed;
                        anim.speed = 0f;
                    }
                }
            }
            OnPauseStarted?.Invoke();
        }

        public void ResumePlayers()
        {
            foreach (var player in players)
            {
                if (player.playerObject != null)
                {
                    // PlayerInputを有効化
                    var input = player.playerObject.GetComponent<PlayerInput>();
                    if (input != null)
                        input.enabled = true;

                    // Rigidbodyのフリーズ解除＆速度復元
                    var rb = player.playerObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        if (lastVelocities.TryGetValue(player.playerID, out var v))
                            rb.velocity = v;
                        if (lastAngularVelocities.TryGetValue(player.playerID, out var av))
                            rb.angularVelocity = av;
                    }

                    // アニメーション速度復元
                    var characterController = player.playerObject.GetComponent<Player.CharacterController>();
                    if (characterController != null)
                    {
                        var anim = characterController.GetAnim();
                        if (lastAnimSpeeds.TryGetValue(player.playerID, out var speed))
                            anim.speed = speed;
                    }
                }
            }
            OnPauseEnded?.Invoke();
        }
        /// <summary>
        /// 指定したプレイヤーのみポーズ状態にする
        /// </summary>
        public void PausePlayer(int playerID, bool isKinematic = true)
        {
            playerID--; // プレイヤーIDは1から始まるため、0ベースに変換
            if (playerID < 0 || playerID >= players.Count) return;
            var player = players[playerID];
            if (player.playerObject != null)
            {
                // PlayerInputを無効化
                var input = player.playerObject.GetComponent<PlayerInput>();
                if (input != null)
                    input.enabled = false;

                // Rigidbodyをフリーズ＆速度保存
                var rb = player.playerObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    lastVelocities[player.playerID] = rb.velocity;
                    lastAngularVelocities[player.playerID] = rb.angularVelocity;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = isKinematic;
                }

                // アニメーション速度保存
                var characterController = player.playerObject.GetComponent<Player.CharacterController>();
                if (characterController != null)
                {
                    var anim = characterController.GetAnim();
                    lastAnimSpeeds[player.playerID] = anim.speed;
                    anim.speed = 0f;
                }
            }
        }

        /// <summary>
        /// 指定したプレイヤーのみポーズ解除する
        /// </summary>
        public void ResumePlayer(int playerID)
        {
            if (playerID < 0 || playerID >= players.Count) return;
            var player = players[playerID];
            if (player.playerObject != null)
            {
                // PlayerInputを有効化
                var input = player.playerObject.GetComponent<PlayerInput>();
                if (input != null)
                    input.enabled = true;

                // Rigidbodyのフリーズ解除＆速度復元
                var rb = player.playerObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    if (lastVelocities.TryGetValue(player.playerID, out var v))
                        rb.velocity = v;
                    if (lastAngularVelocities.TryGetValue(player.playerID, out var av))
                        rb.angularVelocity = av;
                }

                // アニメーション速度復元
                var characterController = player.playerObject.GetComponent<Player.CharacterController>();
                if (characterController != null)
                {
                    var anim = characterController.GetAnim();
                    if (lastAnimSpeeds.TryGetValue(player.playerID, out var speed))
                        anim.speed = speed;
                }
            }
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
        public PlayerData GetPlayerData(int id) => players[id];
        public GameObject GetPlayerObjById(int id) => players[id - 1].playerObject;
        /// <summary>
        /// 指定したプレイヤーID以外の全プレイヤーのGameObjectを取得する
        /// </summary>
        /// <param name="excludePlayerID">除外したいプレイヤーID</param>
        /// <returns>除外対象以外のプレイヤーGameObjectのリスト</returns>
        public List<GameObject> GetOtherPlayerObjects(int excludePlayerID)
        {
            List<GameObject> result = new List<GameObject>();

            foreach (var player in players)
            {
                if (player.playerID != excludePlayerID && player.playerObject != null)
                {
                    result.Add(player.playerObject);
                }
            }

            return result;
        }

        /// <summary>
        /// プレイヤーの初期化
        /// </summary>
        public void ResetPlayer() => players.Clear();
        /// <summary>
        /// GameManagerを使ってセレクト画面で選んだ項目をプレイヤーに反映する
        /// </summary>
        /// <param name="character">characterPrefab</param>
        /// <param name="playerId">プレイヤーID</param>
        public void AddPlayer(GameObject character, int playerId,InputDevice inputDevice)
        {
            var data = new PlayerData();
            playerId++;
            data.playerPrefab = character;
            data.playerID = playerId;
            if (playerId == 1)
                data.initialPosition = p1InitialPosition;
            else if (playerId == 2)
                data.initialPosition = p2InitialPosition;
            else
                Debug.LogError("指定したPlayerIdは存在しえないものです");
            data.inputDevice = inputDevice;
            data.stockCount = 1;
            data.canAttack = true;
            data.isAlive = true;
            data.isInvincible = false;
            players.Add(data);
        }

        public void SetIsUlting(bool value)
        {
            if (isUlting) return;//すでに誰かが必殺技を放っていた場合何もしない
            isUlting = value;
            normalMap.SetActive(!value);
            ultMap.SetActive(value);
            OnUltStart.Invoke();
        }
    }
}