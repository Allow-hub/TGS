using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public partial class CharacterState
    {

        /// <summary>
        /// ダメージを喰らったときのステート
        /// </summary>
        private class DamageState : ImtStateMachine<CharacterState>.State
        {
            private int hitAnim = Animator.StringToHash("IsHitting");
            private float elapsedTime = 0f;
            private float duration = 5;

            private HitData currentHitData;

            protected internal override void Enter()
            {
                base.Enter();
                Context.isHitting = true;
                // 最後に受けたヒットデータを取得
                currentHitData = Context.characterController.GetLastHitData();

                if (currentHitData != null)
                {
                    // ヒットデータに基づいて値を設定
                    duration = currentHitData.hitStunDuration;

                    // アニメーションを設定
                    Context.anim.SetBool(hitAnim, true);
                    // ノックバックを適用
                    Context.characterController.AddForcePlayer(
                        currentHitData.knockbackDirection,
                        currentHitData.knockbackForce,
                        ForceMode.Impulse
                    );

                    // DIの設定
                    //canRecoverWithDI = currentHitData.canDI && currentHitData.hitStunLevel < 2;

                }
                else
                {
                    // デフォルト値（ヒットデータがない場合）
                    duration = 0.5f;
                    Context.anim.SetBool(hitAnim, true);

                    // デフォルトのノックバック
                    Vector3 defaultKnockback = new Vector3(Context.characterController.transform.forward.x * -1, 0.5f, 0);
                    Context.characterController.AddForcePlayer(defaultKnockback, 5f, ForceMode.Impulse);
                }

            }

            protected internal override void Update()
            {
                base.Update();
                elapsedTime += Time.deltaTime;
                if (elapsedTime > duration)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Neutral);
                }
               
            }

            protected internal override void Exit()
            {
                base.Exit();
                Context.isHitting = false;
                elapsedTime = 0f;
                Context.anim.SetBool(hitAnim, false);
            }
        }
    }
}
