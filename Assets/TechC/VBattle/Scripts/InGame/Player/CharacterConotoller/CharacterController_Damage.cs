using System;
using UnityEngine;

namespace TechC.Player
{
    /// <summary>
    /// CharacterController_Damages.cs
    /// ダメージ関連の分離クラス
    /// </summary>
    public partial class CharacterController
    {
        // ------------------------------
        // IDamageable 用メソッド
        // ------------------------------

        public void TakeDamage(float damage)
        {
            PresenterTakeDamage(damage);
        }

        public void Des()
        {
            HandleDeath();
        }

        /// <summary>
        /// HP値を取得する
        /// </summary>
        public float GetHp()
        {
            //複製キャラの場合HP1を返す
            if (isClonePlayer) return 1;

            // hpPresenterがnullでないことを確認
            if (hpPresenter != null)
            {
                return hpPresenter.GetCurrentValue();
            }

            Debug.LogWarning($"Player {playerID}: HPPresenterが見つかりません。デフォルト値を返します。");
            return 0f;
        }

        public void HealHp(float value) => hpPresenter.Heal(value);

        /// <summary>
        /// ダメージを受ける処理
        /// </summary>
        public void PresenterTakeDamage(float damage)
        {
            // hpPresenterがnullでないことを確認
            if (hpPresenter != null)
            {
                hpPresenter.TakeDamage(damage * opponentController.GetMultipiler(BuffType.Attack));
            }
            else
            {
                Debug.LogError($"Player {playerID}: HPPresenterがnullのため、ダメージ処理ができません");
            }
        }

        /// <summary>
        /// キャラクター死亡時の処理
        /// </summary>
        private void HandleDeath()
        {
            BattleJudge.I.PlayerDeath(playerID);
        }

        // ------------------------------
        // カウンター関連
        // ------------------------------

        public void SetCanCounter(bool val) => canCounter = val;

        public void SetCounterAction(Action action) => onCounter = action;
        public void ResetCounterAction() => onCounter = null;
        public void UseCounter()
        {
            if (onCounter == null) return;
            SetCanCounter(false);
            var action = onCounter;
            onCounter = null;
            action.Invoke();
        }

        // ------------------------------
        // ヒットデータ管理
        // ------------------------------

        /// <summary>
        /// 最後に受けた攻撃データを設定
        /// </summary>
        public void SetLastHitData(HitData hitData) => lastHitData = hitData;

        /// <summary>
        /// 最後に受けた攻撃データを取得
        /// </summary>
        public HitData GetLastHitData() => lastHitData;
    }
}
