using System;

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
        }

        public override void Show()
        {
            base.Show();
        }
    }
}
