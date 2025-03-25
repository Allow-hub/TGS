using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class IconManager : MonoBehaviour
    {
        /* アイコンの詳細設定 */
        [Header("アイコンの詳細設定")]
        [SerializeField] private float rorationSpeed = 30f; /* 回転する速度 */
        [SerializeField] private float targetAngle = 30f; /* 左右に折り返すための目標の角度 */

        /* フラグの設定 */
        private bool isRotatingRight = true; /* 右に回転しているかどうか */

        /* 定数の設定 */
        private const float MIN_ANGLE = 0.1f; /* 最小の角度 */

        // Update is called once per frame
        void Update()
        {
            RotationIcons(); /* アイコンを回転する */
        }

        /* アイコンを回転させる処理 */
        private void RotationIcons()
        {
            float currentAngle = transform.rotation.eulerAngles.y; /* 現在の角度を取得する */

            float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rorationSpeed * Time.deltaTime); /* 目標の角度に向かって回転する */

            if (Mathf.Abs(angle - targetAngle) < MIN_ANGLE)
            {
                if (isRotatingRight)
                {
                    targetAngle = -30f;
                }
                else
                {
                    targetAngle = 30f;
                }
                isRotatingRight = !isRotatingRight; /* 回転する向きを反転する */
            }

            /* 回転する処理 */
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}


