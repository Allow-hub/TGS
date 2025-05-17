using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace TechC
{
    /// <summary>
    /// Windowsのウィンドウ操作を簡単に行うためのユーティリティクラス
    /// </summary>
    public static class WindowUtility
    {
        private const string LOGTAG = "WindowUtility";

        #region Win32 API定数

        // ウィンドウスタイル
        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_CAPTION = 0x00C00000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;

        // 拡張ウィンドウスタイル
        public const uint WS_EX_DLGMODALFRAME = 0x00000001;
        public const uint WS_EX_TOPMOST = 0x00000008;
        public const uint WS_EX_ACCEPTFILES = 0x00000010;
        public const uint WS_EX_TRANSPARENT = 0x00000020;
        public const uint WS_EX_TOOLWINDOW = 0x00000080;
        public const uint WS_EX_APPWINDOW = 0x00040000;

        // ShowWindow コマンド
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOW = 5;
        public const int SW_RESTORE = 9;

        // SetWindowPos フラグ
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_DRAWFRAME = SWP_FRAMECHANGED;
        public const uint SWP_ASYNCWINDOWPOS = 0x4000;

        // GetWindow コマンド
        public const uint GW_HWNDNEXT = 2;
        public const uint GW_HWNDPREV = 3;
        public const uint GW_OWNER = 4;
        public const uint GW_CHILD = 5;

        // ウィンドウメッセージ
        public const int WM_CLOSE = 0x0010;
        public const int WM_SETTEXT = 0x000C;
        public const int WM_GETTEXT = 0x000D;

        // 特殊なウィンドウハンドル
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        // ウィンドウ情報フラグ
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const int GWL_WNDPROC = -4;
        public const int GWLP_WNDPROC = -4;
        public const int GWLP_HINSTANCE = -6;
        public const int GWLP_HWNDPARENT = -8;
        public const int GWL_ID = -12;

        #endregion

        #region 構造体定義

        // ウィンドウクラス構造体
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        // ウィンドウ位置情報構造体
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        // ウィンドウ情報構造体
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }

        // ウィンドウ列挙用デリゲート
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion

        #region Win32 API インポート

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, string lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterClassEx(ref WNDCLASSEX lpwcx);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd,int X,int Y,int nWidth,int nHeight,bool bRepaint);


        #endregion

        #region ウィンドウ作成と管理

        /// <summary>
        /// 新しいウィンドウを作成します
        /// </summary>
        /// <param name="className">ウィンドウクラス名（既存のクラスを使用する場合）</param>
        /// <param name="windowName">ウィンドウタイトル</param>
        /// <param name="style">ウィンドウスタイル</param>
        /// <param name="exStyle">拡張ウィンドウスタイル</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="parent">親ウィンドウのハンドル</param>
        /// <returns>作成されたウィンドウのハンドル</returns>
        public static IntPtr CreateWindow(
            string className,
            string windowName,
            uint style = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU,
            uint exStyle = 0,
            int x = 0,
            int y = 0,
            int width = 400,
            int height = 300,
            IntPtr parent = default)
        {
            try
            {
                IntPtr hWnd = CreateWindowEx(
                    exStyle,
                    className,
                    windowName,
                    style,
                    x, y, width, height,
                    parent,
                    IntPtr.Zero,
                    GetModuleHandle(null),
                    IntPtr.Zero
                );

                if (hWnd == IntPtr.Zero)
                {
                    uint error = GetLastError();
                    CustomLogger.Error($"ウィンドウの作成に失敗しました: {error}", LOGTAG);
                }
                else
                {
                    CustomLogger.Info($"ウィンドウを作成しました: {hWnd}", LOGTAG);
                }

                return hWnd;
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウ作成中に例外が発生しました: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        public static IntPtr CreateWebWindow(string url)
        {
            // Chromeを起動して指定されたURLを開く
            Process process = Process.Start("chrome", url);

            // プロセスが起動するまで少し待機
            process.WaitForInputIdle();

            // ウィンドウハンドルを取得
            IntPtr hWnd = process.MainWindowHandle;

            // ウィンドウが正しく取得できたか確認
            if (hWnd != IntPtr.Zero)
            {
                Console.WriteLine($"Chromeウィンドウのハンドル: {hWnd}");
            }
            else
            {
                Console.WriteLine("ウィンドウのハンドルを取得できませんでした");
            }

            return hWnd;
        }
        /// <summary>
        /// 非表示のヘルパーウィンドウを作成します
        /// </summary>
        /// <param name="windowName">ウィンドウ名</param>
        /// <returns>作成されたウィンドウのハンドル</returns>
        public static IntPtr CreateHelperWindow(string windowName = "HelperWindow")
        {
            return CreateWindow(
                "STATIC",          // 標準のSTATICクラスを使用
                windowName,
                WS_OVERLAPPED,     // 最小限のスタイル
                WS_EX_TOOLWINDOW,  // タスクバーに表示されないウィンドウ
                0, 0, 0, 0         // サイズと位置（表示されないので0）
            );
        }

        /// <summary>
        /// ウィンドウを破棄します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功したかどうか</returns>
        public static bool DestroyWindowHandle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                bool result = DestroyWindow(hWnd);
                if (!result)
                {
                    CustomLogger.Warning($"ウィンドウ破棄に失敗: {GetLastError()}", LOGTAG);
                }
                return result;
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウ破棄中に例外が発生: {ex.Message}", LOGTAG);
                return false;
            }
        }

        #endregion

        #region ウィンドウ操作

        /// <summary>
        /// ウィンドウの表示状態を設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="showCommand">表示コマンド（SW_HIDE, SW_SHOW など）</param>
        /// <returns>成功したかどうか</returns>
        public static bool SetWindowVisibility(IntPtr hWnd, int showCommand)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                return ShowWindow(hWnd, showCommand);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウ表示設定中に例外: {ex.Message}", LOGTAG);
                return false;
            }
        }

        /// <summary>
        /// ウィンドウを表示します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功したかどうか</returns>
        public static bool ShowWindowHandle(IntPtr hWnd)
        {
            return SetWindowVisibility(hWnd, SW_SHOW);
        }

        /// <summary>
        /// ウィンドウを非表示にします
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功したかどうか</returns>
        public static bool HideWindowHandle(IntPtr hWnd)
        {
            return SetWindowVisibility(hWnd, SW_HIDE);
        }

        /// <summary>
        /// ウィンドウを最大化します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功したかどうか</returns>
        public static bool MaximizeWindowHandle(IntPtr hWnd)
        {
            return SetWindowVisibility(hWnd, SW_SHOWMAXIMIZED);
        }

        /// <summary>
        /// ウィンドウを最小化します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功したかどうか</returns>
        public static bool MinimizeWindowHandle(IntPtr hWnd)
        {
            return SetWindowVisibility(hWnd, SW_SHOWMINIMIZED);
        }

        /// <summary>
        /// ウィンドウを復元します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功したかどうか</returns>
        public static bool RestoreWindowHandle(IntPtr hWnd)
        {
            return SetWindowVisibility(hWnd, SW_RESTORE);
        }

        /// <summary>
        /// ウィンドウの位置とサイズを設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="flags">フラグ（SWP_NOSIZEなど）</param>
        /// <returns>成功したかどうか</returns>
        public static bool SetWindowPositionAndSize(
            IntPtr hWnd, int x, int y, int width, int height, uint flags = 0)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                return SetWindowPos(hWnd, IntPtr.Zero, x, y, width, height, flags);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウ位置設定中に例外: {ex.Message}", LOGTAG);
                return false;
            }
        }

        /// <summary>
        /// ウィンドウの位置のみを設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <returns>成功したかどうか</returns>
        public static bool SetWindowPosition(IntPtr hWnd, int x, int y)
        {
            return SetWindowPositionAndSize(hWnd, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        /// <summary>
        /// ウィンドウのサイズのみを設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>成功したかどうか</returns>
        public static bool SetWindowSize(IntPtr hWnd, int width, int height)
        {
            return SetWindowPositionAndSize(hWnd, 0, 0, width, height, SWP_NOMOVE | SWP_NOZORDER);
        }

        /// <summary>
        /// ウィンドウを最前面に設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="topMost">常に最前面にするかどうか</param>
        /// <returns>成功したかどうか</returns>
        public static bool SetWindowToForeground(IntPtr hWnd, bool topMost = false)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                IntPtr insertAfter = topMost ? HWND_TOPMOST : HWND_TOP;
                bool result = SetWindowPos(hWnd, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                
                if (result)
                {
                    // アクティブにする
                    SetForegroundWindow(hWnd);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウを前面に設定中に例外: {ex.Message}", LOGTAG);
                return false;
            }
        }

        /// <summary>
        /// ウィンドウ移動
        /// </summary>
        /// <param name="hWnd">移動する対象のウィンドウハンドル</param>
        /// <param name="x">ウィンドウの新しい左上隅のX座標（スクリーン座標）</param>
        /// <param name="y">ウィンドウの新しい左上隅のY座標（スクリーン座標）</param>
        /// <param name="width">ウィンドウの新しい幅（ピクセル単位）</param>
        /// <param name="height">ウィンドウの新しい高さ（ピクセル単位）</param>
        /// <param name="repaint">ウィンドウを再描画するかどうか（true: 再描画する、false: 再描画しない）</param>
        /// <returns>ウィンドウの移動に成功したかどうか（true: 成功、false: 失敗）</returns>
        public static bool MoveWindowHandle(IntPtr hWnd, int x, int y, int width, int height, bool repaint = true)
        {
            if (hWnd == IntPtr.Zero) return false;
            return MoveWindow(hWnd, x, y, width, height, repaint);
        }

        /// <summary>
        /// 一定方向への移動
        /// </summary>
        /// <param name="windowHandle">ウィンドウ</param>
        /// <param name="speed">速度</param>
        /// <param name="direction">方向"up","down","left","right"</param>
        public static void MoveWindow(IntPtr windowHandle, float speed, string direction)
        {
            // ウィンドウの矩形情報を取得
            var rect = GetWindowRect(windowHandle);

            // 幅と高さを計算
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // 新しい位置を計算
            int newX = rect.Left;
            int newY = rect.Top;

            // 指定された方向に基づいて移動する距離を計算
            switch (direction.ToLower())
            {
                case "up":
                    newY = rect.Top - Mathf.RoundToInt(speed * Time.deltaTime); // 上に移動
                    break;
                case "down":
                    newY = rect.Top + Mathf.RoundToInt(speed * Time.deltaTime); // 下に移動
                    break;
                case "left":
                    newX = rect.Left - Mathf.RoundToInt(speed * Time.deltaTime); // 左に移動
                    break;
                case "right":
                    newX = rect.Left + Mathf.RoundToInt(speed * Time.deltaTime); // 右に移動
                    break;
                default:
                    throw new ArgumentException("Invalid direction. Use 'up', 'down', 'left', or 'right'.");
            }

            // ウィンドウを新しい位置に移動
            MoveWindowHandle(windowHandle, newX, newY, width, height);
        }

        /// <summary>
        /// 指定したスクリーン座標にウィンドウを移動します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="screenX">スクリーン座標のX</param>
        /// <param name="screenY">スクリーン座標のY</param>
        /// <param name="repaint">再描画するか</param>
        /// <returns>成功したかどうか</returns>
        public static bool MoveWindowToScreenPosition(IntPtr hWnd, int screenX, int screenY, bool repaint = true)
        {
            if (hWnd == IntPtr.Zero) return false;

            var rect = GetWindowRect(hWnd);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            return MoveWindow(hWnd, screenX, screenY, width, height, repaint);
        }

        /// <summary>
        /// ウィンドウを一定速度で目標位置に向かって移動させます（1フレーム呼び出し用）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="targetX">目標X座標</param>
        /// <param name="targetY">目標Y座標</param>
        /// <param name="speed">速度（ピクセル/秒）</param>
        /// <returns>目標位置に到達したか</returns>
        public static bool MoveWindowToTargetPosition(
            IntPtr hWnd,
            int targetX,
            int targetY,
            float speed)
        {
            if (hWnd == IntPtr.Zero) return false;

            var rect = GetWindowRect(hWnd);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            Vector2 currentPos = new Vector2(rect.Left, rect.Top);
            Vector2 targetPos = new Vector2(targetX, targetY);

            // 目標位置に到達した場合
            if (Vector2.Distance(currentPos, targetPos) < 0.5f)
            {
                // 最終位置補正
                MoveWindowHandle(hWnd, targetX, targetY, width, height);
                return true;
            }

            // 毎フレーム一定距離だけ移動
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);
            MoveWindowHandle(hWnd, (int)newPos.x, (int)newPos.y, width, height);
            return false;
        }

        /// <summary>
        /// ウィンドウのサイズ変更をアニメーション的に行います（1フレーム呼び出し用）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="targetWidth">目標幅</param>
        /// <param name="targetHeight">目標高さ</param>
        /// <param name="speed">速度（ピクセル/秒）</param>
        /// <returns>目標サイズに到達したか</returns>
        public static bool AnimateResizeWindow(
            IntPtr hWnd,
            int targetWidth,
            int targetHeight,
            float speed)
        {
            if (hWnd == IntPtr.Zero) return false;

            var rect = GetWindowRect(hWnd);
            int currentWidth = rect.Right - rect.Left;
            int currentHeight = rect.Bottom - rect.Top;

            // 目標サイズに到達した場合
            if (Mathf.Abs(currentWidth - targetWidth) < 1 && Mathf.Abs(currentHeight - targetHeight) < 1)
            {
                // 最終位置補正
                MoveWindowHandle(hWnd, rect.Left, rect.Top, targetWidth, targetHeight);
                return true;
            }

            // 幅と高さを一定距離だけ変更
            int newWidth = Mathf.RoundToInt(Mathf.MoveTowards(currentWidth, targetWidth, speed * Time.deltaTime));
            int newHeight = Mathf.RoundToInt(Mathf.MoveTowards(currentHeight, targetHeight, speed * Time.deltaTime));

            // ウィンドウを新しいサイズに変更
            MoveWindowHandle(hWnd, rect.Left, rect.Top, newWidth, newHeight);
            return false;
        }




        #endregion

        #region ウィンドウプロパティ取得

        /// <summary>
        /// ウィンドウのタイトルを取得します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>ウィンドウのタイトル</returns>
        public static string GetWindowTitle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return string.Empty;

            try
            {
                int length = GetWindowTextLength(hWnd);
                if (length == 0) return string.Empty;

                StringBuilder sb = new StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウタイトル取得中に例外: {ex.Message}", LOGTAG);
                return string.Empty;
            }
        }

        /// <summary>
        /// ウィンドウのタイトルを設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="title">新しいタイトル</param>
        /// <returns>成功したかどうか</returns>
        public static bool SetWindowTitle(IntPtr hWnd, string title)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                return SendMessage(hWnd, WM_SETTEXT, 0, title) != 0;
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウタイトル設定中に例外: {ex.Message}", LOGTAG);
                return false;
            }
        }

        /// <summary>
        /// ウィンドウのスタイルを取得します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>ウィンドウスタイル</returns>
        public static uint GetWindowStyle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return 0;

            try
            {
                return (uint)GetWindowLongPtr(hWnd, (int)GWL_STYLE).ToInt64();
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウスタイル取得中に例外: {ex.Message}", LOGTAG);
                return 0;
            }
        }

        /// <summary>
        /// ウィンドウの拡張スタイルを取得します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>拡張ウィンドウスタイル</returns>
        public static uint GetWindowExStyle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return 0;

            try
            {
                return (uint)GetWindowLongPtr(hWnd, (int)GWL_EXSTYLE).ToInt64();
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"拡張ウィンドウスタイル取得中に例外: {ex.Message}", LOGTAG);
                return 0;
            }
        }

        /// <summary>
        /// ウィンドウのスタイルを設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="style">新しいウィンドウスタイル</param>
        /// <returns>以前のスタイル</returns>
        public static IntPtr SetWindowStyle(IntPtr hWnd, uint style)
        {
            if (hWnd == IntPtr.Zero) return IntPtr.Zero;

            try
            {
                return SetWindowLongPtr(hWnd, (int)GWL_STYLE, new IntPtr(style));
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウスタイル設定中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// ウィンドウの拡張スタイルを設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="exStyle">新しい拡張ウィンドウスタイル</param>
        /// <returns>以前の拡張スタイル</returns>
        public static IntPtr SetWindowExStyle(IntPtr hWnd, uint exStyle)
        {
            if (hWnd == IntPtr.Zero) return IntPtr.Zero;

            try
            {
                return SetWindowLongPtr(hWnd, (int)GWL_EXSTYLE, new IntPtr(exStyle));
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"拡張ウィンドウスタイル設定中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// ウィンドウの座標を取得します（スクリーン座標系）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>ウィンドウの座標を表すRECT構造体</returns>
        public static RECT GetWindowRect(IntPtr hWnd)
        {
            RECT rect = new RECT();
            if (hWnd != IntPtr.Zero)
            {
                try
                {
                    GetWindowRect(hWnd, out rect);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error($"ウィンドウ座標取得中に例外: {ex.Message}", LOGTAG);
                }
            }
            return rect;
        }

        /// <summary>
        /// ウィンドウのクライアント領域サイズを取得します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>クライアント領域のサイズを表すRECT構造体</returns>
        public static RECT GetClientRect(IntPtr hWnd)
        {
            RECT rect = new RECT();
            if (hWnd != IntPtr.Zero)
            {
                try
                {
                    GetClientRect(hWnd, out rect);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error($"クライアント領域取得中に例外: {ex.Message}", LOGTAG);
                }
            }
            return rect;
        }
        
        #endregion

        #region ウィンドウ検索

        /// <summary>
        /// ウィンドウを名前（タイトル）で検索します
        /// </summary>
        /// <param name="windowName">ウィンドウ名（タイトル）</param>
        /// <param name="className">クラス名（省略可）</param>
        /// <returns>見つかったウィンドウのハンドル</returns>
        public static IntPtr FindWindowByName(string windowName, string className = null)
        {
            try
            {
                return FindWindow(className, windowName);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウ検索中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 子ウィンドウを検索します
        /// </summary>
        /// <param name="parentWindow">親ウィンドウハンドル</param>
        /// <param name="windowName">子ウィンドウ名（タイトル）</param>
        /// <param name="className">クラス名（省略可）</param>
        /// <returns>見つかった子ウィンドウのハンドル</returns>
        public static IntPtr FindChildWindow(IntPtr parentWindow, string windowName = null, string className = null)
        {
            if (parentWindow == IntPtr.Zero) return IntPtr.Zero;

            try
            {
                return FindWindowEx(parentWindow, IntPtr.Zero, className, windowName);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"子ウィンドウ検索中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// すべてのトップレベルウィンドウを列挙します
        /// </summary>
        /// <returns>ウィンドウハンドルのリスト</returns>
        public static List<IntPtr> EnumerateWindows()
        {
            List<IntPtr> windowHandles = new List<IntPtr>();
            
            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    windowHandles.Add(hWnd);
                    return true; // 列挙を続ける
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウ列挙中に例外: {ex.Message}", LOGTAG);
            }
            
            return windowHandles;
        }
        
        /// <summary>
        /// 表示されているトップレベルウィンドウのみを列挙します
        /// </summary>
        /// <returns>表示されているウィンドウハンドルのリスト</returns>
        public static List<IntPtr> EnumerateVisibleWindows()
        {
            List<IntPtr> windowHandles = new List<IntPtr>();
            
            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd))
                    {
                        windowHandles.Add(hWnd);
                    }
                    return true; // 列挙を続ける
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"表示ウィンドウ列挙中に例外: {ex.Message}", LOGTAG);
            }
            
            return windowHandles;
        }
        
        /// <summary>
        /// 特定の条件に一致するウィンドウを列挙します
        /// </summary>
        /// <param name="filter">フィルター関数（ウィンドウハンドルを受け取り、trueを返すと結果に含まれる）</param>
        /// <returns>条件に一致するウィンドウハンドルのリスト</returns>
        public static List<IntPtr> EnumerateWindowsWithFilter(Func<IntPtr, bool> filter)
        {
            List<IntPtr> windowHandles = new List<IntPtr>();
            
            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (filter(hWnd))
                    {
                        windowHandles.Add(hWnd);
                    }
                    return true; // 列挙を続ける
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"条件付きウィンドウ列挙中に例外: {ex.Message}", LOGTAG);
            }
            
            return windowHandles;
        }

        #endregion

        #region ウィンドウメッセージ送信

        /// <summary>
        /// ウィンドウにメッセージを送信します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="msg">メッセージID</param>
        /// <param name="wParam">wParam</param>
        /// <param name="lParam">lParam</param>
        /// <returns>メッセージ処理の結果</returns>
        public static int SendWindowMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (hWnd == IntPtr.Zero) return 0;

            try
            {
                return SendMessage(hWnd, msg, wParam, lParam);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"メッセージ送信中に例外: {ex.Message}", LOGTAG);
                return 0;
            }
        }

        /// <summary>
        /// ウィンドウを閉じるメッセージを送信します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功したかどうか</returns>
        public static bool CloseWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                return SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero) == 0;
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウを閉じる際に例外: {ex.Message}", LOGTAG);
                return false;
            }
        }

        /// <summary>
        /// ウィンドウからテキストを取得します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="maxLength">取得するテキストの最大長</param>
        /// <returns>ウィンドウのテキスト</returns>
        public static string GetWindowText(IntPtr hWnd, int maxLength = 1024)
        {
            if (hWnd == IntPtr.Zero) return string.Empty;

            try
            {
                StringBuilder sb = new StringBuilder(maxLength);
                SendMessage(hWnd, WM_GETTEXT, new IntPtr(maxLength), sb);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウテキスト取得中に例外: {ex.Message}", LOGTAG);
                return string.Empty;
            }
        }

        /// <summary>
        /// ウィンドウにテキストを設定します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="text">設定するテキスト</param>
        /// <returns>成功したかどうか</returns>
        public static bool SetWindowText(IntPtr hWnd, string text)
        {
            if (hWnd == IntPtr.Zero) return false;

            try
            {
                return SendMessage(hWnd, WM_SETTEXT, IntPtr.Zero, IntPtr.Zero) != 0;
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"ウィンドウテキスト設定中に例外: {ex.Message}", LOGTAG);
                return false;
            }
        }

        #endregion

        #region ユーティリティ関数

        /// <summary>
        /// ウィンドウが有効かどうかを確認します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>有効な場合はtrue</returns>
        public static bool IsValidWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;
            
            try
            {
                WINDOWINFO winInfo = new WINDOWINFO();
                winInfo.cbSize = (uint)Marshal.SizeOf(typeof(WINDOWINFO));
                return GetWindowInfo(hWnd, ref winInfo);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ウィンドウが表示されているかを確認します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>表示されている場合はtrue</returns>
        public static bool IsWindowVisibleState(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;
            
            try
            {
                return IsWindowVisible(hWnd);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// クライアント座標をスクリーン座標に変換します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="clientX">クライアントX座標</param>
        /// <param name="clientY">クライアントY座標</param>
        /// <returns>スクリーン座標のPoint構造体</returns>
        public static System.Drawing.Point ClientToScreenPoint(IntPtr hWnd, int clientX, int clientY)
        {
            System.Drawing.Point point = new System.Drawing.Point(clientX, clientY);
            
            if (hWnd != IntPtr.Zero)
            {
                try
                {
                    ClientToScreen(hWnd, ref point);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error($"座標変換中に例外: {ex.Message}", LOGTAG);
                }
            }
            
            return point;
        }

        /// <summary>
        /// スクリーン座標をクライアント座標に変換します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="screenX">スクリーンX座標</param>
        /// <param name="screenY">スクリーンY座標</param>
        /// <returns>クライアント座標のPoint構造体</returns>
        public static System.Drawing.Point ScreenToClientPoint(IntPtr hWnd, int screenX, int screenY)
        {
            System.Drawing.Point point = new System.Drawing.Point(screenX, screenY);
            
            if (hWnd != IntPtr.Zero)
            {
                try
                {
                    ScreenToClient(hWnd, ref point);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error($"座標変換中に例外: {ex.Message}", LOGTAG);
                }
            }
            
            return point;
        }

        /// <summary>
        /// 子ウィンドウを取得します
        /// </summary>
        /// <param name="hWnd">親ウィンドウハンドル</param>
        /// <returns>最初の子ウィンドウのハンドル</returns>
        public static IntPtr GetChildWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return IntPtr.Zero;
            
            try
            {
                return GetWindow(hWnd, GW_CHILD);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"子ウィンドウ取得中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 次のウィンドウを取得します（Zオーダー順）
        /// </summary>
        /// <param name="hWnd">基準ウィンドウハンドル</param>
        /// <returns>次のウィンドウのハンドル</returns>
        public static IntPtr GetNextWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return IntPtr.Zero;
            
            try
            {
                return GetWindow(hWnd, GW_HWNDNEXT);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"次ウィンドウ取得中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 前のウィンドウを取得します（Zオーダー順）
        /// </summary>
        /// <param name="hWnd">基準ウィンドウハンドル</param>
        /// <returns>前のウィンドウのハンドル</returns>
        public static IntPtr GetPreviousWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return IntPtr.Zero;
            
            try
            {
                return GetWindow(hWnd, GW_HWNDPREV);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"前ウィンドウ取得中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// オーナーウィンドウを取得します
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>オーナーウィンドウのハンドル</returns>
        public static IntPtr GetOwnerWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return IntPtr.Zero;
            
            try
            {
                return GetWindow(hWnd, GW_OWNER);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"オーナーウィンドウ取得中に例外: {ex.Message}", LOGTAG);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// すべての子ウィンドウを列挙します
        /// </summary>
        /// <param name="parentHwnd">親ウィンドウハンドル</param>
        /// <returns>子ウィンドウハンドルのリスト</returns>
        public static List<IntPtr> EnumerateChildWindows(IntPtr parentHwnd)
        {
            List<IntPtr> childHandles = new List<IntPtr>();
            
            if (parentHwnd == IntPtr.Zero) return childHandles;
            
            // 最初の子ウィンドウを取得
            IntPtr childHwnd = GetChildWindow(parentHwnd);
            
            while (childHwnd != IntPtr.Zero)
            {
                childHandles.Add(childHwnd);
                
                // 次の兄弟ウィンドウを取得
                childHwnd = GetNextWindow(childHwnd);
            }
            
            return childHandles;
        }

        #endregion
    }
}