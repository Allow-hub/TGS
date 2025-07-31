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
            private float duration = 0.5f;
            protected internal override void Enter()
            {
                base.Enter();
                BattleJudge.I.SetPlayerAttackState(Context.characterController.PlayerID,false);
                Context.isHitting = true;
                Context.anim.SetBool(hitAnim, true);
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
                BattleJudge.I.SetPlayerAttackState(Context.characterController.PlayerID, true);
                Context.isHitting = false;
                elapsedTime = 0f;
                Context.anim.SetBool(hitAnim, false);
            }
        }
    }
}
