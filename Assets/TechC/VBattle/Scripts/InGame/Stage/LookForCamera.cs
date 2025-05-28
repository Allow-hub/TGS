using UnityEngine;

namespace TechC
{
    /// <summary>
    /// このオブジェクトを常にカメラの方向に向ける（ビルボード効果）
    /// </summary>
    public class LookForCamera : MonoBehaviour
    {
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            if (mainCamera == null) return;

            // カメラの位置からこのオブジェクトへ向かせる
            Vector3 direction = transform.position - mainCamera.transform.position;
            direction.y = 0f; // 水平回転のみしたい場合（垂直は無視）

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.forward = -direction.normalized;
            }
        }
    }
}
