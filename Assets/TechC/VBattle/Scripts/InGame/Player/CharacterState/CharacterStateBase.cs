using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public abstract class CharacterStateBase : ImtStateMachine<CharacterState>.State
    {
        // このステートで実行可能なコマンドタイプを保持
        protected HashSet<System.Type> allowedCommandTypes = new HashSet<System.Type>();

        // コマンドキュー
        protected Queue<ICommand> commandQueue = new Queue<ICommand>();

        // ステート初期化時に許可コマンドを設定
        protected abstract void InitializeAllowedCommands();

        protected internal override void Enter()
        {
            base.Enter();
            InitializeAllowedCommands();
        }

        // コマンドをキューに追加する際にフィルタリング
        public bool EnqueueCommand(ICommand command)
        {
            // このステートで許可されたコマンドかチェック
            if (allowedCommandTypes.Contains(command.GetType()))
            {
                commandQueue.Enqueue(command);
                return true;
            }
            return false;
        }

        protected internal override void Update()
        {
            // キューからコマンドを取り出して実行
            while (commandQueue.Count > 0)
            {
                var command = commandQueue.Dequeue();
                command.Execute();
            }
        }
    }
}
