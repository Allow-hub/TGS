using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private bool isHighPerformanceMode = true;
        [SerializeField] private bool canConectWifi = true;
        [SerializeField] private bool preloadLoadingScene = true; // LoadingSceneの事前読み込み

        private List<(GameObject prefab, int playerId,InputDevice inputDevice)> playerInfoList = new();
        private Scene? preloadedLoadingScene = null;

        public bool IsHighPerformanceMode => isHighPerformanceMode;
        public bool CanConectWifi => canConectWifi;
        public bool IsNpc => isNpc;
        private bool isNpc;
        public GameState CurrentState => currentState;
        private GameState currentState = GameState.Title;

        protected override void Init()
        {
            base.Init();
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;

            // LoadingSceneを事前読み込み（オプション）
            if (preloadLoadingScene)
                PreloadLoadingScene().Forget();

            // ChangeTitleState();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                ChangeBattleState();

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
                    LoadSceneWithLoadingAsync(0).Forget();
                    ChangeCursorMode(true, CursorLockMode.None);
                    break;

                case GameState.Select:
                    LoadSceneWithLoadingAsync(1).Forget(); // SelectScene
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
            // 必要ならステートごとの処理をここに
        }

        private void BattleStateInit()
        {
            LoadSceneWithLoadingAsync(2).Forget(); // BattleScene
            // ChangeCursorMode(false, CursorLockMode.Locked);
        }

        private void ChangeCursorMode(bool visible, CursorLockMode cursorLockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = cursorLockMode;
        }

        public void SetActiveSceneRoot(bool value, string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    go.SetActive(value);
                }
            }
        }

        /// <summary>
        /// LoadingSceneを事前読み込み（非アクティブ状態で）
        /// </summary>
        private async UniTask PreloadLoadingScene()
        {
            if (SceneManager.GetSceneByName("LoadScene").isLoaded)
                return;

            var loadOp = SceneManager.LoadSceneAsync("LoadScene", LoadSceneMode.Additive);
            loadOp.allowSceneActivation = false; // 非アクティブで読み込み

            await UniTask.WaitUntil(() => loadOp.isDone);
            preloadedLoadingScene = SceneManager.GetSceneByName("LoadScene");

            // ルートオブジェクトを非アクティブにして隠す
            SetActiveSceneRoot(false, "LoadScene");
        }

        /// <summary>
        /// 最適化されたLoadingSceneを使ったシーン読み込み処理
        /// </summary>
        private async UniTask LoadSceneWithLoadingAsync(int targetSceneIndex)
        {
            var previousScene = SceneManager.GetActiveScene();

            // LoadingSceneの準備
            Scene loadingScene;
            if (preloadedLoadingScene.HasValue && preloadedLoadingScene.Value.isLoaded)
            {
                // 事前読み込み済みの場合
                loadingScene = preloadedLoadingScene.Value;
                SetActiveSceneRoot(true, "LoadScene"); // アクティブ化
                SceneManager.SetActiveScene(loadingScene);
            }
            else
            {
                // 通常の読み込み
                var loadLoadingSceneOp = SceneManager.LoadSceneAsync("LoadScene", LoadSceneMode.Additive);
                await UniTask.WaitUntil(() => loadLoadingSceneOp.isDone);

                loadingScene = SceneManager.GetSceneByName("LoadScene");
                if (loadingScene.IsValid())
                    SceneManager.SetActiveScene(loadingScene);
            }

            // LoadingManagerの取得
            LoadingManager loadingManager = await GetLoadingManagerAsync();

            // 前のシーンをアンロード（LoadingScene表示後）
            if (previousScene.name != "LoadScene")
            {
                var unloadPrevOp = SceneManager.UnloadSceneAsync(previousScene);
                await UniTask.WaitUntil(() => unloadPrevOp.isDone);
            }

            // ターゲットシーン読み込み
            var loadTargetOp = SceneManager.LoadSceneAsync(targetSceneIndex, LoadSceneMode.Additive);

            // プログレス更新
            await loadingManager.UpdateProgressAsync(loadTargetOp);

            // シーン切り替え
            var targetScene = SceneManager.GetSceneByBuildIndex(targetSceneIndex);
            if (targetScene.IsValid())
                SceneManager.SetActiveScene(targetScene);

            // LoadingSceneの処理
            if (preloadLoadingScene)
            {
                // 事前読み込みモードの場合は非アクティブ化して保持
                SetActiveSceneRoot(false, "LoadScene");
            }
            else
            {
                // 通常モードの場合はアンロード
                var unloadLoadingOp = SceneManager.UnloadSceneAsync("LoadScene");
                await UniTask.WaitUntil(() => unloadLoadingOp.isDone);
            }
        }

        /// <summary>
        /// LoadingManagerを効率的に取得
        /// </summary>
        private async UniTask<LoadingManager> GetLoadingManagerAsync()
        {
            LoadingManager loadingManager = null;
            int attempts = 0;
            const int maxAttempts = 30; // 最大1秒待機（30フレーム）

            while (loadingManager == null && attempts < maxAttempts)
            {
                loadingManager = Object.FindObjectOfType<LoadingManager>();
                if (loadingManager == null)
                {
                    attempts++;
                    await UniTask.Yield();
                }
            }

            if (loadingManager == null)
                Debug.LogError("LoadingManagerが見つかりませんでした");

            return loadingManager;
        }

        public void RegisterPlayer(GameObject prefab, int playerId, InputDevice inputDevice) => playerInfoList.Add((prefab, playerId, inputDevice));
        public GameObject GetCharacterById(int id)
        {
            foreach (var info in playerInfoList)
            {
                if (info.playerId == id)
                    return info.prefab;
            }
            return null;
        }
        public void RemovePlayerById(int id) => playerInfoList.RemoveAll(info => info.playerId == id);
        public List<(GameObject prefab, int playerId,InputDevice inputDevice)> GetPlayerInfo() => playerInfoList;
        public bool SetIsNpc(bool value) => isNpc = value;

        public void ChangeTitleState() => SetState(GameState.Title);
        public void ChangeSelectState() => SetState(GameState.Select);
        public void ChangeMenuState() => SetState(GameState.Menu);
        public void ChangeBattleState() => SetState(GameState.Battle);
        public void ChangeResultState() => SetState(GameState.Result);
    }
}