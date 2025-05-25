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
    /// </summary>
    public class CameraManager : Singleton<CameraManager>
    {
        protected override bool UseDontDestroyOnLoad => false;

        [Header("Cinemachine References")]
        public CinemachineVirtualCamera vcam;
        public CinemachineTargetGroup targetGroup;

        [Header("Zoom Settings")]
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float minFOV = 30f;
        [SerializeField] private float maxFOV = 60f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float zoomMargin = 2f; // ズーム時の余白

        [Header("Follow Settings")]
        [SerializeField] private float followSmoothTime = 0.5f;
        [SerializeField] private Vector3 cameraOffset = Vector3.zero;
        [SerializeField] private bool enableYAxisFollow = true;

        [Header("Stage Bounds")]
        [SerializeField] private bool constrainToStage = true;
        [SerializeField] private Bounds stageBounds = new Bounds(Vector3.zero, Vector3.one * 50f);

        [Header("Camera Effects")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float defaultShakeIntensity = 1f;
        [SerializeField] private float defaultShakeDuration = 0.2f;

        [Header("Advanced Settings")]
        [SerializeField] private float deadZone = 2f; // プレイヤーがこの範囲内にいる時はカメラを動かさない
        [SerializeField] private bool adaptToPlayerSpeed = true; // プレイヤーの速度に応じてカメラを先読み
        [SerializeField] private float anticipationFactor = 0.5f;

        [SerializeField, ReadOnly] private const string LOGTAG = "camera";
        // 内部変数
        [SerializeField, ReadOnly] private List<Transform> players = new List<Transform>();
        private Dictionary<Transform, PlayerCameraData> playerDataMap = new Dictionary<Transform, PlayerCameraData>();
        private CinemachineBasicMultiChannelPerlin noiseComponent;
        private float shakeTimer;
        private Vector3 lastTargetPosition;
        private float lastUpdateTime;

        // プロパティ
        public List<Transform> Players => players.ToList(); // 読み取り専用コピーを返す
        public bool IsShaking => shakeTimer > 0f;
        public int PlayerCount => players.Count;

        // イベント
        public System.Action<Transform> OnPlayerAdded;
        public System.Action<Transform> OnPlayerRemoved;
        public System.Action<float> OnZoomChanged;

        protected override void Init()
        {
            base.Init();
            InitializeCamera();
            RegisterPlayers();
            StartShake(300f, 10f);
        }

        void Update()
        {
            UpdateCameraZoom();
            UpdateCameraShake();
            ValidatePlayers();
        }

        /// <summary>
        /// カメラの初期化
        /// </summary>
        private void InitializeCamera()
        {
            if (vcam == null)
            {
                CustomLogger.Error("CameraManager: VirtualCameraが設定されていません", LOGTAG);
                return;
            }

            // ノイズコンポーネントを取得または追加
            noiseComponent = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noiseComponent == null && enableCameraShake)
            {
                noiseComponent = vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }

            // TargetGroupの設定
            if (targetGroup != null)
            {
                vcam.Follow = targetGroup.transform;
                vcam.LookAt = targetGroup.transform;

                // TargetGroupの設定を最適化
                targetGroup.m_PositionMode = CinemachineTargetGroup.PositionMode.GroupCenter;
                targetGroup.m_RotationMode = CinemachineTargetGroup.RotationMode.Manual;
                targetGroup.m_UpdateMethod = CinemachineTargetGroup.UpdateMethod.LateUpdate;
            }

            lastUpdateTime = Time.time;
        }

        /// <summary>
        /// BattleJudgeからプレイヤーを登録
        /// </summary>
        private void RegisterPlayers()
        {
            if (BattleJudge.I?.Players == null) return;

            foreach (var playerInfo in BattleJudge.I.Players)
            {
                if (playerInfo?.playerObject != null)
                {
                    AddPlayer(playerInfo.playerObject.transform);
                }
            }
        }

        /// <summary>
        /// プレイヤー間の距離に基づいてカメラのズームを更新
        /// </summary>
        private void UpdateCameraZoom()
        {
            var activePlayers = GetActivePlayers();
            if (activePlayers.Count < 2) return;

            float maxDistanceBetweenPlayers = CalculateMaxDistance(activePlayers);

            // 先読み機能：プレイヤーの移動速度を考慮
            if (adaptToPlayerSpeed)
            {
                float anticipatedDistance = CalculateAnticipatedDistance(activePlayers, maxDistanceBetweenPlayers);
                maxDistanceBetweenPlayers = Mathf.Max(maxDistanceBetweenPlayers, anticipatedDistance);
            }

            // マージンを追加
            maxDistanceBetweenPlayers += zoomMargin;

            // 距離に応じてFOVを調整
            float t = Mathf.InverseLerp(minDistance, maxDistance, maxDistanceBetweenPlayers);
            float targetFOV = Mathf.Lerp(minFOV, maxFOV, t);

            float currentFOV = vcam.m_Lens.FieldOfView;
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * zoomSpeed);

            vcam.m_Lens.FieldOfView = newFOV;

            // ズーム変更イベント
            if (Mathf.Abs(newFOV - currentFOV) > 0.1f)
            {
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
                    if (!enableYAxisFollow)
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
                        Vector3 anticipatedPos = player.position + velocity * anticipationFactor;

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
            if (!enableCameraShake || noiseComponent == null) return;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;

                // シェイクの強度を時間とともに減衰
                float normalizedTime = Mathf.Clamp01(shakeTimer / defaultShakeDuration);
                noiseComponent.m_AmplitudeGain = defaultShakeIntensity * normalizedTime;
            }
            else
            {
                noiseComponent.m_AmplitudeGain = 0f;
            }
        }

        /// <summary>
        /// 無効になったプレイヤーを検証・削除
        /// </summary>
        private void ValidatePlayers()
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i] == null || !players[i].gameObject.activeInHierarchy)
                {
                    RemovePlayerAtIndex(i);
                }
            }
        }

        #region パブリックメソッド

        /// <summary>
        /// プレイヤーを1人追加し、TargetGroupにも登録
        /// </summary>
        public void AddPlayer(Transform player, float weight = 1f, float radius = 1f)
        {
            if (player == null || players.Contains(player)) return;

            players.Add(player);
            playerDataMap[player] = new PlayerCameraData { lastPosition = player.position };

            if (targetGroup != null)
            {
                targetGroup.AddMember(player, weight, radius);
            }

            OnPlayerAdded?.Invoke(player);
            CustomLogger.Info($"CameraManager: プレイヤー {player.name} を追加しました" + player, LOGTAG);
        }

        /// <summary>
        /// プレイヤーを削除
        /// </summary>
        public void RemovePlayer(Transform player)
        {
            int index = players.IndexOf(player);
            if (index >= 0)
            {
                RemovePlayerAtIndex(index);
            }
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

        /// <summary>
        /// プレイヤーを一括設定してTargetGroupに反映
        /// </summary>
        public void SetPlayers(List<Transform> playerTransforms, float weight = 1f, float radius = 1f)
        {
            ClearTargets();

            foreach (var player in playerTransforms)
            {
                if (player != null)
                {
                    AddPlayer(player, weight, radius);
                }
            }
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
        /// カメラシェイクを開始
        /// </summary>
        public void StartShake(float intensity = -1f, float duration = -1f)
        {
            if (!enableCameraShake) return;

            float shakeIntensity = intensity > 0f ? intensity : defaultShakeIntensity;
            float shakeDuration = duration > 0f ? duration : defaultShakeDuration;

            // より強いシェイクの場合は上書き
            if (shakeTimer <= 0f || intensity > defaultShakeIntensity)
            {
                shakeTimer = shakeDuration;
                if (noiseComponent != null)
                {
                    noiseComponent.m_AmplitudeGain = shakeIntensity;
                }
            }
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
            this.minFOV = Mathf.Max(1f, minFOV);
            this.maxFOV = Mathf.Max(this.minFOV, maxFOV);
            this.minDistance = Mathf.Max(0.1f, minDist);
            this.maxDistance = Mathf.Max(this.minDistance, maxDist);
        }

        /// <summary>
        /// ステージ境界を設定
        /// </summary>
        public void SetStageBounds(Bounds bounds)
        {
            stageBounds = bounds;
        }

        /// <summary>
        /// カメラのフォロー機能を一時的に無効化/有効化
        /// </summary>
        public void SetFollowEnabled(bool enabled)
        {
            if (vcam != null)
            {
                vcam.Follow = enabled && targetGroup != null ? targetGroup.transform : null;
                vcam.LookAt = enabled && targetGroup != null ? targetGroup.transform : null;
            }
        }

        /// <summary>
        /// 特定のプレイヤーのウェイトを変更
        /// </summary>
        public void SetPlayerWeight(Transform player, float weight)
        {
            if (targetGroup == null || !players.Contains(player)) return;

            for (int i = 0; i < targetGroup.m_Targets.Length; i++)
            {
                if (targetGroup.m_Targets[i].target == player)
                {
                    targetGroup.m_Targets[i].weight = weight;
                    break;
                }
            }
        }

        #endregion

        #region デバッグ用

        private void OnDrawGizmosSelected()
        {
            // ステージ境界を描画
            if (constrainToStage)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(stageBounds.center, stageBounds.size);
            }

            // プレイヤー位置とデッドゾーンを描画
            Gizmos.color = Color.red;
            foreach (var player in players)
            {
                if (player != null)
                {
                    Gizmos.DrawWireSphere(player.position, 0.5f);

                    // デッドゾーンを描画
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(player.position, deadZone);
                    Gizmos.color = Color.red;
                }
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