using System;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ゲージの共通基底クラス
    /// </summary>
    public abstract class ParameterModel
    {
        protected float maxValue;
        protected float currentValue;
        protected float changeRate = 1.0f;

        /// <summary>
        /// 値が変わったときのイベント
        /// </summary>
        public event Action<float> OnValueChanged;

        /// <summary>
        /// 値が最大まで満たされたときのイベント
        /// </summary>
        public event Action OnValueFilled;

        /// <summary>
        /// 値が空になったときのイベント
        /// </summary>
        public event Action OnValueEmpty;

        public float CurrentValue => currentValue;
        public float MaxValue => maxValue;
        public float Percentage => currentValue / maxValue;

        public ParameterModel(float maxValue, float initialValue)
        {
            this.maxValue = maxValue;
            this.currentValue = initialValue;
        }

        /// <summary>
        /// 値を増加させる
        /// </summary>
        public virtual void AddValue(float amount)
        {
            float oldValue = currentValue;
            currentValue = Mathf.Clamp(currentValue + (amount * changeRate), 0f, maxValue);

            if (oldValue != currentValue)
            {
                OnValueChanged?.Invoke(currentValue);
                if (oldValue < maxValue && currentValue >= maxValue)
                    OnValueFilled?.Invoke();
            }
        }

        /// <summary>
        /// 値を減少させる
        /// </summary>
        public virtual bool UseValue(float amount)
        {
            if (currentValue >= amount)
            {
                float oldValue = currentValue;
                currentValue -= amount;
                OnValueChanged?.Invoke(currentValue);
                if (currentValue <= 0)
                    OnValueEmpty?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 値を最大にする
        /// </summary>
        public virtual void FillValue()
        {
            currentValue = maxValue;
            OnValueChanged?.Invoke(currentValue);
            OnValueFilled?.Invoke();
        }

        /// <summary>
        /// 値をゼロにする
        /// </summary>
        public virtual void EmptyValue()
        {
            currentValue = 0f;
            OnValueChanged?.Invoke(currentValue);
            OnValueEmpty?.Invoke();
        }

        protected virtual void RaiseValueChangedEvent(float newValue)
        {
            OnValueChanged?.Invoke(newValue);
        }

        protected virtual void RaiseValueEmptyEvent()
        {
            OnValueEmpty?.Invoke();
        }
        /// <summary>
        /// 変化率を設定
        /// </summary>
        public virtual void SetChangeRate(float rate) => changeRate = rate;

        /// <summary>
        /// 現在の値が引数以上かどうか
        /// </summary>
        public virtual bool HasEnoughValue(float amount) => currentValue >= amount;

        /// <summary>
        /// 値が最大かどうか
        /// </summary>
        public virtual bool IsValueFull() => currentValue >= maxValue;

        /// <summary>
        /// 値が空かどうか
        /// </summary>
        public virtual bool IsValueEmpty() => currentValue <= 0;
    }
}