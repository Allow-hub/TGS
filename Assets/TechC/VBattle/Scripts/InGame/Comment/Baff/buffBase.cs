using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{

    /// <summary>
    /// 全てのバフの基本処理をまとめた基底クラス
    /// </summary>
    public class BuffBase : MonoBehaviour
    {
        public string buffName { get; protected set; }
        public string description { get; protected set; }
        public float buffDuration { get; protected set; }
        public float remainingTime { get; protected set; }
        
        /// <summary>
        /// バフを適用する処理
        /// </summary>
        /// <param name="target"></param>
        public virtual void Apply(GameObject target)
        {
            /* 各バフでオーバーライドして使用する */
        }
        
        /// <summary>
        /// バフを解除する処理
        /// </summary>
        /// <param name="target"></param>
        public virtual void Remove(GameObject target)
        {
            /* 各バフでオーバーライドして使用する */
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="target"></param>

        public virtual void UpdateBuff(float deltaTime, GameObject target)
        {
            remainingTime -= deltaTime;
            if (remainingTime <= 0)
            {
                Remove(target);
            }
        }
    }
}
