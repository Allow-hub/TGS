using Cysharp.Threading.Tasks;
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
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
            ChangeTitleState();
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
            LoadSceneWithLoadingAsync(0).Forget(); // BattleScene
            ChangeCursorMode(false, CursorLockMode.Locked);
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
        /// LoadingSceneをAdditiveで読み込み、LoadingManagerを使って指定シーンをロードする処理
        /// </summary>
        private async UniTask LoadSceneWithLoadingAsync(int targetSceneIndex)
        {
            var previousScene = SceneManager.GetActiveScene();

            // LoadingScene を Additive で読み込み
            var loadLoadingSceneOp = SceneManager.LoadSceneAsync("LoadScene", LoadSceneMode.Additive);
            await UniTask.WaitUntil(() => loadLoadingSceneOp.isDone);

            var loadingScene = SceneManager.GetSceneByName("LoadScene");
            if (loadingScene.IsValid())
                SceneManager.SetActiveScene(loadingScene);

            // 明示的にフレームを待つことで UI 描画を確実に行う
            await UniTask.DelayFrame(2); // UIを表示させるため最低2フレーム待つと安定

            // LoadingManager を取得
            LoadingManager loadingManager = null;
            await UniTask.WaitUntil(() =>
            {
                loadingManager = Object.FindObjectOfType<LoadingManager>();
                return loadingManager != null;
            });

            // 前のシーンをアンロード
            if (previousScene.name != "LoadScene")
            {
                var unloadPrevOp = SceneManager.UnloadSceneAsync(previousScene);
                await UniTask.WaitUntil(() => unloadPrevOp.isDone);
            }

            // ターゲットシーン読み込み（Additive）+ 非アクティブ
            var loadTargetOp = SceneManager.LoadSceneAsync(targetSceneIndex, LoadSceneMode.Additive);
            loadTargetOp.allowSceneActivation = false;

            // プログレス更新
            await loadingManager.UpdateProgressAsync(loadTargetOp);

            loadTargetOp.allowSceneActivation = true;
            await UniTask.WaitUntil(() => loadTargetOp.isDone);

            var targetScene = SceneManager.GetSceneByBuildIndex(targetSceneIndex);
            if (targetScene.IsValid())
                SceneManager.SetActiveScene(targetScene);

            // 最後に LoadingScene をアンロード
            var unloadLoadingOp = SceneManager.UnloadSceneAsync("LoadScene");
            await UniTask.WaitUntil(() => unloadLoadingOp.isDone);
        }

        /// <summary>
        /// プレイヤー情報を設定
        /// </summary>
        public void RegisterPlayer(GameObject prefab, int playerId) => playerInfoList.Add((prefab, playerId));

        /// <summary>
        /// 指定されたIDに対応するプレイヤーの選択したキャラのGameObjectを取得する
        /// </summary>
        public GameObject GetCharacterById(int id)
        {
            foreach (var info in playerInfoList)
            {
                if (info.playerId == id)
                    return info.prefab;
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
