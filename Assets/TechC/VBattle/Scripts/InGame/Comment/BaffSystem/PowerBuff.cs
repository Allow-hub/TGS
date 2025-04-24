using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// Playerの攻撃力を上昇させるバフ
    /// </summary>
    public class PowerBuff : BuffBase
    {
        [SerializeField] private float attackMultiplier = 1.5f; /*攻撃力上昇の倍率 */

        public PowerBuff()
        {
            buffName = "PowerBuff";
            description = "攻撃力が上昇する";
            buffDuration = 5.0f;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// 攻撃力上昇のバフを適用する
        /// </summary>
        /// <param name="target"></param>
        public override void Apply(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();
            if(characterController != null)
            {
                // characterController.AddAttackMultiplier(attackMultiplier);
            }
        }

        /// <summary>
        /// 攻撃力上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();
            if(characterController != null)
            {
                // characterController.RemoveAttackMultiplier(attackMultiplier);
            }
        }
    }
}
