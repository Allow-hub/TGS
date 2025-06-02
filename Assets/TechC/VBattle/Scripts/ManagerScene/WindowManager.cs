using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ウィンドウの管理クラス
    /// </summary>
    public class WindowManager : Singleton<WindowManager>
    {
        [SerializeField] private Sprite tex;

        protected override void Init()
        {
            base.Init();
            DelayUtility.StartDelayedAction(this, 0.1f, () =>
            {
                WindowFactory.I.GetWindow(WindowFactory.WindowType.Basic);
            });
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
