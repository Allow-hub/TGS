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
            // Hold専用の機能：コメントを固定位置に保持する
            if (commentTransform != null)
            {
                commentTransform.localPosition = Vector3.zero;
                commentTransform.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// オブジェクトを手に装着する
        /// </summary>
        public void AttachToHand(GameObject obj, Transform handTransform)
        {
            obj.transform.SetParent(handTransform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
    }
}
