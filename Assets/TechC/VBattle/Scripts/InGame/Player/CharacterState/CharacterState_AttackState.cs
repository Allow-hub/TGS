using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        public enum AttackType
        {
            Neutral,//他に入力がないとき
            Right,
            Left,
            Down,
            Up,
        }
        private class AttackState : ImtStateMachine<CharacterState>.State
        {
            private Queue<ICommand> commandQueue = new Queue<ICommand>();
            private ICommand currentCommand = null;
            private AttackType attackType;
            private float duration = 1;
            private float elapsedTime = 0;

            protected internal override void Enter()
            {
                // 初期化を確認
                if (Context.attackManager == null)
                {
                    Debug.LogError("AttackManagerが設定されていません");
                    return;
                }
                //CreateAndEnqueueAttackCommand();

                attackType = CheckAttackType();
                Context.attackManager.ExecuteAttack(attackType, Context);
            }

            protected internal override void Update()
            {
                elapsedTime += Time.deltaTime;
                Debug.Log(commandQueue);
                //if (elapsedTime >= duration)
                //    Context.stateMachine.SendEvent((int)StateEventId.Idle);

            }

            protected internal override void Exit()
            {
                elapsedTime = 0;
                commandQueue.Clear();
                currentCommand = null;
            }
            public void EnqueueCommand(ICommand command)
            {
                commandQueue.Enqueue(command);
            }  
            //// 入力に基づいて攻撃コマンドを作成
            //private void CreateAndEnqueueAttackCommand()
            //{
            //    AttackType attackType = CheckAttackType();
            //    float attackDuration = 1.0f; // 攻撃の持続時間

            //    if (Context.playerInputManager.IsWeakAttacking)
            //    {
            //        ICommand attackCommand = CreateWeakAttackCommand(attackType, attackDuration);
            //        EnqueueCommand(attackCommand);
            //    }
            //    else if (Context.playerInputManager.IsStrongAttacking)
            //    {
            //        ICommand attackCommand = CreateStrongAttackCommand(attackType, attackDuration);
            //        EnqueueCommand(attackCommand);
            //    }
            //}

            //// 弱攻撃コマンドの作成
            //private ICommand CreateWeakAttackCommand(AttackType attackType, float duration)
            //{
            //    switch (attackType)
            //    {
            //        case AttackType.Neutral:
            //            return new WeakAttackCommand(Context.attackManager.WeakAttack, Context.characterController, duration);
            //        case AttackType.Left:
            //            //return new WeakLeftAttackCommand(Context.attackManager.WeakAttack, Context.characterController, duration);
            //        // 他の方向も同様に
            //        default:
            //            return new WeakAttackCommand(Context.attackManager.WeakAttack, Context.characterController, duration);
            //    }
            //}

            //// 強攻撃コマンドの作成（同様に実装）
            //private ICommand CreateStrongAttackCommand(AttackType attackType, float duration)
            //{
            //    // 弱攻撃と同様に実装
            //    return null; // 仮の戻り値
            //}
            /// <summary>
            /// 攻撃種方向の確認
            /// </summary>
            /// <returns></returns>
            private AttackType CheckAttackType()
            {
                if (Context.playerInputManager.MoveInput.x < 0)
                    return AttackType.Left;
                if (Context.playerInputManager.MoveInput.x > 0)
                    return AttackType.Right;
                if (Context.playerInputManager.MoveInput.y < 0)
                    return AttackType.Down;
                if (Context.playerInputManager.MoveInput.y > 0)
                    return AttackType.Up;
                return AttackType.Neutral;
            }
        }
    }
}
