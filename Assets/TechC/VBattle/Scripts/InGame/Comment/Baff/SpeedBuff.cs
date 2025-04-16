using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    [Serializable]
    public class SpeedBuff : BuffBase
    {
        [SerializeField]
        private float speedMultiplier = 3.0f;

        public SpeedBuff()
        {
            buffName = "SpeedBuff";
            description = "移動速度が上昇する";
            buffDuration = 3.0f;
            remainingTime = buffDuration;
        }
        public override void Apply(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();

            if (characterController != null)
            {
                /* スピード倍率を変更 */
                Debug.Log($"<color=orange>[Apply前]</color>:スピードの倍率は{characterController.GetCurrentSpeedMultiplier()}");

                characterController.AddSpeedMultiplier(speedMultiplier);
                Debug.Log($"<color=orange>[Apply後]</color>:スピードの倍率は{characterController.GetCurrentSpeedMultiplier()}");
            }
        }

        public override void Remove(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();

            if (characterController != null)
            {

                Debug.Log($"<color=#00FFFF>[Remove]</color>:スピードの倍率は{characterController.GetCurrentSpeedMultiplier()}");
                characterController.RemoveSpeedMultiplier(speedMultiplier);

                Debug.Log($"<color=#00FFFF>[Remove後]</color>:スピードの倍率は{characterController.GetCurrentSpeedMultiplier()}");


            }
        }
    }
}
