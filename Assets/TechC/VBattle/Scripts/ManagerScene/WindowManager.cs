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
            DelayUtility.StartDelayedAction(this, 1.1f, () =>
            {
                // var screenWidth = Screen.currentResolution.width;
                // var screenHeight = Screen.currentResolution.height;
                // var rand = new System.Random();
                var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Web);
                // if (w is WebWindow webWindow && initialUrl != null)
                // {
                //     webWindow.SetUrl(initialUrl);
                // }
                // StartCoroutine(ShowWindowsCoroutine(screenWidth, screenHeight, rand));
            });
        }

        // private System.Collections.IEnumerator ShowWindowsCoroutine(int screenWidth, int screenHeight, System.Random rand)
        // {
        //     for (int i = 0; i < 40; i++)
        //     {
        //         var win = WindowFactory.I.GetWindow(WindowFactory.WindowType.Basic);
        //         int x = rand.Next(0, screenWidth - 300);   // 200はウィンドウ幅の仮値
        //         int y = rand.Next(0, screenHeight - 300);  // 200はウィンドウ高さの仮値
        //         WindowUtility.MoveWindow((HWND)win.Hwnd, x, y);
        //         windows.Add(win);
        //         yield return new WaitForSeconds(0.05f);
        //     }
        // }

        protected override void OnRelease()
        {
            {
                base.OnRelease();
            }
        }
    }
}