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
            private AttackType attackType;
            private float duration;
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
                duration = Context.attackManager.GetDuration(attackType, Context.playerInputManager.IsWeakAttacking);
            }

            protected internal override void Update()
            {
                elapsedTime += Time.deltaTime;
                if(elapsedTime > duration)
                {
                    Context.ChangeNeutralState();
                }
            }

            protected internal override void Exit()
            {
                elapsedTime = 0;
            }
        
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
