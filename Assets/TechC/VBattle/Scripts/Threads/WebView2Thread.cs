using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    /// <summary>
    /// WebView2の初期化要求をSTAスレッドで順次処理するシングルトン
    /// </summary>
    public class WebView2Thread : IDisposable
    {
        private static WebView2Thread _instance;
        /// <summary>
        /// インスタンスは最初にアクセスされたときに生成されるシングルトンパターン
        /// </summary>
        /// <returns></returns>
        public static WebView2Thread Instance => _instance ??= new WebView2Thread();

        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        private readonly Thread _staThread;

        private WebView2Thread()
        {
            _staThread = new Thread(ThreadLoop);
            _staThread.SetApartmentState(ApartmentState.STA);
            _staThread.IsBackground = true;
            _staThread.Start();
        }

        /// <summary>
        /// WebView2初期化要求をSTAスレッドで実行
        /// </summary>
        public void InitWebView2(string urlOrHtml)
        {
            _queue.Add(() =>
            {
                const string className = "WebView2WindowClass";
                const string title = "WebView2 Window";
                IntPtr hwnd = CustomWindowUtility.CreateWindow(
                className,
                title,
                (uint)WINDOW_EX_STYLE.WS_EX_OVERLAPPEDWINDOW, // 普通のウィンドウスタイルに変更推奨
                (uint)WINDOW_EX_STYLE.WS_EX_APPWINDOW,
                100, 100, 200, 200,
                IntPtr.Zero
                );
                Debug.Log($"[WebView2Thread] 実行スレッドID: {Thread.CurrentThread.ManagedThreadId}");
                CoInitializeEx(IntPtr.Zero, 2); // COINIT_APARTMENTTHREADED
                int hr = WebView2NativeMethods.InitWebView2(hwnd, urlOrHtml);
                Debug.Log(GetHResultMeaning(hr));
                CoUninitialize();
            });
        }

        private void ThreadLoop()
        {
            foreach (var action in _queue.GetConsumingEnumerable())
            {
                try { action(); }
                catch (Exception ex) { Debug.LogError(ex); }
            }
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _staThread.Join();
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
