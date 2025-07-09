using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class HPView : ParameterView
    {
        [SerializeField] private Image ameIconImage;
        [SerializeField] private Image teramiIconImage;
        [SerializeField] private float lowHpThreshold = 0.3f;
        [SerializeField] private Color lowHpColor = Color.red;
        [SerializeField] private Image backImage;
        [SerializeField] private Color color1p, color2p;

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

        public void SetIcon(CharacterType characterType)
        {
            ameIconImage.gameObject.SetActive(false);
            teramiIconImage.gameObject.SetActive(false);

            switch (characterType)
            {
                case CharacterType.Ame:
                    ameIconImage.gameObject.SetActive(true);
                    break;
                case CharacterType.Terami:
                    teramiIconImage.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        public void SetBack(int id)
        {
            if (id == 1)
            {
                backImage.color = color1p;
            }
            else
            {
                backImage.color = color2p;
            }
        }
    }
}