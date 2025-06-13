using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Windows.Win32.Foundation;

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
        }

        void Update()
        {
            // ウィンドウとコライダーを毎フレーム追従
            // foreach (var w in windows)
            // {
            //     if (windowColliders.TryGetValue(w, out var colliderObj) && colliderObj != null)
            //     {
            //         UpdateColliderTransform(w, colliderObj);
            //     }
            // }
            // if (Time.frameCount % 60 == 0) // 60フレームごとに出力（約1秒ごと）
            // {
            //     float fps = 1f / Time.deltaTime;
            //     Debug.Log($"FPS: {fps:F1}");
            // }
        }

        private void UpdateColliderTransform(NativeWindow window, GameObject colliderObj)
        {
            window.SetRect();
            var (nativeX, nativeY) = window.GetScreenPosition();
            int nativeWidth = window.Width;
            int nativeHeight = window.Height;

            // Unityウィンドウのスクリーン座標とサイズを取得
            var unityRect = WindowUtility.GetUnityGameViewRect();
            int unityScreenX = unityRect.left;
            int unityScreenBottom = unityRect.bottom;
            int unityScreenWidth = unityRect.right - unityRect.left;
            int unityScreenHeight = unityRect.bottom - unityRect.top;

            // ネイティブウィンドウの左下座標（スクリーン座標系）
            int nativeLeft = nativeX;
            int nativeBottom = nativeY + nativeHeight;

            // Unityウィンドウ左下を原点とした相対座標
            float relativeX = nativeLeft - unityScreenX;
            float relativeY = unityScreenHeight - (nativeY - unityRect.top);

            // カメラからの距離を計算
            float colliderZ = -5.3f;
            float cameraZ = Camera.main.transform.position.z;
            float zDistance = Mathf.Abs(colliderZ - cameraZ);

            // スクリーン座標（左下・右上）→ワールド座標
            Vector3 screenLeftBottom = new Vector3(relativeX, relativeY, zDistance);
            Vector3 screenRightTop = new Vector3(relativeX + nativeWidth, relativeY + nativeHeight, zDistance);

            Vector3 worldLeftBottom = Camera.main.ScreenToWorldPoint(screenLeftBottom);
            Vector3 worldRightTop = Camera.main.ScreenToWorldPoint(screenRightTop);

            // colliderZでzを上書きしない
            // worldLeftBottom.z = colliderZ;
            // worldRightTop.z = colliderZ;

            // サイズ（zは3f固定）
            Vector3 size3d = new Vector3(
                Mathf.Abs(worldRightTop.x - worldLeftBottom.x),
                Mathf.Abs(worldRightTop.y - worldLeftBottom.y),
                3f
            );

            // 中心座標
            Vector3 worldCenter = (worldLeftBottom + worldRightTop) / 2f;
            colliderObj.transform.position = worldCenter; // zもScreenToWorldPointの値を使う
            colliderObj.transform.localScale = size3d;
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