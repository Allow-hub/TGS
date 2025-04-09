using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class MoveCommand : INeutralUsableCommand, IAirUsableCommand
    {
        private float normalSpeedMagnification = 1.0f;  //通常時の速度倍率
        private float dashSpeedMagnification = 1.8f;    //ダッシュ時の速度倍率
        private Player.CharacterController characterController;
        private PlayerInputManager playerInputManager;
        public bool IsFinished => !playerInputManager.IsMoving;

        public MoveCommand(Player.CharacterController characterController, PlayerInputManager playerInputManager)
        {
            this.characterController = characterController;
            this.playerInputManager = playerInputManager;
        }

        public void Execute()
        {
            if (playerInputManager.IsDashing)
            {
                characterController.MoveCharacter(dashSpeedMagnification);
            }
            else
                characterController.MoveCharacter(normalSpeedMagnification);
        }

        public void Undo()
        {
        }
    }
}
