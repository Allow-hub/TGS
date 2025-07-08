using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

namespace TechC
{
    /// <summary>
    /// インゲームのカメラを管理するクラス
    /// スマブラ風アクションゲーム用に最適化
    /// ScriptableObject対応版
    /// </summary>
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private GameObject camObj;
        [SerializeField] private BoxCollider camCol;
        
        protected override bool UseDontDestroyOnLoad => false;

        [Header("Cinemachine References")]
        public CinemachineVirtualCamera vcam;
        public CinemachineTargetGroup targetGroup;

        [Header("Camera Settings")]
        [SerializeField] private CameraSettings normalCameraSettingsOriginal;
        [SerializeField] private CameraSettings ultCameraSettingsOriginal;

        private CameraSettings currentCameraSettings;
        private CameraSettings normalCameraSettings;
        private CameraSettings ultCameraSettings;

        [SerializeField, ReadOnly] private const string LOGTAG = "camera";

        [SerializeField, ReadOnly] private List<Transform> players = new List<Transform>();
        private Dictionary<Transform, PlayerCameraData> playerDataMap = new Dictionary<Transform, PlayerCameraData>();
        private CinemachineBasicMultiChannelPerlin noiseComponent;

        private float shakeTimer;
        private float lastUpdateTime;

        public System.Action<Transform> OnPlayerAdded;
        public System.Action<Transform> OnPlayerRemoved;
        public System.Action<float> OnZoomChanged;

        private float MinDistance => currentCameraSettings != null ? currentCameraSettings.minDistance : 5f;
        private float MaxDistance => currentCameraSettings != null ? currentCameraSettings.maxDistance : 20f;
        private float MinFOV => currentCameraSettings != null ? currentCameraSettings.minFOV : 30f;
        private float MaxFOV => currentCameraSettings != null ? currentCameraSettings.maxFOV : 60f;
        private float ZoomSpeed => currentCameraSettings != null ? currentCameraSettings.zoomSpeed : 5f;
        private float ZoomMargin => currentCameraSettings != null ? currentCameraSettings.zoomMargin : 2f;
        private bool EnableYAxisFollow => currentCameraSettings != null ? currentCameraSettings.enableYAxisFollow : true;
        private bool EnableCameraShake => currentCameraSettings != null ? currentCameraSettings.enableCameraShake : true;
        private float DefaultShakeIntensity => currentCameraSettings != null ? currentCameraSettings.defaultShakeIntensity : 1f;
        private float DefaultShakeDuration => currentCameraSettings != null ? currentCameraSettings.defaultShakeDuration : 0.2f;
        private NoiseSettings DefaultShakeProfile => currentCameraSettings != null ? currentCameraSettings.defaultShakeProfile : null;
        private bool AdaptToPlayerSpeed => currentCameraSettings != null ? currentCameraSettings.adaptToPlayerSpeed : true;
        private float AnticipationFactor => currentCameraSettings != null ? currentCameraSettings.anticipationFactor : 0.5f;
        private float DefaultPlayerWeight => currentCameraSettings != null ? currentCameraSettings.defaultPlayerWeight : 1f;
        private float DefaultPlayerRadius => currentCameraSettings != null ? currentCameraSettings.defaultPlayerRadius : 1f;

        protected override void Init()
        {
            CustomLogger.Info("CameraManager.Init() called", LOGTAG);
            base.Init();

            if (normalCameraSettingsOriginal == null || ultCameraSettingsOriginal == null)
            {
                CustomLogger.Warning("CameraSettingsOriginal が設定されていません。", LOGTAG);
            }
            else
            {
                normalCameraSettings = Instantiate(normalCameraSettingsOriginal);
                ultCameraSettings = Instantiate(ultCameraSettingsOriginal);
                normalCameraSettings.ValidateSettings();
                ultCameraSettings.ValidateSettings();
                currentCameraSettings = normalCameraSettings;
                CustomLogger.Info("Cloned camera settings and set current to normal.", LOGTAG);
            }

            DelayUtility.StartDelayedAction(this, 0.11f, () =>
            {
                if (vcam != null)
                {
                    InitializeCamera();
                    RegisterPlayers();
                    BattleJudge.I.OnUltStart.AddListener(SetUltCamera);
                }
                else
                {
                    CustomLogger.Error("CameraManager: vcam is not set in delayed action", LOGTAG);
                }
            });
        }

