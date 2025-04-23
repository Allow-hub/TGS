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
    public class GaugeView : MonoBehaviour
    {
        [SerializeField] private Slider gaugeSlider;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private GameObject fullGaugeEffectObject;
        [SerializeField] private Image fillImage; // Sliderのfill imageへの参照

        [Header("アニメーション設定")]
        [SerializeField] private float smoothFillSpeed = 5f;
        [SerializeField] private Color normalColor = Color.blue;
        [SerializeField] private Color fullColor = Color.yellow;

        private float targetValue = 0f;

        private void Start()
        {
            // Sliderのfill imageが設定されていない場合、自動的に取得
            if (fillImage == null)
            {
                fillImage = gaugeSlider.fillRect.GetComponent<Image>();
            }
            fillImage.color = normalColor;
        }

        private void Update()
        {
            // スムーズなゲージ表示のアニメーション
            if (gaugeSlider.value != targetValue)
            {
                gaugeSlider.value = Mathf.Lerp(
                    gaugeSlider.value,
                    targetValue,
                    Time.deltaTime * smoothFillSpeed);
            }
        }

        /// <summary>
        /// ゲージの表示を更新
        /// </summary>
        /// <param name="percentage"></param>
        public void UpdateGaugeDisplay(float percentage)
        {
            targetValue = percentage;

            // テキスト表示を更新
            if (percentageText != null)
            {
                percentageText.text = $"{Mathf.Round(percentage * 100)}%";
            }
        }

        /// <summary>
        /// ゲージ満タン時のエフェクト表示
        /// </summary>
        /// <param name="isActive"></param>
        public void ShowFullGaugeEffect(bool isActive)
        {
            if (fullGaugeEffectObject != null)
            {
                fullGaugeEffectObject.SetActive(isActive);
            }

            // 満タン時の色変更
            if (fillImage != null)
            {
                fillImage.color = isActive ? fullColor : normalColor;
            }
        }

        /// <summary>
        /// ゲージが空になった時のエフェクト
        /// </summary>
        public void ShowEmptyGaugeEffect()
        {
            // 空になった時のアニメーションなど
            if (fillImage != null)
            {
                fillImage.color = normalColor;
            }
        }
    }
}