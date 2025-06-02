using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 通常のウィンドウ
    /// </summary>
    public class BasicWindow : NativeWindow
    {
        public BasicWindow(IntPtr hwnd, int width, int height, WindowFactory.WindowType type) : base(hwnd, width, height, type)
        {
        }
    }
}
