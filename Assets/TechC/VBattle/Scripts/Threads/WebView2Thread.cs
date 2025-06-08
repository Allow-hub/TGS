using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    /// <summary>
    /// WebView2の初期化要求をSTAスレッドで順次処理するシングルトン
    /// </summary>
    public class WebView2Thread : StaThreadRunner, IDisposable
    {
        private static WebView2Thread _instance;
        /// <summary>
        /// インスタンスは最初にアクセスされたときに生成されるシングルトンパターン
        /// </summary>
        public static WebView2Thread Instance => _instance ??= new WebView2Thread();

        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private HWND _hwnd;
        private WebView2Thread()
        {
            // StaThreadRunnerのコンストラクタでスレッド生成・開始される想定
            Init();
        }

        /// <summary>
        /// WebView2初期化要求をSTAスレッドで実行
        /// </summary>
        public void InitWebView2(string urlOrHtml, int x, int y, int width, int height)
        {
            _queue.Add(() =>
            {
                // const string className = "WindowClass_Web";
                // const string title = "WebView2 Window";
                // // STAスレッド内でウィンドウ作成
                // IntPtr hwnd = CustomWindowUtility.CreateWindow(
                //     className,
                //     title,
                //     (uint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                //     (uint)WINDOW_EX_STYLE.WS_EX_APPWINDOW,
                //     x, y, width, height,
                //     IntPtr.Zero
                // );
                // _hwnd = (HWND)hwnd;
                // WindowUtility.ShowWindow((HWND)hwnd);
            });
        }


        /// <summary>
        /// StaThreadRunnerのThreadMainをオーバーライドし、キューを処理
        /// </summary>
        protected override void ThreadMain()
        {
            // _queue からアクションを取り出して順次実行
            foreach (var action in _queue.GetConsumingEnumerable())
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"WebView2Thread action error: {ex}");
                }
            }
        }

        protected override void OnInit()
        {
            int hr = CoInitializeEx(IntPtr.Zero, 2); // COINIT_APARTMENTTHREADED
            if (hr != 0)
                Debug.LogWarning($"CoInitializeEx failed: 0x{hr:X8}");
        }

        protected override void OnStop()
        {
            try
            {
                Debug.Log("ShutdownWebView2 called.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ShutdownWebView2 failed: {ex}");
            }
            if (WindowUtility.DestroyWindowHandle(_hwnd))
            {
                Debug.Log($"WebView2 window destroyed: {_hwnd}");
            }
            else
            {
                Debug.LogWarning($"Failed to destroy WebView2 window: {_hwnd}");
            }
            CoUninitialize(); // スレッド終了時に一度だけ
        }
        public static string GetHResultMeaning(int hr)
        {
            switch ((uint)hr)
            {
                case 0x80070005: return "E_ACCESSDENIED: アクセス拒否。UACやウィンドウの所有権を確認してください。";
                case 0x80004005: return "E_FAIL: 一般的な失敗。詳細はネイティブ側ログを確認。";
                case 0x80070002: return "ERROR_FILE_NOT_FOUND: ファイルが見つかりません。WebView2ランタイムがあるか確認。";
                case 0x8007007E: return "ERROR_MOD_NOT_FOUND: DLLが見つかりません。WebView2Loaderが不足？";
                default: return $"HRESULT: 0x{hr:X8} の意味は不明です。";
            }
        }

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr pvReserved, int dwCoInit);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();
    }
}
