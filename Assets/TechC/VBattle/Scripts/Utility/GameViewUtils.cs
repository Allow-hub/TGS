using UnityEngine;
using Windows.Win32.Foundation;

public static class GameViewUtils
{
    public static Rect GetGameViewScreenRect()
    {
        var unityRect = new Rect(
           Screen.mainWindowPosition.x,
           Screen.mainWindowPosition.y,
           Screen.width,
           Screen.height
       );
        return unityRect;
    }

    public static RECT ToWin32Rect(Rect unityRect)
    {
        int left = Mathf.RoundToInt(unityRect.x);
        int top = Mathf.RoundToInt(unityRect.y);
        int right = Mathf.RoundToInt(unityRect.x + unityRect.width);
        int bottom = Mathf.RoundToInt(unityRect.y + unityRect.height);
        return new RECT
        {
            left = left,
            top = top,
            right = right,
            bottom = bottom
        };
    }
}
