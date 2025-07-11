using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャプチャモードの定義
    /// </summary>
    public enum CaptureMode
    {
        FullScreen,     // フルスクリーン
        CustomArea,     // カスタム範囲
        TwitterSize,    // Twitter用サイズ (800x600)
        InstagramSize,  // Instagram用サイズ (1080x1080)
        FacebookSize    // Facebook用サイズ (1200x630)
    }

    /// <summary>
    /// キャプチャサイズの定義
    /// </summary>
    public static class CaptureSizes
    {
        public static readonly Vector2Int Twitter = new Vector2Int(800, 600);
        public static readonly Vector2Int Instagram = new Vector2Int(1080, 1080);
        public static readonly Vector2Int Facebook = new Vector2Int(1200, 630);
        
        public static Vector2Int GetSize(CaptureMode mode)
        {
            switch (mode)
            {
                case CaptureMode.TwitterSize:
                    return Twitter;
                case CaptureMode.InstagramSize:
                    return Instagram;
                case CaptureMode.FacebookSize:
                    return Facebook;
                default:
                    return Vector2Int.zero;
            }
        }
    }
}
