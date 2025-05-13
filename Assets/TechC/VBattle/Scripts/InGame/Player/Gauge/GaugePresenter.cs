using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// モデルとビューを繋ぐプレゼンター
    /// GaugeView と GaugeModel
    /// </summary>
    public class GaugePresenter : ParameterPresenter
    {
        [SerializeField] private GaugeView gaugeView;
        [SerializeField] private float timeBasedChargeInterval = 1.0f;
        [SerializeField] private float timeBasedChargeAmount = 2.0f;

        private GaugeModel gaugeModel;

        protected override void InitializeModel()
        {
            gaugeModel = new GaugeModel(maxValue);
            model = gaugeModel;
        }

        protected override void InitializeView()
        {
            view = gaugeView;
        }

        private void FixedUpdate()
        {
            // 時間経過によるゲージ増加
            gaugeModel.UpdateTimeBasedCharge(Time.fixedDeltaTime, timeBasedChargeInterval, timeBasedChargeAmount);
        }

        protected override void HandleValueFilled()
        {
            base.HandleValueFilled();
            gaugeView.ShowFullGaugeEffect(true);
        }

        protected override void HandleValueEmpty()
        {
            base.HandleValueEmpty();
            gaugeView.ShowFullGaugeEffect(false);
            gaugeView.ShowEmptyGaugeEffect();
        }

        // ゲージ特有のメソッド
        public void AddGauge(float amount)
        {
            gaugeModel.AddGauge(amount);
        }

        public void NotBoolAddGauge(float amount)
        {
            gaugeModel.AddGaugeInternal(amount);
        }

        public bool TryUseSpecialAttack(float cost)
        {
            return gaugeModel.UseValue(cost);
        }

        public float GetGaugePercentage() => gaugeModel.GetGaugePercentage();
        public void SetCanCharge(bool value) => gaugeModel.SetCanCharge(value);
        public void ResetGauge() => gaugeModel.EmptyValue();
        public void FillGauge() => gaugeModel.FillValue();
        public bool IsSpecialAttackReady(float cost) => gaugeModel.HasEnoughValue(cost);
        public bool CanSpecialAttack() => gaugeModel.IsValueFull();

        public bool GetCanCharge() => gaugeModel.CanCharge;
    }
}