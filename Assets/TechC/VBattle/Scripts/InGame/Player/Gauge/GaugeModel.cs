using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ゲージのデータと基本操作を管理するモデル
    /// </summary>
    public class GaugeModel : ParameterModel
    {
        private bool canCharge;
        private float elapsedTime = 0;

        public bool CanCharge => canCharge;

        public GaugeModel(float maxGauge) : base(maxGauge, 0f)
        {
            this.canCharge = false;
        }

        /// <summary>
        /// ゲージを増加（canChargeに依存）
        /// </summary>
        public void AddGauge(float amount)
        {
            if (!canCharge) return;
            AddValue(amount);
        }

        /// <summary>
        /// canChargeに依存しない内部メソッド
        /// </summary>
        public void AddGaugeInternal(float amount)
        {
            AddValue(amount);
        }

        /// <summary>
        /// 時間経過でゲージを増加
        /// </summary>
        public void UpdateTimeBasedCharge(float deltaTime, float interval, float amount)
        {
            elapsedTime += deltaTime;
            if (elapsedTime >= interval)
            {
                AddValue(amount);
                elapsedTime = 0;
            }
        }

        public float GetGaugePercentage() => currentValue / maxValue * 100;

        /// <summary>
        /// チャージ可能状態かを決める
        /// </summary>
        public void SetCanCharge(bool value) => canCharge = value;
    }
}
