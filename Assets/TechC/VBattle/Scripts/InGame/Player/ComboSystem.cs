using System.Collections;
using System.Collections.Generic;
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

                // チャージ必須コンボで、チャージ状態でない場合はスキップ
                if (combo.requiresCharge && !characterController.IsChargeEnabled())
                    continue;
                // コンボシーケンスをチェック
                if (CheckComboSequence(combo))
                {
                    Debug.Log("SEC");

                    // コンボ成功！ボーナスゲージを追加
                    characterController.NotBoolAddSpecialGauge(combo.gaugeBonus);

                    // 最後に検出されたコンボ名を記録
                    lastDetectedCombo = combo.comboName;

                    // エフェクトや通知を表示
                    if (showDebugInfo)
                    {
                        Debug.Log($"コンボ「{combo.comboName}」発動! ボーナスゲージ +{combo.gaugeBonus}");
                    }

                    // コンボエフェクト再生
                    PlayComboEffect(combo);

                    // オプション：1回のチェックで最初に見つかったコンボだけを発動させる場合
                    // return; 
                }
            }
        }

        /// <summary>
        /// ComboDataSOに基づいてコンボシーケンスをチェック
        /// </summary>
        private bool CheckComboSequence(ComboDataSO combo)
        {
            // コマンド履歴を取得
            var history = commandHistory.GetFullHistory();
            if (history.Count < combo.sequence.Count) return false;

            // コンボの最新のコマンドから順にチェック
            int historyIndex = history.Count - 1;
            float lastExecutionTime = Time.time;

            // コンボの各ステップを逆順にチェック
            for (int i = combo.sequence.Count - 1; i >= 0; i--)
            {
                bool stepFound = false;

                // コンボステップの情報
                var comboStep = combo.sequence[i];

                // 履歴をさかのぼって適合するコマンドを探す
                while (historyIndex >= 0)
                {
                    var record = history[historyIndex];
                    if (IsExcludedCommand(record, combo))
                    {
                        historyIndex --;
                        continue;
                    }

                    // 時間枠をチェック
                    if (lastExecutionTime - record.executionTime > combo.timeWindow)
                    {
                        // 時間枠を超えたらこのコンボは不成立
                        return false;
                    }

                    // コマンドの種類と強さをチェック
                    if (IsMatchingCommand(record, comboStep))
                    {
                        // 一致するコマンドが見つかった
                        stepFound = true;
                        lastExecutionTime = record.executionTime;
                        break;
                    }

                    historyIndex--;
                }

                if (!stepFound)
                {
                    // 一つでも不一致ならこのコンボは不成立
                    return false;
                }
            }

            // すべてのステップが一致した
            return true;
        }

        private bool IsExcludedCommand(CommandHistory.CommandRecord record, ComboDataSO combo)
        {
            var command = record.commandInstance;
            if (command == null) return false;

            foreach (var excludedType in combo.GetExcludedCommandTypes())
            {
                if (excludedType == command.GetType())
                    return true;
            }
            return false;
        }


        /// <summary>
        /// コマンド履歴がコンボステップと一致するかチェック
        /// </summary>
        private bool IsMatchingCommand(CommandHistory.CommandRecord record, ComboDataSO.ComboStep step)
        {
            // 成功したコマンドのみチェック
            if (!record.wasSuccessful) return false;
            // コマンド名からタイプと強さを解析
            string commandName = record.commandName;
            Debug.Log(commandName);

            // 弱攻撃のチェック
            if (commandName.StartsWith("Weak") && step.attackStrength == AttackManager.AttackStrength.Weak)
            {
                // 攻撃方向のチェック
                return CheckAttackDirection(commandName, step.attackType);
            }

            // 強攻撃のチェック
            if (commandName.StartsWith("Strong") && step.attackStrength == AttackManager.AttackStrength.Strong)
            {
                return CheckAttackDirection(commandName, step.attackType);
            }

            // アピールコマンドのチェック
            if (commandName == "AppealCommand" && step.attackStrength == AttackManager.AttackStrength.Appeal)
            {
                return true; // アピールには方向がないのでtrueを返す
            }

            return false;
        }

        /// <summary>
        /// コマンド名から攻撃方向をチェック
        /// </summary>
        private bool CheckAttackDirection(string commandName, CharacterState.AttackType attackType)
        {
            switch (attackType)
            {
                case CharacterState.AttackType.Neutral:
                    return commandName.Contains("Neutral");
                case CharacterState.AttackType.Left:
                    return commandName.Contains("Left");
                case CharacterState.AttackType.Right:
                    return commandName.Contains("Right");
                case CharacterState.AttackType.Down:
                    return commandName.Contains("Down");
                case CharacterState.AttackType.Up:
                    return commandName.Contains("Up");
                default:
                    return false;
            }
        }

        /// <summary>
        /// コンボ成功時のエフェクト表示
        /// </summary>
        private void PlayComboEffect(ComboDataSO combo)
        {
            // エフェクトの再生
            if (combo.effectPrefab != null)
            {
                Instantiate(combo.effectPrefab, characterController.transform.position, Quaternion.identity);
            }

            // サウンドエフェクトの再生
            if (combo.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(combo.soundEffect, characterController.transform.position);
            }

            // アニメーション実行（オプション）
            if (!string.IsNullOrEmpty(combo.animationTrigger))
            {
                characterController.GetAnim().SetTrigger(combo.animationTrigger);
            }
        }

        /// <summary>
        /// エディタ用にReadOnlyの属性を定義
        /// </summary>
        public class ReadOnlyAttribute : PropertyAttribute { }
    }
}