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
        private const int InitialPoolSize = 5;

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

        public NativeWindow GetWindow(WindowType type)
        {
            if (poolByType.TryGetValue(type, out var queue) && queue.Count > 0)
            {
                return queue.Dequeue(); // 再利用
            }

            return CreateNewWindow(type, $"{type} Window", 100, 100); // 必要なら新規作成
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

        public NativeWindow CreateNewWindow(WindowType type, string title, int width, int height)
        {
            IntPtr hwnd = WindowUtility.CreateWindow(
                "STATIC", title,
                (uint)(WINDOW_STYLE.WS_POPUP),
                (uint)(WINDOW_EX_STYLE.WS_EX_NOACTIVATE | WINDOW_EX_STYLE.WS_EX_TOPMOST),
                100, 100, width, height,
                IntPtr.Zero
            );

            if (hwnd == IntPtr.Zero)
                return null;

            switch (type)
            {
                // case WindowType.Image:
                //     return new ImageWindow(hwnd, width, height, type);
                // case WindowType.Notification:
                //     return new NotificationWindow(hwnd, width, height, type);
                default:
                    return new NativeWindow(hwnd, width, height, type);
            }
        }

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
