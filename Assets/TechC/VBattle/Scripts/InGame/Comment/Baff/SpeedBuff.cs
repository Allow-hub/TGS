using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using TechC.Player;

namespace TechC
{
    public class SpeedBuff : BuffBase
    {
        public float speedMultiplier = 1.5f;
        public override void Apply(GameObject target)
        {
            TechC.Player.CharacterController cc = target.GetComponent<TechC.Player.CharacterController>();

            if (cc != null)
            {
                /* スピード倍率を変更 */
                cc.AddSpeedMultiplier(speedMultiplier);
            }
        }

        public override void Remove(GameObject target)
        {
            TechC.Player.CharacterController cc = target.GetComponent<TechC.Player.CharacterController>();

            if (cc != null)
            {
                cc.RemoveSpeedMultiplier(speedMultiplier);
            }
        }
    }
}
