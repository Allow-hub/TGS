using UnityEngine;
using System;

namespace TechC.CommentSystem
{
    [Serializable]
    public class ThrowAbility : ICommentAbility
    {
        [SerializeField] private float throwPower = 10f;
        [SerializeField] private Vector2 throwUpwardPower = new Vector2(0.5f, 1.0f);

        private Rigidbody rb;
        private Transform commentTransform;
        public void Init(SpecialCommentTrigger trigger)
        {
            rb = trigger.GetComponent<Rigidbody>();
            commentTransform = trigger.transform;
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            if (CommentDisplay.I.IsCommentFrozen) return;

            var characterController = collider.GetComponentInParent<Player.CharacterController>();
            if (characterController != null)
            {
                characterController.SpawnGrassEffect(this);
            }
        }

        public void Throw(Rigidbody rb, GameObject commentObj)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity = true;
            var character = commentObj.transform.root;
            var dirZ = UnityEngine.Random.Range(throwUpwardPower.x, throwUpwardPower.y);
            Vector3 throwDirection = (character.forward + Vector3.up * dirZ).normalized;
            rb.velocity = Vector3.zero;
            rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);
            commentObj.transform.SetParent(null);
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
    }
}
