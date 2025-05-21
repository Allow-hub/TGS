using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class CrouchCommand : INeutralUsableCommand
    {
        private BaseInputManager playerInputManager;
        private Player.CharacterController characterController;
        private int crouchAnim = Animator.StringToHash("IsCrouching");
        private bool isForceFinished = false;

        public bool IsFinished => isForceFinished || !playerInputManager.IsCrouching;
        public CrouchCommand(Player.CharacterController characterController, BaseInputManager playerInputManager)
        {
            this.characterController = characterController;
            this.playerInputManager = playerInputManager;
        }

        public void Execute()
        {
            isForceFinished = false;
            characterController.SetAnim(crouchAnim, true);
            if (IsFinished)
                characterController.SetAnim(crouchAnim, false);
        }

        public void Undo()
        {
        }

        public void ForceFinish()
        {
            isForceFinished = true;
            ForceFinishAsync().Forget();
        }
        public async UniTask ForceFinishAsync()
        {
            isForceFinished = true;
            await DelayUtility.RunAfterDelay(0.3f, () =>
            {
                characterController.SetAnim(crouchAnim, false);
            });
        }

    }
}
