using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class GuardCommand : INeutralUsableCommand
    {
        //参照
        private GameObject guardObj;
        private Player.CharacterController characterController;
        private BaseInputManager playerInputManager;
        private CharacterData characterData;
        private CharacterState characterState;

        //animation
        private int guardAnim = Animator.StringToHash("IsGuarding");


        //終了
        private bool isForceFinished;//強制終了
        public bool IsFinished => isForceFinished || !playerInputManager.IsGuarding;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="characterController"></param>
        /// <param name="playerInputManager"></param>
        /// <param name="characterData"></param>
        /// <param name="guardObj"></param>
        public GuardCommand(CharacterState characterState,Player.CharacterController characterController,BaseInputManager playerInputManager,CharacterData characterData,GameObject guardObj)
        {
            this.characterState = characterState;
            this.characterController = characterController;
            this.playerInputManager = playerInputManager;
            this.characterData = characterData;
            this.guardObj = guardObj;
        }

        public void Execute()
        {
            if (characterController.GetGuardPower() <= 0)
            {
                Debug.Log("ガードが復活してません");
                ForceFinish();
            }
            isForceFinished = false;
            characterState.ChangeGuardrState();
            characterController.SetAnim(guardAnim,true);
            guardObj.SetActive(true);
            characterController.SetLastGuardTime(Time.time);//ガードをした時間を記録
            if (IsFinished)
            {
                characterController.SetAnim(guardAnim, false);
                guardObj.SetActive(false);
                characterState.ChangeNeutralState();
            }
        }

        public void Undo()
        {
        }

        public void ForceFinish()
        {
            Debug.Log("ガードが強制終了しました");
            isForceFinished = true;
            playerInputManager.ResetInput();

        }
    }
}
