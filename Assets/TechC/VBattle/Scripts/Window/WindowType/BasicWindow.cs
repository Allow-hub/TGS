using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 通常のウィンドウ
    /// </summary>
    public class BasicWindow : NativeWindow
    {
        public BasicWindow(IntPtr hwnd, int width, int height) : base(hwnd, width, height, WindowFactory.WindowType.Basic)
        {
        }

        public override void Show()
        {
            base.Show();
            CustomLogger.Info($"[Show] hwnd: {Hwnd}", LOGTAG);
        }

        public override void Hide()
        {
            base.Hide();
            CustomLogger.Info($"[Hide] hwnd: {Hwnd}", LOGTAG);
        }

        public override void SetRect()
        {
            base.SetRect();
            CustomLogger.Info($"[SetRect] hwnd: {Hwnd}, Width: {Width}, Height: {Height}", LOGTAG);
        }
        public void ResizeWindow(int width, int height, int delay = 0)
        {
            base.ResizeWindow(width, height, delay);
            CustomLogger.Info($"[ResizeWindow] hwnd: {Hwnd}, Width: {Width}, Height: {Height}", LOGTAG);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
