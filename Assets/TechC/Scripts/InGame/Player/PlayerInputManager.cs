using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.Player
{
    public class PlayerInputManager : MonoBehaviour
    {

        public Action jumpAction;
        public Action appealAction;
        public Action<WeakAttackMode> weakAttackAction;
        public Action strongAttackAction;


        public Vector2 MoveInput => moveInput;
        public bool IsCrouching => isCrouching;
        public bool IsGarding => isGarding;
        private Vector2 moveInput;


        private bool isCrouching;
        private bool isGarding; 

    

        public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
        public void OnJump(InputAction.CallbackContext context) => jumpAction?.Invoke();
        public void OnAppeal(InputAction.CallbackContext context) => appealAction.Invoke();

        public void OnGard(InputAction.CallbackContext context)
        {
            if (context.started)
                isGarding = true;
            else if (context.canceled)
                isGarding = false;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
                isCrouching = true;
            else if (context.canceled)
                isCrouching = false;
        }


        public void OnWeakAttack(InputAction.CallbackContext context)
        {
            weakAttackAction?.Invoke(WeakAttackMode.Neutral);
        }

        public void OnStrongAttack(InputAction.CallbackContext context)
        {

        }
    }
}