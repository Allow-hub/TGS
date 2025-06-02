using System;
using UnityEngine;

namespace TechC
{
    public class ImageWindow : NativeWindow
    {
        private Texture2D image;

        public ImageWindow(IntPtr hwnd, int width, int height, Texture2D texture)
            : base(hwnd, width, height, WindowFactory.WindowType.Image)
        {
            image = texture;
        }

        public override void Show()
        {
            base.Show();
            if (image != null)
            {
                DrawWindowUtility.DrawTextureToWindow(Hwnd, image);
            }
        }

        public void SetImage(Texture2D texture)
        {
            image = texture;
            DrawWindowUtility.DrawTextureToWindow(Hwnd, image);
        }
    }
}
