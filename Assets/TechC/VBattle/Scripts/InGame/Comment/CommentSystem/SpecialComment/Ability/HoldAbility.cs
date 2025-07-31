using UnityEngine;

namespace TechC.CommentSystem
{
    public class HoldAbility : ICommentAbility
    {
        private Transform commentTransform;

        public void Init(SpecialCommentTrigger trigger)
        {
            commentTransform = trigger.transform;
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            if (commentTransform != null)
            {
                commentTransform.localPosition = Vector3.zero;
                commentTransform.localRotation = Quaternion.identity;
            }
        }

    }
}
