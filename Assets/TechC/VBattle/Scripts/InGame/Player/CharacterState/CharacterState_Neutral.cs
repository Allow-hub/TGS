using IceMilkTea.StateMachine;

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
            protected internal override void Enter()
            {
                base.Enter();
                Context.characterController.SetAnim(AnimatorParams.IsWalking,false);
                Context.characterController.SetAnim(AnimatorParams.IsRunning,false);
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
            }
        }
    }
}