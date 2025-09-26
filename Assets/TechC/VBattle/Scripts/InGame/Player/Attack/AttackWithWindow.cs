using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 攻撃にWindowを追従させるクラス
    /// </summary>
    public class AttackWithWindow : IAttackBehaviour
    {
        [SerializeField] private Sprite windowSprite;
        [SerializeField] private Vector2 windowSize = new Vector2(100, 100);
        [SerializeField] private Vector2 offset = new Vector2(0, 0);

        private NativeWindow currentWindow;
        private GameObject ownerObj;
        private bool once = false;

        public void Initialize(GameObject owner)
        {
            ownerObj = owner;
        }

        public void OnRelease()
        {
            if (currentWindow == null) return;
            if (WindowFactory.I == null) return;
            WindowFactory.I.ReturnWindow(currentWindow);
            currentWindow = null;
        }

        public void OnUpdate(float deltaTime)
        {
            if (ownerObj == null || currentWindow == null) return;
            WindowUtility.MoveWindow((HWND)currentWindow.Hwnd, 
                (int)(Camera.main.WorldToScreenPoint(ownerObj.transform.position).x - offset.x), 
                (int)(Camera.main.WorldToScreenPoint(ownerObj.transform.position).y + offset.y));
        }

        public void Activate(GameObject character)
        {
            currentWindow = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
            WindowUtility.ResizeWindow((HWND)currentWindow.Hwnd, (int)windowSize.x, (int)windowSize.y);
            currentWindow.SetRect();
            var imageWindow = currentWindow as ImageWindow;
            if (!once)
            {
                float delay = 0.1f;
                DelayUtility.StartDelayedActionWithPause(WindowFactory.I, delay, BattleJudge.I.GetPauseStateFunc, () =>
                {
                    if (ownerObj == null) return;
                    var pos = Camera.main.WorldToScreenPoint(ownerObj.transform.position);
                    WindowUtility.MoveWindow((HWND)currentWindow.Hwnd, (int)(pos.x + offset.x), (int)(pos.y + offset.y));
                    imageWindow.SetImage(windowSprite.texture);
                    once = true;
                });
            }
            else
            {
                var pos = Camera.main.WorldToScreenPoint(ownerObj.transform.position);
                WindowUtility.MoveWindow((HWND)currentWindow.Hwnd, (int)(pos.x + offset.x), (int)(pos.y + offset.y));
                imageWindow.SetImage(windowSprite.texture);
            }
        }
    }
}
