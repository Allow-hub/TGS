using System;
using Unity.VisualScripting;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    /// <summary>
    /// ウェブウィンドウクラス
    /// </summary>
    public class WebWindow : NativeWindow
    {
        public string Url { get; private set; }

        private System.Diagnostics.Process _browserProcess;
        private MonoBehaviour mono;
        private HWND webWindow;

        public WebWindow(IntPtr hwnd, int width, int height, MonoBehaviour mono, string url)
            : base(hwnd, width, height, WindowFactory.WindowType.Web)
        {
            Url = url;
            this.mono = mono;
            StartExe();
            // Debug.Log($"WebWindow created with URL: {Url}\nHwnd: {hwnd}, Width: {width}, Height: {height}");
        }

        public override void Show()
        {
            base.Show();
            WindowUtility.SetWindowVisibility(webWindow, (int)SHOW_WINDOW_CMD.SW_SHOW);
            WebView2NativeMethods.SendUrlToWebView2(Url);
        }

        public override void Destroy()
        {
            base.Destroy();

            try
            {
                if (_browserProcess != null && !_browserProcess.HasExited)
                {
                    _browserProcess.Kill();
                    _browserProcess.Dispose();
                    _browserProcess = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to kill external browser: {ex}");
            }
        }

        public void Move()
        {
            // Get current window position
            int currentX = 0, currentY = 0;
           var r=  WindowUtility.GetWindowRect(webWindow);
           currentX = r.X;
            currentY = r.Y;

            if (Input.GetKey(KeyCode.LeftArrow)) WindowUtility.MoveWindow(webWindow,currentX - 30, currentY);
            if (Input.GetKey(KeyCode.RightArrow)) WindowUtility.MoveWindow(webWindow,currentX + 30, currentY);
            if (Input.GetKey(KeyCode.UpArrow)) WindowUtility.MoveWindow(webWindow,currentX, currentY - 30);
            if (Input.GetKey(KeyCode.DownArrow)) WindowUtility.MoveWindow(webWindow,currentX, currentY + 30);
        }

        /// <summary>
        /// 外部ブラウザを起動して指定URLを表示
        /// </summary>
        private void StartExe()
        {
            try
            {
                // Assets/WebApp/WindowsFormsApp1.exe を参照
                string exeName = "WindowsFormsApp1.exe";
                string exePath = System.IO.Path.Combine(Application.streamingAssetsPath, exeName);
                string args = $"\"{Url}\" {Hwnd} {Width} {Height}";
                _browserProcess = System.Diagnostics.Process.Start(exePath, args);
                int processId = _browserProcess.Id;
                Debug.Log($"WebWindow process started with ID: {processId}");
                DelayUtility.StartDelayedAction(mono, 0.1f, () =>
                {
                    webWindow = WindowUtility.GetWindowByProcessId(processId);

                    // ウィンドウを最前面にする
                    WindowUtility.SetWindowPos(
                        webWindow,
                        HWND.HWND_TOPMOST,
                        0, 0, 0, 0,
                        SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
                    );

                    Debug.Log($"WebWindow process started: {webWindow}");
                    WindowUtility.SetWindowVisibility(webWindow, (int)SHOW_WINDOW_CMD.SW_HIDE);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to launch external browser: {ex}");
            }
        }

        /// <summary>
        /// URLを設定し、WebView2に送信
        /// </summary>
        /// <param name="url"></param>
        public void SetUrl(string url)
        {
            Url = url;
            if (webWindow != HWND.Null)
            {
                WebView2NativeMethods.SendUrlToWebView2(Url);
            }
            else
            {
                Debug.LogWarning("Web window is not initialized yet.");
            }
        }

    }
}