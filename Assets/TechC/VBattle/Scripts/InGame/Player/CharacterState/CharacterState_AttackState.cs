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

        // 攻撃履歴を保持する静的変数
        private static AttackType lastAttackType = AttackType.Neutral;
        private static AttackStrength lastAttackStrength = AttackStrength.Weak;
        private static int consecutiveAttackCount = 0;

        private class AttackState : ImtStateMachine<CharacterState>.State
        {
            private AttackType attackType;
            private AttackManager.AttackStrength attackStrength;
            private float duration;
            private float elapsedTime = 0;
            private bool isEarlyExit = true;
            // 同じ攻撃を何回繰り返すとゲージ減少が始まるか
            private const int PENALTY_THRESHOLD = 3;
            // ゲージ減少量
            private const float GAUGE_PENALTY = -5f;

            protected internal override void Enter()
            {
                // 初期化を確認
                if (Context.attackManager == null)
                {
                    Debug.LogError("AttackManagerが設定されていません");
                    return;
                }

                attackType = CheckAttackType();

                // 攻撃強度の判定
                if (Context.playerInputManager.IsWeakAttacking)
                    attackStrength = AttackStrength.Weak;
                else if (Context.playerInputManager.IsStrongAttacking)
                    attackStrength = AttackStrength.Strong;
                else if (Context.playerInputManager.IsAppealing)
                    attackStrength = AttackStrength.Appeal;

                // 同じ攻撃の連続使用をチェック
                CheckConsecutiveAttacks();

                Context.attackManager.ExecuteAttack(attackType, Context);
                duration = Context.attackManager.GetDuration(attackType, attackStrength);
            }

            protected internal override void Update()
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > duration)
                {
                    isEarlyExit = false;
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
                
                if (isEarlyExit)
                {

                    Context.attackManager.ForceFinish(attackStrength);Debug.Log("Earty");
                }
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

            /// <summary>
            /// 同じ攻撃の連続使用をチェックし、必要に応じてゲージを減らす
            /// </summary>
            private void CheckConsecutiveAttacks()
            {
                if (attackType == lastAttackType && attackStrength == lastAttackStrength)
                {
                    // 同じ攻撃が連続で使われている
                    consecutiveAttackCount++;

                    // しきい値を超えたらペナルティを適用
                    if (consecutiveAttackCount >= PENALTY_THRESHOLD)
                    {
                        // ゲージを減少させる
                        var characterController = Context.characterController as Player.CharacterController;
                        if (characterController != null)
                        {
                            // ここでゲージを減少（設定により調整可能）
                            characterController.NotBoolAddSpecialGauge(GAUGE_PENALTY);
                            Debug.Log($"同じ攻撃を{consecutiveAttackCount}回連続で使用: ゲージを{GAUGE_PENALTY}減少");
                        }
                    }
                }
                else
                {
                    // 異なる攻撃に変わったらリセット
                    consecutiveAttackCount = 1;
                    lastAttackType = attackType;
                    lastAttackStrength = attackStrength;
                }
            }
        }
    }
}