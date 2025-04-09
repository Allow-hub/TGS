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
            private ICommand currentCommand = null;



            protected internal override void Enter()
            {
                base.Enter();
            }

            protected internal override void Update()
            {
                base.Update();
                // 現在のコマンドを実行中なら続けて実行
                if (currentCommand != null)
                {
                    currentCommand.Execute();

                    // コマンドが終了しているかチェック
                    if ((currentCommand is IAirUsableCommand usableCommand && usableCommand.IsFinished) ||
                        (currentCommand is ICommand && (currentCommand as ICommand).IsFinished))
                    {
                        currentCommand = null; // コマンドが終了したら次のコマンドを受付けられるように
                    }
                    return;
                }

                // 実行中のコマンドがなければ、新しいコマンドを取り出す
                while (Context.commandQueue.Count > 0)
                {
                    var command = Context.commandQueue.Dequeue();
                    if (command is IAirUsableCommand usable)
                    {
                        Debug.Log($"[AirState] 対応コマンド: {command.GetType().Name}");

                        currentCommand = usable;
                        currentCommand.Execute(); // 最初の1回
                        break;
                    }
                    else
                    {
                        Debug.Log($"[AirState] 非対応コマンド: {command.GetType().Name}");
                    }
                }
            }

            protected internal override void Exit()
            {
                base.Exit();
            }
          
        }   
    }
}
