using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// プレイヤーのアイコンを制御するクラス（1Pまたは2Pごとに設置）
    /// </summary>
    public class PlayerIconController : MonoBehaviour
    {
        [Header("キャラ別アイコン")]
        [SerializeField] private Image ameIconImage;
        [SerializeField] private Image teramiIconImage;

        [Header("対応するプレイヤー番号（0: 1P, 1: 2P）")]
        [SerializeField] private int playerIndex = 0;

        private bool iconsUpdated = false;

        private void Update()
        {
            // BattleJudgeが初期化され、まだアイコン更新してなければ実行
            if (!iconsUpdated && BattleJudge.I != null && BattleJudge.I.Players != null)
            {
                // 安全性確認
                if (BattleJudge.I.Players.Count > playerIndex)
                {
                    UpdateIconByPlayer();
                    iconsUpdated = true;
                }
            }
        }

        /// <summary>
        /// 該当プレイヤーのPrefab名に基づいてアイコンを表示
        /// </summary>
        private void UpdateIconByPlayer()
        {
            // 全アイコン非表示
            ameIconImage.gameObject.SetActive(false);
            teramiIconImage.gameObject.SetActive(false);

            var player = BattleJudge.I.Players[playerIndex];
            string prefabName = player.playerPrefab.name;

            if (prefabName.Contains("Ame"))
            {
                ameIconImage.gameObject.SetActive(true);
            }
            else if (prefabName.Contains("Terami"))
            {
                teramiIconImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"[PlayerIconController] Prefab名にAme, Teramiが含まれていません: {prefabName}");
            }
        }
    }
}
