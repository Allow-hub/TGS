using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace TechC
{
    public class JumpCommand : INeutralUsableCommand, IAirUsableCommand
    {
        private Player.CharacterController character;
        private BaseInputManager inputManager;
        private bool isForceFinish = false;
        private float doubleJumpAnimResetTime = 1f;
        // クールタイム設定
        private float jumpCooldown = 0.3f; 
        private float lastJumpTime = -Mathf.Infinity;
        /// <summary>
        /// Mathf.Infinity は、「正の無限大」を表す定数です。Unity（というか C# の float）で使える特殊な値のひとつです。
        ///Mathf.Infinity は、次のような用途でよく使われます：
        ///使用例 説明
        ///float.MaxValue より大きい値として    無限に大きな数として使う
        ///まだ何も起きていないことの初期値に使う 例：最初のジャンプがまだされていない → lastJumpTime = -Mathf.Infinity
        ///除算の結果などで無限が必要な時 例： 1 / 0f は Mathf.Infinity
        /// </summary>

        public bool IsFinished => !inputManager.IsJumping|| isForceFinish;

        public JumpCommand(Player.CharacterController character,BaseInputManager baseInputManager)
        {
            this.character = character;
            this.inputManager = baseInputManager;
        }

        public async void Execute()
        {
            /// <summary>
            /// なぜか一回でもJumpを入れないとMoveCommandに割り込めない不具合あり
            /// </summary>
            // if (!IsFinished) return; // クールタイム中なら無視
            if (Time.time - lastJumpTime <= jumpCooldown) return;
            // Debug.Log("AA");
            character.SetAnim(AnimatorParams.IsWalking, false);
            character.SetAnim(AnimatorParams.IsRunning, false);
            if (Time.time - lastJumpTime <= jumpCooldown) return;
            if (character.IsGrounded())
            {
                character.Jump();
                //AudioManager.I.PlayCharacterSE(CharacterType.)
                character.SetAnim(AnimatorParams.IsJumping, true);
            }
            else
            {
                character.SetAnim(AnimatorParams.IsJumping, false);
                character.DoubleJump();
                character.SetAnim(AnimatorParams.IsDoubleJumping, true);
                await ResetDoubleJumpAnim();
            }

            lastJumpTime = Time.time; // 最後のジャンプ時刻を記録
        }

        public void Undo()
        {
        }

        public void ForceFinish()
        {
            character.SetAnim(AnimatorParams.IsJumping, false);
            character.SetAnim(AnimatorParams.IsDoubleJumping, false);
            isForceFinish = true;
        }

        private async UniTask ResetDoubleJumpAnim()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(doubleJumpAnimResetTime));
            character.SetAnim(AnimatorParams.IsDoubleJumping, false);
        }
    }
}
