// using System;
// using System.Runtime.InteropServices;
// using System.Text;
// using UnityEngine;
// using Windows.Win32;
// using Windows.Win32.Foundation;
// using Windows.Win32.UI.WindowsAndMessaging;
// using Windows.Win32.Graphics.Gdi;

// namespace TechC
// {
//     /// <summary>
//     /// Windowsのウィンドウ操作を簡単に行うためのユーティリティクラス
//     /// </summary>
//     public static class WindowUtility
//     {
//         #region ウィンドウ作成・取得

//         /// <summary>
//         /// 現在のUnityウィンドウのハンドルを取得
//         /// </summary>
//         /// <returns>ウィンドウハンドル</returns>
//         public static HWND GetUnityWindowHandle()
//         {
//             return new HWND(PInvoke.FindWindow("UnityWndClass", null));
//         }

//         /// <summary>
//         /// アクティブウィンドウのハンドルを取得
//         /// </summary>
//         /// <returns>アクティブウィンドウのハンドル</returns>
//         public static HWND GetActiveWindow()
//         {
//             return PInvoke.GetActiveWindow();
//         }

//         /// <summary>
//         /// 指定したプロセス名のウィンドウハンドルを取得
//         /// </summary>
//         /// <param name="processName">プロセス名</param>
//         /// <returns>ウィンドウハンドル</returns>
//         public static HWND GetWindowByProcessName(string processName)
//         {
//             return new HWND(PInvoke.FindWindow(null, processName));
//         }

//         /// <summary>
//         /// ウィンドウタイトルでウィンドウハンドルを取得
//         /// </summary>
//         /// <param name="windowTitle">ウィンドウタイトル</param>
//         /// <returns>ウィンドウハンドル</returns>
//         public static HWND GetWindowByTitle(string windowTitle)
//         {
//             return new HWND(PInvoke.FindWindow(null, windowTitle));
//         }

//         #endregion

//         #region ウィンドウ位置・サイズ操作

//         /// <summary>
//         /// ウィンドウを指定した位置に移動
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <param name="x">X座標</param>
//         /// <param name="y">Y座標</param>
//         /// <returns>成功した場合true</returns>
//         public static bool MoveWindow(HWND hwnd, int x, int y)
//         {
//             var rect = GetWindowRect(hwnd);
//             int width = rect.right - rect.left;
//             int height = rect.bottom - rect.top;
            
//             return PInvoke.MoveWindow(hwnd, x, y, width, height, true);
//         }

//         /// <summary>
//         /// ウィンドウのサイズを変更
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <param name="width">幅</param>
//         /// <param name="height">高さ</param>
//         /// <returns>成功した場合true</returns>
//         public static bool ResizeWindow(HWND hwnd, int width, int height)
//         {
//             var rect = GetWindowRect(hwnd);
//             return PInvoke.MoveWindow(hwnd, rect.left, rect.top, width, height, true);
//         }

//         /// <summary>
//         /// ウィンドウの位置とサイズを設定
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <param name="x">X座標</param>
//         /// <param name="y">Y座標</param>
//         /// <param name="width">幅</param>
//         /// <param name="height">高さ</param>
//         /// <returns>成功した場合true</returns>
//         public static bool SetWindowBounds(HWND hwnd, int x, int y, int width, int height)
//         {
//             return PInvoke.MoveWindow(hwnd, x, y, width, height, true);
//         }

//         /// <summary>
//         /// ウィンドウを中央に配置
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>成功した場合true</returns>
//         public static bool CenterWindow(HWND hwnd)
//         {
//             var screenSize = GetScreenSize();
//             var windowRect = GetWindowRect(hwnd);
            
//             int windowWidth = windowRect.right - windowRect.left;
//             int windowHeight = windowRect.bottom - windowRect.top;
            
//             int x = (screenSize.Width - windowWidth) / 2;
//             int y = (screenSize.Height - windowHeight) / 2;
            
//             return MoveWindow(hwnd, x, y);
//         }

//         #endregion

//         #region ウィンドウ状態操作

