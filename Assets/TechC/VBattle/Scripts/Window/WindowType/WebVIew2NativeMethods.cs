using System.IO.Pipes;
using System.IO;

namespace TechC
{
    /// <summary>
    /// WebView2のDLLインポートメソッドを定義するクラス。
    /// </summary>
    internal static class WebView2NativeMethods
    {
        public static void SendUrlToWebView2(string url)
        {
            using (var pipe = new NamedPipeClientStream(".", "WebView2Pipe", PipeDirection.Out))
            {
                pipe.Connect(1000); // 1秒待つ
                using (var writer = new StreamWriter(pipe))
                {
                    writer.WriteLine(url);
                    writer.Flush();
                }
            }
        }
    }
}
