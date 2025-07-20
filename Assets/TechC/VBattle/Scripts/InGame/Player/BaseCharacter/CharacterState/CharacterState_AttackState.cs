using IceMilkTea.StateMachine;
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

            // 攻撃履歴を保持する静的変数
            private AttackType lastAttackType = AttackType.Neutral;
            private AttackStrength lastAttackStrength = AttackStrength.Weak;
            private int consecutiveAttackCount = 0; private AttackType attackType;
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

                attackType = Context.CheckAttackType();
                attackStrength = Context.CheckAttackStrength();

                // 同じ攻撃の連続使用をチェック
                CheckConsecutiveAttacks();

                Context.attackManager.ExecuteAttack(attackType);
                duration = Context.attackManager.GetDuration(attackType, attackStrength);
            }

            protected internal override void Update()
            {
                if (BattleJudge.I.IsPaused) return;
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
                // Context.anim.speed = Context.characterController.DefaultAnimSpeed;
                if (data != null)
                {
                    Context.anim.SetBool(data.animHash, false);
                }
                else
                {
                    CustomLogger.Warning($"AttackData is null for type {attackType} and strength {attackStrength}");
                }
                //もし攻撃時間がたたずに他ステートから割り込まれたときに強制終了のメソッドを呼ぶ

                if (isEarlyExit)
                {
                    Context.attackManager.ForceFinish(attackStrength);
                    Debug.Log("Early");
                }
                Context.currentCommand = null;
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