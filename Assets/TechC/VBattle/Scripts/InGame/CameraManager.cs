using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace TechC
{
    /// <summary>
    /// インゲームのカメラを管理するクラス
    /// </summary>
    public class CameraManager : Singleton<CameraManager>
    {
        protected override bool UseDontDestroyOnLoad => false;

        [Header("Cinemachine References")]
        public CinemachineVirtualCamera vcam;
        public CinemachineTargetGroup targetGroup;

        [Header("Zoom Settings")]
        public float minDistance = 5f;
        public float maxDistance = 20f;
        public float minFOV = 30f;
        public float maxFOV = 60f;
        public float zoomSpeed = 5f;

        [SerializeField, ReadOnly] private List<Transform> players = new List<Transform>();

        protected override void Init()
        {
            base.Init();

            // BattleJudgeからプレイヤー取得・追加
            foreach (var playerInfo in BattleJudge.I.Players)
            {
                AddPlayer(playerInfo.playerObject.transform);
            }
        }

        void Update()
        {
            if (players.Count < 2) return;

            // プレイヤー間の最大距離を取得
            float maxDistanceBetweenPlayers = 0f;

            for (int i = 0; i < players.Count; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    float dist = Vector3.Distance(players[i].position, players[j].position);
                    if (dist > maxDistanceBetweenPlayers)
                        maxDistanceBetweenPlayers = dist;
                }
            }

            // 距離に応じてFOVを調整
            float t = Mathf.InverseLerp(minDistance, maxDistance, maxDistanceBetweenPlayers);
            float targetFOV = Mathf.Lerp(minFOV, maxFOV, t);
            vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }

        /// <summary>
        /// プレイヤーを1人追加し、TargetGroupにも登録
        /// </summary>
        public void AddPlayer(Transform player)
        {
            if (players.Contains(player)) return;

            players.Add(player);
            targetGroup.AddMember(player, 1f, 1f); // weight=1, radius=1
        }

        /// <summary>
        /// プレイヤーを一括設定してTargetGroupに反映
        /// </summary>
        public void SetPlayers(List<Transform> playerTransforms)
        {
            ClearTargets();

            foreach (var player in playerTransforms)
            {
                AddPlayer(player);
            }
        }

        /// <summary>
        /// 全プレイヤーとターゲットグループを初期化
        /// </summary>
        public void ClearTargets()
        {
            players.Clear();
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[0]; // 全クリア
        }
    }
}