//         /// <summary>
//         /// ウィンドウを表示
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <param name="showCommand">表示コマンド</param>
//         /// <returns>成功した場合true</returns>
//         public static bool ShowWindow(HWND hwnd, SHOW_WINDOW_CMD showCommand = SHOW_WINDOW_CMD.SW_SHOW)
//         {
//             return PInvoke.ShowWindow(hwnd, showCommand);
//         }

//         /// <summary>
//         /// ウィンドウを最小化
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>成功した場合true</returns>
//         public static bool MinimizeWindow(HWND hwnd)
//         {
//             return ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_MINIMIZE);
//         }

//         /// <summary>
//         /// ウィンドウを最大化
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>成功した場合true</returns>
//         public static bool MaximizeWindow(HWND hwnd)
//         {
//             return ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_MAXIMIZE);
//         }

//         /// <summary>
//         /// ウィンドウを復元
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>成功した場合true</returns>
//         public static bool RestoreWindow(HWND hwnd)
//         {
//             return ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_RESTORE);
//         }

//         /// <summary>
//         /// ウィンドウをアクティブにして前面に表示
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>成功した場合true</returns>
//         public static bool BringToFront(HWND hwnd)
//         {
//             return PInvoke.SetForegroundWindow(hwnd);
//         }

//         #endregion

//         #region ウィンドウ情報取得

//         /// <summary>
//         /// ウィンドウの矩形を取得
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>ウィンドウの矩形</returns>
//         public static RECT GetWindowRect(HWND hwnd)
//         {
//             RECT rect;
//             PInvoke.GetWindowRect(hwnd, out rect);
//             return rect;
//         }

//         /// <summary>
//         /// ウィンドウのクライアント領域の矩形を取得
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>クライアント領域の矩形</returns>
//         public static RECT GetClientRect(HWND hwnd)
//         {
//             RECT rect;
//             PInvoke.GetClientRect(hwnd, out rect);
//             return rect;
//         }

//         /// <summary>
//         /// ウィンドウのタイトルを取得
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>ウィンドウタイトル</returns>
//         public static string GetWindowTitle(HWND hwnd)
//         {
//             var length = PInvoke.GetWindowTextLength(hwnd);
//             if (length == 0) return string.Empty;

//             var sb = new StringBuilder(length + 1);
//             PInvoke.GetWindowText(hwnd, sb, sb.Capacity);
//             return sb.ToString();
//         }

//         /// <summary>
//         /// ウィンドウが表示されているかどうかを確認
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>表示されている場合true</returns>
//         public static bool IsWindowVisible(HWND hwnd)
//         {
//             return PInvoke.IsWindowVisible(hwnd);
//         }

//         /// <summary>
//         /// ウィンドウが最小化されているかどうかを確認
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>最小化されている場合true</returns>
//         public static bool IsWindowMinimized(HWND hwnd)
//         {
//             return PInvoke.IsIconic(hwnd);
//         }

//         /// <summary>
//         /// ウィンドウが最大化されているかどうかを確認
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>最大化されている場合true</returns>
//         public static bool IsWindowMaximized(HWND hwnd)
//         {
//             return PInvoke.IsZoomed(hwnd);
//         }

//         #endregion

//         #region スクリーン座標・マウス操作

//         /// <summary>
//         /// スクリーンサイズを取得
//         /// </summary>
//         /// <returns>スクリーンサイズ</returns>
//         public static Vector2Int GetScreenSize()
//         {
//             int width = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
//             int height = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
//             return new Vector2Int(width, height);
//         }

//         /// <summary>
//         /// マウスカーソルの位置を取得
//         /// </summary>
//         /// <returns>マウス座標</returns>
//         public static Vector2Int GetCursorPosition()
//         {
//             POINT point;
//             PInvoke.GetCursorPos(out point);
//             return new Vector2Int(point.X, point.Y);
//         }

