using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Windows.Win32.Foundation;

namespace TechC
{
    /// <summary>
    /// ウィンドウのギミック管理
    /// </summary>
    [Serializable]
    public class WindowGimmickController : IGimmick
    {
        [SerializeField] private Vector2 intervalRange;
        [SerializeField] private float appearTime = 5f;
        [SerializeField] private MonoBehaviour monoBehaviour;
        private int initWindowPosX = -50;
        private float timer;
        private float currentInterval;
        private bool isEventRunning = false;
        private NativeWindow nativeWindow;

        public void OnEnter()
        {
            timer = 0f;
            currentInterval = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
        }

        public void OnUpdate(float deltaTime)
        {
            if (isEventRunning) return; // イベント中はタイマー止める

            timer += deltaTime;
            if (timer >= currentInterval)
            {
                isEventRunning = true;
                ExcuteEvent();

                currentInterval = UnityEngine.Random.Range(intervalRange.x, intervalRange.y);
                timer = 0f;
            }
        }

        public void OnExit() { }

        private void ExcuteEvent()
        {
            nativeWindow = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
            WindowManager.I.AddColliderWindow(nativeWindow);
            WindowUtility.ResizeWindow((HWND)nativeWindow.Hwnd, 10, Screen.height);
            WindowUtility.MoveWindow((HWND)nativeWindow.Hwnd, initWindowPosX, 0);

            WindowUtility.MoveWindowToTargetAsync(nativeWindow, Screen.width / 3, 0).Forget();
            DelayUtility.StartDelayedActionWithPause(monoBehaviour, appearTime, BattleJudge.I.GetPauseStateFunc, () =>
            {
                WindowFactory.I.ReturnWindow(nativeWindow);
                WindowManager.I.RemoveColliderWindow(nativeWindow);
                isEventRunning = false;
            });
        }
    }

}
