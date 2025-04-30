using System;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// Presenterの共通基底クラス
    /// </summary>
    public abstract class ParameterPresenter : MonoBehaviour
    {
        protected ParameterModel model;
        protected ParameterView view;

        [SerializeField] protected float maxValue = 100f;

        public event Action OnValueFilled;
        public event Action OnValueEmpty;

        public virtual void Init()
        {
            InitializeModel();
            InitializeView();

            // モデルのイベントをサブスクライブ
            model.OnValueChanged += HandleValueChanged;
            model.OnValueFilled += HandleValueFilled;
            model.OnValueEmpty += HandleValueEmpty;
        }

        protected abstract void InitializeModel();
        protected abstract void InitializeView();

        protected virtual void HandleValueChanged(float newValue)
        {
            view.UpdateDisplay(model.Percentage, newValue, model.MaxValue);
        }

        protected virtual void HandleValueFilled()
        {
            OnValueFilled?.Invoke();
        }

        protected virtual void HandleValueEmpty()
        {
            OnValueEmpty?.Invoke();
        }

        // 共通外部インターフェース
        public virtual float GetPercentage() => model.Percentage;
        public virtual float GetCurrentValue() => model.CurrentValue;
        public virtual float GetMaxValue() => model.MaxValue;
        public virtual void SetChangeRate(float rate) => model.SetChangeRate(rate);
    }
}