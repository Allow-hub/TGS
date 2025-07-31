using UnityEngine;

namespace TechC.CommentSystem
{
    public class HoldAbility : ICommentAbility
    {
        private Transform commentTransform;
        private Transform handTransform;

        public void Init(SpecialCommentTrigger trigger)
        {
            commentTransform = trigger.transform;
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            Debug.Log("Hold");
            if (handTransform != null && commentTransform != null)
            {
                commentTransform.SetParent(handTransform);
                commentTransform.localPosition = Vector3.zero;
                commentTransform.localRotation = Quaternion.identity;
            }
        }

    }
}
