using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// Damageを受けることが可能
    /// オブジェクト・キャラ問わず
    /// </summary>
    public interface IDamageable 
    {
        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(float damage);

        /// <summary>
        /// 破壊されたとき
        /// </summary>
        public void Des();
    }
}
