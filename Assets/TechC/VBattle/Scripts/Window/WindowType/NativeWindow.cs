using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    public class NativeWindow
    {
        public IntPtr Hwnd { get; private set; }
        public int Width { get; }
        public int Height { get; }
        public WindowFactory.WindowType Type { get; }

        public NativeWindow(IntPtr hwnd, int width, int height, WindowFactory.WindowType type)
        {
            Hwnd = hwnd;
            Width = width;
            Height = height;
            Type = type;
        }

        public virtual void Show() => WindowUtility.SetWindowVisibility(Hwnd, (int)SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
        public virtual void Hide() => WindowUtility.SetWindowVisibility(Hwnd, (int)SHOW_WINDOW_CMD.SW_HIDE);
        public virtual void Destroy()
        {
            Hide();
            Debug.Log($"[Destroy] hwnd: {Hwnd}");

            if (Hwnd == IntPtr.Zero)
            {
                Debug.LogWarning("Hwnd is zero before destroy");
                return;
            }
            bool isWindow = WindowUtility.IsValidWindow((HWND)Hwnd);
            Debug.Log($"IsWindow before destroy: {isWindow}");

            bool success = WindowUtility.DestroyWindowHandle(Hwnd);
            Debug.Log($"DestroyWindowHandle success: {success}");
            Hwnd = IntPtr.Zero;
        }

        public virtual void MoveWindowToTargetPosition(IntPtr hWnd, int targetX, int targetY, float speed) => WindowUtility.MoveWindowToTargetPosition(hWnd, targetX, targetY, speed);
        public virtual void ResizeWindow(IntPtr hWnd, int targetWidth, int targetHeight, float speed) => WindowUtility.AnimateResizeWindow(Hwnd, targetWidth, targetHeight, speed);
    }

}