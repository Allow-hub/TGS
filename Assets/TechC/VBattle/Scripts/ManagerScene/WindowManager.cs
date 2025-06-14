using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Windows.Win32.Foundation;
using UnityEditor.PackageManager.UI;

namespace TechC
{
    /// <summary>
    /// ウィンドウの管理クラス
    /// </summary>
    public class WindowManager : Singleton<WindowManager>
    {
        private List<NativeWindow> windows = new();
        private Dictionary<NativeWindow, GameObject> windowColliders = new();

        // コライダー移動許可範囲（ワールド座標系で指定）
        [Header("コライダー移動許可範囲")]
        public Vector2 areaCenter = Vector2.zero;
        public Vector2 areaSize = new Vector2(10, 6);

        protected override void Init()
        {
            base.Init();
            DelayUtility.StartDelayedAction(this, 0.1f, () =>
            {
                var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Basic);
                w.SetRect();
                windows.Add(w);
                var windowCollider = WindowColliderFactory.I.GetWindowColliderPrefab();
                windowColliders[w] = windowCollider;
                UpdateColliderTransform(w, windowCollider);
            });

            var gameView = WindowUtility.FindWindowWithTitleSubstring("Game");
            var gameViewRect = WindowUtility.GetWindowRect(gameView);
            
            WindowUtility.GetClientRect(gameView, out var rect);
            Debug.Log($"GameViewQQQ Rect: {gameViewRect.left}, {gameViewRect.top}, {gameViewRect.right}, {gameViewRect.bottom}");

            Debug.Log($"GameView Rect: {rect.left}, {rect.top}, {rect.right}, {rect.bottom}");
        }

        void Update()
        {
            // ウィンドウとコライダーを毎フレーム追従
            foreach (var w in windows)
            {
                if (windowColliders.TryGetValue(w, out var colliderObj) && colliderObj != null)
                {
                    UpdateColliderTransform(w, colliderObj);
                }
            }
            // if (Time.frameCount % 60 == 0) // 60フレームごとに出力（約1秒ごと）
            // {
            //     float fps = 1f / Time.deltaTime;
            //     Debug.Log($"FPS: {fps:F1}");
            // }
        }

        private void UpdateColliderTransform(NativeWindow window, GameObject colliderObj)
        {
            // 1. リサイズと位置設定
            WindowUtility.ResizeWindow((HWND)window.Hwnd, 100, 100);
            window.SetRect();
            WindowUtility.MoveWindow((HWND)window.Hwnd, Screen.width / 2, Screen.height / 2);

            // 2. ネイティブのスクリーン座標取得
            var (nativeX, nativeY) = window.GetScreenPosition();
            // Debug.Log($"[Native] Screen Position: ({nativeX}, {nativeY})");

            // 3. Unityゲームビューのスクリーン上の矩形
            var unityRect = WindowUtility.GetUnityGameViewRect(); // Rect { X, Y, Width, Height }
            // Debug.Log($"[Unity] GameView Rect: X={unityRect.X}, Y={unityRect.Y}, Width={unityRect.Width}, Height={unityRect.Height}");

            // 4. ネイティブ座標 -> Unityスクリーン座標へ変換
            float unityX = nativeX - unityRect.X;
            float unityY = (unityRect.Y + unityRect.Height) - nativeY;
            Vector2 screenPos2D = new Vector2(unityX, unityY);
            // Debug.Log($"[Unity] Converted Screen Pos: {screenPos2D}");

            // 5. カメラからの距離
            float zDistance = 5.3f;
            Vector3 screenPos3D = new Vector3(screenPos2D.x, screenPos2D.y, zDistance);
            // Debug.Log($"[Unity] Screen Pos 3D for World: {screenPos3D}");

            // 6. ワールド座標への変換
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos3D);
            // Debug.Log($"[Unity] World Position: {worldPos}");

            // 7. オブジェクト移動
            colliderObj.transform.position = worldPos;
            Vector3 size3d = new Vector3(
                3,
                3,
                3f
            );
            colliderObj.transform.localScale = size3d;
            // int unityScreenX = unityRect.left;
            // int unityScreenBottom = unityRect.bottom;
            // int unityScreenWidth = unityRect.right - unityRect.left;
            // int unityScreenHeight = unityRect.bottom - unityRect.top;
            // // ネイティブウィンドウの左下座標（スクリーン座標系）
            // int nativeLeft = nativeX;
            // int nativeBottom = nativeY + nativeHeight;

            // // Unityウィンドウ左下を原点とした相対座標
            // float relativeX = nativeLeft - unityScreenX;
            // float relativeY = unityScreenHeight - (nativeY - unityRect.top);

            // // カメラからの距離を計算
            // float colliderZ = -5.3f;
            // float cameraZ = Camera.main.transform.position.z;
            // float zDistance = Mathf.Abs(colliderZ - cameraZ);

