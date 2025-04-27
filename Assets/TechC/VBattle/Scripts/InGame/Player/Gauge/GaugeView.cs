using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace TechC
{
    /// <summary>
    /// ゲージのUI表示を担当するビュー
    /// 今回のプロジェクトでは必要ないかもしれないが勉強のためにMVPを使う
    /// </summary>
    public class GaugeView : ParameterView
    {
        [SerializeField] private GameObject fullGaugeEffectObject;

        protected override void UpdateText(float currentValue, float maxValue)
        {
            valueText.text = $"{Mathf.Round(currentValue / maxValue * 100)}%";
        }

        public void ShowFullGaugeEffect(bool isActive)
        {
            if (fullGaugeEffectObject != null)
            {
                fullGaugeEffectObject.SetActive(isActive);
            }

            ChangeColor(isActive ? highlightColor : normalColor);
        }

        /// <summary>
        /// 空になったときにエフェクト
        /// </summary>
        public void ShowEmptyGaugeEffect()
        {
            ChangeColor(normalColor);
        }
    }
}