//         /// <summary>
//         /// マウスカーソルの位置を設定
//         /// </summary>
//         /// <param name="x">X座標</param>
//         /// <param name="y">Y座標</param>
//         /// <returns>成功した場合true</returns>
//         public static bool SetCursorPosition(int x, int y)
//         {
//             return PInvoke.SetCursorPos(x, y);
//         }

//         /// <summary>
//         /// スクリーン座標をクライアント座標に変換
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <param name="screenPoint">スクリーン座標</param>
//         /// <returns>クライアント座標</returns>
//         public static Vector2Int ScreenToClient(HWND hwnd, Vector2Int screenPoint)
//         {
//             POINT point = new POINT { X = screenPoint.x, Y = screenPoint.y };
//             PInvoke.ScreenToClient(hwnd, ref point);
//             return new Vector2Int(point.X, point.Y);
//         }

//         /// <summary>
//         /// クライアント座標をスクリーン座標に変換
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <param name="clientPoint">クライアント座標</param>
//         /// <returns>スクリーン座標</returns>
//         public static Vector2Int ClientToScreen(HWND hwnd, Vector2Int clientPoint)
//         {
//             POINT point = new POINT { X = clientPoint.x, Y = clientPoint.y };
//             PInvoke.ClientToScreen(hwnd, ref point);
//             return new Vector2Int(point.X, point.Y);
//         }

//         #endregion

//         #region ユーティリティメソッド

//         /// <summary>
//         /// ウィンドウハンドルが有効かどうかを確認
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         /// <returns>有効な場合true</returns>
//         public static bool IsValidWindow(HWND hwnd)
//         {
//             return !hwnd.IsNull && PInvoke.IsWindow(hwnd);
//         }

//         /// <summary>
//         /// Unity用の便利メソッド：現在のUnityウィンドウを中央に配置
//         /// </summary>
//         /// <returns>成功した場合true</returns>
//         public static bool CenterUnityWindow()
//         {
//             var unityWindow = GetUnityWindowHandle();
//             return IsValidWindow(unityWindow) && CenterWindow(unityWindow);
//         }

//         /// <summary>
//         /// Unity用の便利メソッド：現在のUnityウィンドウのサイズを設定
//         /// </summary>
//         /// <param name="width">幅</param>
//         /// <param name="height">高さ</param>
//         /// <returns>成功した場合true</returns>
//         public static bool SetUnityWindowSize(int width, int height)
//         {
//             var unityWindow = GetUnityWindowHandle();
//             return IsValidWindow(unityWindow) && ResizeWindow(unityWindow, width, height);
//         }

//         /// <summary>
//         /// Unity用の便利メソッド：現在のUnityウィンドウの位置とサイズを設定
//         /// </summary>
//         /// <param name="x">X座標</param>
//         /// <param name="y">Y座標</param>
//         /// <param name="width">幅</param>
//         /// <param name="height">高さ</param>
//         /// <returns>成功した場合true</returns>
//         public static bool SetUnityWindowBounds(int x, int y, int width, int height)
//         {
//             var unityWindow = GetUnityWindowHandle();
//             return IsValidWindow(unityWindow) && SetWindowBounds(unityWindow, x, y, width, height);
//         }

//         /// <summary>
//         /// デバッグ用：ウィンドウ情報をログ出力
//         /// </summary>
//         /// <param name="hwnd">ウィンドウハンドル</param>
//         public static void LogWindowInfo(HWND hwnd)
//         {
//             if (!IsValidWindow(hwnd))
//             {
//                 Debug.LogWarning("Invalid window handle");
//                 return;
//             }

//             var rect = GetWindowRect(hwnd);
//             var clientRect = GetClientRect(hwnd);
//             var title = GetWindowTitle(hwnd);

//             Debug.Log($"Window Info:\n" +
//                      $"Title: {title}\n" +
//                      $"Window Rect: ({rect.left}, {rect.top}, {rect.right}, {rect.bottom})\n" +
//                      $"Client Rect: ({clientRect.left}, {clientRect.top}, {clientRect.right}, {clientRect.bottom})\n" +
//                      $"Visible: {IsWindowVisible(hwnd)}\n" +
//                      $"Minimized: {IsWindowMinimized(hwnd)}\n" +
//                      $"Maximized: {IsWindowMaximized(hwnd)}");
//         }

