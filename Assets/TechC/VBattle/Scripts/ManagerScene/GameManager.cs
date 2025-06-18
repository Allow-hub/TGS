using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TechC
{
    public enum GameState
    {
        Title,
        Select,
        Menu,
        Battle,
        Result,
    }
    /// <summary>
    /// ゲーム全体を管理するクラス
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private int targetFrameRate = 144;
        [SerializeField] private bool isHighPerformanceMode = true;// 高パフォーマンスモードかどうか
        [SerializeField] private bool canConectWifi = true;// Wi-Fi接続可能かどうか
        private List<(GameObject prefab, int playerId)> playerInfoList = new();

        public bool IsHighPerformanceMode => isHighPerformanceMode;
        public bool CanConectWifi => canConectWifi;

        public GameState CurrentState => currentState;
        private GameState currentState = GameState.Title;


        protected override void Init()
        {
            base.Init();
            Application.runInBackground = true;
            // VSyncCount を Dont Sync に変更
            QualitySettings.vSyncCount = 0;
            // fps 144 を目標に設定
            Application.targetFrameRate = targetFrameRate;
            ChangeTitleState();
        }


        private void Update()
        {
            //テスト用完成時に消す
            if (Input.GetKeyDown(KeyCode.Space))
                ChangeBattleState();

            //将来的にパッド対応させる必要あり、現在はPauseするとInputManagerを消しているので取れない
            if (Input.GetKeyDown(KeyCode.Escape))
                MenuManager.I.OpenMenu();
            StateHandler();
        }

        private void SetState(GameState state)
        {
            currentState = state;

            switch (state)
            {
                case GameState.Title:
                    // LoadSceneAsync(0); // 0 = TitleScene
                    // AudioManager.I.PlayBGM(BGMID.Title);
                    ChangeCursorMode(true, CursorLockMode.None);
                    break;

                case GameState.Select:
                    LoadSceneAsync(1); // 1 = SelectScene
                    ChangeCursorMode(true, CursorLockMode.None);
                    break;

                case GameState.Menu:
                    ChangeCursorMode(true, CursorLockMode.None);
                    break;

                case GameState.Battle:
                    BattleStateInit();
                    break;

                case GameState.Result:
                    ChangeCursorMode(true, CursorLockMode.None);
                    break;

                default:
                    Debug.LogWarning($"未対応のステート: {state}");
                    break;
            }
        }

        private void StateHandler()
        {
            // switch (currentState)
            // {
            //     case GameState.Battle:
            //         break;
            // }
        }
        private void BattleStateInit()
        {
            LoadSceneAsync(0);
            ChangeCursorMode(false, CursorLockMode.Locked);
        }


        private void ChangeCursorMode(bool visible, CursorLockMode cursorLockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = cursorLockMode;
        }


        // 非同期でシーンをロード
        public void LoadSceneAsync(int sceneIndex)
        {
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }

        // 非同期でシーンをロードするコルーチン
        private IEnumerator LoadSceneCoroutine(int sceneIndex)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
            asyncOperation.allowSceneActivation = false;

            // シーンのロードが終わるまで待機
            while (!asyncOperation.isDone)
            {
                // ロードが進んだら進行状況を表示
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                Debug.Log("Loading progress: " + (progress * 100) + "%");

                // ロードが完了したらシーンをアクティブ化
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        /// <summary>
        /// プレイヤー情報を設定
        /// </summary>
        public void RegisterPlayer(GameObject prefab, int playerId) => playerInfoList.Add((prefab, playerId));

        /// <summary>
        /// 指定されたIDに対応するプレイヤーの選択したキャラのGameObjectを取得する。
        /// 見つからなければ null を返す。
        /// </summary>
        public GameObject GetCharacterById(int id)
        {
            foreach (var info in playerInfoList)
            {
                if (info.playerId == id)
                {
                    return info.prefab;
                }
            }
            return null;
        }



        /// <summary>
        /// プレイヤー情報を削除
        /// </summary>
        public void RemovePlayerById(int id) => playerInfoList.RemoveAll(info => info.playerId == id);
        public List<(GameObject prefab, int playerId)> GetPlayerInfo() => playerInfoList;


        public void ChangeTitleState() => SetState(GameState.Title);
        public void ChangeSelectState() => SetState(GameState.Select);
        public void ChangeMenuState() => SetState(GameState.Menu);
        public void ChangeBattleState() => SetState(GameState.Battle);
        public void ChangeResultState() => SetState(GameState.Result);
    }
}
