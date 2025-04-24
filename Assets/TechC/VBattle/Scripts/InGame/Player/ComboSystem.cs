using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TechC.CommandHistory;

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
        [SerializeField] private GameObject comboEffect;
        [SerializeField] private float effectActiveTime;

        // コンボデータリスト
        [SerializeField] private List<ComboDataSO> combos = new List<ComboDataSO>();

        // Unity Inspectorで表示するためのデバッグ情報
        [SerializeField, ReadOnly] private string lastDetectedCombo = "なし";
        [SerializeField, ReadOnly] private string comboCheckLogId = "comboCheck";
        [SerializeField, ReadOnly] private string comboClearLogId = "comboClear";
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
            if (showDebugInfo)
            {
                CustomLogger.Info("コンボチェック開始 ----------------" + Time.time, comboCheckLogId);

                var history = commandHistory.GetFullHistory(5);
                //CustomLogger.Info($"最新履歴: {string.Join(" -> ", history.Select(h => $"{h.attackType}/{h.attackStrength}/{h.executionTime:F2}"))}", comboCheckLogId);

                // 除外された履歴
                List<CommandRecord> excludedRecords = new();
                List<CommandRecord> includedRecords = new();

                foreach (var record in history)
                {
                    bool isExcluded = false;
                    foreach (var combo in combos)
                    {
                        if (IsExcludedCommand(record, combo))
                        {
                            excludedRecords.Add(record);
                            //Debug.Log($"除外された履歴: {record.attackType}/{record.attackStrength}/{record.executionTime:F2} → 除外理由: {record.commandInstance?.GetType()?.Name} が {combo.comboName} の除外対象");
                            isExcluded = true;
                            break;
                        }
                    }

                    if (!isExcluded)
                    {
                        includedRecords.Add(record);
                    }
                }

                // 除外されていない履歴のみの表示
                CustomLogger.Info($"除外された状態の最新履歴: {string.Join(" -> ", includedRecords.Select(r => $"{r.attackType}/{r.attackStrength}/{r.executionTime:F2}"))}", comboCheckLogId);
            }



            foreach (var combo in combos)
            {
                if (combo.requiresCharge && !characterController.IsChargeEnabled())
                    continue;
                bool result = CheckComboSequence(combo);
                CustomLogger.Info($"CheckComboSequence({combo.comboName}) = {result}", comboCheckLogId);

                if (result)
                {
                    CustomLogger.Info($"コンボシーケンス確認: {combo.comboName}", comboClearLogId);
                    characterController.NotBoolAddSpecialGauge(combo.gaugeBonus);
                    lastDetectedCombo = combo.comboName;

                    if (showDebugInfo)
                    {
                        CustomLogger.Info($"コンボ「{combo.comboName}」発動! ボーナスゲージ +{combo.gaugeBonus}", comboClearLogId);
                    }

                    PlayComboEffect(combo);
                    break;
                }
            }

            if (showDebugInfo)
            {
                CustomLogger.Info("コンボチェック終了 ----------------", comboCheckLogId);
            }
        }


        /// <summary>
        /// ComboDataSOに基づいてコンボシーケンスをチェック
        /// </summary>
        private bool CheckComboSequence(ComboDataSO combo)
        {
            var history = commandHistory.GetFullHistory();
            if (history.Count < combo.sequence.Count) return false;

            // デバッグ情報表示
            if (showDebugInfo)
            {

                CustomLogger.Info($"コンボチェック - コンボ名: {combo.comboName}{Time.time}", comboCheckLogId);
                CustomLogger.Info($"必要なシーケンス: {string.Join(" -> ", combo.sequence.Select(s => $"{s.attackType}/{s.attackStrength}"))}", comboCheckLogId);
                CustomLogger.Info($"実際の履歴: {string.Join(" -> ", history.Select(h => $"{h.attackType}/{h.attackStrength}({h.executionTime:F2})"))}", comboCheckLogId);
            }

            // 履歴の中から連続するコンボとして一致するものを探す
            List<CommandHistory.CommandRecord> matchedRecords = new List<CommandHistory.CommandRecord>();

            // 履歴を新しい順に検索
            int currentHistoryIndex = history.Count - 1;

            // コンボシーケンスを後ろから（最新の動作から）確認
            for (int comboStepIndex = combo.sequence.Count - 1; comboStepIndex >= 0; comboStepIndex--)
            {
                var step = combo.sequence[comboStepIndex];
                bool foundMatch = false;

                // 履歴を新しいものから順に検索
                while (currentHistoryIndex >= 0)
                {
                    var record = history[currentHistoryIndex];
                    currentHistoryIndex--;

                    // 既にコンボに使われたものはスキップ
                    if (record.wasUsedForCombo) continue;

                    // 除外コマンドはスキップ
                    if (IsExcludedCommand(record, combo)) continue;

                    // 成功していないものはスキップ
                    if (!record.wasSuccessful) continue;

                    // タイプと強度が一致するか確認
                    if (record.attackType == step.attackType && record.attackStrength == step.attackStrength)
                    {
                        // 時間内か確認（リストが空でなければ前のコマンドとの時間差をチェック）
                        if (matchedRecords.Count > 0)
                        {
                            float timeDiff = record.executionTime - matchedRecords[matchedRecords.Count - 1].executionTime;
                            if (timeDiff > combo.timeWindow) continue;
                        }

                        // 一致したら記録して次のステップへ
                        matchedRecords.Add(record);
                        foundMatch = true;
                        break;
                    }
                }

                // 一つでも一致しなければコンボ不成立
                if (!foundMatch) return false;
            }

            // すべてのステップが一致したら、使用済みとマーク
            foreach (var record in matchedRecords)
            {
                record.wasUsedForCombo = true;
            }

            if (showDebugInfo)
            {
                CustomLogger.Info($"コンボ成功: {combo.comboName}", comboClearLogId);
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

            // 直接attackTypeとattackStrengthを比較
            bool typeMatches = record.attackType == step.attackType;
            bool strengthMatches = record.attackStrength == step.attackStrength;
            if (typeMatches && strengthMatches)
            {
                if (showDebugInfo)
                {
                    CustomLogger.Info($"コマンド一致: {record.commandName} (Type: {record.attackType}, Strength: {record.attackStrength})", comboCheckLogId);
                }
                return true;
            }

            // Appealコマンドの特別処理
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
                comboEffect.SetActive(true);
                StopAllCoroutines();
                StartCoroutine(ResetEffect());
            }

            if (combo.soundEffect != null)
            {
            }   
        }

        private IEnumerator ResetEffect()
        {
            yield return new WaitForSeconds(effectActiveTime);
            comboEffect.SetActive(false);
        }
    }
}