using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    public class IconManager : Singleton<IconManager>
    {
        [SerializeField, ReadOnly] private const string LOGTAG = "window";

        // メニューアイテムID
        private const uint IDM_EXIT = 0;
        private const uint IDM_HELLO = 1;
        private const uint WM_TRAYICON = PInvoke.WM_APP + 1;
        // private const uint WM_COMMAND = PInvoke.WM_APP + 2;



        // ウィンドウメッセージ処理用デリゲート
        private delegate LRESULT WNDPROC(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);


        #region メンバー変数

        // インスタンス変数
        private HWND hWnd;
        private HICON hIcon;
        private NOTIFYICONDATAW nid;
        private bool isInitialized = false;
        private static WNDPROC wndProc;

        #endregion

        /// <summary>
        /// シングルトンの初期化
        /// </summary>
        protected override void Init()
        {
            base.Init();

            // string iconPath = Path.Combine(Application.streamingAssetsPath, "icon.ico");

            // Windows用にパス区切りを \\ に変換
            // iconPath = iconPath.Replace('/', '\\');
            string tooltipText = "V-LinkBattle"; // ツールチップテキスト
            CreateNotificationIcon(tooltipText);
        }

        /// <summary>
        /// アプリケーション終了時に呼ばれるUnityイベント
        /// </summary>
        private void OnApplicationQuit()
        {
            // アプリケーション終了時の処理
            RemoveNotificationIcon();
        }

        /// <summary>
        /// 通知アイコンを作成します
        /// </summary>
        /// <param name="iconPath">カスタムアイコンのパス（省略可）</param>
        /// <param name="tooltipText">ツールチップテキスト（省略可）</param>
        public void CreateNotificationIcon(string tooltipText = "Unity アプリケーション")
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
                    CustomLogger.Error("ヘルパーウィンドウの作成に失敗しました。エラーコード: ", LOGTAG);
                    return;
                }

                CustomLogger.Info("ヘルパーウィンドウを作成しました: " + hWnd, LOGTAG);
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                unsafe
                {
                    fixed (char* exePathPtr = exePath)
                    {
                        PCWSTR path = new PCWSTR(exePathPtr);
                        //第三引数は現在のアプリを取りたい場合の0
                        hIcon = PInvoke.ExtractIcon(HINSTANCE.Null, path, 0);
                    }
                }

                if (hIcon == IntPtr.Zero)
                {
                    CustomLogger.Error("アイコンの読み込みに失敗しました。エラーコード: " + LOGTAG);
                    return;
                }

                CustomLogger.Info("アイコンを読み込みました: " + hIcon, LOGTAG);

                // 通知アイコンデータの構造体を初期化
                nid = new NOTIFYICONDATAW
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
                    hWnd = hWnd,
                    uID = 1,
                    uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
                    uCallbackMessage = WM_TRAYICON, // 例: WM_APP + 1
                    hIcon = hIcon, // ExtractIcon などで取得済みのアイコン
                    szTip = "アプリケーション名"
                };
                // 通知アイコンの追加
                bool result = PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, in nid);
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
            if (isInitialized)
            {
                CustomLogger.Info("通知アイコンを削除します", LOGTAG);
                if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, in nid))
                    CustomLogger.Info("通知アイコンの削除に失敗しました", LOGTAG);
                isInitialized = false;
            }
            CleanupResources();
        }

        /// <summary>
        /// リソースのクリーンアップを行います
        /// </summary>
        private void CleanupResources()
        {
            // アイコンハンドルを解放
            if (hIcon != IntPtr.Zero)
            {
                PInvoke.DestroyIcon((HICON)hIcon);
                hIcon = HICON.Null;
            }

            // ヘルパーウィンドウを破棄
            if (hWnd != IntPtr.Zero)
            {
                CustomLogger.Info("ヘルパーウィンドウを削除します", LOGTAG);
                WindowUtility.DestroyWindowHandle(hWnd);
                hWnd = HWND.Null;
            }
        }

        /// <summary>
        /// バルーン通知を表示します
        /// </summary>
        /// <param name="title">通知のタイトル</param>
        /// <param name="message">通知のメッセージ</param>
        // public void ShowBalloonNotification(string title, string message)
        // {
        //     if (!isInitialized) return;

        //     try
        //     {
        //         nid.uFlags |= NIF_INFO;
        //         nid.szInfoTitle = title;
        //         nid.szInfo = message;
        //         nid.dwInfoFlags = 0;

        //         bool result = Shell_NotifyIcon(NIM_MODIFY, ref nid);

        //         if (!result)
        //         {
        //             CustomLogger.Warning("バルーン通知の表示に失敗しました: " +  LOGTAG);
        //         }

        //         // フラグをリセット
        //         nid.uFlags &= ~NIF_INFO;
        //     }
        //     catch (Exception ex)
        //     {
        //         CustomLogger.Warning("バルーン通知表示中にエラー: " + ex.Message, LOGTAG);
        //     }
        // }


        /// <summary>
        /// 通知アイコンのメッセージ処理用ヘルパーウィンドウを作成します
        /// </summary>
        private void CreateHelperWindow()
        {
            // ウィンドウプロシージャデリゲートを作成
            wndProc = new WNDPROC(WindowProc);
            unsafe
            {
                fixed (char* classNamePtr = "STATIC")
                fixed (char* windowNamePtr = "MyHelperWindow")
                {
                    hWnd = PInvoke.CreateWindowEx(
                       WINDOW_EX_STYLE.WS_EX_TOOLWINDOW,
                       (PCWSTR)classNamePtr,
                       (PCWSTR)windowNamePtr,
                       WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                       0, 0, 0, 0,
                       default,          // 親ウィンドウなし
                       default,          // メニューなし
                       PInvoke.GetModuleHandle(new PCWSTR(null)),  // hInstance（自身のモジュール）
                       null              // lpParam
                   );

                    if (hWnd == default)
                    {
                        int error = Marshal.GetLastWin32Error();
                        Console.WriteLine($"CreateWindowEx failed with error: {error}");
                    }
                }
            }
        }



        /// <summary>
        /// ウィンドウプロシージャ（トレイアイコンからのメッセージを処理）
        /// </summary>
        private LRESULT WindowProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            // トレイアイコンからのメッセージを処理
            if (msg == WM_TRAYICON)
            {
                int mouseMsg = (int)lParam;

                switch (mouseMsg)
                {
                    case (int)PInvoke.WM_LBUTTONDOWN:
                        CustomLogger.Info("トレイアイコンが左クリックされました", LOGTAG);
                        break;

                    case (int)PInvoke.WM_RBUTTONDOWN:
                        CustomLogger.Info("トレイアイコンが右クリックされました", LOGTAG);
                        // トレイアイコンの右クリックメニューを表示
                        // ShowContextMenu(hWnd);
                        break;
                }
                return new LRESULT(0);
            }
            else if (msg == PInvoke.WM_COMMAND)
            {
                // メニューコマンドの処理
                uint cmdId = (uint)(wParam);
                // HandleMenuCommand(cmdId);
                return new LRESULT();
            }

            // デフォルトのウィンドウプロシージャにメッセージを転送
            return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        //         #endregion

        //         #region コンテキストメニュー

        //         /// <summary>
        //         /// コンテキストメニューを表示します
        //         /// </summary>
        //         private void ShowContextMenu(IntPtr hWnd)
        //         {
        //             try
        //             {
        //                 // ウィンドウを前面に持ってくる
        //                 SetForegroundWindow(hWnd);

        //                 // ポップアップメニューを作成
        //                 IntPtr hMenu = CreatePopupMenu();
        //                 if (hMenu == IntPtr.Zero)
        //                 {
        //                     CustomLogger.Error("メニューの作成に失敗しました: " +  LOGTAG);
        //                     return;
        //                 }

        //                 // メニュー項目を追加
        //                 InsertMenu(hMenu, 0, MF_BYPOSITION | MF_STRING, IDM_HELLO, "こんにちは");
        //                 InsertMenu(hMenu, 1, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
        //                 InsertMenu(hMenu, 2, MF_BYPOSITION | MF_STRING, IDM_EXIT, "ゲーム終了");

        //                 // マウスカーソルの位置を取得
        //                 POINT cursorPos = new POINT();
        //                 GetCursorPos(ref cursorPos);

        //                 // メニューを表示
        //                 bool result = TrackPopupMenu(
        //                     hMenu,
        //                     TPM_LEFTALIGN | TPM_RIGHTBUTTON,
        //                     cursorPos.x,
        //                     cursorPos.y,
        //                     0,
        //                     hWnd,
        //                     IntPtr.Zero
        //                 );

        //                 if (!result)
        //                 {
        //                     CustomLogger.Warning("メニュー表示に失敗しました: " +  LOGTAG);
        //                 }

        //                 // 使用後はメニューを破棄
        //                WindowUtility.DestroyWindowHandle(hMenu);

        //                 // メニューが表示されたあとにメッセージループをリセット
        //                 PostMessage(hWnd, 0, IntPtr.Zero, IntPtr.Zero);
        //             }
        //             catch (Exception ex)
        //             {
        //                 CustomLogger.Error("コンテキストメニュー表示中にエラー: " + ex.Message, LOGTAG);
        //             }
        //         }

        //         /// <summary>
        //         /// メニューコマンドの処理を行います
        //         /// </summary>
        //         private void HandleMenuCommand(uint cmdId)
        //         {
        //             try
        //             {
        //                 switch (cmdId)
        //                 {
        //                     case IDM_EXIT:
        //                         CustomLogger.Info("ゲーム終了が選択されました", LOGTAG);

        //                         // ゲームを終了する処理
        //                         // 少し遅延させて終了（メニュー処理を完了させるため）
        //                         Invoke("QuitApplication", 0.1f);
        //                         break;

        //                     case IDM_HELLO:
        //                         CustomLogger.Info("こんにちはが選択されました", LOGTAG);
        //                         ShowBalloonNotification("メッセージ", "こんにちは！");
        //                         break;
        //                 }
        //             }
        //             catch (Exception ex)
        //             {
        //                 CustomLogger.Error("メニュー処理中にエラー: " + ex.Message, LOGTAG);
        //             }
        //         }

        //         /// <summary>
        //         /// アプリケーションを終了します
        //         /// </summary>
        //         private void QuitApplication()
        //         {
        // #if UNITY_EDITOR
        //             UnityEditor.EditorApplication.isPlaying = false;
        // #else
        //             Application.Quit();
        // #endif
        //         }

        //         #endregion

    }
}
