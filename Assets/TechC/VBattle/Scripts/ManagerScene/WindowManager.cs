using UnityEngine;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    /// <summary>
    /// ウィンドウの管理クラス
    /// </summary>
    public class WindowManager : Singleton<WindowManager>
    {
        private List<NativeWindow> windows = new();
        private Dictionary<NativeWindow, GameObject> windowColliders = new();

        [Header("コライダー移動許可範囲")]
        public Vector2 areaCenter = Vector2.zero;
        public Vector2 areaSize = new Vector2(10, 6);

        protected override void Init()
        {
            base.Init();
            DelayUtility.StartDelayedAction(this, 0.1f, () =>
            {
                var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Basic);
                uint white = 0x00FFFFFF;
                PInvoke.SetLayeredWindowAttributes((HWND)w.Hwnd, (COLORREF)white, 100, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);

                w.SetRect();
                windows.Add(w);
                var windowCollider = WindowColliderFactory.I.GetWindowColliderPrefab();
                windowColliders[w] = windowCollider;
                UpdateColliderTransform(w, windowCollider);
            });
        }

        void Update()
        {
            foreach (var w in windows)
            {
                if (windowColliders.TryGetValue(w, out var colliderObj) && colliderObj != null)
                {
                    UpdateColliderTransform(w, colliderObj);
                }
            }
        }

      private void UpdateColliderTransform(NativeWindow window, GameObject colliderObj)
        {
            window.SetRect();

            var (nativeX, nativeY) = window.GetScreenPosition();

            // スクリーン解像度に合わせる（特にビルド時）
            int screenW = Display.main.systemWidth;
            int screenH = Display.main.systemHeight;

            // ウィンドウの中心座標を計算（スクリーン座標系）
            float windowCenterX = nativeX + window.Width * 0.5f;
            float windowCenterY = nativeY + window.Height * 0.5f;

            // Unityのスクリーン座標に正規化（0〜screen.width / height）
            float normalizedX = (windowCenterX / screenW) * Screen.width;
            float normalizedY = ((screenH - windowCenterY) / screenH) * Screen.height;

            var screenPos = new Vector3(normalizedX, normalizedY, 0);

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main Camera not found!");
                return;
            }

            // Z距離を固定してワールド座標に変換
            float fixedZ = -5.3f;
            screenPos.z = Mathf.Abs(cam.transform.position.z - fixedZ);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = fixedZ;

            Vector3 clampedPos = ClampToAllowedArea(worldPos);
            colliderObj.transform.position = clampedPos;

            // サイズをワールド座標単位で設定
            colliderObj.transform.localScale = GetWindowSizeInWorldUnits(window.Width, window.Height, cam, screenPos.z);
        }


        private Vector3 ClampToAllowedArea(Vector3 worldPos)
        {
            float halfWidth = areaSize.x * 0.5f;
            float halfHeight = areaSize.y * 0.5f;

            float clampedX = Mathf.Clamp(worldPos.x, areaCenter.x - halfWidth, areaCenter.x + halfWidth);
            float clampedY = Mathf.Clamp(worldPos.y, areaCenter.y - halfHeight, areaCenter.y + halfHeight);

            return new Vector3(clampedX, clampedY, worldPos.z);
        }

        private Vector3 GetWindowSizeInWorldUnits(int pixelWidth, int pixelHeight, Camera camera, float zDistance)
        {
            // より正確なサイズ計算のため、画面中央を基準点として使用
            Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, zDistance);
            Vector3 offset = new Vector3(pixelWidth * 0.5f, pixelHeight * 0.5f, 0);

            Vector3 worldCenter = camera.ScreenToWorldPoint(center);
            Vector3 worldCorner = camera.ScreenToWorldPoint(center + offset);

            float worldWidth = Mathf.Abs(worldCorner.x - worldCenter.x) * 2f;
            float worldHeight = Mathf.Abs(worldCorner.y - worldCenter.y) * 2f;

            return new Vector3(worldWidth, worldHeight, 1f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(areaCenter.x, areaCenter.y, -5.3f), new Vector3(areaSize.x, areaSize.y, 0.1f));
        }

        public void PopupWindowWindow(WindowFactory.WindowType type, int maxSize = 500, int tileSize = 200, float duration = 1f, Sprite tex = null)
        {
            var unityRect = WindowUtility.GetUnityGameViewRect();
            int unityScreenX = unityRect.left;
            int unityScreenY = unityRect.top;
            int unityScreenWidth = unityRect.right - unityRect.left;
            int unityScreenHeight = unityRect.bottom - unityRect.top;

            var rnd = new System.Random();
            int xCount = Mathf.CeilToInt((float)unityScreenWidth / tileSize);
            int yCount = Mathf.CeilToInt((float)unityScreenHeight / tileSize);
            int windowCount = xCount * yCount;
            float interval = duration / windowCount;

            List<(int xi, int yi)> gridList = new();
            for (int xi = 0; xi < xCount; xi++)
                for (int yi = 0; yi < yCount; yi++)
                    gridList.Add((xi, yi));

            for (int i = gridList.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (gridList[i], gridList[j]) = (gridList[j], gridList[i]);
            }

            int created = 0;
            DelayUtility.StartRepeatedAction(this, duration, interval, () =>
            {
                if (created >= gridList.Count) return;

                var (xi, yi) = gridList[created];
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