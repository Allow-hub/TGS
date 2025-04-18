using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラクターの必殺技のゲージ管理
    /// </summary>
    public class CharacterGauge
    {
        private float maxGauge;
        private float currentGauge;
        private bool canCharge;

        /// 他のコンポーネントが購読できるイベント
        public event Action<float> OnGaugeChanged;
        public event Action OnGaugeFilled;
        public event Action OnGaugeEmpty;

        public bool CanCharge => canCharge;
        public float CurrentGauge => currentGauge;
        public float MaxGauge => maxGauge;
        /// <summary>
        /// ゲージのパーセンテージ
        /// </summary>
        public float GaugePercentage => currentGauge / maxGauge;

        /// <summary>
        /// コンストラクタ、引数でゲージの最大値を設定
        /// </summary>
        /// <param name="maxGauge"></param>
        public CharacterGauge(float maxGauge)
        {
            this.maxGauge = maxGauge;
            this.currentGauge = 0f;
        }

        /// <summary>
        /// ゲージの加算
        /// </summary>
        /// <param name="amount"></param>
        public void AddGauge(float amount)
        {
            if (!canCharge) return;
            float oldValue = currentGauge;
            currentGauge = Mathf.Clamp(currentGauge + amount, 0f, maxGauge);

            if (oldValue != currentGauge)
            {
                OnGaugeChanged?.Invoke(currentGauge);

                // ゲージがちょうど満タンになった場合
                if (oldValue < maxGauge && currentGauge >= maxGauge)
                    OnGaugeFilled?.Invoke();
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
        /// ゲージをマックスにする
        /// </summary>
        public void FillGauge()
        {
            currentGauge = maxGauge;
            OnGaugeChanged?.Invoke(currentGauge);
            OnGaugeFilled?.Invoke();
        }

        /// <summary>
        /// ゲージが最大かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsGaugeFull() => currentGauge >= maxGauge;

        /// <summary>
        /// 引数に与えられた数値を現在満たしているかどうか
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool HasEnoughGauge(float amount) => currentGauge >= amount;
        public void ChangeCanCharge(bool value) => canCharge = value;
    }
}
