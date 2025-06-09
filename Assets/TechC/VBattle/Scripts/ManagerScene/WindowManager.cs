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
            // DelayUtility.StartDelayedAction(this, 1.1f, () =>
            // {
            //     var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Web);
            //     windows.Add(w);
            // });
        }

        void Update()
        {
            // if(windows[0] is WebWindow webWindow)
            // {
            //     webWindow.Move();
            // }
        }
        protected override void OnRelease()
        {
            {
                base.OnRelease();
            }
        }
    }
}