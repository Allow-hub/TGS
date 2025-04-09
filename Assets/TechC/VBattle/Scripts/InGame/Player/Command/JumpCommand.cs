using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ジャンプコマンド
    /// NeutralStateとAirStateで使える
    /// ICommandが重複しているようにみえるが問題ないらしい
    /// </summary>
    public class JumpCommand : INeutralUsableCommand , IAirUsableCommand
    {
        private Player.CharacterController character;
        private int jumpAnim = Animator.StringToHash("IsJumping");
        private int doubleJumpAnim = Animator.StringToHash("IsDoubleJumping");

        protected float duration;
        protected float elapsedTime = 0;

        public bool IsFinished =>true;
        public JumpCommand(Player.CharacterController character)
        {
            this.character = character;
        }

        public void Execute()
        {
            if (character.IsGrounded())
            {
                character.Jump();
                character.SetAnim(jumpAnim, true);
            }
            else
            {
                character.DoubleJump();
                character.SetAnim(doubleJumpAnim, true);
            }
        }

        public void Undo()
        {
            // 必要に応じてジャンプをキャンセル
        }
    }
}
