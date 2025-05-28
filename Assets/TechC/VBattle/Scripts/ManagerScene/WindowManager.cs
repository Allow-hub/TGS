using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class WindowManager : Singleton<WindowManager>
    {
        [SerializeField] private Sprite tex;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_EX_NOACTIVATE = 0x08000000;
        private const uint WS_EX_TOPMOST = 0x00000008;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int SW_SHOWNOACTIVATE = 4;

        private List<IntPtr> windows;

        protected override void Init()
        {
            base.Init();
            windows = new List<IntPtr>();
            SpawnWindows(1);
        }

        private void Update()
        {
            // Debug.Log(Screen.width + "+" + Screen.height);
            // foreach (var hWnd in windows)
            // {
            //     int centerX = Screen.width / 2;
                // var move = WindowUtility.MoveWindowToTargetPosition(hWnd, 0, Screen.height/2, 1000f);
                // var resize = WindowUtility.AnimateResizeWindow(hWnd, 100, Screen.currentResolution.height, 1000f);
            // }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            if (windows == null) return;
            foreach (var hWnd in windows)
            {
                WindowUtility.DestroyWindowHandle(hWnd);
            }
        }

        private void SpawnWindows(int count)
        {
            const int WindowWidth = 500;
            const int WindowHeight = 300;
            var rand = new System.Random();

            int screenWidth = Screen.currentResolution.width;
            int screenHeight = Screen.currentResolution.height;

            for (int i = 0; i < count; i++)
            {
                int x = rand.Next(0, screenWidth - WindowWidth);
                int y = rand.Next(0, screenHeight - WindowHeight);

                IntPtr hWnd = WindowUtility.CreateWindow(
                    "STATIC",
                    $"Window {i + 1}",
                    WS_POPUP,
                    WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
                    x, y,
                    WindowWidth, WindowHeight,
                    IntPtr.Zero
                );

                if (hWnd != IntPtr.Zero)
                {
                    // WindowUtility.SubclassWindow(hWnd); 
                    WindowUtility.SetWindowVisibility(hWnd, SW_SHOWNOACTIVATE);
                    WindowUtility.SetWindowPositionAndSize(hWnd, WindowUtility.HWND_TOPMOST, x, y, WindowWidth, WindowHeight, SWP_NOACTIVATE | SWP_SHOWWINDOW);
                    windows.Add(hWnd);
                    // DrawWindowUtility.DrawTextureToWindow(windows[0], tex.texture);

                }
            }
        }
    }
}
