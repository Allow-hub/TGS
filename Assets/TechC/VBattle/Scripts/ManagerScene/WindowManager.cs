using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace TechC
{
    public class WindowManager : Singleton<WindowManager>
    {
        [SerializeField, ReadOnly] private const string LOGTAG = "window"; 

        #region 定数と列挙型

        // Win32 API定数
        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;
        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_INFO = 0x00000010;
        private const int WM_USER = 0x0400;
        private const int WM_TRAYICON = WM_USER + 200;
        private const int NOTIFYICON_VERSION = 4;

        // トレイアイコンメッセージ
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;

        // 独自のウィンドウ用の定数
        private const string WINDOW_CLASS_NAME = "UnityTrayIconHelperClass";
        private const string WINDOW_NAME = "UnityTrayIconHelper";
        private const uint WS_OVERLAPPED = 0x00000000;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const int IDI_APPLICATION = 32512;
        private const int GWLP_WNDPROC = -4;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        // メニュー関連の定数
        private const uint MF_BYPOSITION = 0x00000400;
        private const uint MF_STRING = 0x00000000;
        private const uint MF_SEPARATOR = 0x00000800;
        private const uint TPM_LEFTALIGN = 0x0000;
        private const uint TPM_RIGHTBUTTON = 0x0002;
        private const uint WM_COMMAND = 0x0111;

        // メニューアイテムID
        private const uint IDM_EXIT = 1000;
        private const uint IDM_HELLO = 1001;

        #endregion

        #region 構造体定義

        // 通知アイコンのデータ構造体 (Shell32.dll Ver.6.0以降)
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public int uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }

        // ウィンドウクラス構造体
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WNDCLASSEX
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

        // マウスカーソル位置構造体
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        #endregion

        #region デリゲートとイベント

        // ウィンドウメッセージ処理用デリゲート
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // イベントハンドラー
        public delegate void TrayIconClickHandler();
        public event TrayIconClickHandler OnLeftClick;
        public event TrayIconClickHandler OnRightClick;
        public event TrayIconClickHandler OnDoubleClick;

        #endregion

        #region Win32 API インポート

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA pnid);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
          uint dwExStyle, string lpClassName, string lpWindowName,
          uint dwStyle, int x, int y, int nWidth, int nHeight,
          IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WndProcDelegate newProc);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // コンテキストメニュー関連
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool InsertMenu(IntPtr hMenu, uint uPosition, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32.dll")]
        private static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        private static extern int GetCursorPos(ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern int PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region メンバー変数
        private float speed =100;
        private float elapsedTime =0;

        // インスタンス変数
        private IntPtr hWnd = IntPtr.Zero;
        private IntPtr hIcon = IntPtr.Zero;
        private NOTIFYICONDATA nid;
        private bool isInitialized = false;
        private WndProcDelegate wndProcDelegate;
        private IntPtr defaultWndProc = IntPtr.Zero;

        private List<IntPtr> windows;

        #endregion

        #region 初期化と終了処理

        /// <summary>
        /// シングルトンの初期化
        /// </summary>
        protected override void Init()
        {
            base.Init();

            string iconPath = Path.Combine(Application.streamingAssetsPath, "icon.ico");

            // Windows用にパス区切りを \\ に変換
            iconPath = iconPath.Replace('/', '\\'); 
            string tooltipText = "V-LinkBattle"; // ツールチップテキスト
            CreateNotificationIcon(iconPath, tooltipText);
            // var w = WindowUtility.CreateWebWindow("https://www.youtube.com/");
            // windows.Add(w);
            // SpawnWindows(100);
        }
        void SpawnWindows(int count)
        {
            const int WindowWidth = 300;
            const int WindowHeight = 300;
            System.Random rand = new System.Random();
            windows =new List<IntPtr>();

            // 画面のサイズ取得
            int screenWidth = Screen.currentResolution.width;
            int screenHeight = Screen.currentResolution.height;

            for (int i = 0; i < count; i++)
            {
                int x = rand.Next(0, screenWidth - WindowWidth);
                int y = rand.Next(0, screenHeight - WindowHeight);

                IntPtr hWnd = WindowUtility.CreateWindow(
                    "STATIC",
                    $"Window {i + 1}",
                    WS_OVERLAPPEDWINDOW, // or WS_POPUP | WS_VISIBLE
                    0,
                    x, y,
                    WindowWidth, WindowHeight,
                    IntPtr.Zero
                );

                if (hWnd != IntPtr.Zero)
                {
                    windows.Add(hWnd);
                    WindowUtility.ShowWindowHandle(hWnd);
                }
            }
        }
            private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;


        /// <summary>
        /// コンポーネントが破棄されるときに呼ばれるUnityイベント
        /// </summary>
        private void OnDestroy()
        {
            // foreach (var w in windows)
            // {
            //     Debug.Log(windows.Count);

            //     WindowUtility.DestroyWindowHandle(w);
            // }
            // アイコンとウィンドウの破棄
            RemoveNotificationIcon();
        }

        /// <summary>
        /// アプリケーション終了時に呼ばれるUnityイベント
        /// </summary>
        private void OnApplicationQuit()
        {
            // アプリケーション終了時の処理
            RemoveNotificationIcon();
        }

        #endregion

        #region 通知アイコン処理

        /// <summary>
        /// 通知アイコンを作成します
        /// </summary>
        /// <param name="iconPath">カスタムアイコンのパス（省略可）</param>
        /// <param name="tooltipText">ツールチップテキスト（省略可）</param>
        public void CreateNotificationIcon(string iconPath = null, string tooltipText = "Unity アプリケーション")
        {
            try
            {
                // 既に初期化済みなら何もしない
                if (isInitialized)
                {
                    CustomLogger.Info("通知アイコンは既に初期化済みです", LOGTAG);
                    return;
                }

                CustomLogger.Info("通知アイコンの作成を開始します", LOGTAG);

                // 独自の非表示ウィンドウを作成（通知アイコンのメッセージ受信用）
                CreateHelperWindow();

                if (hWnd == IntPtr.Zero)
                {
                    CustomLogger.Error("ヘルパーウィンドウの作成に失敗しました。エラーコード: " + GetLastError(), LOGTAG);
                    return;
                }

                CustomLogger.Info("ヘルパーウィンドウを作成しました: " + hWnd, LOGTAG);

                // アイコンを読み込み
                if (string.IsNullOrEmpty(iconPath))
                {
                    // システムアイコンを使用
                    hIcon = LoadIcon(IntPtr.Zero, new IntPtr(IDI_APPLICATION));
                }
                else
                {
                    // カスタムアイコンを読み込み
                    hIcon = LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE);
                }

                if (hIcon == IntPtr.Zero)
                {
                    CustomLogger.Error("アイコンの読み込みに失敗しました。エラーコード: " + GetLastError(), LOGTAG);

                    // フォールバック：システムのデフォルトアイコンを使用
                    hIcon = LoadIcon(IntPtr.Zero, new IntPtr(IDI_APPLICATION));
                    if (hIcon == IntPtr.Zero)
                    {
                        CustomLogger.Error("デフォルトアイコンの読み込みにも失敗しました。アイコン作成を中止します。", LOGTAG);
                        return;
                    }
                }

                CustomLogger.Info("アイコンを読み込みました: " + hIcon, LOGTAG);

                // 通知アイコンデータの構造体を初期化
                nid = new NOTIFYICONDATA();
                nid.cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA));
                nid.hWnd = hWnd;
                nid.uID = 1;
                nid.uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE;
                nid.uCallbackMessage = WM_TRAYICON; // トレイアイコンからのメッセージ用ID
                nid.hIcon = hIcon;
                nid.szTip = tooltipText;
                nid.uVersion = NOTIFYICON_VERSION;

                // 通知アイコンの追加
                bool result = Shell_NotifyIcon(NIM_ADD, ref nid);
                uint error = GetLastError();

                CustomLogger.Info("通知アイコン追加の結果: " + result + ", エラーコード: " + error, LOGTAG);

                if (result)
                {
                    isInitialized = true;
                    CustomLogger.Info("通知アイコンが正常に追加されました", LOGTAG);
                }
                else
                {
                    CustomLogger.Error("通知アイコンの追加に失敗しました", LOGTAG);
                    CleanupResources();
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error("通知アイコン作成中に例外が発生しました: " + ex.Message + "\n" + ex.StackTrace, LOGTAG);
                CleanupResources();
            }
        }

        /// <summary>
        /// 通知アイコンを削除します
        /// </summary>
        public void RemoveNotificationIcon()
        {
            try
            {
                if (isInitialized)
                {
                    CustomLogger.Info("通知アイコンを削除します", LOGTAG);
                    Shell_NotifyIcon(NIM_DELETE, ref nid);
                    isInitialized = false;
                }

                CleanupResources();
            }
            catch (Exception ex)
            {
                CustomLogger.Error("通知アイコン削除中に例外が発生しました: " + ex.Message, LOGTAG);
            }
        }

        /// <summary>
        /// リソースのクリーンアップを行います
        /// </summary>
        private void CleanupResources()
        {
            // アイコンハンドルを解放
            if (hIcon != IntPtr.Zero)
            {
                DestroyIcon(hIcon);
                hIcon = IntPtr.Zero;
            }

            // ヘルパーウィンドウを破棄
            if (hWnd != IntPtr.Zero)
            {
                CustomLogger.Info("ヘルパーウィンドウを削除します", LOGTAG);
                DestroyWindow(hWnd);
                hWnd = IntPtr.Zero;
            }
        }

        /// <summary>
        /// アイコンの定期更新を行います
        /// </summary>
        private void RefreshIcon()
        {
            if (!isInitialized) return;

            try
            {
                bool result = Shell_NotifyIcon(NIM_MODIFY, ref nid);
                CustomLogger.Info("アイコン更新結果: " + result, LOGTAG);
            }
            catch (Exception ex)
            {
                CustomLogger.Warning("アイコン更新中にエラー: " + ex.Message, LOGTAG);
            }
        }

        /// <summary>
        /// バルーン通知を表示します
        /// </summary>
        /// <param name="title">通知のタイトル</param>
        /// <param name="message">通知のメッセージ</param>
        public void ShowBalloonNotification(string title, string message)
        {
            if (!isInitialized) return;

            try
            {
                nid.uFlags |= NIF_INFO;
                nid.szInfoTitle = title;
                nid.szInfo = message;
                nid.dwInfoFlags = 0;

                bool result = Shell_NotifyIcon(NIM_MODIFY, ref nid);

                if (!result)
                {
                    CustomLogger.Warning("バルーン通知の表示に失敗しました: " + GetLastError(), LOGTAG);
                }

                // フラグをリセット
                nid.uFlags &= ~NIF_INFO;
            }
            catch (Exception ex)
            {
                CustomLogger.Warning("バルーン通知表示中にエラー: " + ex.Message, LOGTAG);
            }
        }

        #endregion

        #region ヘルパーウィンドウとメッセージ処理

        /// <summary>
        /// 通知アイコンのメッセージ処理用ヘルパーウィンドウを作成します
        /// </summary>
        private void CreateHelperWindow()
        {
            try
            {
                // ウィンドウプロシージャデリゲートを作成
                wndProcDelegate = new WndProcDelegate(WindowProc);

                // 非表示のヘルパーウィンドウ作成
                hWnd = CreateWindowEx(
                    WS_EX_TOOLWINDOW,       // 拡張スタイル: ツールウィンドウ（タスクバーに表示されない）
                    "STATIC",               // 標準のウィンドウクラス
                    WINDOW_NAME,            // ウィンドウ名
                    WS_OVERLAPPED,          // 基本的なウィンドウスタイル
                    0, 0, 0, 0,             // サイズと位置（表示されないので0でOK）
                    IntPtr.Zero,            // 親ウィンドウなし
                    IntPtr.Zero,            // メニューなし
                    GetModuleHandle(null),  // 現在のモジュールのハンドル
                    IntPtr.Zero             // 追加パラメータなし
                );

                if (hWnd != IntPtr.Zero)
                {
                    // ウィンドウプロシージャをサブクラス化
                    defaultWndProc = SetWindowLongPtr(hWnd, GWLP_WNDPROC, wndProcDelegate);
                }
                else
                {
                    CustomLogger.Error("ヘルパーウィンドウの作成に失敗しました: " + GetLastError(), LOGTAG);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error("ヘルパーウィンドウ作成中にエラーが発生しました: " + ex.Message, LOGTAG);
            }
        }

        /// <summary>
        /// ウィンドウプロシージャ（トレイアイコンからのメッセージを処理）
        /// </summary>
        private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            // トレイアイコンからのメッセージを処理
            if (msg == WM_TRAYICON)
            {
                int mouseMsg = lParam.ToInt32();

                switch (mouseMsg)
                {
                    case WM_LBUTTONUP:
                        CustomLogger.Info("トレイアイコンが左クリックされました", LOGTAG);
                        OnLeftClick?.Invoke();
                        break;

                    case WM_RBUTTONUP:
                        CustomLogger.Info("トレイアイコンが右クリックされました", LOGTAG);
                        OnRightClick?.Invoke();

                        // トレイアイコンの右クリックメニューを表示
                        ShowContextMenu(hWnd);
                        break;

                    case WM_LBUTTONDBLCLK:
                        CustomLogger.Info("トレイアイコンがダブルクリックされました", LOGTAG);
                        OnDoubleClick?.Invoke();
                        break;
                }
                return IntPtr.Zero;
            }
            else if (msg == WM_COMMAND)
            {
                // メニューコマンドの処理
                uint cmdId = (uint)(wParam.ToInt32() & 0xFFFF);
                HandleMenuCommand(cmdId);
                return IntPtr.Zero;
            }

            // デフォルトのウィンドウプロシージャにメッセージを転送
            return CallWindowProc(defaultWndProc, hWnd, msg, wParam, lParam);
        }

        #endregion

        #region コンテキストメニュー

        /// <summary>
        /// コンテキストメニューを表示します
        /// </summary>
        private void ShowContextMenu(IntPtr hWnd)
        {
            try
            {
                // ウィンドウを前面に持ってくる
                SetForegroundWindow(hWnd);

                // ポップアップメニューを作成
                IntPtr hMenu = CreatePopupMenu();
                if (hMenu == IntPtr.Zero)
                {
                    CustomLogger.Error("メニューの作成に失敗しました: " + GetLastError(), LOGTAG);
                    return;
                }

                // メニュー項目を追加
                InsertMenu(hMenu, 0, MF_BYPOSITION | MF_STRING, IDM_HELLO, "こんにちは");
                InsertMenu(hMenu, 1, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
                InsertMenu(hMenu, 2, MF_BYPOSITION | MF_STRING, IDM_EXIT, "ゲーム終了");

                // マウスカーソルの位置を取得
                POINT cursorPos = new POINT();
                GetCursorPos(ref cursorPos);

                // メニューを表示
                bool result = TrackPopupMenu(
                    hMenu,
                    TPM_LEFTALIGN | TPM_RIGHTBUTTON,
                    cursorPos.x,
                    cursorPos.y,
                    0,
                    hWnd,
                    IntPtr.Zero
                );

                if (!result)
                {
                    CustomLogger.Warning("メニュー表示に失敗しました: " + GetLastError(), LOGTAG);
                }

                // 使用後はメニューを破棄
                DestroyMenu(hMenu);

                // メニューが表示されたあとにメッセージループをリセット
                PostMessage(hWnd, 0, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                CustomLogger.Error("コンテキストメニュー表示中にエラー: " + ex.Message, LOGTAG);
            }
        }

        /// <summary>
        /// メニューコマンドの処理を行います
        /// </summary>
        private void HandleMenuCommand(uint cmdId)
        {
            try
            {
                switch (cmdId)
                {
                    case IDM_EXIT:
                        CustomLogger.Info("ゲーム終了が選択されました", LOGTAG);

                        // ゲームを終了する処理
                        // 少し遅延させて終了（メニュー処理を完了させるため）
                        Invoke("QuitApplication", 0.1f);
                        break;

                    case IDM_HELLO:
                        CustomLogger.Info("こんにちはが選択されました", LOGTAG);
                        ShowBalloonNotification("メッセージ", "こんにちは！");
                        break;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error("メニュー処理中にエラー: " + ex.Message, LOGTAG);
            }
        }

        /// <summary>
        /// アプリケーションを終了します
        /// </summary>
        private void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Unityライフサイクルメソッド

        /// <summary>
        /// 毎フレーム呼ばれるUnityイベント
        /// </summary>
        private void Update()
        {
            // 一定間隔で通知アイコンの更新を試みる
            if (isInitialized && Time.frameCount % 600 == 0)
            {
                RefreshIcon();
            }
            // foreach (var w in windows)
            // {
            //     Debug.Log(w);
            //     // int centerX = Screen.currentResolution.width / 2;
            //     // int centerY = Screen.currentResolution.height / 2;

            //     // var move = WindowUtility.MoveWindowToTargetPosition(w, centerX, 0, 1000f);
            //     // var reSize = WindowUtility.AnimateResizeWindow(w, 10, Screen.currentResolution.height, 1000f);
            //     WindowUtility.SetWindowToForeground(w);
            //     WindowUtility.MoveWindow(w,speed,"up");
            //     WindowUtility.MoveWindow(w, speed, "right");
            // }

        }

        #endregion
    }
}