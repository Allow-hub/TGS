using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class SpecialCommentTrigger : MonoBehaviour
    {
        public SpecialCommentType specialType;

        private List<GameObject> chars;

        public void SetType(SpecialCommentType type) => specialType = type;
        public void SetChars(List<GameObject> chars) => this.chars = chars;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            switch (specialType)
            {
                case SpecialCommentType.Grass:
                    var characterController = other.transform.parent.GetComponent<Player.CharacterController>();
                    if (characterController != null)
                    {
                        characterController.SpawnGrassEffect();
                    }
                    break;
                case SpecialCommentType.Freeze:
                    if (CommentDisplay.I != null)
                    {
                        CommentDisplay.I.OnFreezeTriggered();
                    }
                    break;
            }
        }
    }
}