            // // スクリーン座標（左下・右上）→ワールド座標
            // Vector3 screenLeftBottom = new Vector3(relativeX, relativeY, zDistance);
            // Vector3 screenRightTop = new Vector3(relativeX + nativeWidth, relativeY + nativeHeight, zDistance);

            // Vector3 worldLeftBottom = Camera.main.ScreenToWorldPoint(screenLeftBottom);
            // Vector3 worldRightTop = Camera.main.ScreenToWorldPoint(screenRightTop);

            // // colliderZでzを上書きしない
            // // worldLeftBottom.z = colliderZ;
            // // worldRightTop.z = colliderZ;

            // // サイズ（zは3f固定）
            // Vector3 size3d = new Vector3(
            //     Mathf.Abs(worldRightTop.x - worldLeftBottom.x),
            //     Mathf.Abs(worldRightTop.y - worldLeftBottom.y),
            //     3f
            // );

            // // 中心座標
            // Vector3 worldCenter = (worldLeftBottom + worldRightTop) / 2f;
            // colliderObj.transform.position = worldCenter; // zもScreenToWorldPointの値を使う
            // colliderObj.transform.localScale = size3d;
            // Debug.Log($"Position: {colliderObj.transform.position}, Scale: {colliderObj.transform.localScale}");
            // Debug.Log($"nativeX:{nativeX}, nativeY:{nativeY}, nativeWidth:{nativeWidth}, nativeHeight:{nativeHeight}");
            // Debug.Log($"unityRect: left={unityRect.left}, top={unityRect.top}, right={unityRect.right}, bottom={unityRect.bottom}");
            // Debug.Log($"relativeX:{relativeX}, relativeY:{relativeY}");
        }

        // --- Gizmoで範囲を可視化 ---
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(areaCenter.x, areaCenter.y, -5.3f), new Vector3(areaSize.x, areaSize.y, 0.1f));
        }

        /// <summary>
        /// ウィンドウを画面を覆い隠すように分割して表示する（Ameの必殺技用）
        /// </summary>
        /// <param name="type">windowの種類</param>
        /// <param name="maxSize">windowのサイズ</param>
        /// <param name="tileSize">分割サイズ</param>
        /// <param name="duration">時間</param>
        /// <param name="tex">Imageの場合Texture</param>
        public void PopupWindowWindow(WindowFactory.WindowType type, int maxSize = 500, int tileSize = 200, float duration = 1f, Sprite tex = null)
        {
            // 画面サイズ取得
            var unityRect = WindowUtility.GetUnityGameViewRect();
            int unityScreenX = unityRect.left;
            int unityScreenY = unityRect.top;
            int unityScreenWidth = unityRect.right - unityRect.left;
            int unityScreenHeight = unityRect.bottom - unityRect.top;

            // Windowで隙間なく覆うための分割数を計算
            var rnd = new System.Random();

            // 横・縦に何枚並べるか
            int xCount = Mathf.CeilToInt((float)unityScreenWidth / tileSize);
            int yCount = Mathf.CeilToInt((float)unityScreenHeight / tileSize);

            int windowCount = xCount * yCount;
            float interval = duration / windowCount;

            // グリッドの全パターンをリスト化してシャッフル
            List<(int xi, int yi)> gridList = new List<(int xi, int yi)>();
            for (int xi = 0; xi < xCount; xi++)
                for (int yi = 0; yi < yCount; yi++)
                    gridList.Add((xi, yi));

            // シャッフル
            for (int i = gridList.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var tmp = gridList[i];
                gridList[i] = gridList[j];
                gridList[j] = tmp;
            }

            int created = 0;
            DelayUtility.StartRepeatedAction(this, duration, interval, () =>
            {
                if (created >= gridList.Count) return;

                var (xi, yi) = gridList[created];

                // 各グリッドで残り幅・高さを計算
                int remainWidth = unityScreenWidth - xi * tileSize;
                int remainHeight = unityScreenHeight - yi * tileSize;

                int wMin = Mathf.Min(tileSize, remainWidth);
                int wMax = Mathf.Min(maxSize, remainWidth);
                int hMin = Mathf.Min(tileSize, remainHeight);
                int hMax = Mathf.Min(maxSize, remainHeight);

                int w = (wMin < wMax) ? rnd.Next(wMin, wMax + 1) : wMin;
                int h = (hMin < hMax) ? rnd.Next(hMin, hMax + 1) : hMin;

                int x = unityScreenX + xi * tileSize;
                int y = unityScreenY + yi * tileSize;

                var win = WindowFactory.I.GetWindow(type);
                WindowUtility.MoveWindow((HWND)win.Hwnd, x, y);
                WindowUtility.ResizeWindow((HWND)win.Hwnd, w, h);
                win.SetRect();
                if (win is ImageWindow imageWindow)
                {
                    imageWindow.SetImage(tex.texture, w, h);
                }
                windows.Add(win);
                created++;
            });
        }

        protected override void OnRelease()
        {
            windows.Clear();
            base.OnRelease();
        }
    }
}