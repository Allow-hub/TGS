using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class CharacterSelectManager : Singleton<CharacterSelectManager>
    {
        private const float initializeDelay = 0.1f;
        protected override bool UseDontDestroyOnLoad => false;

        private bool[] hasPicked = new bool[2]; // P1, P2 のピック済み管理

        protected override void Init()
        {
            base.Init();

            DelayUtility.StartDelayedAction(this, initializeDelay, () =>
            {
                // 初期化処理
                foreach (var info in GameManager.I.GetPlayerInfo())
                {
                    GameManager.I.RemovePlayerById(info.playerId);
                }

                if (SelectUIManager.I == null)
                {
                    Debug.Log("SelectUIManagerの初期化が済んでいません");
                    return;
                }

                // イベント購読
                SelectUIManager.I.OnCharacterPicked += OnCharacterPicked;
                SelectUIManager.I.OnDicidePicked += DicidePick;
            });
        }

        private void OnCharacterPicked(int playerIndex)
        {
            if (hasPicked[playerIndex])
            {
                Debug.Log($"Player {playerIndex} はすでにピック済みです");
                return;
            }

            hasPicked[playerIndex] = true;

            Debug.Log($"Player {playerIndex} がキャラ {SelectUIManager.I.CurrentPicks[playerIndex].characterObject.name} をピックしました");

            // UIのボタンを無効化（任意）
            SelectUIManager.I.playerUIs[playerIndex].ameButton.interactable = false;
            SelectUIManager.I.playerUIs[playerIndex].teramiButton.interactable = false;
        }

        private void DicidePick()
        {
            if (!hasPicked[0] || !hasPicked[1])
            {
                Debug.Log("まだ全プレイヤーがピックしていません");
                return;
            }
            foreach (var pick in SelectUIManager.I.CurrentPicks)
            {
                GameManager.I.RegisterPlayer(pick.characterObject, pick.playerId);
            }

            GameManager.I.ChangeBattleState();
        }
    }
}