//         #endregion

//         #region WindowManager用の追加メソッド

//         /// <summary>
//         /// 新しいウィンドウを作成
//         /// </summary>
//         /// <param name="className">ウィンドウクラス名</param>
//         /// <param name="windowName">ウィンドウ名</param>
//         /// <param name="style">ウィンドウスタイル</param>
//         /// <param name="exStyle">拡張ウィンドウスタイル</param>
//         /// <param name="x">X座標</param>
//         /// <param name="y">Y座標</param>
//         /// <param name="width">幅</param>
//         /// <param name="height">高さ</param>
//         /// <param name="parent">親ウィンドウハンドル</param>
//         /// <returns>作成されたウィンドウハンドル</returns>
//         public static IntPtr CreateWindow(string className, string windowName, uint style, uint exStyle, 
//             int x, int y, int width, int height, IntPtr parent)
//         {
//             var hwnd = PInvoke.CreateWindowEx(
//                 (WINDOW_EX_STYLE)exStyle,
//                 className,
//                 windowName,
//                 (WINDOW_STYLE)style,
//                 x, y, width, height,
//                 new HWND(parent),
//                 null,
//                 PInvoke.GetModuleHandle((string)null),
//                 null
//             );
            
//             return hwnd.Value;
//         }

//         /// <summary>
//         /// ウィンドウをサブクラス化（カスタムメッセージ処理用）
//         /// </summary>
//         /// <param name="hWnd">ウィンドウハンドル</param>
//         /// <returns>成功した場合true</returns>
//         public static bool SubclassWindow(IntPtr hWnd)
//         {
//             // サブクラス化の実装は用途に応じてカスタマイズ
//             // ここでは基本的な実装のみ
//             return IsValidWindow(new HWND(hWnd));
//         }

//         /// <summary>
//         /// ウィンドウの表示状態を設定
//         /// </summary>
//         /// <param name="hWnd">ウィンドウハンドル</param>
//         /// <param name="showCommand">表示コマンド</param>
//         /// <returns>成功した場合true</returns>
//         public static bool SetWindowVisibility(IntPtr hWnd, int showCommand)
//         {
//             return PInvoke.ShowWindow(new HWND(hWnd), (SHOW_WINDOW_CMD)showCommand);
//         }

//         /// <summary>
//         /// ウィンドウの位置とサイズを設定（詳細版）
//         /// </summary>
//         /// <param name="hWnd">ウィンドウハンドル</param>
//         /// <param name="insertAfter">Zオーダー位置</param>
//         /// <param name="x">X座標</param>
//         /// <param name="y">Y座標</param>
//         /// <param name="width">幅</param>
//         /// <param name="height">高さ</param>
//         /// <param name="flags">フラグ</param>
//         /// <returns>成功した場合true</returns>
//         public static bool SetWindowPositionAndSize(IntPtr hWnd, IntPtr insertAfter, int x, int y, 
//             int width, int height, uint flags)
//         {
//             return PInvoke.SetWindowPos(
//                 new HWND(hWnd),
//                 new HWND(insertAfter),
//                 x, y, width, height,
//                 (SET_WINDOW_POS_FLAGS)flags
//             );
//         }

//         /// <summary>
//         /// ウィンドウハンドルを破棄
//         /// </summary>
//         /// <param name="hWnd">ウィンドウハンドル</param>
//         /// <returns>成功した場合true</returns>
//         public static bool DestroyWindowHandle(IntPtr hWnd)
//         {
//             return PInvoke.DestroyWindow(new HWND(hWnd));
//         }

//         /// <summary>
//         /// ウィンドウの再描画設定
//         /// </summary>
//         /// <param name="hWnd">ウィンドウハンドル</param>
//         /// <param name="redraw">再描画するかどうか</param>
//         public static void SetRedraw(IntPtr hWnd, bool redraw)
//         {
//             PInvoke.SendMessage(new HWND(hWnd), PInvoke.WM_SETREDRAW, 
//                 new WPARAM((nuint)(redraw ? 1 : 0)), new LPARAM(0));
            
