using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    /// <summary>
    /// ウィンドウを生成するFactoryクラス
    /// </summary>
    public class WindowFactory : Singleton<WindowFactory>
    {
        public enum WindowType { Basic, Notification, Image }

        private Dictionary<WindowType, Queue<NativeWindow>> poolByType = new();
        private const int InitialPoolSize = 10;

        protected override void Init()
        {
            base.Init();

            foreach (WindowType type in Enum.GetValues(typeof(WindowType)))
            {
                poolByType[type] = new Queue<NativeWindow>();
                for (int i = 0; i < InitialPoolSize; i++)
                {
                    var window = CreateNewWindow(type, $"{type} Window {i}", 100, 100);
                    if (window != null)
                        poolByType[type].Enqueue(window);
                }
            }
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            DisposeAll();
        }

        public NativeWindow GetWindow(WindowType type)
        {
            NativeWindow window = null;

            if (poolByType.TryGetValue(type, out var queue) && queue.Count > 0)
            {
                window = queue.Dequeue(); // 再利用
            }
            else
            {
                window = CreateNewWindow(type, $"{type} Window", 100, 100); // 必要なら新規作成
            }

            // ウィンドウを表示
            if (window != null)
            {
                window.Show();
            }

            return window;
        }
        public void ReturnWindow(NativeWindow window)
        {
            window.Hide();
            if (!poolByType.ContainsKey(window.Type))
            {
                poolByType[window.Type] = new Queue<NativeWindow>();
            }
            poolByType[window.Type].Enqueue(window);
        }

        public NativeWindow CreateNewWindow() =>
            CreateNewWindow(WindowType.Basic, "Default", 100, 100);


        /// <summary>
        /// 新しいウィンドウの作成
        /// </summary>
        /// <param name="type">ウィンドウタイプ</param>
        /// <param name="title">ウィンドウ名</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="tex">ImageTypeの場合必要</param>
        /// <returns></returns>
        public NativeWindow CreateNewWindow(WindowType type, string title, int width, int height, Texture2D tex = null)
        {
            IntPtr hwnd = WindowUtility.CreateWindow(
                "STATIC", title,
                (uint)WINDOW_STYLE.WS_POPUP,
                (uint)(WINDOW_EX_STYLE.WS_EX_NOACTIVATE | WINDOW_EX_STYLE.WS_EX_TOPMOST),
                100, 100, width, height,
                IntPtr.Zero
            );

            if (hwnd == IntPtr.Zero)
                return null;

            switch (type)
            {
                case WindowType.Image:
                    return new ImageWindow(hwnd, width, height, tex);
                // case WindowType.Notification:
                //     return new NotificationWindow(hwnd, width, height, type);
                default:
                    return new NativeWindow(hwnd, width, height, type);
            }
        }

        /// <summary>
        /// 全削除
        /// </summary>
        public void DisposeAll()
        {
            foreach (var queue in poolByType.Values)
            {
                foreach (var window in queue)
                {
                    window.Destroy();
                }
                queue.Clear();
            }
        }
    }
}
