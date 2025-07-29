using System;
using UnityEngine;

namespace TechC.CommentSystem
{
    [Serializable]
    public class GrassAbility : ICommentAbility
    {

        [SerializeField] private GameObject grassChar;
        [SerializeField] private GameObject grassEffect;
        [SerializeField] private float returnDelay = 3f;
        [SerializeField] private Vector2 throwUpwardPower = new Vector2(0.5f, 1.0f);
        [SerializeField] private float throwPower = 10f;

        private Transform commentTransform;
        private SpecialCommentTrigger specialCommentTrigger;

        public void Init(GameObject commentObj)
        {
            commentTransform = commentObj.transform;
            specialCommentTrigger = commentObj.GetComponent<SpecialCommentTrigger>();
        }

        public void Init(SpecialCommentTrigger trigger)
        {
            Init(trigger.gameObject);
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            var characterController = collider.GetComponentInParent<Player.CharacterController>();
            characterController.SpawnGrassEffect(this);
        }


        public void Throw(Rigidbody rb,GameObject  throwObj)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.useGravity = true;
            var character = throwObj.transform.root;
            var dirZ = UnityEngine.Random.Range(throwUpwardPower.x, throwUpwardPower.y);
            Vector3 throwDirection = (character.forward + Vector3.up * dirZ).normalized;
            rb.velocity = Vector3.zero;
            rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);
            throwObj.transform.SetParent(null);
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

    }
}
