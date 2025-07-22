using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 攻撃オブジェクトの移動処理
    /// AttackObjectControllerが実行を管理
    /// </summary>
    [System.Serializable]
    public class AttackMover : IAttackBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Vector3 moveDir = Vector3.forward;

        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            if (rb == null) return;
            Vector3 delta = moveDir.normalized * moveSpeed * deltaTime;
            rb.MovePosition(rb.position + delta);
        }

        /// <summary>
        /// 移動方向を変更する
        /// </summary>
        public void SetDirection(Vector3 direction) => moveDir = direction.normalized;

        /// <summary>
        /// 移動速度を変更する
        /// </summary>
        public void SetSpeed(float speed) => moveSpeed = speed;
    }
}