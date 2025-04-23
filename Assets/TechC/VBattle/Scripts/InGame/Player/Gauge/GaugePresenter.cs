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
    public class GaugePresenter : MonoBehaviour
    {
        [SerializeField] private GaugeView gaugeView;
        [SerializeField] private float maxGauge = 100f;
        [SerializeField] private float timeBasedChargeInterval = 1.0f;
        [SerializeField] private float timeBasedChargeAmount = 2.0f;

        private GaugeModel gaugeModel;

        /// <summary>
        /// ゲージがマックスになったとき
        /// </summary>
        public event Action OnGaugeFilled;

        /// <summary>
        /// ゲージが空になったとき
        /// </summary>
        public event Action OnGaugeEmpty;

        private void Awake()
        {
            gaugeModel = new GaugeModel(maxGauge);

            // モデルのイベントをサブスクライブ
            gaugeModel.OnGaugeChanged += HandleGaugeChanged;
            gaugeModel.OnGaugeFilled += HandleGaugeFilled;
            gaugeModel.OnGaugeEmpty += HandleGaugeEmpty;
        }

        private void FixedUpdate()
        {
            // 時間経過によるゲージ増加
            gaugeModel.UpdateTimeBasedCharge(Time.fixedDeltaTime, timeBasedChargeInterval, timeBasedChargeAmount);
        }

        /// <summary>
        /// ゲージが変化した時の処理
        /// </summary>
        /// <param name="newValue"></param>
        private void HandleGaugeChanged(float newValue)
        {
            gaugeView.UpdateGaugeDisplay(gaugeModel.GaugePercentage);
        }

        /// <summary>
        /// ゲージが満タンになった時の処理
        /// </summary>
        private void HandleGaugeFilled()
        {
            gaugeView.ShowFullGaugeEffect(true);
            OnGaugeFilled?.Invoke();
        }

        /// <summary>
        /// ゲージが空になった時の処理
        /// </summary>
        private void HandleGaugeEmpty()
        {
            gaugeView.ShowFullGaugeEffect(false);
            gaugeView.ShowEmptyGaugeEffect();
            OnGaugeEmpty?.Invoke();
        }

        /// <summary>
        /// 外部からゲージを増加させるメソッド
        /// </summary>
        /// <param name="amount"></param>
        public void AddGauge(float amount)
        {
            gaugeModel.AddGauge(amount);
        }
        /// <summary>
        /// 外部からゲージを増加させるメソッド,ブール値に依存しない
        /// </summary>
        /// <param name="amount"></param>
        public void NotBoolAddGauge(float amount)
        {
            gaugeModel.AddGaugeInternal(amount);
        }
        /// <summary>
        /// 必殺技使用時のゲージ消費
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public bool TryUseSpecialAttack(float cost)
        {
            return gaugeModel.UseGauge(cost);
        }

        /// <summary>
        /// 外部からの制御用メソッド
        /// </summary>
        /// <param name="value"></param>
        public void SetCanCharge(bool value) => gaugeModel.SetCanCharge(value);
        public void ResetGauge() => gaugeModel.ResetGauge();
        public void FillGauge() => gaugeModel.FillGauge();
        public bool IsSpecialAttackReady(float cost) => gaugeModel.HasEnoughGauge(cost);
        public float GetGaugePercentage() => gaugeModel.GaugePercentage;
    }
}
