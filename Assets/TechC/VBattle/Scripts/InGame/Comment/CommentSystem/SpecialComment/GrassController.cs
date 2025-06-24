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
        [SerializeField] private float returnDelay = 3f;

        private bool isReturning = false;

        /* 角度の定数 */
        private const float ROTATE_GROUND = 0f;
        private const float ROTATE_CEILING = 180f;
        private const float ROTATE_RIGHT_WALL = 90f;
        private const float ROTATE_LEFT_WALL = -90f;

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

                if (!isReturning)
                {
                    isReturning = true;

                    DelayUtility.StartDelayedAction(this, returnDelay, () =>
                    {
                        EffectFactory.I.ReturnEffect(gameObject);
                    });
                }
            }
        }
    }
}