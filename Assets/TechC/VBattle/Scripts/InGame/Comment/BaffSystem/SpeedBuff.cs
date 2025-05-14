using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// Playerの移動速度を上昇させるバフ
    /// </summary>
    [Serializable]
    public class SpeedBuff : BuffBase
    {
        [SerializeField] private float speedMultiplier = 3.0f;

        [SerializeField] private GameObject effectPrefab; /* エフェクトの元となるPrefab */
        [SerializeField] private GameObject effectInstance; /* 実際にInstantiateで生成されたエフェクトのインスタンス */

        public SpeedBuff()
        {
            buffName = "SpeedBuff";
            description = "移動速度が上昇する";
            buffDuration = 3.0f;
            remainingTime = buffDuration;
        }

        /// <summary>
        /// 移動速度上昇のバフを適用する
        /// </summary>
        /// <param name="target"></param>
        public override void Apply(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();

            /* 速度上昇のバフを適用 */
            if (characterController != null)
            { 
                characterController.AddMultiplier(BuffType.Speed, speedMultiplier);
            }
        }
        
        /// <summary>
        /// 移動速度上昇のバフを解除する
        /// </summary>
        /// <param name="target"></param>
        public override void Remove(GameObject target)
        {
            Player.CharacterController characterController = target.GetComponent<Player.CharacterController>();

            /* 速度上昇のバフを解除 */
            if (characterController != null)
            {
                characterController.RemoveMultiplier(BuffType.Speed, speedMultiplier);
            }

            /* エフェクトを削除する */
            if(effectInstance != null)
            {
                UnityEngine.Object.Destroy(effectInstance);
                effectInstance = null;
            }
        }
    }
}
