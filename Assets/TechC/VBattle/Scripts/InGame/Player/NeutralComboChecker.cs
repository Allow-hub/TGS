using System;
using System.Collections;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    /// <summary>
    /// 連続攻撃（コンボ）の管理を担当するクラス
    /// </summary>
    [Serializable]
    public class NeutralComboChecker
    {
        [SerializeField] private float neutralAttackInterval = 3.0f;

        private CommandHistory commandHistory;
        private int neutralAttackCount = 0;

        public NeutralComboChecker(CommandHistory commandHistory)
        {
            this.commandHistory = commandHistory;
        }

        /// <summary>
        /// ニュートラル攻撃のコンボ状態を管理し、適切な攻撃データを返す
        /// </summary>
        public AttackData GetNextNeutralAttackData(AttackData neutralAttack1, AttackData neutralAttack2, AttackData neutralAttack3)
        {
            if (commandHistory.WasCommandExecutedRecently<AttackCommand>(neutralAttackInterval))
            {
                neutralAttackCount++;
                // CustomLogger.Info(neutralAttackCount.ToString(), "comboCheck");
                switch (neutralAttackCount)
                {
                    case 1:
                        return neutralAttack1;
                    case 2:
                        return neutralAttack2;
                    case 3:
                        // コンボ終了でリセット
                        AttackData result = neutralAttack3;
                        neutralAttackCount = 0;
                        return result;
                    default:
                        neutralAttackCount = 0;
                        return neutralAttack1;
                }
            }
            else
            {
                neutralAttackCount = 1;
                return neutralAttack1;
            }
        }

        /// <summary>
        /// コンボカウントをリセット
        /// </summary>
        public void ResetCombo()
        {
            neutralAttackCount = 0;
        }
    }
}