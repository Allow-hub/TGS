using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC
{
    /// <summary>
    /// ウィンドウの管理クラス
    /// </summary>
    public class WindowManager : Singleton<WindowManager>
    {
        [SerializeField] private Sprite tex;
        NativeWindow window;
        protected override void Init()
        {
            base.Init();
        }

        private void Update()
        {
        }

        protected override void OnRelease()
        {
            base.OnRelease();
        }
    }
}
