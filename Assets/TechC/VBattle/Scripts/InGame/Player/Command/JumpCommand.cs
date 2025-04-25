using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class JumpCommand : INeutralUsableCommand, IAirUsableCommand
    {
        private Player.CharacterController character;
        private int jumpAnim = Animator.StringToHash("IsJumping");
        private int doubleJumpAnim = Animator.StringToHash("IsDoubleJumping");

        // クールタイム設定
        private float jumpCooldown = 0.3f; // 0.3秒のクールタイム（お好みで調整）
        private float lastJumpTime = -Mathf.Infinity;
        /// <summary>
        /// Mathf.Infinity は、「正の無限大」を表す定数です。Unity（というか C# の float）で使える特殊な値のひとつです。
        ///Mathf.Infinity は、次のような用途でよく使われます：
        ///使用例 説明
        ///float.MaxValue より大きい値として    無限に大きな数として使う
        ///まだ何も起きていないことの初期値に使う 例：最初のジャンプがまだされていない → lastJumpTime = -Mathf.Infinity
        ///除算の結果などで無限が必要な時 例： 1 / 0f は Mathf.Infinity
        /// </summary>

        public bool IsFinished => Time.time - lastJumpTime >= jumpCooldown;

        public JumpCommand(Player.CharacterController character)
        {
            this.character = character;
        }

        public void Execute()
        {
            if (!IsFinished) return; // クールタイム中なら無視

            if (character.IsGrounded())
            {
                character.Jump();
                //AudioManager.I.PlayCharacterSE(CharacterType.)
                character.SetAnim(jumpAnim, true);
            }
            //else
            //{
            //    Debug.Log("Jumped!");

            //    character.DoubleJump();
            //    character.SetAnim(doubleJumpAnim, true);
            //}

            lastJumpTime = Time.time; // 最後のジャンプ時刻を記録
        }

        public void Undo()
        {
            // 必要に応じてジャンプをキャンセル
        }

        public void ForceFinish()
        {
            // 状態を強制終了したい場合に実装
        }
    }
}
