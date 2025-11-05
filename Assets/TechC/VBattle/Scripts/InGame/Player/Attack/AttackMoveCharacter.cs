using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// この攻撃を放ったキャラクターを移動させる
    /// </summary>
    [System.Serializable]
    public class AttackMoveCharacter : IAttackBehaviour
    {
        [SerializeField] private float moveSpeed =5;
        [SerializeField] private Vector3 moveDir = Vector3.forward;

        private Rigidbody charRb;
        private Vector3 currentMoveDir;
        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
            if(charRb==null)return;
            charRb.velocity = Vector3.zero; // リリース時に速度をリセット
            currentMoveDir = Vector3.zero; // 移動方向もリセット
        }

        public void OnUpdate(float deltaTime)
        {
            if (charRb == null) return;
            Vector3 delta = currentMoveDir.normalized * moveSpeed * deltaTime;
            charRb.MovePosition(charRb.position + delta);
        }
                
        public void Activate(GameObject character)
        {
            charRb = character.transform.root.GetComponent<Rigidbody>();
            if (charRb == null) return;
            currentMoveDir = new Vector3(moveDir.x * character.transform.forward.x, moveDir.y, moveDir.z);
        }
    }
}
