using UnityEngine;

namespace TechC
{
    public class MoveCommand : INeutralUsableCommand, IAirUsableCommand
    {
        private float normalSpeedMagnification = 1.0f;  //通常時の速度倍率
        private Player.CharacterController characterController;
        private BaseInputManager playerInputManager;
        public bool IsFinished => !playerInputManager.IsMoving;

        public MoveCommand(Player.CharacterController characterController, BaseInputManager playerInputManager)
        {
            this.characterController = characterController;
            this.playerInputManager = playerInputManager;
        }

        public void Execute()
        {
            characterController.SetAnim(AnimatorParams.IsWalking,false);
            characterController.SetAnim(AnimatorParams.IsRunning,false);

            if (playerInputManager.IsDashing)
            {
                characterController.MoveCharacter(characterController.GetCharacterData().DashMultipiler);
                characterController.SetAnim(AnimatorParams.IsRunning,true);
            }
            else
            {
                characterController.MoveCharacter(normalSpeedMagnification);
                characterController.SetAnim(AnimatorParams.IsWalking,true);
            }
        }

        public void Undo()
        {
        }

        public void ForceFinish()
        {
            characterController.SetAnim(AnimatorParams.IsWalking,false);
            characterController.SetAnim(AnimatorParams.IsRunning,false);
        }
    }
}
