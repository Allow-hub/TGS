using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class GrassController : MonoBehaviour
    {
        [Header("文字のPrefab")]
        [SerializeField] private GameObject grassChar;

        [Header("エフェクトのPrefab")]
        [SerializeField] private GameObject grassPrefab;

        /* 角度の定数 */
        private const float ROTATE_GROUND = 0f;
        private const float ROTATE_CEILING = 180f;
        private const float ROTATE_RIGHT_WALL = 90f;
        private const float ROTATE_LEFT_WALL = -90f;

        private Rigidbody rigidbody;

        private void Start()
        {
            // Rigidbodyがアタッチされていなければ取得
            if (rigidbody == null)
            {
                rigidbody = GetComponent<Rigidbody>();
            }

            // 上方向に力を加えてジャンプさせる（向き確認用）
            const float TEST_FORCE = -15f;
            if (rigidbody != null)
            {
                rigidbody.AddForce(Vector3.right * TEST_FORCE, ForceMode.Impulse);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            string layerName = LayerMask.LayerToName(other.gameObject.layer);
            if (layerName == "Ground" || layerName == "Wall")
            {
                // 衝突点を取得
                Vector3 contactPoint = other.ClosestPoint(transform.position);
                Vector3 direction = (transform.position - contactPoint).normalized;

                Quaternion targetRotation = Quaternion.identity;

                if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
                {
                    // 上下
                    if (direction.y > 0)
                    {
                        // 地面（下向き）: 通常
                        targetRotation = Quaternion.Euler(0, 0, ROTATE_GROUND);
                    }
                    else
                    {
                        // 天井（上向き）: 逆さま
                        targetRotation = Quaternion.Euler(0, 0, ROTATE_CEILING);
                    }
                }
                else
                {
                    // 左右
                    if (direction.x > 0)
                    {
                        // 右壁
                        targetRotation = Quaternion.Euler(0, 0, ROTATE_RIGHT_WALL);
                    }
                    else
                    {
                        // 左壁
                        targetRotation = Quaternion.Euler(0, 0, ROTATE_LEFT_WALL);
                    }
                }

                // 親オブジェクト（このスクリプトがアタッチされているオブジェクト）を回転・移動
                transform.position = contactPoint;
                transform.rotation = targetRotation;

                grassChar.SetActive(false);
                grassPrefab.SetActive(true);

                // Rigidbodyの動きを止める
                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                    rigidbody.isKinematic = true;
                }
            }
        }
    }
}