using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private PlayerData playerData;


        [SerializeField] private float maxGage = 100;

        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private bool isDrawingRay;
        private Rigidbody rb;
        private float currentHp;

        private void Start()
        {
            rb =GetComponent<Rigidbody>();
            currentHp = playerData.Hp;
        }

        private void Update()
        {
        }

        public void AddForcePlayer(Vector3 dir, float force, ForceMode forceMode)
            => rb.AddForce(dir * force, forceMode);
        private void AddHp(float value)=> currentHp -= value;

        public float GetHp() => currentHp;




        public bool IsGrounded()
        {
            Vector3 rayOrigin = transform.position + Vector3.up;
            RaycastHit hit;

            return Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, LayerMask.GetMask("Ground"));
        }
        private void OnDrawGizmos()
        {
            if (!isDrawingRay) return;
            // レイの発射位置
            Vector3 rayOrigin = transform.position + Vector3.up;

            // 地面に当たるときは緑、そうでないときは赤
            Gizmos.color = IsGrounded() ? Color.green : Color.red;

            // レイの描画
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayLength);

            // レイの終端に球を表示
            Gizmos.DrawSphere(rayOrigin + Vector3.down * rayLength, 0.05f);
        }
    }
}
