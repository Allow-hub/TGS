using System;
using UnityEngine;
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

        /// <summary>
        /// 外部ブラウザを起動して指定URLを表示
        /// </summary>
        private void StartExe()
        {
            try
            {
                // StreamingAssets内のEXEパスを組み立て
                string exePath = System.IO.Path.Combine(Application.streamingAssetsPath, "WindowsFormsApp1.exe");
                string args = $"\"{Url}\" {Hwnd} {Width} {Height}";
                _browserProcess = System.Diagnostics.Process.Start(exePath, args);


                DelayUtility.StartDelayedAction(mono, 0.1f, () =>
                {

                    webWindow = WindowUtility.GetWindowByProcessName("AA");
                    Debug.Log($"WebWindow process started: {webWindow}");
                    WindowUtility.SetWindowVisibility(webWindow, (int)SHOW_WINDOW_CMD.SW_HIDE);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to launch external browser: {ex}");
            }
        }

    }
}