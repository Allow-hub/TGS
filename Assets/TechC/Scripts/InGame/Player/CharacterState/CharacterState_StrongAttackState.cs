using IceMilkTea.StateMachine;
using TechC.Player;
using UnityEngine;
using static TechC.Player.IWeakAttackBase;

namespace TechC
{
    public partial class CharacterState
    {
        /// <summary>
        /// 強攻撃を管理するステート
        /// </summary>
        private class StrongAttackState : ImtStateMachine<CharacterState>.State
        {
            [Header("アニメーション設定")]
            private int normalAnim_1 = Animator.StringToHash("IsNormalStrongAttack");
            private int rightAnim = Animator.StringToHash("IsRightStrongAttack");
            private int leftAnim = Animator.StringToHash("IsLeftStrongAttack");
            private int downAnim = Animator.StringToHash("IsDownStrongAttack");
            private int upAnim = Animator.StringToHash("IsUpStrongAttack");


            private float duration = 1;
            private float elapsedTime = 0;

            private IStrongAttackBase.StrongAttackType strongAttackType;

            protected internal override void Enter()
            {
                base.Enter();
                strongAttackType = CheckAttackType();
                switch (strongAttackType)
                {
                    case IStrongAttackBase.StrongAttackType.Normal:
                        NormalInit();
                        break;
                    case IStrongAttackBase.StrongAttackType.Right:
                        RightInit();
                        break;
                    case IStrongAttackBase.StrongAttackType.Left:
                        LeftInit();
                        break;
                    case IStrongAttackBase.StrongAttackType.Down:
                        DownInit();
                        break;
                    case IStrongAttackBase.StrongAttackType.Up:
                        UpInit();
                        break;
                    default:
                        Debug.LogError("存在しない弱攻撃です");
                        break;
                }
            }
            protected internal override void Update()
            {
                base.Update();
                elapsedTime += Time.deltaTime;
                if (elapsedTime > duration)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Idle);
                }
            }
            protected internal override void Exit()
            {
                base.Exit();
                elapsedTime = 0;
                switch (strongAttackType)
                {
                    case IStrongAttackBase.StrongAttackType.Normal:
                        NormalReset();
                        break;
                    case IStrongAttackBase.StrongAttackType.Right:
                        RightReset();
                        break;
                    case IStrongAttackBase.StrongAttackType.Left:
                        LeftReset();
                        break;
                    case IStrongAttackBase.StrongAttackType.Down:
                        DownReset();
                        break;
                    case IStrongAttackBase.StrongAttackType.Up:
                        UpReset();
                        break;
                    default:
                        Debug.LogError("存在しない弱攻撃です");
                        break;
                }
            }

            /// <summary>
            /// 弱攻撃の元と派生攻撃の初期処理
            /// </summary>
            private void NormalInit()
            {
                Context.anim.SetBool(normalAnim_1, true);
                Context.weakAttack.NeutraAttack();
            }
            private void RightInit()
            {
                Context.anim.SetBool(rightAnim, true);
                Context.weakAttack.RightAttack();
            }
            private void LeftInit()
            {
                Context.anim.SetBool(leftAnim, true);
                Context.weakAttack.LeftAttack();
            }
            private void DownInit()
            {
                Context.weakAttack.DownAttack();
                Context.anim.SetBool(downAnim, true);
            }
            private void UpInit()
            {
                Context.anim.SetBool(upAnim, true);
            }
            /// <summary>
            /// 弱攻撃の元と派生攻撃のリセット処理
            /// </summary>
            private void NormalReset()
            {
                Context.anim.SetBool(normalAnim_1, false);
            }
            private void RightReset()
            {
                Context.anim.SetBool(rightAnim, false);
            }
            private void LeftReset()
            {
                Context.anim.SetBool(leftAnim, false);
            }
            private void DownReset()
            {
                Context.anim.SetBool(downAnim, false);
            }
            private void UpReset()
            {
                Context.anim.SetBool(upAnim, false);
            }
            /// <summary>
            /// 攻撃種を判別する
            /// </summary>
            /// <returns></returns>
            private IStrongAttackBase.StrongAttackType CheckAttackType()
            {
                //左の入力が同時に入っているとき
                if (Context.playerInputManager.MoveInput.x < 0)
                    return IStrongAttackBase.StrongAttackType.Left;
                if (Context.playerInputManager.MoveInput.x > 0)
                    return IStrongAttackBase.StrongAttackType.Right;
                if (Context.playerInputManager.MoveInput.y < 0)
                    return IStrongAttackBase.StrongAttackType.Down;
                if (Context.playerInputManager.MoveInput.y > 0)
                    return IStrongAttackBase.StrongAttackType.Up;
                return IStrongAttackBase.StrongAttackType.Normal;
            }
        }
    }
}
