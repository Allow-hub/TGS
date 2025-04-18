using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TechC.AttackManager;

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
            private AttackManager.AttackStrength attackStrength;
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
                if (Context.playerInputManager.IsWeakAttacking)
                    attackStrength = AttackStrength.Weak;
                else if (Context.playerInputManager.IsStrongAttacking)
                    attackStrength = AttackStrength.Strong;
                else if (Context.playerInputManager.IsAppealing)
                    attackStrength = AttackStrength.Appeal;

                duration = Context.attackManager.GetDuration(attackType, attackStrength);

            }

            protected internal override void Update()
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > duration)
                {
                    Context.ChangeNeutralState();
                }
            }

            protected internal override void Exit()
            {
                elapsedTime = 0;
                AttackData data = Context.attackManager.GetAttackData(attackType, attackStrength);
                Context.anim.speed = Context.characterController.DefaultAnimSpeed;
                if (data != null)
                {
                    Context.anim.SetBool(data.animHash, false);
                }
                else
                {
                    Debug.LogWarning($"AttackData is null for type {attackType} and strength {attackStrength}");
                }
                //もし攻撃時間がたたずに他ステートから割り込まれたときに強制終了のメソッドを呼ぶ
                var isEarlyExit = elapsedTime < duration;
                if (isEarlyExit)
                    Context.attackManager.ForceFinish(attackStrength);
                Context.currentCommand = null;
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
