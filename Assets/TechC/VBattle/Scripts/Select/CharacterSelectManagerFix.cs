using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC.Select
{
    public class CharacterSelectManagerFix : Singleton<CharacterSelectManagerFix>
    {
        

        private const float initializeDelay = 0.1f;
        protected override bool UseDontDestroyOnLoad => false;


        protected override void Init()
        {
            base.Init();

            DelayUtility.StartDelayedAction(this, initializeDelay, () =>
            {
                // 初期化処理
                // プレイヤー情報を一旦コピー
                var playerInfos = new List<(GameObject prefab, int playerId, InputDevice inputDevice)>(GameManager.I.GetPlayerInfo());

                foreach (var info in playerInfos)
                {
                    GameManager.I.RemovePlayerById(info.playerId);
                }

                if (SelectUIManager.I == null)
                {
                    Debug.Log("SelectUIManagerの初期化が済んでいません");
                    return;
                }
            });
        }
    }
}
