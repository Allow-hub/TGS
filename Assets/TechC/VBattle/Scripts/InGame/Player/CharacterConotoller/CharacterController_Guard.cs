using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.Player
{
    public partial class CharacterController
    {
        /// <summary>
        /// ガードパワーを取得
        /// </summary>
        public float GetGuardPower() => currentGuardPower;

        /// <summary>
        /// ガード中に耐久値減少
        /// </summary>
        public void DecreaseGuardPower() => currentGuardPower -= characterData.GuardDecreasePower;

        /// <summary>
        /// 最後にガードした時間を設定
        /// </summary>
        public void SetLastGuardTime(float time) => lastGuardTime = time;

        /// <summary>
        /// ガード時のダメージ処理
        /// </summary>
        public void GuardDamage(float damage, ICommand guardCommand)
        {
            currentGuardPower -= damage;
            AudioManager.I.PlayCharacterSE(characterType, CharacterSEType.Guard);
            if (inputDevice is Gamepad gamepad)
                GamepadVibrationUtility.Vibrate(lowFrequency, highFrequency, duration, gamepad);
            if (currentGuardPower > 0) return;
            currentGuardPower = 0;
            GuardBreak(guardCommand);
        }

        /// <summary>
        /// ガードブレイク処理
        /// </summary>
        public void GuardBreak(ICommand guardCommand)
        {
            AudioManager.I.PlayCharacterSE(characterType, CharacterSEType.GuardBreak);
            guardCommand.ForceFinish();
            currentGuardPower = 0; // ガードがマイナスで保存されないように
            Debug.Log("Guardが破壊されました");
        }

        /// <summary>
        /// ガードパワー回復処理
        /// </summary>
        public void HealGuardPower(float value)
        {
            if (currentGuardPower < characterData.GuardPower)
                currentGuardPower += value;
            else
                currentGuardPower = characterData.GuardPower;
            //Debug.Log($"{currentGuardPower}");
        }

        /// <summary>
        /// ガード回復が可能な状態かを判定
        /// </summary>
        public bool CanHeal()
        {
            var lastGuard = Time.time - lastGuardTime;
            return lastGuard >= characterData.GuardRecoveryInterval;
        }
    }
}
