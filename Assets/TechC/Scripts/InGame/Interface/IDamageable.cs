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
        public void TakeDamage(float damage);//ダメージを受ける処理
        public void Des();//破壊されたとき
    }
}