        protected override void OnRelease()
        {
            CustomLogger.Info("CameraManager.OnRelease() called", LOGTAG);
            base.OnRelease();
            BattleJudge.I.OnUltStart.RemoveListener(SetUltCamera);
            ClearTargets();
        }

        void Update()
        {
            CustomLogger.Info("CameraManager.Update() called", LOGTAG);
            UpdateCameraZoom();
            UpdateCameraShake();
            ValidatePlayers();
        }

        /// <summary>
        /// カメラの初期化
        /// </summary>
        private void InitializeCamera()
        {
            CustomLogger.Info("InitializeCamera() called", LOGTAG);
            if (vcam == null)
            {
                CustomLogger.Error("CameraManager: VirtualCameraが設定されていません", LOGTAG);
                return;
            }
            else
            {
                CustomLogger.Info("vcam is set: " + vcam.name, LOGTAG);
            }

            noiseComponent = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noiseComponent == null && EnableCameraShake)
            {
                noiseComponent = vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                CustomLogger.Info("noiseComponent was null, added new CinemachineBasicMultiChannelPerlin", LOGTAG);
            }
            else
            {
                CustomLogger.Info("noiseComponent is set", LOGTAG);
            }
            if (noiseComponent != null)
            {
                noiseComponent.m_AmplitudeGain = 0f;
            }

            if (targetGroup != null)
            {
                CustomLogger.Info("targetGroup is set: " + targetGroup.name, LOGTAG);
                vcam.Follow = targetGroup.transform;
                vcam.LookAt = targetGroup.transform;
                targetGroup.m_PositionMode = CinemachineTargetGroup.PositionMode.GroupCenter;
                targetGroup.m_RotationMode = CinemachineTargetGroup.RotationMode.Manual;
                targetGroup.m_UpdateMethod = CinemachineTargetGroup.UpdateMethod.LateUpdate;
            }
            else
            {
                CustomLogger.Error("CameraManager: targetGroupが設定されていません", LOGTAG);
            }
            
