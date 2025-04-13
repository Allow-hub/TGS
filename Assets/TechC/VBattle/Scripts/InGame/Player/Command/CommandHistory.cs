using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コマンド履歴を管理するクラス
    /// 実行されたコマンドを保存し、履歴の取得や分析を可能にする
    /// </summary>
    public class CommandHistory : MonoBehaviour
    {
        // コマンド履歴の最大保存数
        [SerializeField] private int maxHistorySize = 50;

        // コマンド情報を保持する構造体
        [Serializable]
        public class CommandRecord
        {
            public string commandName;      // コマンド名
            public string stateName;        // 実行時の状態名
            public float executionTime;     // 実行時間
            public bool wasSuccessful;      // 成功したか
            public Vector3 playerPosition;  // 実行時のプレイヤー位置

            public CommandRecord(ICommand command, string stateName, bool wasSuccessful, Vector3 position)
            {
                this.commandName = command.GetType().Name;
                this.stateName = stateName;
                this.executionTime = Time.time;
                this.wasSuccessful = wasSuccessful;
                this.playerPosition = position;
            }

            public override string ToString()
            {
                return $"[{executionTime:F2}] {commandName} @ {stateName} - {(wasSuccessful ? "成功" : "失敗")}";
            }
        }

        // コマンド履歴リスト
        private List<CommandRecord> commandHistory = new List<CommandRecord>();

        // デバッグ表示用
        [SerializeField] private bool showDebugLog = true;
        [SerializeField] private bool showGUI = false;
        [SerializeField] private int displayCount = 10;

        /// <summary>
        /// コマンド実行を記録する
        /// </summary>
        /// <param name="command">実行されたコマンド</param>
        /// <param name="stateName">現在の状態名</param>
        /// <param name="wasSuccessful">コマンドが成功したか</param>
        /// <param name="position">プレイヤーの位置</param>
        public void RecordCommand(ICommand command, string stateName, bool wasSuccessful, Vector3 position)
        {
            if (command == null) return;

            CommandRecord record = new CommandRecord(command, stateName, wasSuccessful, position);

            // 履歴に追加
            commandHistory.Add(record);

            // 最大サイズを超えたら古いものから削除
            if (commandHistory.Count > maxHistorySize)
            {
                commandHistory.RemoveAt(0);
            }

            // デバッグログ表示
            if (showDebugLog)
            {
                Debug.Log($"Command Executed: {record}");
            }
        }

        /// <summary>
        /// 指定したタイプのコマンドが最近実行されたかチェック
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <param name="timeWindow">チェックする時間範囲（秒）</param>
        /// <returns>指定時間内に実行されていればtrue</returns>
        public bool WasCommandExecutedRecently<T>(float timeWindow = 1.0f) where T : ICommand
        {
            string typeName = typeof(T).Name;
            float currentTime = Time.time;

            for (int i = commandHistory.Count - 1; i >= 0; i--)
            {
                if (currentTime - commandHistory[i].executionTime > timeWindow)
                    break; // 時間範囲を超えたら終了

                if (commandHistory[i].commandName == typeName && commandHistory[i].wasSuccessful)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 特定の状態で実行されたコマンドの履歴を取得
        /// </summary>
        /// <param name="stateName">状態名</param>
        /// <param name="count">取得する数</param>
        /// <returns>コマンド履歴リスト</returns>
        public List<CommandRecord> GetHistoryByState(string stateName, int count = 10)
        {
            List<CommandRecord> result = new List<CommandRecord>();

            for (int i = commandHistory.Count - 1; i >= 0 && result.Count < count; i--)
            {
                if (commandHistory[i].stateName == stateName)
                {
                    result.Add(commandHistory[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// すべてのコマンド履歴を取得
        /// </summary>
        /// <param name="count">取得する数（デフォルトは全て）</param>
        /// <returns>コマンド履歴リスト</returns>
        public List<CommandRecord> GetFullHistory(int count = 0)
        {
            if (count <= 0 || count > commandHistory.Count)
                return new List<CommandRecord>(commandHistory);

            return commandHistory.GetRange(commandHistory.Count - count, count);
        }

        /// <summary>
        /// 履歴をクリア
        /// </summary>
        public void ClearHistory()
        {
            commandHistory.Clear();
        }

        /// <summary>
        /// コマンドの成功率を計算
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <returns>成功率（0～1）</returns>
        public float GetCommandSuccessRate<T>() where T : ICommand
        {
            string typeName = typeof(T).Name;
            int total = 0;
            int success = 0;

            foreach (var record in commandHistory)
            {
                if (record.commandName == typeName)
                {
                    total++;
                    if (record.wasSuccessful)
                        success++;
                }
            }

            return total > 0 ? (float)success / total : 0f;
        }

        /// <summary>
        /// 特定のコンボが実行されたかチェック
        /// </summary>
        /// <param name="commandTypes">チェックするコマンドの型の配列</param>
        /// <param name="maxTimeBetweenCommands">コマンド間の最大許容時間</param>
        /// <returns>コンボが検出されたかどうか</returns>
        public bool CheckCombo(Type[] commandTypes, float maxTimeBetweenCommands = 1.0f)
        {
            if (commandTypes.Length == 0 || commandHistory.Count < commandTypes.Length)
                return false;

            int historyIndex = commandHistory.Count - 1;
            for (int i = commandTypes.Length - 1; i >= 0; i--)
            {
                bool found = false;
                float lastTime = i < commandTypes.Length - 1 ?
                    commandHistory[historyIndex + 1].executionTime :
                    float.MaxValue;

                // 時間内のコマンドを探す
                while (historyIndex >= 0)
                {
                    var record = commandHistory[historyIndex];
                    if (record.commandName == commandTypes[i].Name &&
                        record.wasSuccessful &&
                        (lastTime - record.executionTime) <= maxTimeBetweenCommands)
                    {
                        found = true;
                        break;
                    }
                    historyIndex--;
                }

                if (!found)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 最新のコマンドの名前を拾う
        /// </summary>
        /// <returns></returns>
        public string GetLatestCommandName()=> commandHistory[0].commandName;

        // デバッグ用GUI表示
        private void OnGUI()
        {
            if (!showGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 25 * displayCount));
            GUILayout.Label("コマンド履歴:");

            int startIndex = Mathf.Max(0, commandHistory.Count - displayCount);
            for (int i = startIndex; i < commandHistory.Count; i++)
            {
                GUILayout.Label(commandHistory[i].ToString());
            }

            GUILayout.EndArea();
        }
    }
}