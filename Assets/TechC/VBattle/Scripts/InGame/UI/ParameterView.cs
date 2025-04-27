using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// Viewクラスの共通基底クラス
    /// </summary>
    public abstract class ParameterView : MonoBehaviour
    {
        [SerializeField] protected Slider valueSlider;
        [SerializeField] protected TextMeshProUGUI valueText;
        [SerializeField] protected Image fillImage;

        [Header("アニメーション設定")]
        [SerializeField] protected float smoothFillSpeed = 5f;
        [SerializeField] protected Color normalColor = Color.blue;
        [SerializeField] protected Color highlightColor = Color.yellow;

        protected float targetValue = 0f;

        protected virtual void Start()
        {
            if (fillImage == null && valueSlider != null)
            {
                fillImage = valueSlider.fillRect.GetComponent<Image>();
            }
            if (fillImage != null)
            {
                fillImage.color = normalColor;
            }
        }

        protected virtual void Update()
        {
            if (valueSlider != null && valueSlider.value != targetValue)
            {
                valueSlider.value = Mathf.Lerp(
                    valueSlider.value,
                    targetValue,
                    Time.deltaTime * smoothFillSpeed);
            }
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        public virtual void UpdateDisplay(float percentage, float currentValue, float maxValue)
        {
            targetValue = percentage;

            if (valueText != null)
            {
                UpdateText(currentValue, maxValue);
            }
        }

        protected abstract void UpdateText(float currentValue, float maxValue);

        /// <summary>
        /// 色を変更
        /// </summary>
        public virtual void ChangeColor(Color color)
        {
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }
    }
}
