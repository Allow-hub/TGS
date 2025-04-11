using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class GuardCommand : INeutralUsableCommand
    {
        private Player.CharacterController characterController;
        private PlayerInputManager playerInputManager;
        private int guardAnim = Animator.StringToHash("IsGuarding");
        private bool isForceFinished;

        public bool IsFinished => isForceFinished || !playerInputManager.IsGuarding;

        public GuardCommand(Player.CharacterController characterController,PlayerInputManager playerInputManager)
        {
            this.characterController = characterController;
            this.playerInputManager = playerInputManager;
        }

        public void Execute()
        {
            isForceFinished = false;
            characterController.SetAnim(guardAnim,true);
            if(IsFinished)
            {
                characterController.SetAnim(guardAnim, false);

            }
        }

        public void Undo()
        {
        }

        public void ForceFinish()
        {
            isForceFinished = true;
            characterController.SetAnim(guardAnim, false);
        }
    }
}
