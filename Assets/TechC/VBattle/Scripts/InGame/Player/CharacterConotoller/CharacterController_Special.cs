using UnityEngine;

namespace TechC.Player
{
    /// <summary>
    /// CharacterController_Special.cs
    /// 必殺技の分離クラス
    /// </summary>
    public partial class CharacterController
    {
        /// <summary>
        /// 必殺技ゲージを増加させる
        /// </summary>
        public void AddSpecialGauge(float amount)
        {
            if (gaugePresenter != null)
            {
                gaugePresenter.AddGauge(amount);
            }
            else
            {
                Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、ゲージ追加ができません");
            }
        }

        /// <summary>
        /// 必殺技ゲージを増加させる、bool値を問わず
        /// </summary>
        public void NotBoolAddSpecialGauge(float amount)
        {
            if (gaugePresenter != null)
            {
                gaugePresenter.NotBoolAddGauge(amount);
            }
            else
            {
                Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、ゲージ追加ができません");
            }
        }

        /// <summary>
        /// 必殺技を使用する（使用可能な場合のみ成功）
        /// </summary>
        public bool TryUseSpecialAttack(float cost)
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.TryUseSpecialAttack(cost);
            }
            Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、必殺技使用ができません");
            return false;
        }

        public void ResetSpecial() => gaugePresenter.ResetGauge();

        /// <summary>
        /// 必殺技ゲージの割合を取得（UI表示用など）
        /// </summary>
        public float GetSpecialGaugePercentage()
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.GetGaugePercentage();
            }
            Debug.LogWarning($"Player {playerID}: GaugePresenterがnullのため、ゲージ割合が取得できません");
            return 0f;
        }

        /// <summary>
        /// 必殺技が使用可能かどうか
        /// </summary>
        public bool CanSpecialAttack()
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.CanSpecialAttack();
            }
            Debug.LogWarning($"Player {playerID}: GaugePresenterがnullのため、必殺技準備状態が確認できません");
            return false;
        }

        /// <summary>
        /// 必殺技がチャージ可能かどうかを切り替える
        /// </summary>
        public void ChangeCanCharge(bool value)
        {
            if (gaugePresenter != null)
            {
                gaugePresenter.SetCanCharge(value);
            }
            else
            {
                Debug.LogError($"Player {playerID}: GaugePresenterがnullのため、チャージ状態を変更できません");
            }
        }

        /// <summary>
        /// チャージ可能状態かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsChargeEnabled()
        {
            if (gaugePresenter != null)
            {
                return gaugePresenter.GetCanCharge();
            }
            Debug.LogWarning($"Player {playerID}: GaugePresenterがnullのため、チャージ状態が確認できません");
            return false;
        }
    }
}
