using IceMilkTea.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechC
{
    public partial class CharacterState
    {
        /// <summary>
        /// 地上での通常ステート
        /// 移動、ジャンプ、しゃがみを取り扱う
        /// このステートからは攻撃中、空中、被ダメージ中に移動する
        /// </summary>
        private class NeutralState : ImtStateMachine<CharacterState>.State
        {
            private bool isJumping = false;
            private float jumpCooldown = 0.5f;
            private float elapsedTime = 0;



            protected internal override void Enter()
            {
                base.Enter();
                Context.characterController.SetAnim(Context.jumpAnim, false);
                Context.characterController.SetAnim(Context.doubleJumpAnim, false);
            }

            protected internal override void Update()
            {
                base.Update();

                Context.HandleCommand<INeutralUsableCommand>(ref Context.currentCommand);

            }


            protected internal override void Exit()
            {
                base.Exit();
                isJumping = false;
                elapsedTime = 0;
                //currentCommand = null;
            }
        }
    }
}