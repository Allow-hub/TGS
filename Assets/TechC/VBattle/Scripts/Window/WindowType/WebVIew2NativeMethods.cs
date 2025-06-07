using System;
using System.Runtime.InteropServices;

namespace TechC
{
    /// <summary>
    /// WebView2のDLLインポートメソッドを定義するクラス。
    /// </summary>
    internal static class WebView2NativeMethods
    {
        [DllImport("TestDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitWebView2(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string urlOrHtml);
    }
}
