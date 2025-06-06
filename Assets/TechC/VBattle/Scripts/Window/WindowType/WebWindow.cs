using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TechC
{
    /// <summary>
    /// ウェブウィンドウクラス
    /// </summary>
    public class WebWindow : NativeWindow
    {
        // [DllImport("WebView2Unity.dll")]
        // private static extern int InitializeWebView(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string url);

        // [DllImport("WebView2Unity.dll")]
        // private static extern int GetLastError();

        // [DllImport("WebView2Unity.dll")]
        // private static extern int GetLastErrorMessageLength();

        // [DllImport("WebView2Unity.dll")]
        // private static extern int GetLastErrorMessage([MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, int bufferSize);

        // [DllImport("WebView2Unity.dll")]
        // private static extern void ShutdownWebView();

        // [DllImport("WebView2Unity.dll")]
        // private static extern bool IsWebViewReady();

        // [DllImport("WebView2Unity.dll")]
        // private static extern bool NavigateToUrl([MarshalAs(UnmanagedType.LPWStr)] string url);

        // [DllImport("WebView2Unity.dll")]
        // private static extern void ResizeWebView(int width, int height);

        // [DllImport("WebView2Unity.dll")]
        // private static extern void ShowWebView();

        [DllImport("TestDll.dll")]
        private static extern int Add(int a, int b);
        public string Url { get; private set; }
        // private bool isInitialized = false;
        // private CancellationTokenSource cancellationTokenSource;

        public WebWindow(IntPtr hwnd, int width, int height, string url)
            : base(hwnd, width, height, WindowFactory.WindowType.Web)
        {
            Url = url;
            // cancellationTokenSource = new CancellationTokenSource();
            // Debug.Log(Add(1, 2));
            // InitializeWebViewAsync(cancellationTokenSource.Token).Forget();
        }

        // private async UniTask InitializeWebViewAsync(CancellationToken cancellationToken = default)
        // {
        //     try
        //     {
        //         // WebView2の初期化を試行
        //         int result = InitializeWebView(Hwnd, Url);
        //         if (result != 0)
        //         {
        //             Debug.LogError($"WebView2 initialization failed: {GetWebViewErrorMessage()}");
        //             return;
        //         }

        //         isInitialized = true;
        //         Debug.Log("WebView2 initialization started for URL: " + Url);

        //         // 初期化完了を待ってからサイズ設定
        //         await WaitForWebViewReady(cancellationToken);
        //     }
        //     catch (OperationCanceledException)
        //     {
        //         Debug.Log("WebView2 initialization was cancelled");
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError($"Error during WebView2 initialization: {ex.Message}");
        //     }
        // }

        // private async UniTask WaitForWebViewReady(CancellationToken cancellationToken = default)
        // {
        //     const float timeoutSeconds = 10f; // 10秒タイムアウト
        //     const int checkIntervalMs = 100; // 100ms間隔でチェック

        //     int maxRetries = (int)(timeoutSeconds * 1000 / checkIntervalMs);
        //     int retryCount = 0;

        //     while (!IsWebViewReady() && retryCount < maxRetries)
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         await UniTask.Delay(checkIntervalMs, cancellationToken: cancellationToken);
        //         retryCount++;
        //     }

        //     if (IsWebViewReady())
        //     {
        //         Debug.Log("WebView2 is ready!");
        //         ResizeWebView(Width, Height);
        //         ShowWebView();
        //     }
        //     else
        //     {
        //         Debug.LogError("WebView2 initialization timed out");
        //     }
        // }

        // public async void SetUrl(string url)
        // {
        //     if (string.IsNullOrEmpty(url))
        //     {
        //         Debug.LogWarning("URL is empty or null");
        //         return;
        //     }

        //     Url = url;

        //     if (isInitialized && IsWebViewReady())
        //     {
        //         // 既に初期化済みの場合は、ナビゲートのみ実行
        //         bool success = NavigateToUrl(Url);
        //         if (success)
        //         {
        //             Debug.Log("Navigated to URL: " + Url);
        //         }
        //         else
        //         {
        //             Debug.LogError("Failed to navigate to URL: " + Url);
        //         }
        //     }
        //     else
        //     {
        //         Debug.LogWarning("WebView2 is not ready yet. Waiting for initialization...");

        //         // WebView2の準備完了を待ってからナビゲート
        //         try
        //         {
        //             await WaitForWebViewReady(cancellationTokenSource.Token);

        //             if (IsWebViewReady())
        //             {
        //                 bool success = NavigateToUrl(Url);
        //                 if (success)
        //                 {
        //                     Debug.Log("Navigated to URL after waiting: " + Url);
        //                 }
        //                 else
        //                 {
        //                     Debug.LogError("Failed to navigate to URL after waiting: " + Url);
        //                 }
        //             }
        //         }
        //         catch (OperationCanceledException)
        //         {
        //             Debug.Log("Navigation was cancelled");
        //         }
        //     }
        // }

        // public override void Show()
        // {
        //     base.Show();
        //     if (isInitialized && IsWebViewReady())
        //     {
        //         ShowWebView();
        //     }
        // }

        // public override void Destroy()
        // {
        //     Debug.Log("Destroying WebWindow");

        //     // キャンセレーションを発行
        //     cancellationTokenSource?.Cancel();
        //     cancellationTokenSource?.Dispose();

        //     if (isInitialized)
        //     {
        //         ShutdownWebView();
        //         isInitialized = false;
        //     }
        //     base.Destroy();
        // }

        // // WebView2の状態をチェック
        // public bool IsReady()
        // {
        //     return isInitialized && IsWebViewReady();
        // }

        // // URLを設定して初期化完了を待つ（UniTask版）
        // public async UniTask SetUrlAsync(string url, CancellationToken cancellationToken = default)
        // {
        //     if (string.IsNullOrEmpty(url))
        //     {
        //         Debug.LogWarning("URL is empty or null");
        //         return;
        //     }

        //     Url = url;

        //     if (!isInitialized || !IsWebViewReady())
        //     {
        //         Debug.Log("Waiting for WebView2 to be ready...");
        //         await WaitForWebViewReady(cancellationToken);
        //     }

        //     if (IsWebViewReady())
        //     {
        //         bool success = NavigateToUrl(Url);
        //         if (success)
        //         {
        //             Debug.Log("Successfully navigated to URL: " + Url);
        //         }
        //         else
        //         {
        //             Debug.LogError("Failed to navigate to URL: " + Url);
        //             throw new InvalidOperationException($"Failed to navigate to URL: {Url}");
        //         }
        //     }
        //     else
        //     {
        //         throw new TimeoutException("WebView2 initialization timed out");
        //     }
        // }

        // private string GetWebViewErrorMessage()
        // {
        //     int length = GetLastErrorMessageLength();
        //     if (length <= 0) return "Unknown error";
        //     var sb = new StringBuilder(length);
        //     GetLastErrorMessage(sb, length);
        //     return sb.ToString();
        // }
    }
}