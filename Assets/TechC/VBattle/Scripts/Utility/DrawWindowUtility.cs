using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public static class DrawWindowUtility
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern int SetDIBitsToDevice(
            IntPtr hdc,
            int xDest,
            int yDest,
            uint w,
            uint h,
            int xSrc,
            int ySrc,
            uint StartScan,
            uint cLines,
            IntPtr lpvBits,
            ref BITMAPINFO lpbmi,
            uint ColorUse);

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint bmiColors;
        }

        private const int BI_RGB = 0;
        private const uint DIB_RGB_COLORS = 0;

        public static void DrawTextureToWindow(IntPtr hWnd, Texture2D texture)
        {
            if (hWnd == IntPtr.Zero || texture == null) return;

            IntPtr hdc = GetDC(hWnd);
            if (hdc == IntPtr.Zero) return;

            try
            {
                var width = texture.width;
                var height = texture.height;

                // UnityのTexture2Dは左上が原点、GDIは左下が原点なので上下反転が必要
                Color32[] pixels = texture.GetPixels32();

                // ピクセル配列を上下反転させてバイト配列に変換
                byte[] bmpData = new byte[width * height * 4];
                for (int y = 0; y < height; y++)
                {
                    int srcRow = height - 1 - y;
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = pixels[srcRow * width + x];
                        int idx = (y * width + x) * 4;
                        bmpData[idx] = pixel.b;     // B
                        bmpData[idx + 1] = pixel.g; // G
                        bmpData[idx + 2] = pixel.r; // R
                        bmpData[idx + 3] = pixel.a; // A (透明度は無視してもOK)
                    }
                }

                // BITMAPINFOをセットアップ
                BITMAPINFO bmi = new BITMAPINFO();
                bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                bmi.bmiHeader.biWidth = width;
                bmi.bmiHeader.biHeight = height;
                bmi.bmiHeader.biPlanes = 1;
                bmi.bmiHeader.biBitCount = 32; // 32ビット（BGRA）
                bmi.bmiHeader.biCompression = BI_RGB;
                bmi.bmiHeader.biSizeImage = (uint)(width * height * 4);

                // bmpDataをアンマネージドメモリにコピー
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bmpData.Length);
                Marshal.Copy(bmpData, 0, unmanagedPointer, bmpData.Length);

                // 描画（左上座標0,0に描画）
                SetDIBitsToDevice(hdc, 0, 0, (uint)width, (uint)height, 0, 0, 0, (uint)height, unmanagedPointer, ref bmi, DIB_RGB_COLORS);

                Marshal.FreeHGlobal(unmanagedPointer);
            }
            finally
            {
                ReleaseDC(hWnd, hdc);
            }
        }
    }
}
