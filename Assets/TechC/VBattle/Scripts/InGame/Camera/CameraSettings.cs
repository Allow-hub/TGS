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
        [Header("Zoom Settings")]
        [SerializeField] public float minDistance = 5f;
        [SerializeField] public float maxDistance = 20f;
        [SerializeField] public float minFOV = 30f;
        [SerializeField] public float maxFOV = 60f;
        [SerializeField] public float zoomSpeed = 5f;
        [SerializeField] public float zoomMargin = 2f; // ズーム時の余白

        [Header("Follow Settings")]
        [SerializeField] public bool enableYAxisFollow = true;

        [Header("Camera Effects")]
        [SerializeField] public bool enableCameraShake = true;
        [SerializeField] public float defaultShakeIntensity = 1f;
        [SerializeField] public float defaultShakeDuration = 0.2f;
        [SerializeField] public NoiseSettings defaultShakeProfile;

        [Header("Advanced Settings")]
        public float cameraDistance = 10f;
        public Vector2 screenOffset = new Vector2(0.5f, 0.5f);
        [SerializeField] public float deadZone = 2f; // プレイヤーがこの範囲内にいる時はカメラを動かさない
        [SerializeField] public bool adaptToPlayerSpeed = true; // プレイヤーの速度に応じてカメラを先読み
        [SerializeField] public float anticipationFactor = 0.5f;

        [Header("Target Group Settings")]
        [SerializeField] public float defaultPlayerWeight = 1f;
        [SerializeField] public float defaultPlayerRadius = 1f;

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