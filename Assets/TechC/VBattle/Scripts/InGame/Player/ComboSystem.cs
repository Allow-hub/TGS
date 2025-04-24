using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コンボシステムを管理するクラス
    /// ScriptableObjectを使用してコンボを定義する
    /// </summary>
    public class ComboSystem : MonoBehaviour
    {
        [SerializeField] private CommandHistory commandHistory;
        [SerializeField] private Player.CharacterController characterController;

        // コンボデータリスト
        [SerializeField] private List<ComboDataSO> combos = new List<ComboDataSO>();

        // Unity Inspectorで表示するためのデバッグ情報
        [SerializeField, ReadOnly] private string lastDetectedCombo = "なし";
        [SerializeField] private bool showDebugInfo = true;

        private void Awake()
        {
            // コンボデータの検証
            ValidateCombos();
        }

        private void ValidateCombos()
        {
            foreach (var combo in combos)
            {
                if (combo.sequence.Count == 0)
                {
                    Debug.LogWarning($"コンボ「{combo.comboName}」の攻撃シーケンスが空です");
                }
            }
        }

        /// <summary>
        /// 攻撃ヒット時に呼び出し、登録されたコンボをチェックする
        /// </summary>
        public void CheckCombos()
        {
            foreach (var combo in combos)
            {
                if (combo.requiresCharge && !characterController.IsChargeEnabled())
                    continue;

                if (CheckComboSequence(combo))
                {
                    Debug.Log($"コンボシーケンス確認: {combo.comboName}");
                    characterController.NotBoolAddSpecialGauge(combo.gaugeBonus);
                    lastDetectedCombo = combo.comboName;

                    if (showDebugInfo)
                    {
                        Debug.Log($"コンボ「{combo.comboName}」発動! ボーナスゲージ +{combo.gaugeBonus}");
                    }

                    PlayComboEffect(combo);

                    break;
                }
            }
        }


        /// <summary>
        /// ComboDataSOに基づいてコンボシーケンスをチェック
        /// </summary>
        private bool CheckComboSequence(ComboDataSO combo)
        {
            var history = commandHistory.GetFullHistory();
            if (history.Count < combo.sequence.Count) return false;

            float lastMatchedTime = float.MaxValue;
            int matchCount = 0;
            List<CommandHistory.CommandRecord> usedRecords = new();
            //if (showDebugInfo)
            //{
            //    Debug.Log($"コンボチェック - コンボ名: {combo.comboName}");
            //    Debug.Log($"必要なシーケンス: {string.Join(" -> ", combo.sequence.Select(s => $"{s.attackType}/{s.attackStrength}"))}");
            //    Debug.Log($"実際の履歴: {string.Join(" -> ", history.Select(h => $"{h.commandName}({h.executionTime:F2})"))}");
            //}


            for (int stepIndex = combo.sequence.Count - 1; stepIndex >= 0; stepIndex--)
            {
                var step = combo.sequence[stepIndex];
                bool found = false;
                Debug.Log(stepIndex);
                for (int i = history.Count - 1; i >= 0; i--)
                {
                    var record = history[i];
                    if (IsExcludedCommand(record, combo)) continue;
                    if (!record.wasSuccessful) continue;
                    if (!IsMatchingCommand(record, step)) continue;

                    if (record.executionTime >= lastMatchedTime) continue;
                    float timeBetween = lastMatchedTime - record.executionTime;
                    if (timeBetween > combo.timeWindow) continue;
                    Debug.Log(record.attackType);
                    lastMatchedTime = record.executionTime;
                    usedRecords.Add(record); 
                    found = true;
                    matchCount++;
                    break;
                }

                if (!found)
                {
                    return false;
                }
            }

            foreach (var record in usedRecords)
            {
                record.wasUsedForCombo = true;
            }

            return true;
        }





        private bool IsExcludedCommand(CommandHistory.CommandRecord record, ComboDataSO combo)
        {
            var command = record.commandInstance;
            if (record.wasUsedForCombo) return true;
            if (command == null) return false;

            foreach (var excludedType in combo.GetExcludedCommandTypes())
            {
                if (excludedType == command.GetType()) return true;
            }
            return false;
        }

        /// <summary>
        /// コマンド履歴がコンボステップと一致するかチェック
        /// </summary>
        private bool IsMatchingCommand(CommandHistory.CommandRecord record, ComboDataSO.ComboStep step)
        {
            if (!record.wasSuccessful) return false;

            if (record.commandInstance is AttackCommand attackCommand)
            {
                bool typeMatches = attackCommand.Type == step.attackType;
                bool strengthMatches = attackCommand.Strength == step.attackStrength;

                if (typeMatches && strengthMatches)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"コマンド一致: {record.commandName} (Type: {attackCommand.Type}, Strength: {attackCommand.Strength})");
                    }
                    return true;
                }
                return false;
            }

            if (record.commandName == "AppealCommand" && step.attackStrength == AttackManager.AttackStrength.Appeal)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// コンボ成功時のエフェクト表示
        /// </summary>
        private void PlayComboEffect(ComboDataSO combo)
        {
            if (combo.effectPrefab != null)
            {
                Instantiate(combo.effectPrefab, characterController.transform.position, Quaternion.identity);
            }

            if (combo.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(combo.soundEffect, characterController.transform.position);
            }

            if (!string.IsNullOrEmpty(combo.animationTrigger))
            {
                characterController.GetAnim().SetTrigger(combo.animationTrigger);
            }
        }
    }
}