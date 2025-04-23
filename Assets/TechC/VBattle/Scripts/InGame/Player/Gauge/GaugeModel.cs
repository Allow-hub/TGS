using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ゲージのデータと基本操作を管理するモデル
    /// </summary>
    public class GaugeModel
    {
        private float maxGauge;
        private float currentGauge;
        private bool canCharge;
        private float elapsedTime = 0;
        [Tooltip(" チャージ倍率（バフ/デバフ用）")]
        private float chargeRate = 1.0f;

        public event Action<float> OnGaugeChanged;
        public event Action OnGaugeFilled;
        public event Action OnGaugeEmpty;

        public float CurrentGauge => currentGauge;
        public float MaxGauge => maxGauge;
        public float GaugePercentage => currentGauge / maxGauge;
        public bool CanCharge => canCharge;

        public GaugeModel(float maxGauge)
        {
            this.maxGauge = maxGauge;
            this.currentGauge = 0f;
            this.canCharge = true;
        }

        /// <summary>
        /// ゲージを増加（canChargeに依存）
        /// </summary>
        /// <param name="amount"></param>
        public void AddGauge(float amount)
        {
            if (!canCharge) return;
            AddGaugeInternal(amount * chargeRate);
        }

        /// <summary>
        /// ゲージを増加（canChargeに依存しない内部メソッド）
        /// </summary>
        /// <param name="amount"></param>
        public void AddGaugeInternal(float amount)
        {
            float oldValue = currentGauge;
            currentGauge = Mathf.Clamp(currentGauge + amount, 0f, maxGauge);

            if (oldValue != currentGauge)
            {
                OnGaugeChanged?.Invoke(currentGauge);
                if (oldValue < maxGauge && currentGauge >= maxGauge)
                    OnGaugeFilled?.Invoke();
            }
        }

        /// <summary>
        /// 時間経過でゲージを増加（canChargeに依存しない）
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="interval"></param>
        /// <param name="amount"></param>
        public void UpdateTimeBasedCharge(float deltaTime, float interval, float amount)
        {
            elapsedTime += deltaTime;
            if (elapsedTime >= interval)
            {
                Debug.Log("Y");

                AddGaugeInternal(amount);
                elapsedTime = 0;
            }
        }

        /// <summary>
        /// ゲージを使用
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool UseGauge(float amount)
        {
            if (currentGauge >= amount)
            {
                currentGauge -= amount;
                OnGaugeChanged?.Invoke(currentGauge);
                if (currentGauge <= 0)
                    OnGaugeEmpty?.Invoke();
                return true;
            }
            return false;
        }

        /// <summary>
        /// ゲージをリセット
        /// </summary>
        public void ResetGauge()
        {
            currentGauge = 0f;
            OnGaugeChanged?.Invoke(currentGauge);
            OnGaugeEmpty?.Invoke();
        }

        /// <summary>
        /// ゲージを満タンにする
        /// </summary>
        public void FillGauge()
        {
            currentGauge = maxGauge;
            OnGaugeChanged?.Invoke(currentGauge);
            OnGaugeFilled?.Invoke();
        }

        /// <summary>
        /// チャージ倍率を決める
        /// </summary>
        /// <param name="rate"></param>
        public void SetChargeRate(float rate) => chargeRate = rate;

        /// <summary>
        /// チャージ可能状態かを決める
        /// </summary>
        /// <param name="value"></param>
        public void SetCanCharge(bool value) => canCharge = value;

        /// <summary>
        /// 現在のゲージ量が引数の数値を超えているかどうか
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool HasEnoughGauge(float amount) => currentGauge >= amount;

        /// <summary>
        /// ゲージがマックスかどうかの判定
        /// </summary>
        /// <returns></returns>
        public bool IsGaugeFull() => currentGauge >= maxGauge;
    }
}
