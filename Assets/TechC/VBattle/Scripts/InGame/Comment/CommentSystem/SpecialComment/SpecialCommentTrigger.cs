using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 特殊コメントの当たり判定を取り、種類によって通知を送るクラス
    /// </summary>
    public class SpecialCommentTrigger : MonoBehaviour
    {
        [HideInInspector] public SpecialCommentType specialType;

        private List<GameObject> chars;

        public void SetType(SpecialCommentType type) => specialType = type;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            switch (specialType)
            {
                case SpecialCommentType.Grass:
                    var characterController = other.transform.parent.GetComponent<Player.CharacterController>();
                    characterController.SpawnGrassEffect();
                    break;

                case SpecialCommentType.Freeze:
                    SpecialCommentManager.I.HandleFreeze(gameObject, chars);
                    CommentDisplay.I.OnFreezeTriggered();
                    break;
            }
        }
    }
}
