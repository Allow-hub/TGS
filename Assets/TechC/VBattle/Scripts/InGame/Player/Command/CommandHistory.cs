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
        [Serializable]
        public class CommandRecord
        {
            public string commandName;                           // コマンド名（型名）
            public string stateName;                             // 実行時の状態名
            public float executionTime;                          // 実行時間
            public bool wasSuccessful;                           // 成功したか
            public Vector3 playerPosition;                       // 実行時のプレイヤー位置
            public ICommand commandInstance;                     // ICommandインスタンス本体
            public bool wasUsedForCombo;

            // 攻撃コマンド用の追加情報
            public CharacterState.AttackType attackType;         // 攻撃タイプ
            public AttackManager.AttackStrength attackStrength;  // 攻撃強度
            public string commandSignature;                      // 攻撃コマンドの識別子

            public CommandRecord(ICommand command, string stateName, bool wasSuccessful, Vector3 position)
            {
                commandName = command.GetType().Name;
                commandInstance = command;
                this.stateName = stateName;
                this.executionTime = Time.time;
                this.wasSuccessful = wasSuccessful;
                this.playerPosition = position;

                // 攻撃コマンドの場合は追加情報を取得
                if (command is AttackCommand attackCommand)
                {
                    attackType = attackCommand.Type;
                    attackStrength = attackCommand.Strength;
                    commandSignature = attackCommand.GetCommandSignature();
                }
            }

            public override string ToString()
            {
                if (commandInstance is AttackCommand)
                {
                    return $"[{executionTime:F2}] {commandName} ({attackStrength}_{attackType}) @ {stateName} - {(wasSuccessful ? "成功" : "失敗")}";
                }
                return $"[{executionTime:F2}] {commandName} @ {stateName} - {(wasSuccessful ? "成功" : "失敗")}";
            }
        }

        [SerializeField] private int maxHistorySize = 50;
        private List<CommandRecord> commandHistory = new();

        [SerializeField] private bool showDebugLog = true;
        [SerializeField] private bool showGUI = false;
        [SerializeField] private int displayCount = 10;

        /// <summary>
        /// コマンド実行を記録
        /// </summary>
        /// <param name="command"></param>
        /// <param name="stateName"></param>
        /// <param name="wasSuccessful"></param>
        /// <param name="position"></param>
        public void RecordCommand(ICommand command, string stateName, bool wasSuccessful, Vector3 position)
        {
            if (command == null) return;

            var record = new CommandRecord(command, stateName, wasSuccessful, position);
            commandHistory.Add(record);

            if (commandHistory.Count > maxHistorySize)
                commandHistory.RemoveAt(0);

            if (showDebugLog)
                Debug.Log(record);
        }

        /// <summary>
        /// 指定時間内に特定の型のコマンドが成功したか
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="timeWindow"></param>
        /// <returns></returns>
        public bool WasCommandExecutedRecently<T>(float timeWindow = 1.0f) where T : ICommand
        {
            float now = Time.time;
            string typeName = typeof(T).Name;
            for (int i = commandHistory.Count - 1; i >= 0; i--)
            {
                var r = commandHistory[i];
                if (now - r.executionTime > timeWindow) break;
                if (r.commandName == typeName && r.wasSuccessful) return true;
            }
            return false;
        }

        /// <summary>
        /// 指定の状態で実行された履歴を取得
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<CommandRecord> GetHistoryByState(string stateName, int count = 10)
        {
            List<CommandRecord> result = new();
            for (int i = commandHistory.Count - 1; i >= 0 && result.Count < count; i--)
            {
                if (commandHistory[i].stateName == stateName)
                    result.Add(commandHistory[i]);
            }
            return result;
        }

        /// <summary>
        /// 全履歴を取得
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<CommandRecord> GetFullHistory(int count = 0)
        {
            if (count <= 0 || count > commandHistory.Count)
                return new List<CommandRecord>(commandHistory);
            return commandHistory.GetRange(commandHistory.Count - count, count);
        }

        /// <summary>
        /// 履歴をクリア
        /// </summary>
        public void ClearHistory() => commandHistory.Clear();

        /// <summary>
        /// コマンドの成功率を取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public float GetCommandSuccessRate<T>() where T : ICommand
        {
            int total = 0, success = 0;
            string typeName = typeof(T).Name;
            foreach (var r in commandHistory)
            {
                if (r.commandName == typeName)
                {
                    total++;
                    if (r.wasSuccessful) success++;
                }
            }
            return total > 0 ? (float)success / total : 0f;
        }

        /// <summary>
        /// GUIで表示（デバッグ用）
        /// </summary>
        private void OnGUI()
        {
            if (!showGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 25 * displayCount));
            GUILayout.Label("コマンド履歴:");

            int start = Mathf.Max(0, commandHistory.Count - displayCount);
            for (int i = start; i < commandHistory.Count; i++)
            {
                GUILayout.Label(commandHistory[i].ToString());
            }
            GUILayout.EndArea();
        }

        /// <summary>
        /// 最新のコマンド名を取得
        /// </summary>
        /// <returns></returns>
        public string GetLatestCommandName() => commandHistory.Count > 0 ? commandHistory[^1].commandName : "なし";

        /// <summary>
        /// コマンドの型一致でコンボをチェック（シンプル版）
        /// </summary>
        /// <param name="types"></param>
        /// <param name="maxInterval"></param>
        /// <returns></returns>
        public bool CheckCombo(Type[] types, float maxInterval = 1.0f)
        {
            if (types.Length == 0 || commandHistory.Count < types.Length) return false;

            int index = commandHistory.Count - 1;
            for (int i = types.Length - 1; i >= 0; i--)
            {
                bool found = false;
                float lastTime = i < types.Length - 1 ? commandHistory[index + 1].executionTime : float.MaxValue;

                while (index >= 0)
                {
                    var r = commandHistory[index];
                    if (r.commandName == types[i].Name && r.wasSuccessful && lastTime - r.executionTime <= maxInterval)
                    {
                        found = true;
                        break;
                    }
                    index--;
                }

                if (!found) return false;
            }
            return true;
        }
    }
}