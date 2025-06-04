using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TechC
{
    public static class DrawWindowUtility
    {
        private const int BI_RGB = 0;
        private const uint DIB_RGB_COLORS = 0;
        /// <summary>
        /// ウィンドウにテクスチャを描画する。
        /// </summary>
        /// <param name="hWnd">ウィンドウハンドル</param>
        /// <param name="texture">テクスチャ</param>
        /// <param name="drawWidth">横幅</param>
        /// <param name="drawHeight">高さ</param>
        /// <param name="orientation">向き</param>
        public static void DrawTextureToWindow(IntPtr hWnd, Texture2D texture, int drawWidth, int drawHeight, ImageOrientation orientation = ImageOrientation.Normal)
        {
            if (hWnd == IntPtr.Zero || texture == null) return;

            HWND hwnd = new HWND(hWnd);
            HDC hdc = PInvoke.GetDC(hwnd);
            if (hdc == HDC.Null) return;

            try
            {
                int srcWidth = texture.width;
                int srcHeight = texture.height;
                Color32[] pixels = texture.GetPixels32();
                byte[] bmpData = new byte[srcWidth * srcHeight * 4];

                for (int y = 0; y < srcHeight; y++)
                {
                    for (int x = 0; x < srcWidth; x++)
                    {
                        int srcX = x, srcY = y;

                        switch (orientation)
                        {
                            case ImageOrientation.FlipVertical:
                                srcY = srcHeight - 1 - y;
                                break;
                            case ImageOrientation.FlipHorizontal:
                                srcX = srcWidth - 1 - x;
                                break;
                            case ImageOrientation.Rotate180:
                                srcX = srcWidth - 1 - x;
                                srcY = srcHeight - 1 - y;
                                break;
                        }

                        // GDIは上下反転なのでさらに反転
                        int gdiY = srcHeight - 1 - srcY;
                        if (orientation == ImageOrientation.FlipVertical)
                        {
                            // 反転済みなのでそのまま
                            gdiY = srcY;
                        }
                        var pixel = pixels[srcY * srcWidth + srcX];
                        int idx = (gdiY * srcWidth + x) * 4;
                        bmpData[idx] = pixel.b;
                        bmpData[idx + 1] = pixel.g;
                        bmpData[idx + 2] = pixel.r;
                        bmpData[idx + 3] = pixel.a;
                    }
                }

                unsafe
                {
                    fixed (byte* pBmp = bmpData)
                    {
                        BITMAPINFO bmi = new BITMAPINFO();
                        bmi.bmiHeader.biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();
                        bmi.bmiHeader.biWidth = srcWidth;
                        bmi.bmiHeader.biHeight = srcHeight;
                        bmi.bmiHeader.biPlanes = 1;
                        bmi.bmiHeader.biBitCount = 32;
                        bmi.bmiHeader.biCompression = BI_RGB;
                        bmi.bmiHeader.biSizeImage = (uint)(srcWidth * srcHeight * 4);

                        PInvoke.StretchDIBits(
                            hdc,
                            0, 0, drawWidth, drawHeight,
                            0, 0, srcWidth, srcHeight,
                            pBmp,
                            &bmi,
                            DIB_RGB_COLORS,
                            ROP_CODE.SRCCOPY
                        );
                    }
                }
            }
            finally
            {
                PInvoke.ReleaseDC(hwnd, hdc);
            }
        }
    }

    /// <summary>
    /// 画像の向きを表す列挙型
    /// </summary>
    public enum ImageOrientation
    {
        Normal,
        FlipVertical,
        FlipHorizontal,
        Rotate180,
    }
}
