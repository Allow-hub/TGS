using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC
{
    public class CrouchCommand : INeutralUsableCommand
    {
        private BaseInputManager playerInputManager;
        private Player.CharacterController characterController;
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
            characterController.SetAnim(AnimatorParams.IsCrouching, true);
            if (IsFinished)
                characterController.SetAnim(AnimatorParams.IsCrouching, false);
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
                characterController.SetAnim(AnimatorParams.IsCrouching, false);
            });
        }

    }
}
