using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC
{
    public static class GamepadVibrationUtility
    {
        /// <summary>
        /// 指定時間だけゲームパッドを振動させる
        /// </summary>
        public static void Vibrate(float lowFrequency, float highFrequency, float duration, Gamepad gamepad = null)
        {
            gamepad ??= Gamepad.current;
            if (gamepad == null)
            {
                Debug.LogWarning("GamepadVibrationUtility: No gamepad connected.");
                return;
            }

            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            DelayUtility.RunAfterDelay(duration, () =>
            {
                gamepad.SetMotorSpeeds(0f, 0f);
            }).Forget();
        }

        /// <summary>
        /// 明示的に振動を止める
        /// </summary>
        public static void Stop(Gamepad gamepad = null)
        {
            gamepad ??= Gamepad.current;
            gamepad?.SetMotorSpeeds(0f, 0f);
        }
    }
}
