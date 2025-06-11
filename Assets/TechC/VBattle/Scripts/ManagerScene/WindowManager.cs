using UnityEngine;
using System.Collections.Generic;

namespace TechC
{
    /// <summary>
    /// ウィンドウの管理クラス
    /// </summary>
    public class WindowManager : Singleton<WindowManager>
    {
        [SerializeField] private Sprite tex;
        private List<NativeWindow> windows = new();
        private Dictionary<NativeWindow, GameObject> windowColliders = new();

        // コライダー移動許可範囲（ワールド座標系で指定）
        [Header("コライダー移動許可範囲")]
        public Vector2 areaCenter = Vector2.zero;
        public Vector2 areaSize = new Vector2(10, 6);

        protected override void Init()
        {
            base.Init();
            // ウィンドウ生成を遅延実行
            DelayUtility.StartDelayedAction(this, 1.1f, () =>
            {
                var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Basic);
                if (w != null)
                {
                    windows.Add(w);

                    // コライダー生成・紐付け
                    var colliderObj = WindowColliderFactory.I.GetWindowColliderPrefab();
                    if (colliderObj != null)
                    {
                        windowColliders[w] = colliderObj;
                        UpdateColliderTransform(w, colliderObj); // 初期位置・サイズ合わせ
                    }
                }
                else
                {
                    Debug.LogWarning("Webウィンドウの取得に失敗しました");
                }
            });
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
            Debug.Log($"Position: {colliderObj.transform.position}, Scale: {colliderObj.transform.localScale}");
            Debug.Log($"nativeX:{nativeX}, nativeY:{nativeY}, nativeWidth:{nativeWidth}, nativeHeight:{nativeHeight}");
            Debug.Log($"unityRect: left={unityRect.left}, top={unityRect.top}, right={unityRect.right}, bottom={unityRect.bottom}");
            Debug.Log($"relativeX:{relativeX}, relativeY:{relativeY}");
        }

        // --- Gizmoで範囲を可視化 ---
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(areaCenter.x, areaCenter.y, -5.3f), new Vector3(areaSize.x, areaSize.y, 0.1f));
        }

        protected override void OnRelease()
        {
            windows.Clear();
            base.OnRelease();
        }
    }
}