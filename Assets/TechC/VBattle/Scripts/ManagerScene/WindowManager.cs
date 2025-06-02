using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    public class WindowManager : Singleton<WindowManager>
    {
        [SerializeField] private Sprite tex;

        private const uint WS_POPUP = (uint)WINDOW_STYLE.WS_POPUP;
        private const uint WS_EX_NOACTIVATE = (uint)WINDOW_EX_STYLE.WS_EX_NOACTIVATE;
        private const uint WS_EX_TOPMOST = (uint)WINDOW_EX_STYLE.WS_EX_TOPMOST;
        private const uint WS_EX_TOOLWINDOW = (uint)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
        private const uint SWP_NOACTIVATE = (uint)SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE;
        private const uint SWP_SHOWWINDOW = (uint)SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW;
        private const int SW_SHOWNOACTIVATE = (int)SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE;


        private List<IntPtr> windows;

        protected override void Init()
        {
            base.Init();
            windows = new List<IntPtr>();
            // SpawnWindows(1);
        }

        private void Update()
        {
            // Debug.Log(Screen.width + "+" + Screen.height);
            foreach (var hWnd in windows)
            {
                int centerX = Screen.width / 2;
                int centerY = Screen.height / 2;

                var move = WindowUtility.MoveWindowToTargetPosition(hWnd, centerX, centerY, 100f);
                var resize = WindowUtility.AnimateResizeWindow(hWnd, 300, Screen.currentResolution.height, 1000f);
            }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
        }

        // private void SpawnWindows(int count)
        // {
        //     const int WindowWidth = 500;
        //     const int WindowHeight = 300;
        //     var rand = new System.Random();

        //     int screenWidth = Screen.currentResolution.width;
        //     int screenHeight = Screen.currentResolution.height;

        //     for (int i = 0; i < count; i++)
        //     {
        //         int x = rand.Next(0, screenWidth - WindowWidth);
        //         int y = rand.Next(0, screenHeight - WindowHeight);

        //         IntPtr hWnd = WindowUtility.CreateWindow(
        //             "STATIC",
        //             $"Window {i + 1}",
        //             WS_POPUP,
        //             WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
        //             x, y,
        //             WindowWidth, WindowHeight,
        //             IntPtr.Zero
        //         );

        //         if (hWnd != IntPtr.Zero)
        //         {
        //             WindowUtility.SubclassWindow(hWnd); 
        //             WindowUtility.SetWindowVisibility(hWnd, SW_SHOWNOACTIVATE);
        //             WindowUtility.SetWindowPositionAndSize(hWnd, WindowUtility.HWND_TOPMOST, x, y, WindowWidth, WindowHeight, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        //             windows.Add(hWnd);
        //             DrawWindowUtility.DrawTextureToWindow(windows[0], tex.texture);
        //             WindowUtility.SetRedraw(hWnd, false);

        //         }
        //     }
        // }
    }
}
