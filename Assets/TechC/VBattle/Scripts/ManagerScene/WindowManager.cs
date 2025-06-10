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
            window.SetRect(); // ウィンドウの位置とサイズを更新
            var (x, y) = window.GetScreenPosition();
            int width = window.Width;
            int height = window.Height;

            // Unityのスクリーン座標系は左下原点、Windowsは左上原点
            int unityScreenHeight = Screen.height;
            float yUnityTop = unityScreenHeight - y;           // ウィンドウ左上
            float yUnityBottom = unityScreenHeight - (y + height); // ウィンドウ左下

            float colliderZ = -5.3f;
            float cameraZ = Camera.main.transform.position.z;
            float zDistance = colliderZ - cameraZ;

            // 左下・右上のスクリーン座標をワールド座標に変換
            Vector3 worldLeftBottom = Camera.main.ScreenToWorldPoint(new Vector3(x, yUnityBottom, zDistance));
            Vector3 worldRightTop = Camera.main.ScreenToWorldPoint(new Vector3(x + width, yUnityTop, zDistance));

            // サイズ（ワールド単位でウィンドウと等しく）
            Vector3 size3d = new Vector3(
                Mathf.Abs(worldRightTop.x - worldLeftBottom.x),
                Mathf.Abs(worldRightTop.y - worldLeftBottom.y),
                3f
            );

            // オブジェクトの中心をウィンドウの中心に合わせる
            Vector3 worldCenter = (worldLeftBottom + worldRightTop) / 2f;
            colliderObj.transform.position = new Vector3(worldCenter.x, worldCenter.y, colliderZ);

            // オブジェクトのローカルスケールをウィンドウサイズに合わせる
            colliderObj.transform.localScale = size3d;
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