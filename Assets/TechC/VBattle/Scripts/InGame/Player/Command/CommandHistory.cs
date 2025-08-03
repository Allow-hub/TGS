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
            public CharacterState.AttackStrength attackStrength;  // 攻撃強度
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
        /// 指定秒数前から現在までのAttackCommandを古い順に順次再実行する（Coroutine版）
        /// </summary>
        /// <param name="secondsAgo">何秒前から再実行するか</param>
        /// <param name="attackManager">AttackManager</param>
        public void ReplayAttackCommandsFromSecondsAgo(float secondsAgo, Player.CharacterController characterController)
        {
            StopAllCoroutines();
            StartCoroutine(ReplayAttackCoroutine(secondsAgo, characterController));
        }
        private IEnumerator ReplayAttackCoroutine(float secondsAgo, Player.CharacterController characterController)
        {
            float replayFrom = Time.time - secondsAgo;

            var attacksToReplay = new List<CommandRecord>();
            foreach (var record in commandHistory)
            {
                if (record.executionTime >= replayFrom && record.commandInstance is AttackCommand)
                {
                    attacksToReplay.Add(record);
                }
            }

            attacksToReplay.Sort((a, b) => a.executionTime.CompareTo(b.executionTime));

            if (attacksToReplay.Count == 0)
            {
                Debug.LogWarning("[Replay] 再生対象の攻撃が見つかりませんでした");
                yield break;
            }

            // === 初期待機時間を追加 ===
            float firstDelay = attacksToReplay[0].executionTime - replayFrom;
            if (firstDelay > 0f)
                yield return new WaitForSeconds(firstDelay);

            for (int i = 0; i < attacksToReplay.Count; i++)
            {
                var current = attacksToReplay[i];
                if (current.commandInstance is not AttackCommand attackCmd)
                    continue;

                if (current.attackType == CharacterState.AttackType.Neutral &&
                    current.attackStrength == CharacterState.AttackStrength.Strong)
                    continue;

                try
                {
                    attackCmd.RePlayAttack(current.attackType, current.attackStrength, characterController);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Replay] 攻撃再生中に例外が発生: {ex.Message}\n{ex.StackTrace}");
                }

                if (i + 1 < attacksToReplay.Count)
                {
                    float interval = attacksToReplay[i + 1].executionTime - current.executionTime;
                    if (interval > 0f)
                        yield return new WaitForSeconds(interval);
                }
            }
        }
        /// <summary>
        /// 最新のコマンド名を取得
        /// </summary>
        /// <returns></returns>
        public string GetLatestCommandName() => commandHistory.Count > 0 ? commandHistory[^1].commandName : "なし";

    }
}