using UnityEngine;
using System;

namespace TechC.CommentSystem
{
    [Serializable]
    public class GrassAbility : ICommentAbility
    {
        [SerializeField] private ThrowAbility throwAbility;
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
            Debug.Log("Grass");

            var characterController = collider.GetComponentInParent<Player.CharacterController>();
            if (throwAbility == null)
            {
                Debug.LogError("GrassAbility: ThrowAbilityがアサインされていません");
                return;
            }
            characterController.SpawnGrassEffect(throwAbility);
        }
    }
}
