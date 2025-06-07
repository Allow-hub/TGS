using System;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ウェブウィンドウクラス
    /// </summary>
    public class WebWindow : NativeWindow
    {
        public string Url { get; private set; }

        public WebWindow(IntPtr hwnd, int width, int height, string url)
            : base(hwnd, width, height, WindowFactory.WindowType.Web)
        {
            Url = url;
            Debug.Log($"WebWindow created with URL: {Url}\nHwnd: {hwnd}, Width: {width}, Height: {height}");

        }
        public override void Show()
        {
            base.Show();
            
            // STAスレッドでWebView2初期化を依頼
            WebView2Thread.Instance.InitWebView2(Url);
        }

    }
}