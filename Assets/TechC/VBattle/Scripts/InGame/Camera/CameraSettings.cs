using UnityEngine;
using Cinemachine;

namespace TechC
{
    /// <summary>
    /// CameraManagerの設定値を管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "TechC/Camera Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("カメラ設定")]
        [SerializeField] private Vector3 camRot;
        [SerializeField] private Vector3 camColliderPos;

        [Header("ズーム設定")]
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float minFOV = 30f;
        [SerializeField] private float maxFOV = 60f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float zoomMargin = 2f;

        [Header("追従設定")]
        [SerializeField] private bool enableYAxisFollow = true;

        [Header("カメラエフェクト")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float defaultShakeIntensity = 1f;
        [SerializeField] private float defaultShakeDuration = 0.2f;
        [SerializeField] private NoiseSettings defaultShakeProfile;

        [Header("高度な設定")]
        [SerializeField] private float cameraDistance = 10f;
        [SerializeField] private Vector2 screenOffset = new Vector2(0.5f, 0.5f);
        [SerializeField] private float deadZone = 2f;
        [SerializeField] private bool adaptToPlayerSpeed = true;
        [SerializeField] private float anticipationFactor = 0.5f;

        [Header("ターゲットグループ設定")]
        [SerializeField] private float defaultPlayerWeight = 1f;
        [SerializeField] private float defaultPlayerRadius = 1f;

        // プロパティで外部参照
        public Vector3 CamRot => camRot;
        public Vector3 CamColliderPos => camColliderPos;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
        public float MinFOV => minFOV;
        public float MaxFOV => maxFOV;
        public float ZoomSpeed => zoomSpeed;
        public float ZoomMargin => zoomMargin;
        public bool EnableYAxisFollow => enableYAxisFollow;
        public bool EnableCameraShake => enableCameraShake;
        public float DefaultShakeIntensity => defaultShakeIntensity;
        public float DefaultShakeDuration => defaultShakeDuration;
        public NoiseSettings DefaultShakeProfile => defaultShakeProfile;
        public float CameraDistance => cameraDistance;
        public Vector2 ScreenOffset => screenOffset;
        public float DeadZone => deadZone;
        public bool AdaptToPlayerSpeed => adaptToPlayerSpeed;
        public float AnticipationFactor => anticipationFactor;
        public float DefaultPlayerWeight => defaultPlayerWeight;
        public float DefaultPlayerRadius => defaultPlayerRadius;

        /// <summary>
        /// 設定値を検証して有効な範囲に調整
        /// </summary>
        public void ValidateSettings()
        {
            minDistance = Mathf.Max(0.1f, minDistance);
            maxDistance = Mathf.Max(minDistance, maxDistance);
            minFOV = Mathf.Clamp(minFOV, 1f, 179f);
            maxFOV = Mathf.Clamp(maxFOV, minFOV, 179f);
            zoomSpeed = Mathf.Max(0.1f, zoomSpeed);
            zoomMargin = Mathf.Max(0f, zoomMargin);
            defaultShakeIntensity = Mathf.Max(0f, defaultShakeIntensity);
            defaultShakeDuration = Mathf.Max(0.01f, defaultShakeDuration);
            deadZone = Mathf.Max(0f, deadZone);
            anticipationFactor = Mathf.Max(0f, anticipationFactor);
            defaultPlayerWeight = Mathf.Max(0f, defaultPlayerWeight);
            defaultPlayerRadius = Mathf.Max(0f, defaultPlayerRadius);
        }

        private void OnValidate()
        {
            ValidateSettings();
        }
    }
}