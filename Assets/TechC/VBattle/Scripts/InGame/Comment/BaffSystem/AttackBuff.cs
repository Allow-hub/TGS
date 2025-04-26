using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// Playerの攻撃力を上昇させるバフ
    /// </summary>
    public class AttackBuff : BuffBase
    {
        [SerializeField] private float attackMultiplier = 1.5f; /*攻撃力上昇の倍率 */

        public AttackBuff()
        {
            buffName = "AttackBuff";
            description = "攻撃力が上昇する";
            buffDuration = 2.0f;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// 攻撃力上昇のバフを適用する
        /// </summary>
        /// <param name="target"></param>
        public override void Apply(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();
            if (characterController != null)
            {
                // Debug.Log($"<color=orange>[Apply前]</color>攻撃の倍率は{characterController.GetCurrentAttackMultiplier()}");
                // public float GetCurrentAttackMultiplier() => GetMultipiler(BuffType.Attack); これをcharacterControllerに書く


                characterController.AddMultiplier(BuffType.Attack, attackMultiplier);

                // Debug.Log($"<color=orange>[Apply後]</color>:攻撃の倍率は{characterController.GetCurrentAttackMultiplier()}");

            }
        }

        /// <summary>
        /// 攻撃力上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();
            if (characterController != null)
            {
                // Debug.Log($"<color=blue>[Remove後]</color>:攻撃の倍率は{characterController.GetCurrentAttackMultiplier()}");

                characterController.RemoveMultiplier(BuffType.Attack, attackMultiplier);
                // Debug.Log($"<color=blue>[Remove後]</color>:攻撃の倍率は{characterController.GetCurrentAttackMultiplier()}");
                
            }
        }
    }
}
