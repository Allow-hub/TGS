using UnityEngine;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
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

            float dpiScale = WindowUtility.GetDpiScaleRatio((HWND)window.Hwnd);
            var (nativeX, nativeY) = window.GetScreenPosition();

            float correctedX = nativeX / dpiScale;
            float correctedY = nativeY / dpiScale;
            float correctedWidth = window.Width / dpiScale;
            float correctedHeight = window.Height / dpiScale;

            float windowCenterX = correctedX + correctedWidth * 0.5f;
            float windowCenterY = correctedY + correctedHeight * 0.5f;

            var unityRect = WindowUtility.GetUnityGameViewRect();
            float unityHeight = Screen.height;

            float offsetY = windowCenterY - unityRect.top;
            float unityScreenY = unityHeight - offsetY;
            float unityScreenX = windowCenterX - unityRect.left;

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main Camera not found!");
                return;
            }
#if false
            float zDistance = 4.2f;
            Vector3 screenPos = new Vector3(unityScreenX, unityScreenY, zDistance);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = -5.3f;
#else
            // var colliderPos = colliderObj.transform.position;
            var colliderPos = new Vector3(colliderObj.transform.position.x, colliderObj.transform.position.y, -5.3f);

            var screenPos = cam.WorldToScreenPoint(colliderPos);
            screenPos.x = unityScreenX;
            screenPos.y = unityScreenY;
            var worldPos = cam.ScreenToWorldPoint(screenPos);
#endif
            Vector3 clampedPos = ClampToAllowedArea(worldPos);
            colliderObj.transform.position = clampedPos;

            colliderObj.transform.localScale = GetWindowSizeInWorldUnits(correctedWidth, correctedHeight, cam);
        }


        private Vector3 ClampToAllowedArea(Vector3 worldPos)
        {
            float halfWidth = areaSize.x * 0.5f;
            float halfHeight = areaSize.y * 0.5f;

            float clampedX = Mathf.Clamp(worldPos.x, areaCenter.x - halfWidth, areaCenter.x + halfWidth);
            float clampedY = Mathf.Clamp(worldPos.y, areaCenter.y - halfHeight, areaCenter.y + halfHeight);

            return new Vector3(clampedX, clampedY, worldPos.z);
        }
        private Vector3 GetWindowSizeInWorldUnits(float pixelWidth, float pixelHeight, Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError("Camera is null!");
                return Vector3.one;
            }

            float zDepth = -5.3f; // ウィンドウコライダーの Z 座標に合わせる

            // スクリーン座標系での中心 + 幅・高さ
            Vector3 screenCenter = camera.WorldToScreenPoint(new Vector3(0f, 0f, zDepth));
            Vector3 screenRight = screenCenter + new Vector3(pixelWidth, 0f, 0f);
            Vector3 screenTop = screenCenter + new Vector3(0f, pixelHeight, 0f);

            Vector3 worldCenter = camera.ScreenToWorldPoint(screenCenter);
            Vector3 worldRight = camera.ScreenToWorldPoint(screenRight);
            Vector3 worldTop = camera.ScreenToWorldPoint(screenTop);

            float worldWidth = Mathf.Abs(worldRight.x - worldCenter.x);
            float worldHeight = Mathf.Abs(worldTop.y - worldCenter.y);

            return new Vector3(worldWidth, worldHeight, 1f);
        }



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