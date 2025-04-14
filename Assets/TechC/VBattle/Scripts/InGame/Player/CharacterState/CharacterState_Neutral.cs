using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechC
{
    public partial class CharacterState
    {
        /// <summary>
        /// 地上での通常ステート
        /// 移動、ジャンプ、しゃがみを取り扱う
        /// このステートからは攻撃中、空中、被ダメージ中に移動する
        /// </summary>
        private class NeutralState : ImtStateMachine<CharacterState>.State
        {
            private bool isJumping = false;
            private float jumpCooldown = 0.5f;
            private float elapsedTime = 0;
            private ICommand currentCommand = null;



            protected internal override void Enter()
            {
                base.Enter();
                Context.characterController.SetAnim(Context.jumpAnim, false);
                Context.characterController.SetAnim(Context.doubleJumpAnim, false);
            }

            protected internal override void Update()
            {
                base.Update();

                // 実行中のコマンドがなければ、新しいコマンドを取り出す
                if (currentCommand == null)
                {
                    GetNextCommand();
                    // コマンド履歴に記録
                    if (Context.commandHistory != null)
                    {
                        Context.commandHistory.RecordCommand(
                            currentCommand,
                            GetType().Name,
                            !(currentCommand is INeutralUsableCommand usableCommand) || usableCommand.IsFinished,
                            Context.characterController.transform.position
                        );

                        //Debug.Log(Context.commandHistory.GetFullHistory());
                    }
                }
                // 現在のコマンドを実行中かつ割り込みコマンドがキューにあるかチェック
                else
                {
                    // 現在実行中のコマンドよりも優先度の高いコマンドがキューにあるか確認
                    var highPriorityCommand = CheckForHighPriorityCommand();

                    if (highPriorityCommand != null)
                    {
                        if (Context.canDebugLog)
                            // 現在のコマンドを中断（必要に応じて中断処理を追加）
                            Debug.Log($"[NeutralState] コマンド {currentCommand.GetType().Name} を中断し、{highPriorityCommand.GetType().Name} を実行");
                        currentCommand.ForceFinish();
                        currentCommand = highPriorityCommand;
                        currentCommand.Execute(); // 最初の1回
                                                  // コマンド履歴に記録
                        if (Context.commandHistory != null)
                        {
                            Context.commandHistory.RecordCommand(
                                currentCommand,
                                GetType().Name,
                                //パターンマッチング
                                //currentCommand が INeutralUsableCommand インターフェースを実装しているかをチェック。
                                //もしそうなら、usableCommand という変数にキャストされる。
                                !(currentCommand is INeutralUsableCommand usableCommand) || usableCommand.IsFinished,
                                Context.characterController.transform.position
                            );
                            //Debug.Log(Context.commandHistory.GetFullHistory());
                        }
                    }
                    else
                    {
                        // 通常通り現在のコマンドを実行
                        currentCommand.Execute();
                        // コマンドが終了しているかチェック
                        if ((currentCommand is INeutralUsableCommand usableCommand && usableCommand.IsFinished) ||
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
                    if (command is INeutralUsableCommand usable)
                    {
                        if(Context.canDebugLog)
                        Debug.Log($"[NeutralState] 対応コマンド: {command.GetType().Name}");
                        currentCommand = usable;
                        currentCommand.Execute(); // 最初の1回
                        break;
                    }
                    else
                    {
                        Debug.Log($"[NeutralState] 非対応コマンド: {command.GetType().Name}");
                    }
                }
            }

            // 優先度の高いコマンドがキューにあるか確認
            private INeutralUsableCommand CheckForHighPriorityCommand()
            {
                if (Context.commandQueue.Count == 0 || currentCommand == null)
                    return null;

                int currentPriority = GetCommandPriority(currentCommand.GetType());

                // キューから優先度の高いコマンドを探す
                foreach (var command in Context.commandQueue)
                {
                    if (command is INeutralUsableCommand usable)
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
            }

            protected internal override void Exit()
            {
                base.Exit();
                isJumping = false;
                elapsedTime = 0;
                currentCommand = null;
            }
        }
    }
}