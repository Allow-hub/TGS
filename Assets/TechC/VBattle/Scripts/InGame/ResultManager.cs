using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class ResultManager : Singleton<ResultManager>
    {
        [SerializeField] private GameObject resultCanvas;
        [SerializeField] private Button titleButton;
        [SerializeField] private TextMeshProUGUI p1Tex, p2Tex;

        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
  
            DelayUtility.StartDelayedAction(this, 0.1f, () =>
            {
                titleButton.onClick.AddListener(() =>
                {
                    GameManager.I.ChangeTitleState();
                });
                BattleJudge.I.OnBattleEnd.AddListener((winner) => ShowResult(winner));
            });
        }

        /// <summary>
        /// リザルトを表示
        /// </summary>
        /// <param name="winner"></param>
        private void ShowResult(BattleJudge.PlayerData winner)
        {
            GameManager.I.ChangeResultState();
            resultCanvas.SetActive(true);

            var resultTexts = new Dictionary<int, string[]>
            {
                { 1, new[] { "1pWin", "2pLose" } },
                { 2, new[] { "1pLose", "2pWin" } },
                { -1, new[] { "1pDraw", "2pDraw" } }
            };

            int winnerId = winner?.playerID ?? -1;

            if (resultTexts.TryGetValue(winnerId, out var texts))
            {
                p1Tex.text = texts[0];
                p2Tex.text = texts[1];
            }
            else
            {
                p1Tex.text = "???";
                p2Tex.text = "???";
                Debug.LogWarning("未知の勝者ID: " + winnerId);
            }
        }
    }
}
