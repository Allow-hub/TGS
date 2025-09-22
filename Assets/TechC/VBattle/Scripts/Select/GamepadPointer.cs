using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Windows.Win32.Foundation;
using System;
using UnityEngine.InputSystem.Utilities;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC.Select
{
    public class GamepadPointer : MonoBehaviour
    {
        [SerializeField] private Sprite pointerSprite;
        [SerializeField] private float cursorSpeed = 800f; // 1秒あたりの移動速度（ピクセル）

        private Dictionary<InputDevice, NativeWindow> nativeWindows = new Dictionary<InputDevice, NativeWindow>();
        private Dictionary<InputDevice, Vector2> cursorPositions = new Dictionary<InputDevice, Vector2>();

        private IDisposable currentListener;

        private void OnEnable()
        {
            StartCoroutine(WaitForNextDevice());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            currentListener?.Dispose();
            currentListener = null;

            nativeWindows.Clear();
            cursorPositions.Clear();
        }

        private void Update()
        {
            foreach (var pair in nativeWindows)
            {
                var device = pair.Key;
                var window = pair.Value;

                if (device is Gamepad gamepad)
                {
                    // スティック入力（-1～1）
                    Vector2 stick = gamepad.leftStick.ReadValue();

                    // 前回位置に加算（deltaTimeを掛けてスピード制御）
                    if (!cursorPositions.ContainsKey(device))
                        cursorPositions[device] = new Vector2(Screen.width / 2, Screen.height / 2);

                    Vector2 pos = cursorPositions[device];
                    pos += stick * cursorSpeed * Time.deltaTime;

                    // 画面内に制限
                    pos.x = Mathf.Clamp(pos.x, 0, Screen.width - 1);
                    pos.y = Mathf.Clamp(pos.y, 0, Screen.height - 1);

                    cursorPositions[device] = pos;

                    // Y座標を反転してWin32の座標系に変換
                    int winX = (int)pos.x;
                    int winY = (int)(Screen.height - pos.y);

                    // ウィンドウを移動
                    WindowUtility.MoveWindow((HWND)window.Hwnd, winX, winY);
                }
            }
        }

        private IEnumerator WaitForNextDevice()
        {
            yield return null;

            currentListener = InputSystem.onAnyButtonPress.CallOnce(ctrl =>
            {
                if (!this) return;

                var device = ctrl.device;
                if (device is Mouse || device is Keyboard)
                {
                    StartCoroutine(WaitForNextDevice());
                    return;
                }

                if (nativeWindows.ContainsKey(device))
                {
                    Debug.Log($"既に登録済み: {device.displayName}");
                    StartCoroutine(WaitForNextDevice());
                    return;
                }

                // --- カーソルウィンドウを生成 ---
                var w = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
                int style = PInvoke.GetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
                style &= ~(int)WINDOW_STYLE.WS_CAPTION; // タイトルバー削除
                style &= ~(int)WINDOW_STYLE.WS_THICKFRAME; // 枠削除
                style |= unchecked((int)WINDOW_STYLE.WS_POPUP);

                PInvoke.SetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);

                int exStyle = PInvoke.GetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
                exStyle |= (int)WINDOW_EX_STYLE.WS_EX_LAYERED;
                PInvoke.SetWindowLong((HWND)w.Hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);
                WindowUtility.ResizeWindow((HWND)w.Hwnd, 64, 64); // 小さめのカーソルサイズ
                if (w is ImageWindow imageWindow)
                    imageWindow?.SetTextureToBitmap(pointerSprite.texture);
                // 初期位置 = 画面中央
                Vector2 startPos = new Vector2(Screen.width / 2, Screen.height / 2);
                cursorPositions[device] = startPos;

                // Y座標を反転
                int startX = (int)startPos.x;
                int startY = (int)(Screen.height - startPos.y);
                WindowUtility.MoveWindow((HWND)w.Hwnd, startX, startY);

                nativeWindows[device] = w;

                Debug.Log($"新規デバイス検出: {device.displayName}");

                // 次の参加者を待つ
                StartCoroutine(WaitForNextDevice());
            });
        }
    }
}