using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    public static class CustomWindowUtility
    {
        private static bool _classRegistered = false;
        private static string _basicClassName = "WindowClass_Basic";
        private static string _imageClassName = "WindowClass_Image";


        // WndProcデリゲート保持（GC防止）
        private static readonly WNDPROC _basicWndProc = BasicWndProc;
        private static readonly WNDPROC _imageWndProc = ImageWndProc;

        public static void RegisterWindowClasses()
        {
            if (_classRegistered)
                return;

            var hInstance = PInvoke.GetModuleHandle((PCWSTR)default);
            RegisterClassEx(_basicClassName, _basicWndProc, hInstance);
            RegisterClassEx(_imageClassName, _imageWndProc, hInstance);

            _classRegistered = true;
        }
        public static void UnregisterWindowClasses()
        {
            if (!_classRegistered)
                return;

            var hInstance = PInvoke.GetModuleHandle((PCWSTR)default);

            unsafe
            {
                fixed (char* basicName = _basicClassName)
                {
                    if (!PInvoke.UnregisterClass(new PCWSTR(basicName), hInstance))
                        Debug.LogError($"UnregisterClass failed for {_basicClassName}, error: {Marshal.GetLastWin32Error()}");
                }

                fixed (char* imageName = _imageClassName)
                {
                    if (!PInvoke.UnregisterClass(new PCWSTR(imageName), hInstance))
                        Debug.LogError($"UnregisterClass failed for {_imageClassName}, error: {Marshal.GetLastWin32Error()}");
                }
            }

            _classRegistered = false;
            Debug.Log("Window classes unregistered.");
        }

        private static void RegisterClassEx(string className, WNDPROC wndProc, HMODULE hInstance)
        {
            unsafe
            {
                fixed (char* cName = className)
                {
                    WNDCLASSEXW wndClass = new()
                    {
                        cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                        style = 0,
                        lpfnWndProc = wndProc,
                        cbClsExtra = 0,
                        cbWndExtra = 0,
                        hInstance = (HINSTANCE)hInstance,
                        hIcon = default,
                        hCursor = PInvoke.LoadCursor(HINSTANCE.Null, PInvoke.IDC_ARROW),
                        hbrBackground = new Windows.Win32.Graphics.Gdi.HBRUSH((nint)5 + 1),
                        lpszMenuName = null,
                        lpszClassName = new PCWSTR(cName),
                        hIconSm = default,
                    };

                    ushort atom = PInvoke.RegisterClassEx(wndClass);
                    if (atom == 0)
                        Debug.LogError($"RegisterClassEx failed for {className}, error: {Marshal.GetLastWin32Error()}");
                    else
                        Debug.Log($"Window class '{className}' registered successfully.");
                }
            }
        }


        public static IntPtr CreateWindow(
            string className,
            string title,
            uint style,
            uint exStyle,
            int x, int y, int width, int height,
            IntPtr parent)
        {

            HWND hwnd;
            unsafe
            {
                fixed (char* cName = className)
                fixed (char* titleName = title)
                {

                    hwnd = PInvoke.CreateWindowEx(
                        (WINDOW_EX_STYLE)exStyle,
                       new PCWSTR(cName),
                       new PCWSTR(titleName),
                        (WINDOW_STYLE)style,
                        x, y, width, height,
                        new HWND(parent),
                        HMENU.Null,
                        PInvoke.GetModuleHandle((PCWSTR)default),
                        null
                    );
                }
            }
            Debug.Log($"CreateWindowEx success: hwnd = {hwnd}");
            if (hwnd == HWND.Null)
                Debug.LogError($"CreateWindowEx failed, error: {Marshal.GetLastWin32Error()}");

            return hwnd;
        }

        private static LRESULT BasicWndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case PInvoke.WM_DESTROY:
                    break;
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static LRESULT ImageWndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case PInvoke.WM_PAINT:
                    // 描画処理
                    break;
                case PInvoke.WM_DESTROY:
                    break;
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }
}
