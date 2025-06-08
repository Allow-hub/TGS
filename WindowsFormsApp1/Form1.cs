using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class webView2 : Form
    {
        public webView2()
        {
            InitializeComponent();
            StartPipeServer();
        }

        private void webView_Load(object sender, EventArgs e)
        {

        }

        // URLをWebView2に設定して読み込むメソッド
        public void LoadUrl(string url)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => webView21.Source = new Uri(url)));
            }
            else
            {
                webView21.Source = new Uri(url);
            }
        }

        // NamedPipeサーバでUnityからのURLコマンドを受け取る
        private void StartPipeServer()
        {
            Task.Run(() =>
            {
                using (var pipe = new NamedPipeServerStream("WebView2Pipe", PipeDirection.In))
                using (var reader = new StreamReader(pipe))
                {
                    while (true)
                    {
                        pipe.WaitForConnection();
                        string url = reader.ReadLine();
                        if (!string.IsNullOrEmpty(url))
                        {
                            LoadUrl(url);
                        }
                        pipe.Disconnect();
                    }
                }
            });
        }
    }
}
