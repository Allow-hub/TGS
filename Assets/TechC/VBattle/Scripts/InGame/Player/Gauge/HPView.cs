using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class HPView : ParameterView
    {
        [SerializeField] private float lowHpThreshold = 0.3f;
        [SerializeField] private Color lowHpColor = Color.red;

        protected override void UpdateText(float currentValue, float maxValue)
        {
            valueText.text = $"{Mathf.Round(currentValue)}/{Mathf.Round(maxValue)}";
        }

        public void UpdateHpDisplay(float percentage, float currentHp, float maxHp)
        {
            base.UpdateDisplay(percentage, currentHp, maxHp);

            // HP残量に応じて色を変更
            if (fillImage != null)
            {
                fillImage.color = percentage <= lowHpThreshold ? lowHpColor : normalColor;
            }
        }

        public void ShowDamageEffect()
        {
            // ダメージ時のエフェクト
        }

        public void ShowHealEffect()
        {
            // 回復時のエフェクト
        }
    }
}