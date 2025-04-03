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
        public Action strongAttackAction;


        public Vector2 MoveInput => moveInput;
        public bool IsCrouching => isCrouching;
        public bool IsJumping => isJumping; 
        public bool IsGarding => isGarding;
        public bool IsAppealing => isAppealing; 
        public bool IsWeakAttacking => isWeakAttacking;
        public bool IsStrongAttacking => isStrongAttacking;
        private Vector2 moveInput;


        private bool isCrouching;
        private bool isGarding;
        private bool isJumping;
        private bool isAppealing;
        private bool isWeakAttacking;
        private bool isStrongAttacking;

        public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
                isJumping = true;
            else if(context.canceled)
                isJumping = false;
        }
        public void OnAppeal(InputAction.CallbackContext context)
        {
            if (context.started)
                isAppealing = true;
            else if (context.canceled)
                isAppealing = false;
        }
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
            if (context.started)
                isWeakAttacking = true;
            else if (context.canceled)
                isWeakAttacking = false;
        }

        public void OnStrongAttack(InputAction.CallbackContext context)
        {
            if (context.started)
                isStrongAttacking = true;
            else if (context.canceled)
                isStrongAttacking = false;
        }
    }
}