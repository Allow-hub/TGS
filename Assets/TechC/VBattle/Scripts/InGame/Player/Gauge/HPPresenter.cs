using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class HPPresenter : ParameterPresenter
    {
        [SerializeField] private CharacterData characterData;

        [SerializeField] private HPView hpView;
        [SerializeField] private bool enableAutoRegeneration = false;//自動HP回復機能を有効にするかどうかのフラグ
        [SerializeField] private float regenerationInterval = 1.0f;//自動回復の間隔（秒）
        [SerializeField] private float regenerationAmount = 1.0f;//自動回復量

        private HPModel hpModel;
        private float elapsedTime = 0f;

        public event Action OnDeath;

        protected override void InitializeModel()
        {
            hpModel = new HPModel(characterData.Hp);
            model = hpModel;
            hpModel.FillValue();
        }

        protected override void InitializeView()
        {
            view = hpView;
            HandleValueChanged(GetMaxValue());
        }

        private void FixedUpdate()
        {
            // 自動回復が有効な場合、一定間隔でHP回復
            if (enableAutoRegeneration && !hpModel.IsValueEmpty())
            {
                elapsedTime += Time.fixedDeltaTime;
                if (elapsedTime >= regenerationInterval)
                {
                    hpModel.Heal(regenerationAmount);
                    elapsedTime = 0f;
                }
            }
        }

        protected override void HandleValueChanged(float newValue)
        {
            hpView.UpdateHpDisplay(model.Percentage, newValue, model.MaxValue);
        }

        protected override void HandleValueEmpty()
        {
            base.HandleValueEmpty();
            hpView.ShowDamageEffect();
            OnDeath?.Invoke();
        }

        protected override void HandleValueFilled()
        {
            base.HandleValueFilled();
            hpView.ShowHealEffect();
        }

        // HP特有のメソッド
        public void TakeDamage(float damage)
        {
            hpModel.TakeDamage(damage);
        }

        public void Heal(float amount)
        {
            hpModel.Heal(amount);
        }

        public void FullHeal() => hpModel.FillValue();
        public bool IsDead() => hpModel.IsValueEmpty();
    }
}