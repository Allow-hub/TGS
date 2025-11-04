using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// ヒットした地点にテレポート
    /// </summary>
    [System.Serializable]
    public class AttackTeleportHitPos : IAttackBehaviour
    {
        [SerializeField] private Vector2 teleportOffset;
        private GameObject character;
        private AttackObjectController attackObjectController;

        public void Initialize(GameObject owner)
        {
            attackObjectController = owner.GetComponent<AttackObjectController>();
        }

        public void OnRelease()
        {
            character = null;
        }

        public void OnUpdate(float deltaTime)
        {
        }
        public void Activate(GameObject character)
        {
            this.character = character;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(attackObjectController.PlayerTag)) return;
            var characterController = other.transform.root.GetComponent<CharacterController>();
            if (characterController == null) return;
            if (characterController.PlayerID == attackObjectController.PlayerID) return;
            if (character == null) return;

            // キャラクターの向きに応じてX方向のオフセットを調整
            float directionMultiplier = character.transform.forward.x < 0 ? -1 : 1;
            var adjustedOffset = new Vector2(teleportOffset.x * directionMultiplier, teleportOffset.y);

            var pos = new Vector3(
                other.transform.position.x + adjustedOffset.x,
                other.transform.position.y + adjustedOffset.y,
                other.transform.position.z
            );
            character.transform.root.transform.position = pos;
        }
    }
}