            lastUpdateTime = Time.time;
        }

        /// <summary>
        /// BattleJudgeからプレイヤーを登録
        /// </summary>
        private void RegisterPlayers()
        {
            CustomLogger.Info("RegisterPlayers() called", LOGTAG);
            if (BattleJudge.I?.Players == null)
            {
                CustomLogger.Error("BattleJudge.I.Players is null", LOGTAG);
                return;
            }
            ApplyZoomSettings();

            foreach (var playerInfo in BattleJudge.I.Players)
            {
                if (playerInfo?.playerObject != null)
                {
                    CustomLogger.Info("Registering player: " + playerInfo.playerObject.name, LOGTAG);
                    AddPlayer(playerInfo.playerObject.transform);
                }
                else
                {
                    CustomLogger.Warning("playerInfo or playerObject is null", LOGTAG);
                }
            }
        }

        /// <summary>
        /// プレイヤー間の距離に基づいてカメラのズームを更新
        /// </summary>
        private void UpdateCameraZoom()
        {
            CustomLogger.Info("UpdateCameraZoom() called", LOGTAG);
            var activePlayers = GetActivePlayers();
            CustomLogger.Info($"ActivePlayers count: {activePlayers.Count}", LOGTAG);
            if (activePlayers.Count < 2)
            {
                CustomLogger.Warning("ActivePlayers less than 2, skipping zoom update", LOGTAG);
                return;
            }

            float maxDistanceBetweenPlayers = CalculateMaxDistance(activePlayers);
            CustomLogger.Info($"maxDistanceBetweenPlayers: {maxDistanceBetweenPlayers}", LOGTAG);

            // 先読み機能：プレイヤーの移動速度を考慮
            if (AdaptToPlayerSpeed)
            {
                float anticipatedDistance = CalculateAnticipatedDistance(activePlayers, maxDistanceBetweenPlayers);
                CustomLogger.Info($"anticipatedDistance: {anticipatedDistance}", LOGTAG);
                maxDistanceBetweenPlayers = Mathf.Max(maxDistanceBetweenPlayers, anticipatedDistance);
            }

            // マージンを追加
            maxDistanceBetweenPlayers += ZoomMargin;

            // 距離に応じてFOVを調整
            float t = Mathf.InverseLerp(MinDistance, MaxDistance, maxDistanceBetweenPlayers);
            float targetFOV = Mathf.Lerp(MinFOV, MaxFOV, t);

            float currentFOV = vcam.m_Lens.FieldOfView;
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * ZoomSpeed);

            CustomLogger.Info($"currentFOV: {currentFOV}, targetFOV: {targetFOV}, newFOV: {newFOV}", LOGTAG);

            vcam.m_Lens.FieldOfView = newFOV;

            // ズーム変更イベント
            if (Mathf.Abs(newFOV - currentFOV) > 0.1f)
            {
                CustomLogger.Info($"OnZoomChanged invoked: {newFOV}", LOGTAG);
                OnZoomChanged?.Invoke(newFOV);
            }
        }

        /// <summary>
        /// アクティブなプレイヤーのリストを取得
        /// </summary>
        private List<Transform> GetActivePlayers()
        {
            return players.Where(p => p != null && p.gameObject.activeInHierarchy).ToList();
        }

        /// <summary>
        /// プレイヤー間の最大距離を計算
        /// </summary>
        private float CalculateMaxDistance(List<Transform> activePlayers)
        {
            float maxDistance = 0f;

            for (int i = 0; i < activePlayers.Count; i++)
            {
                for (int j = i + 1; j < activePlayers.Count; j++)
                {
                    Vector3 pos1 = activePlayers[i].position;
                    Vector3 pos2 = activePlayers[j].position;

                    // Y軸フォローが無効の場合はY軸を無視
                    if (!EnableYAxisFollow)
                    {
                        pos1.y = 0;
                        pos2.y = 0;
                    }

                    float dist = Vector3.Distance(pos1, pos2);
                    maxDistance = Mathf.Max(maxDistance, dist);
                }
            }

            return maxDistance;
        }

        /// <summary>
        /// プレイヤーの移動速度を考慮した予測距離を計算
        /// </summary>
        private float CalculateAnticipatedDistance(List<Transform> activePlayers, float currentMaxDistance)
        {
            float anticipatedDistance = currentMaxDistance;
            float deltaTime = Time.time - lastUpdateTime;

            if (deltaTime > 0f)
            {
                foreach (var player in activePlayers)
                {
                    if (playerDataMap.TryGetValue(player, out PlayerCameraData data))
                    {
                        Vector3 velocity = (player.position - data.lastPosition) / deltaTime;
                        Vector3 anticipatedPos = player.position + velocity * AnticipationFactor;

                        // 他のプレイヤーとの予測距離を計算
                        foreach (var otherPlayer in activePlayers)
                        {
                            if (otherPlayer != player)
                            {
                                float anticipatedDist = Vector3.Distance(anticipatedPos, otherPlayer.position);
                                anticipatedDistance = Mathf.Max(anticipatedDistance, anticipatedDist);
                            }
                        }

                        data.lastPosition = player.position;
                    }
                }
            }

            lastUpdateTime = Time.time;
            return anticipatedDistance;
        }

        /// <summary>
        /// カメラシェイクの更新
        /// </summary>
        private void UpdateCameraShake()
        {
            if (!EnableCameraShake || noiseComponent == null)
            {
                CustomLogger.Warning("Camera shake disabled or noiseComponent is null", LOGTAG);
                return;
            }

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;

                // シェイクの強度を時間とともに減衰
                float normalizedTime = Mathf.Clamp01(shakeTimer / DefaultShakeDuration);
                noiseComponent.m_AmplitudeGain = DefaultShakeIntensity * normalizedTime;
                CustomLogger.Info($"Shake active: amplitude={noiseComponent.m_AmplitudeGain}, timer={shakeTimer}", LOGTAG);
            }
            else
            {
                noiseComponent.m_AmplitudeGain = 0f;
                CustomLogger.Info("Shake ended", LOGTAG);
            }
        }

        /// <summary>
        /// 無効になったプレイヤーを検証・削除
        /// </summary>
        private void ValidatePlayers()
        {
            CustomLogger.Info("ValidatePlayers() called", LOGTAG);
            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i] == null || !players[i].gameObject.activeInHierarchy)
                {
                    CustomLogger.Warning($"Player at index {i} is null or inactive, removing", LOGTAG);
                    RemovePlayerAtIndex(i);
                }
            }
        }

        private void SetUltCamera()
        {
            SwitchCameraSettings(true);

        }

        #region パブリックメソッド
        /// <summary>
        /// プレイヤーを1人追加し、TargetGroupにも登録
        /// </summary>
        public void AddPlayer(Transform player, float weight = -1f, float radius = -1f)
        {
            if (player == null || players.Contains(player)) return;

            float actualWeight = weight > 0f ? weight : DefaultPlayerWeight;
            float actualRadius = radius > 0f ? radius : DefaultPlayerRadius;

            players.Add(player);
            playerDataMap[player] = new PlayerCameraData { lastPosition = player.position };

            if (targetGroup != null)
            {
                targetGroup.AddMember(player, actualWeight, actualRadius);
            }

            OnPlayerAdded?.Invoke(player);
            CustomLogger.Info($"CameraManager: プレイヤー {player.name} を追加しました" + player, LOGTAG);
        }
        /// <summary>
        /// 指定インデックスのプレイヤーを削除
        /// </summary>
        private void RemovePlayerAtIndex(int index)
        {
            if (index < 0 || index >= players.Count) return;

            Transform player = players[index];
            players.RemoveAt(index);
            playerDataMap.Remove(player);

            if (targetGroup != null)
            {
                targetGroup.RemoveMember(player);
            }

            OnPlayerRemoved?.Invoke(player);
            CustomLogger.Info($"CameraManager: プレイヤー {player?.name} を削除しました", LOGTAG);
        }
        public void SetCameraDistance(float value)
        {
            var transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer == null)
            {
                CustomLogger.Warning("CameraManager: FramingTransposer が見つかりません", LOGTAG);
                return;
            }

            transposer.m_CameraDistance = value;
            CustomLogger.Info($"CameraManager: カメラ距離を {value} に設定しました", LOGTAG);
        }

        /// <summary>
        /// 全プレイヤーとターゲットグループを初期化
        /// </summary>
        public void ClearTargets()
        {
            // イベント通知
            foreach (var player in players.ToList())
            {
                OnPlayerRemoved?.Invoke(player);
            }

            players.Clear();
            playerDataMap.Clear();

            if (targetGroup != null)
            {
                targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
            }

            CustomLogger.Info("CameraManager: 全ターゲットをクリアしました", LOGTAG);
        }

        /// <summary>
        /// カメラシェイクを開始,CM Vcam1->Noise->NoiseProfileの歯車からEditで
        /// Shakeのデータを拾えます
        /// </summary>
        public void StartShake(float intensity = -1f, float duration = -1f,NoiseSettings noiseSettings = null)
        {
            if (!EnableCameraShake) return;

            float shakeIntensity = intensity > 0f ? intensity : DefaultShakeIntensity;
            float shakeDuration = duration > 0f ? duration : DefaultShakeDuration;
            NoiseSettings shakeProfile = noiseSettings != null ? noiseSettings : DefaultShakeProfile;

            if (noiseComponent == null)
            {
                noiseComponent = vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }

            if (shakeProfile != null)
            {
                noiseComponent.m_NoiseProfile = shakeProfile;
            }

            noiseComponent.m_AmplitudeGain = shakeIntensity;
            noiseComponent.m_FrequencyGain = 1.0f;
            shakeTimer = shakeDuration;
        }

        /// <summary>
        /// カメラシェイクを停止
        /// </summary>
        public void StopShake()
        {
            shakeTimer = 0f;
            if (noiseComponent != null)
            {
                noiseComponent.m_AmplitudeGain = 0f;
            }
        }

        /// <summary>
        /// ズーム設定を動的に変更
        /// </summary>
        public void SetZoomSettings(float minFOV, float maxFOV, float minDist, float maxDist)
        {
            if (currentCameraSettings == null)
            {
                CustomLogger.Warning("CameraManager: currentCameraSettingsが設定されていません", LOGTAG);
                return;
            }

            currentCameraSettings.minFOV = Mathf.Max(1f, minFOV);
            currentCameraSettings.maxFOV = Mathf.Max(currentCameraSettings.minFOV, maxFOV);
            currentCameraSettings.minDistance = Mathf.Max(0.1f, minDist);
            currentCameraSettings.maxDistance = Mathf.Max(currentCameraSettings.minDistance, maxDist);

            ApplyZoomSettings();
        }
        /// <summary>
        /// カメラのオフセットを設定
        /// </summary>
        public void SetCameraOffset(Vector3 offset)
        {
            if (vcam == null)
            {
                CustomLogger.Warning("CameraManager: vcam が設定されていません", LOGTAG);
                return;
            }

            var transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer == null)
            {
                CustomLogger.Warning("CameraManager: FramingTransposer が見つかりません", LOGTAG);
                return;
            }

            transposer.m_ScreenX = Mathf.Clamp01(offset.x);
            transposer.m_ScreenY = Mathf.Clamp01(offset.y);

            CustomLogger.Info($"CameraManager: Screen位置を X={offset.x}, Y={offset.y} に設定しました", LOGTAG);
        }

        /// <summary>
        /// デッドゾーンを設定
        /// </summary>
        public void SetDeadZone(float zone)
        {
            if (currentCameraSettings != null)
            {
                currentCameraSettings.deadZone = Mathf.Max(0f, zone);
                ApplyZoomSettings(); 
                CustomLogger.Info($"CameraManager: デッドゾーンを {zone} に設定しました", LOGTAG);
            }
        }
        /// <summary>
        /// カメラ設定をノーマルまたはアルティメットに切り替える
        /// </summary>
        public void SwitchCameraSettings(bool useUlt)
        {
            currentCameraSettings = useUlt ? ultCameraSettings : normalCameraSettings;
            camCol.center = currentCameraSettings.camColliderPos;
            camObj.transform.eulerAngles = currentCameraSettings.camRot;
            ApplyZoomSettings();
            CustomLogger.Info($"CameraManager: Switched camera settings to {(useUlt ? "ULT" : "NORMAL")}", LOGTAG);
        }

        /// <summary>
        /// ズーム設定をCinemachineコンポーネントに適用
        /// </summary>
        private void ApplyZoomSettings()
        {
            if (vcam == null) return;

            var transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer != null)
            {
                transposer.m_MinimumFOV = MinFOV;
                transposer.m_MaximumFOV = MaxFOV;

                transposer.m_CameraDistance = currentCameraSettings.cameraDistance;
                transposer.m_ScreenX = Mathf.Clamp01(currentCameraSettings.screenOffset.x);
                transposer.m_ScreenY = Mathf.Clamp01(currentCameraSettings.screenOffset.y);
                transposer.m_DeadZoneWidth = currentCameraSettings.deadZone;
                transposer.m_DeadZoneHeight = currentCameraSettings.deadZone;

            }
        }

        #endregion

        /// <summary>
        /// プレイヤーのカメラ用データ
        /// </summary>
        private class PlayerCameraData
        {
            public Vector3 lastPosition;
            public Vector3 velocity;
            public float lastUpdateTime;
        }
    }
}