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
            private int wallHitAnim = Animator.StringToHash("IsWallHitting");
            private float elapsedTime = 0f;
            private float duration = 0.5f;
            private float wallHitDuration = 1.0f;
            private float currentDuration = 0f;
            private bool isWallHit = false;
            protected internal override void Enter()
            {
                base.Enter();
                BattleJudge.I.SetPlayerAttackState(Context.characterController.PlayerID, false);
                Context.isHitting = true;
                isWallHit = Context.characterController.IsWallHitting;
                if (isWallHit)
                {
                    Context.anim.SetBool(wallHitAnim, true);
                    currentDuration = wallHitDuration;
                }
                else
                {
                    Context.anim.SetBool(hitAnim, true);
                    currentDuration = duration;
                }
            }

            protected internal override void Update()
            {
                base.Update();
                elapsedTime += Time.deltaTime;
                if (elapsedTime > currentDuration)
                {
                    Context.stateMachine.SendEvent((int)StateEventId.Neutral);
                }
            }

            protected internal override void Exit()
            {
                base.Exit();
                BattleJudge.I.SetPlayerAttackState(Context.characterController.PlayerID, true);
                Context.isHitting = false;
                elapsedTime = 0f;
                Context.anim.SetBool(hitAnim, false);
                Context.anim.SetBool(wallHitAnim, false);
                isWallHit = false;
            }
        }
    }
}
