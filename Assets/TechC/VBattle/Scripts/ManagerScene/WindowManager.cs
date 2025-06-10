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

        protected override void Init()
        {
            base.Init();
            // ウィンドウ生成を遅延実行
            // DelayUtility.StartDelayedAction(this, 1.1f, () =>
            // {
            //     var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Web);
            //     if (w != null)
            //     {
            //         windows.Add(w);
            //     }
            //     else
            //     {
            //         Debug.LogWarning("Webウィンドウの取得に失敗しました");
            //     }
            // });
        }

        void Update()
        {
            // 配列範囲チェックと型チェック
            if (windows.Count > 0 && windows[0] is WebWindow webWindow)
            {
                webWindow.Move();
            }
        }

        protected override void OnRelease()
        {
            windows.Clear();
            base.OnRelease();
        }
    }
}