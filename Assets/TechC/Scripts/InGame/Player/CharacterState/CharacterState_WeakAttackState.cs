using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {
        [Header("弱攻撃設定")]
        [SerializeField] private WeakAttack weakAttack;
        /// <summary>
        /// 弱攻撃を管理する
        /// </summary>
        private class WeakAttackState : ImtStateMachine<CharacterState>.State
        {
            private float coolTime = 1f;
            private float elapsedTime = 0;

            private int normalAnim_1 = Animator.StringToHash("IsNormalWeakAttack_1");
            private int rightAnim = Animator.StringToHash("IsRightWeakAttack");
            private int leftAnim = Animator.StringToHash("IsLeftWeakAttack");
            private int downAnim = Animator.StringToHash("IsDownWeakAttack");
            private int upAnim = Animator.StringToHash("IsUpWeakAttack");

            private IWeakAttackBase.WeakAttackType weakAttackType;
            protected internal override void Enter()
            {
                base.Enter();
                weakAttackType = CheckAttackType();
                switch (weakAttackType)
                {
                    case IWeakAttackBase.WeakAttackType.Normal:
                        NormalInit();
                        break;
                    case IWeakAttackBase.WeakAttackType.Right:
                        RightInit();
                        break;
                    case IWeakAttackBase.WeakAttackType.Left:
                        LeftInit();
                        break;
                    case IWeakAttackBase.WeakAttackType.Down:
                        DownInit();
                        break;
                    case IWeakAttackBase.WeakAttackType.Up:
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
                if (elapsedTime > coolTime)
                    Context.stateMachine.SendEvent((int)(StateEventId.Idle));
            }

            protected internal override void Exit()
            {
                base.Exit();
                elapsedTime = 0;
                switch (weakAttackType)
                {
                    case IWeakAttackBase.WeakAttackType.Normal:
                        NormalReset();
                        break;
                    case IWeakAttackBase.WeakAttackType.Right:
                        RightReset();
                        break;
                    case IWeakAttackBase.WeakAttackType.Left:
                        LeftReset();
                        break;
                    case IWeakAttackBase.WeakAttackType.Down:
                        DownReset();
                        break;
                    case IWeakAttackBase.WeakAttackType.Up:
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
            private IWeakAttackBase.WeakAttackType CheckAttackType()
            {
                //左の入力が同時に入っているとき
                if (Context.playerInputManager.MoveInput.x < 0)
                    return IWeakAttackBase.WeakAttackType.Left;
                if (Context.playerInputManager.MoveInput.x > 0)
                    return IWeakAttackBase.WeakAttackType.Right;
                if (Context.playerInputManager.MoveInput.y < 0)
                    return IWeakAttackBase.WeakAttackType.Down; 
                if (Context.playerInputManager.MoveInput.y > 0)
                    return IWeakAttackBase.WeakAttackType.Up;
                return IWeakAttackBase.WeakAttackType.Normal;
            }
        }
    }
}
