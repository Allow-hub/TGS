using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class SpeedBuff : BuffBase
    {
        public float speedMultiplier = 3.0f;

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
                characterController.AddSpeedMultiplier(speedMultiplier);
            }
        }

        public override void Remove(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();

            if (characterController != null)
            {
                characterController.RemoveSpeedMultiplier(speedMultiplier);
            }
        }
    }
}
