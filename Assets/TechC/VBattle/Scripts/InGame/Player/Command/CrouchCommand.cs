using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class CrouchCommand : INeutralUsableCommand
    {
        private PlayerInputManager playerInputManager;
        private Player.CharacterController characterController;
        private int crouchAnim = Animator.StringToHash("IsCrouching");
        public bool IsFinished => !playerInputManager.IsCrouching;

        public CrouchCommand(Player.CharacterController characterController, PlayerInputManager playerInputManager)
        {
            this.characterController = characterController;
            this.playerInputManager = playerInputManager;
        }

        public void Execute()
        {
            characterController.SetAnim(crouchAnim, true);
            if(IsFinished) 
                characterController.SetAnim(crouchAnim,false);
        }

        public void Undo()
        {
        }
    }
}
