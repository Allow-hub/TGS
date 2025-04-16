using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        private class AirState : ImtStateMachine<CharacterState>.State
        {



            protected internal override void Enter()
            {
                base.Enter();
            }

            protected internal override void Update()
            {
                base.Update();
                Context.HandleCommand<IAirUsableCommand>(ref Context.currentCommand);

<<<<<<< HEAD
                    if (highPriorityCommand != null)
                    {
                        // 現在のコマンドを中断（必要に応じて中断処理を追加）
                        Debug.Log($"[NeutralState] コマンド {currentCommand.GetType().Name} を中断し、{highPriorityCommand.GetType().Name} を実行");
                        currentCommand.ForceFinish();
                        currentCommand = highPriorityCommand;
                        currentCommand.Execute(); // 最初の1回
                    }
                    else
                    {
                        // 通常通り現在のコマンドを実行
                        currentCommand.Execute();
                        // コマンドが終了しているかチェック
                        if ((currentCommand is IAirUsableCommand usableCommand && usableCommand.IsFinished) ||
                            (currentCommand is ICommand && (currentCommand as ICommand).IsFinished))
                        {
                            currentCommand = null; // コマンドが終了したら次のコマンドを受付けられるように
                            GetNextCommand(); // 次のコマンドを取得
                        }
                    }
                }
            }

            // 次のコマンドを取得する
            private void GetNextCommand()
            {
                while (Context.commandQueue.Count > 0)
                {
                    var command = Context.commandQueue.Dequeue();
                    if (command is IAirUsableCommand usable)
                    {
                        // Debug.Log($"[NeutralState] 対応コマンド: {command.GetType().Name}");
                        currentCommand = usable;
                        currentCommand.Execute(); // 最初の1回
                        break;
                    }
                    else
                    {
                        // Debug.Log($"[NeutralState] 非対応コマンド: {command.GetType().Name}");
                    }
                }
            }

            // 優先度の高いコマンドがキューにあるか確認
            private IAirUsableCommand CheckForHighPriorityCommand()
            {
                if (Context.commandQueue.Count == 0 || currentCommand == null)
                    return null;

                int currentPriority = GetCommandPriority(currentCommand.GetType());

                // キューから優先度の高いコマンドを探す
                foreach (var command in Context.commandQueue)
                {
                    if (command is IAirUsableCommand usable)
                    {
                        int commandPriorityValue = GetCommandPriority(command.GetType());
                        if (commandPriorityValue > currentPriority)
                        {
                            // キューから削除して返す
                            Context.commandQueue = new Queue<ICommand>(
                                System.Linq.Enumerable.Where(Context.commandQueue, c => c != command)
                            );
                            return usable;
                        }
                    }
                }

                return null;
            }

            // コマンドの優先度を取得
            private int GetCommandPriority(System.Type commandType)
            {
                if (Context.commandPriority.TryGetValue(commandType, out int priority))
                {
                    return priority;
                }
                return 0; // デフォルトの優先度
=======
>>>>>>> main
            }

            protected internal override void Exit()
            {
                base.Exit();
                Context.currentCommand = null;
            }

        }
    }
}