//             if (redraw)
//             {
//                 PInvoke.InvalidateRect(new HWND(hWnd), null, true);
//                 PInvoke.UpdateWindow(new HWND(hWnd));
//             }
//         }

//         /// <summary>
//         /// HWND_TOPMOST定数
//         /// </summary>
//         public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

//         /// <summary>
//         /// HWND_NOTOPMOST定数
//         /// </summary>
//         public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

//         #endregion

//         #region アニメーション用メソッド

//         private static Dictionary<IntPtr, Vector2> _targetPositions = new Dictionary<IntPtr, Vector2>();
//         private static Dictionary<IntPtr, Vector2> _targetSizes = new Dictionary<IntPtr, Vector2>();

//         /// <summary>
//         /// ウィンドウを目標位置にアニメーションで移動
//         /// </summary>
//         /// <param name="hWnd">ウィンドウハンドル</param>
//         /// <param name="targetX">目標X座標</param>
//         /// <param name="targetY">目標Y座標</param>
//         /// <param name="speed">移動速度</param>
//         /// <returns>目標位置に到達した場合true</returns>
//         public static bool MoveWindowToTargetPosition(IntPtr hWnd, int targetX, int targetY, float speed)
//         {
//             if (!IsValidWindow(new HWND(hWnd))) return false;

//             var rect = GetWindowRect(new HWND(hWnd));
//             var currentX = rect.left;
//             var currentY = rect.top;
            
//             var targetPos = new Vector2(targetX, targetY);
//             var currentPos = new Vector2(currentX, currentY);
            
//             if (Vector2.Distance(currentPos, targetPos) < 1f)
//             {
//                 return true; // 到達済み
//             }

//             var direction = (targetPos - currentPos).normalized;
//             var moveDistance = speed * Time.deltaTime;
//             var newPos = currentPos + direction * moveDistance;
            
//             // 目標を超えないように調整
//             if (Vector2.Distance(currentPos, targetPos) < moveDistance)
//             {
//                 newPos = targetPos;
//             }

//             MoveWindow(new HWND(hWnd), (int)newPos.x, (int)newPos.y);
//             return Vector2.Distance(newPos, targetPos) < 1f;
//         }

//         /// <summary>
//         /// ウィンドウサイズをアニメーションで変更
//         /// </summary>
//         /// <param name="hWnd">ウィンドウハンドル</param>
//         /// <param name="targetWidth">目標幅</param>
//         /// <param name="targetHeight">目標高さ</param>
//         /// <param name="speed">変更速度</param>
//         /// <returns>目標サイズに到達した場合true</returns>
//         public static bool AnimateResizeWindow(IntPtr hWnd, int targetWidth, int targetHeight, float speed)
//         {
//             if (!IsValidWindow(new HWND(hWnd))) return false;

//             var rect = GetWindowRect(new HWND(hWnd));
//             var currentWidth = rect.right - rect.left;
//             var currentHeight = rect.bottom - rect.top;
            
//             var targetSize = new Vector2(targetWidth, targetHeight);
//             var currentSize = new Vector2(currentWidth, currentHeight);
            
//             if (Vector2.Distance(currentSize, targetSize) < 1f)
//             {
//                 return true; // 到達済み
//             }

//             var direction = (targetSize - currentSize).normalized;
//             var resizeSpeed = speed * Time.deltaTime;
//             var newSize = currentSize + direction * resizeSpeed;
            
//             // 目標を超えないように調整
//             if (Vector2.Distance(currentSize, targetSize) < resizeSpeed)
//             {
//                 newSize = targetSize;
//             }

//             ResizeWindow(new HWND(hWnd), (int)newSize.x, (int)newSize.y);
//             return Vector2.Distance(newSize, targetSize) < 1f;
//         }

//         #endregion
//     }
// }