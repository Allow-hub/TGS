using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TechC
{
    /// <summary>
    /// Windowsのウィンドウ操作を簡単に行うためのユーティリティクラス
    /// </summary>
    public static class WindowUtility
    {
        public static string WINDOWLOGTAG = "window";
        #region ウィンドウ作成・取得

        public static float GetDpiScaleRatio(HWND hwnd)
        {
            try
            {
                float dpiX = PInvoke.GetDpiForWindow(hwnd);
                return dpiX / 96.0f; // 96 DPIが基準
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetDpiScaleRatio failed: {ex.Message}");
                return 1.0f; // デフォルトのスケール
            }
        }
        /// <summary>
        /// 現在のUnityウィンドウのハンドルを取得（より確実な方法）
        /// </summary>
        /// <returns>ウィンドウハンドル</returns>
        public static HWND GetUnityWindowHandle()
        {
#if UNITY_EDITOR
            // Editor時は0固定でOK
            return HWND.Null;
#else
            // クラス名でUnityウィンドウのみ取得
            int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            return GetWindowByProcessId(pid, "UnityWndClass");
#endif
        }

        /// <summary>
        /// Unityのゲームビューの矩形を取得
        /// </summary>
        /// <returns>ゲームビューの矩形</returns>
        public static RECT GetUnityGameViewRect()
        {
// #if UNITY_EDITOR
            return GameViewUtils.ToWin32Rect(GameViewUtils.GetGameViewScreenRect());
// #else
            // return GetWindowRect(GetUnityWindowHandle());
// #endif
        }

        public static HWND FindWindowWithTitleSubstring(string substring)
        {
            HWND result = default;

            PInvoke.EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                int length = PInvoke.GetWindowTextLength(hWnd);
                if (length == 0)
                    return true;

                Span<char> buffer = stackalloc char[length + 1];
                int copied = GetWindowText(hWnd, buffer);
                string title = copied > 0 ? new string(buffer.Slice(0, copied)) : "";

                if (title.Contains(substring, StringComparison.OrdinalIgnoreCase))
                {
                    result = hWnd;
                    return false; // 見つかったので列挙停止
                }

                return true; // 続行
            }, IntPtr.Zero);

            return result;
        }

        private static int GetWindowText(HWND hWnd, Span<char> buffer)
        {
            unsafe
            {
                fixed (char* ptr = buffer)
                {
                    return PInvoke.GetWindowText(hWnd, ptr, buffer.Length);
                }
            }
        }

        /// <summary>
        /// アクティブウィンドウのハンドルを取得
        /// </summary>
        /// <returns>アクティブウィンドウのハンドル</returns>
        public static HWND GetActiveWindow()
        {
            return PInvoke.GetActiveWindow();
        }
    
        /// <summary>
        /// 指定したプロセス名のウィンドウハンドルを取得
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>ウィンドウハンドル</returns>
        public static HWND GetWindowByProcessName(string processName)
        {
            return new HWND((IntPtr)PInvoke.FindWindow(null, processName));
        }

        /// <summary>
        /// 指定したウィンドウハンドルのプロセスIDを取得
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>プロセスID（失敗時は0）</returns>
        public static int GetWindowProcessId(HWND hwnd)
        {
            uint pid = 0;
            unsafe
            {
                PInvoke.GetWindowThreadProcessId(hwnd, &pid);
            }
            return (int)pid;
        }


        /// <summary>
        /// ウィンドウタイトルでウィンドウハンドルを取得
        /// </summary>
        /// <param name="windowTitle">ウィンドウタイトル</param>
        /// <returns>ウィンドウハンドル</returns>
        public static HWND GetWindowByTitle(string windowTitle)
        {
            return new HWND((IntPtr)PInvoke.FindWindow(null, windowTitle));
        }


        /// <summary>
        /// プロセスIDから最初に見つかったトップレベルウィンドウハンドルを取得
        /// </summary>
        /// <param name="processId">プロセスID</param>
        /// <param name="className">ウィンドウクラス名（省略可）</param>
        /// <returns>ウィンドウハンドル（見つからなければ HWND.Null）</returns>
        public static HWND GetWindowByProcessId(int processId, string className = null)
        {
            HWND found = HWND.Null;
            PInvoke.EnumWindows((hwnd, lParam) =>
            {
                uint pid = 0;
                unsafe { PInvoke.GetWindowThreadProcessId(hwnd, &pid); }
                if (pid == processId)
                {
                    if (className != null)
                    {
                        char[] buffer = new char[256];
                        int len;
                        unsafe
                        {
                            fixed (char* pBuffer = buffer)
                            {
                                len = PInvoke.GetClassName(hwnd, new PWSTR(pBuffer), buffer.Length);
                            }
                        }
                        string winClass = new string(buffer, 0, len);
                        if (winClass == className)
                        {
                            found = hwnd;
                            return false; // stop enumeration
                        }
                    }
                    else
                    {
                        found = hwnd;
                        return false; // stop enumeration
                    }
                }
                return true; // continue enumeration
            }, 0);
            return found;
        }

        #endregion

        #region ウィンドウ位置・サイズ操作

        /// <summary>
        /// ウィンドウを指定した位置に移動
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <returns>成功した場合true</returns>
        public static bool MoveWindow(HWND hwnd, int x, int y)
        {
            var rect = GetWindowRect(hwnd);
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            return PInvoke.MoveWindow(hwnd, x, y, width, height, true);
        }
        #endregion

        #region ウィンドウ状態操作

        /// <summary>
        /// ウィンドウのサイズを変更
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns>成功した場合true</returns>
        public static bool ResizeWindow(HWND hwnd, int width, int height)
        {
            var rect = GetWindowRect(hwnd);
            return PInvoke.MoveWindow(hwnd, rect.left, rect.top, width, height, true);
        }
        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <param name="showCommand">表示コマンド</param>
        /// <returns>成功した場合true</returns>
        public static bool ShowWindow(HWND hwnd, SHOW_WINDOW_CMD showCommand = SHOW_WINDOW_CMD.SW_SHOW)
        {
            return PInvoke.ShowWindow(hwnd, showCommand);
        }

        #endregion

        #region ウィンドウ情報取得

        /// <summary>
        /// ウィンドウの矩形を取得
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>ウィンドウの矩形</returns>
        public static RECT GetWindowRect(HWND hwnd)
        {
            RECT rect;
            PInvoke.GetWindowRect(hwnd, out rect);
            return rect;
        }
        /// <summary>
        /// ウィンドウが表示されているかどうかを確認
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>表示されている場合true</returns>
        public static bool IsWindowVisible(HWND hwnd)
        {
            return PInvoke.IsWindowVisible(hwnd);
        }

        /// <summary>
        /// ウィンドウハンドルが有効かどうかを確認
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        /// <returns>有効な場合true</returns>
        public static bool IsValidWindow(HWND hwnd)
        {
            return !hwnd.IsNull && PInvoke.IsWindow(hwnd);
        }
        #endregion

        #region WindowManager用の追加メソッド

        /// <summary>
        /// 新しいウィンドウを作成
        /// </summary>
        /// <param name="className">ウィンドウクラス名</param>
        /// <param name="windowName">ウィンドウ名</param>
        /// <param name="style">ウィンドウスタイル</param>
        /// <param name="exStyle">拡張ウィンドウスタイル</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="parent">親ウィンドウハンドル</param>
        /// <returns>作成されたウィンドウハンドル</returns>
        public static IntPtr CreateWindow(string className, string windowName, uint style, uint exStyle,
            int x, int y, int width, int height, IntPtr parent)
        {
            HWND hwnd;
            unsafe
            {
                hwnd = PInvoke.CreateWindowEx(
                    (WINDOW_EX_STYLE)exStyle,
                    className,
                    windowName,
                    (WINDOW_STYLE)style,
                    x, y, width, height,
                    new HWND(parent),
                    null,
                    PInvoke.GetModuleHandle((string)null),
                    null
                );
            }

            return hwnd;
        }

        /// <summary>
        /// ウィンドウをサブクラス化（カスタムメッセージ処理用）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功した場合true</returns>
        public static bool SubclassWindow(IntPtr hWnd)
        {
            // サブクラス化の実装は用途に応じてカスタマイズ
            // ここでは基本的な実装のみ
            return IsValidWindow(new HWND(hWnd));
        }

        /// <summary>
        /// ウィンドウの表示状態を設定
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="showCommand">表示コマンド</param>
        /// <returns>成功した場合true</returns>
        public static bool SetWindowVisibility(IntPtr hWnd, int showCommand)
        {
            return PInvoke.ShowWindow(new HWND(hWnd), (SHOW_WINDOW_CMD)showCommand);
        }

        /// <summary>
        /// ウィンドウの位置とサイズを設定（詳細版）
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="insertAfter">Zオーダー位置</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="flags">フラグ</param>
        /// <returns>成功した場合true</returns>
        public static bool SetWindowPositionAndSize(IntPtr hWnd, IntPtr insertAfter, int x, int y,
            int width, int height, uint flags)
        {
            return PInvoke.SetWindowPos(
                new HWND(hWnd),
                new HWND(insertAfter),
                x, y, width, height,
                (SET_WINDOW_POS_FLAGS)flags
            );
        }

        public static bool SetWindowPos(HWND hwnd, HWND insertAfter, int x, int y,
            int width, int height, SET_WINDOW_POS_FLAGS flags)
        {
            return PInvoke.SetWindowPos(hwnd, insertAfter, x, y, width, height, flags);
        }
        public static void GetClientRect(HWND hwnd, out RECT rect)
        {
            if (hwnd.IsNull)
            {
                Debug.LogWarning("GetClientRect: hwnd is null.");
                rect = new RECT();
                return;
            }

            if (!PInvoke.GetClientRect(hwnd, out rect))
            {
                int error = Marshal.GetLastWin32Error();
                Debug.LogError($"GetClientRect failed with error code {error}");
            }
        }
        /// <summary>
        /// ウィンドウハンドルを破棄
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <returns>成功した場合true</returns>
        public static bool DestroyWindowHandle(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                Debug.LogWarning("DestroyWindowHandle: hwnd is null.");
                return false;
            }

            if (!PInvoke.IsWindow((HWND)hwnd))
            {
                Debug.LogWarning("DestroyWindowHandle: hwnd is not a valid window.");
                return false;
            }

            if (!PInvoke.DestroyWindow((HWND)hwnd))
            {
                int error = Marshal.GetLastWin32Error();
                Debug.LogError($"DestroyWindowHandle failed with error code {error}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ウィンドウの再描画設定
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="redraw">再描画するかどうか</param>
        public static void SetRedraw(IntPtr hWnd, bool redraw)
        {
            PInvoke.SendMessage(new HWND(hWnd), PInvoke.WM_SETREDRAW,
                new WPARAM((nuint)(redraw ? 1 : 0)), new LPARAM(0));

            if (redraw)
            {
                PInvoke.InvalidateRect(new HWND(hWnd), new RECT(), true);
                PInvoke.UpdateWindow(new HWND(hWnd));
            }
        }

        public static void UpdateWindow(HWND hwnd)
        {
            if (hwnd.IsNull)
            {
                Debug.LogWarning("UpdateWindow: hwnd is null.");
                return;
            }

            if (!PInvoke.IsWindow(hwnd))
            {
                Debug.LogWarning("UpdateWindow: hwnd is not a valid window.");
                return;
            }

            PInvoke.UpdateWindow(hwnd);
        }



        #endregion

        #region アニメーション用メソッド


        /// <summary>
        /// ウィンドウを目標位置にアニメーションで移動
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="targetX">目標X座標</param>
        /// <param name="targetY">目標Y座標</param>
        /// <param name="speed">移動速度</param>
        /// <returns>目標位置に到達した場合true</returns>
        public static bool MoveWindowToTargetPosition(IntPtr hWnd, int targetX, int targetY, float speed)
        {
            if (!IsValidWindow(new HWND(hWnd))) return false;

            var rect = GetWindowRect(new HWND(hWnd));
            var currentX = rect.left;
            var currentY = rect.top;

            var targetPos = new Vector2(targetX, targetY);
            var currentPos = new Vector2(currentX, currentY);

            if (Vector2.Distance(currentPos, targetPos) < 1f)
            {
                return true; // 到達済み
            }

            var direction = (targetPos - currentPos).normalized;
            var moveDistance = speed * Time.deltaTime;
            var newPos = currentPos + direction * moveDistance;

            // 目標を超えないように調整
            if (Vector2.Distance(currentPos, targetPos) < moveDistance)
            {
                newPos = targetPos;
            }

            MoveWindow(new HWND(hWnd), (int)newPos.x, (int)newPos.y);
            return Vector2.Distance(newPos, targetPos) < 1f;
        }

        /// <summary>
        /// ウィンドウサイズをアニメーションで変更
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="targetWidth">目標幅</param>
        /// <param name="targetHeight">目標高さ</param>
        /// <param name="speed">変更速度</param>
        /// <returns>目標サイズに到達した場合true</returns>
        public static bool AnimateResizeWindow(IntPtr hWnd, int targetWidth, int targetHeight, float speed)
        {
            if (!IsValidWindow(new HWND(hWnd))) return false;

            var rect = GetWindowRect(new HWND(hWnd));
            var currentWidth = rect.right - rect.left;
            var currentHeight = rect.bottom - rect.top;

            var targetSize = new Vector2(targetWidth, targetHeight);
            var currentSize = new Vector2(currentWidth, currentHeight);

            if (Vector2.Distance(currentSize, targetSize) < 1f)
            {
                return true; // 到達済み
            }

            var direction = (targetSize - currentSize).normalized;
            var resizeSpeed = speed * Time.deltaTime;
            var newSize = currentSize + direction * resizeSpeed;

            // 目標を超えないように調整
            if (Vector2.Distance(currentSize, targetSize) < resizeSpeed)
            {
                newSize = targetSize;
            }

            ResizeWindow(new HWND(hWnd), (int)newSize.x, (int)newSize.y);
            return Vector2.Distance(newSize, targetSize) < 1f;
        }

        /// <summary>
        /// 子ウィンドウの親ウィンドウを設定
        /// </summary>
        /// <param name="childHwnd">子ウィンドウハンドル</param>
        /// <param name="parentHwnd">親ウィンドウハンドル</param>
        /// <returns>成功した場合true</returns>
        public static bool SetParentWindow(IntPtr childHwnd, IntPtr parentHwnd)
        {
            if (childHwnd == IntPtr.Zero || parentHwnd == IntPtr.Zero)
            {
                Debug.LogWarning("SetParentWindow: childHwnd or parentHwnd is null.");
                return false;
            }
            var result = PInvoke.SetParent((HWND)childHwnd,(HWND)parentHwnd);
            return result != HWND.Null;
        }

        #endregion
    }
}