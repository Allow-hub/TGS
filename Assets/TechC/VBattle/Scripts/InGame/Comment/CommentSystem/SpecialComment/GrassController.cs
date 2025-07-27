
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// 草コメントのエフェクトと返却処理を管理するクラス。
    /// </summary>
    public class GrassController : MonoBehaviour
    {
        [Header("文字のPrefab")]
        [SerializeField] private GameObject grassChar;

        [Header("エフェクトのPrefab")]
        [SerializeField] private GameObject grassEffect;
        [SerializeField] private float returnDelay = 3f;
        [SerializeField] private Vector2 throwUpwardPower; // 斜め上成分
        [SerializeField] private float throwPower = 10f;        // 投げる力

        private bool isReturning = false;

        /* 角度の定数 */
        private const float ROTATE_GROUND = 0f;
        private const float ROTATE_CEILING = 180f;
        private const float ROTATE_RIGHT_WALL = -90f;
        private const float ROTATE_LEFT_WALL = 90f;

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

                var rb = GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeAll;  // 位置、回転を固定

                grassChar.SetActive(false);
                grassEffect.SetActive(true);
                AudioManager.I.PlaySE(SEID.Grass);

                if (!isReturning)
                {
                    isReturning = true;

                    DelayUtility.StartDelayedAction(this, returnDelay, () =>
                    {
                        EffectFactory.I.ReturnEffect(gameObject);
                        isReturning = false;
                    });
                }
            }
        }


        public void Init()
        {
            grassChar.SetActive(true);
            grassEffect.SetActive(false);
        }
        /// <summary>
        /// 草を投げる処理
        /// </summary>
        public void Throw()
        {
            var rb = GetComponent<Rigidbody>();
            var collider = GetComponent<BoxCollider>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.isKinematic = false;
                rb.useGravity = true;

                collider.isTrigger = true;
                var character = transform.root;
                var dirZ = Random.Range(throwUpwardPower.x, throwUpwardPower.y);

                // X方向（左右）＋斜め上に投げる
                Vector3 throwDirection = (character.transform.forward + Vector3.up * dirZ).normalized;

                rb.velocity = Vector3.zero;
                rb.AddForce(throwDirection * throwPower, ForceMode.Impulse);
                transform.SetParent(null);
                // Z軸の移動を完全に固定
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            }
        }
    }
}