using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class HPModel : ParameterModel
    {
        public HPModel(float maxHp) : base(maxHp, maxHp)
        {
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage)
        {
            // UseValueとは逆の動作なので、自前で実装
            float oldValue = currentValue;
            currentValue = Mathf.Clamp(currentValue - (damage * changeRate), 0f, maxValue);
            if (oldValue != currentValue)
            {
                RaiseValueChangedEvent(currentValue); 
                if (currentValue <= 0)
                    RaiseValueEmptyEvent(); 
            }
        }

        /// <summary>
        /// 回復する
        /// </summary>
        public void Heal(float amount)
        {
            AddValue(amount);
        }
    }
}