using TechC.CommentSystem;
using UnityEngine;

namespace TechC.CommentSystem
{
    public class HoldAbility : ICommentAbility
    {
        private Transform commentTransform;
        private Transform handTransform;

        // handTransformはInitで外部から渡す or Inspectorでセット
        public void Init(SpecialCommentTrigger trigger)
        {
            commentTransform = trigger.transform;
        }

        public void Release() { }

        public void OnTriggerEnter(Collider collider)
        {
            if (handTransform != null && commentTransform != null)
            {
                commentTransform.SetParent(handTransform);
                commentTransform.localPosition = Vector3.zero;
                commentTransform.localRotation = Quaternion.identity;
            }
        }

    }
}
