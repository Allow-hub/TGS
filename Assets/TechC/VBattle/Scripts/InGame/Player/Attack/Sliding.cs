using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player.Attack
{
    public class Sliding : IAttackBehaviour
    {
        [SerializeField] private float slidingSpeed = 5f;
        [SerializeField] private float chageColliderSpeed = 5f;// スライディング時の変化後の自分の当たり判定
        [SerializeField] private Vector3 changeHitBox;
        private CharacterController characterController;
        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
            if (characterController == null) return;
            characterController.ResetHitCollider(chageColliderSpeed);
            characterController.ChangeColliderTrigger(false);
            characterController = null;
        }

        public void OnUpdate(float deltaTime)
        {
        }
        public void Activate(GameObject character)
        {
            if (characterController == null)
                characterController = character.GetComponent<CharacterController>();
            characterController.StopVelocity();
            characterController.AddForcePlayer(character.transform.forward, slidingSpeed, ForceMode.Impulse);
            characterController.ChangeHitCollider(changeHitBox, chageColliderSpeed);
            characterController.ChangeColliderTrigger(true);
        }
    }
}
