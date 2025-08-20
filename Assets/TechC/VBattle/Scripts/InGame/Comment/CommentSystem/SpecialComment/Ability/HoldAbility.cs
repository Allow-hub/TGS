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

            // GameObject grassInstance = CreateAndAttachGrass(characterController);
            //     RegisterThrowEvent(characterController, grassInstance);
        }

         /// <summary>
        /// 草オブジェクトを生成し、キャラクターの手に装着する
        /// </summary>
        private GameObject CreateAndAttachGrass(Player.CharacterController characterController)
        {
            GameObject grassInstance = EffectFactory.I.GetEffectObj(
                characterController.GrassPrefab, 
                characterController.HandPos.position, 
                Quaternion.identity
            );
            
            AttachToHand(grassInstance, characterController.HandPos);
            return grassInstance;
        }

        /// <summary>
        /// オブジェクトを手に装着する
        /// </summary>
        private void AttachToHand(GameObject obj, Transform handTransform)
        {
            obj.transform.SetParent(handTransform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }

    }
}
