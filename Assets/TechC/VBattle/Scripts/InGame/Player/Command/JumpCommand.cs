using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace TechC
{
    public class JumpCommand : INeutralUsableCommand, IAirUsableCommand
    {
        private Player.CharacterController character;
        private int jumpAnim = Animator.StringToHash("IsJumping");
        private int doubleJumpAnim = Animator.StringToHash("IsDoubleJumping");

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

        public bool IsFinished => inputManager.IsJumping|| isForceFinish;

        public JumpCommand(Player.CharacterController character,BaseInputManager baseInputManager)
        {
            this.character = character;
            this.inputManager = baseInputManager;
        }

        public async void Execute()
        {
            if (!IsFinished) return; // クールタイム中なら無視

            if (Time.time - lastJumpTime <= jumpCooldown) return;
            if (character.IsGrounded())
            {
                character.Jump();
                //AudioManager.I.PlayCharacterSE(CharacterType.)
                character.SetAnim(jumpAnim, true);
            }
            else
            {
                character.SetAnim(jumpAnim, false);
                character.DoubleJump();
                character.SetAnim(doubleJumpAnim, true);
                await ResetDoubleJumpAnim();
            }

            lastJumpTime = Time.time; // 最後のジャンプ時刻を記録
        }

        public void Undo()
        {
            // 必要に応じてジャンプをキャンセル
        }

        public void ForceFinish()
        {
            character.SetAnim(jumpAnim, false);
            character.SetAnim(doubleJumpAnim, false);
            isForceFinish = true;
        }

        private async UniTask ResetDoubleJumpAnim()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(doubleJumpAnimResetTime));
            character.SetAnim(doubleJumpAnim, false);
        }
    }
}
