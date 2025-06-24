using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// GameManagerなどにデータを受け渡す
    /// セレクトシーンの管理クラス
    /// </summary>
    public class CharacterSelectManager : Singleton<CharacterSelectManager>
    {
        // [SerializeField] private GameObject amePrefab;
        // [SerializeField] private GameObject teramiPrefab;
        private const float initializeDelay = 0.1f;
        protected override bool UseDontDestroyOnLoad => false;

        protected override void Init()
        {
            base.Init();
            DelayUtility.StartDelayedAction(this, initializeDelay, () =>
            {
                foreach (var info in GameManager.I.GetPlayerInfo())
                {
                    GameManager.I.RemovePlayerById(info.playerId);
                }
                if (SelectUIManager.I == null)
                {
                    Debug.Log("SelectUIManagerの初期化が済んでいません");
                }
                SelectUIManager.I.OnCharacterPicked += OnCharacterPicked;
                SelectUIManager.I.OnDicidePicked += DicidePick;
            });
        }

        private void DicidePick()
        {
                // GameManager.I.RegisterPlayer(amePrefab, 0);
                // GameManager.I.RegisterPlayer(amePrefab, 1);
                // Debug.Log(GameManager.I.GetCharacterById(0));
                // Debug.Log(GameManager.I.GetCharacterById(1));

        }

        private void OnCharacterPicked(int playerIndex)
        {
            int selectedChar = SelectUIManager.I.GetSelectedCharacterIndex(playerIndex);
            Debug.Log($"Player {playerIndex + 1} picked character {selectedChar}");

            // ここで選択済みキャラに登録するなど
        }
    }
}