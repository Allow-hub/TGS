using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// キャラクターセレクト画面におけるUI管理クラス。
    /// 各プレイヤーのUIを管理し、ボタン操作によってキャラ変更や決定処理を行う。
    /// </summary>
    public class SelectUIManager : Singleton<SelectUIManager>
    {
        [SerializeField] private Button npcButton;
        [SerializeField] private GameObject[] characterPrefabs; // 実体として選ばれるキャラクター
        [SerializeField] private GameObject[] characterNpcPrefabs; // 実体として選ばれるキャラクター

        [SerializeField] private Button dicisionButton;//ゲームを開始するボタン
        public PlayerSelectUI[] playerUIs; // [0]=P1, [1]=P2 のUI情報を保持

        public System.Action<int> OnCharacterPicked;
        public System.Action OnDicidePicked;

        /// <summary>
        /// 各プレイヤーの現在の選択状態（インデックスとオブジェクト）を保持する構造体。
        /// </summary>
        public struct CharacterPick
        {
            public int playerId;
            public GameObject characterObject;
            public InputDevice inputDevice;
        }

        public CharacterPick[] CurrentPicks => currentPicks;
        private CharacterPick[] currentPicks = new CharacterPick[2];

        public bool IsNpc => isNpc;
        private bool isNpc;
        protected override bool UseDontDestroyOnLoad => false;

        /// <summary>
        /// 初期化時に各UIのボタンに処理を登録する。
        /// </summary>
        protected override void Init()
        {
            base.Init();
            List<string> deviceNames = new List<string>();

            deviceNames.Add("Keyboard");

            // 接続されているGamepadを表示
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                deviceNames.Add($"Gamepad {i + 1} ({Gamepad.all[i].displayName})");
            }
            for (int i = 0; i < playerUIs.Length; i++)
            {
                int index = i; // キャプチャ用ローカル変数
                playerUIs[i].ameButton.onClick.AddListener(() => ChangeCharacter(index, 0));
                playerUIs[i].teramiButton.onClick.AddListener(() => ChangeCharacter(index, 1));
                playerUIs[i].pickButton.onClick.AddListener(() => OnCharacterPicked?.Invoke(index)); // ピックボタン
                var dropdown = playerUIs[i].inputDeviceDropdown;
                dropdown.ClearOptions();
                dropdown.AddOptions(deviceNames);
                dropdown.value = index;
                dropdown.onValueChanged.AddListener((value) => OnInputDeviceChanged(index, value));
            }

            //どちらもあめで初期化
            ChangeCharacter(0, 0);
            ChangeCharacter(1, 0);
            OnInputDeviceChanged(0, 0);
            OnInputDeviceChanged(1, 1);
            npcButton.onClick.AddListener(() => SetNpc());
            dicisionButton.onClick.AddListener(() => Dicide());
        }

        /// <summary>
        /// 指定プレイヤーの選択キャラクターを変更する。
        /// </summary>
        /// <param name="playerIndex">プレイヤー番号（0 or 1）</param>
        /// <param name="characterIndex">キャラインデックス（0 = あめ,1 = てらみ</param>
        private void ChangeCharacter(int playerIndex, int characterIndex)
        {
            var ui = playerUIs[playerIndex];

            // UIの切り替え（片方だけアクティブにする）
            bool isAme = characterIndex == 0;

            ui.ameObj.SetActive(isAme);
            ui.ameTextImage.SetActive(isAme);
            ui.teramiObj.SetActive(!isAme);
            ui.teramiTextImage.SetActive(!isAme);

            // 選択情報を保存
            ui.currentCharacterIndex = characterIndex;

            // NPCが選択されているかでプレハブを切り替え
            GameObject selectedPrefab;
            if (isNpc)
            {
                if (characterNpcPrefabs != null && characterNpcPrefabs.Length > characterIndex)
                {
                    selectedPrefab = characterNpcPrefabs[characterIndex];
                }
                else
                {
                    Debug.LogWarning("characterNpcPrefabsが設定されていないかインデックスが範囲外です");
                    selectedPrefab = null;
                }
            }
            else
            {
                if (characterPrefabs != null && characterPrefabs.Length > characterIndex)
                {
                    selectedPrefab = characterPrefabs[characterIndex];
                }
                else
                {
                    Debug.LogWarning("characterPrefabsが設定されていないかインデックスが範囲外です");
                    selectedPrefab = null;
                }
            }

            currentPicks[playerIndex] = new CharacterPick
            {
                playerId = playerIndex,
                characterObject = selectedPrefab
            };
        }

        private void OnInputDeviceChanged(int playerIndex, int dropdownValue)
        {
            Debug.Log($"Player {playerIndex} の入力デバイスを変更: {dropdownValue}");


            InputDevice deviceToAssign = null;

            switch (dropdownValue)
            {
                case 0:
                    // キーボード
                    deviceToAssign = Keyboard.current;
                    break;
                case 1:
                    // ゲームパッド1
                    if (Gamepad.all.Count > 0)
                        deviceToAssign = Gamepad.all[0];
                    break;
                case 2:
                    // ゲームパッド2
                    if (Gamepad.all.Count > 1)
                        deviceToAssign = Gamepad.all[1];
                    break;
                default:
                    Debug.LogWarning("未知のデバイス選択");
                    break;
            }

            if (deviceToAssign != null)
            {
                Debug.Log($"Player {playerIndex} に {deviceToAssign.displayName} を割り当て");

                // すでにキャラ情報があるなら、更新して保存し直す
                var current = currentPicks[playerIndex];
                current.inputDevice = deviceToAssign;
                currentPicks[playerIndex] = current;

                // 必要なら制御スキームも切り替える（省略中）
            }
        }
        private void Dicide()
        {
            OnDicidePicked?.Invoke();
        }

        private void SetNpc()
        {
            isNpc = !isNpc;
            currentPicks[1].characterObject = characterNpcPrefabs[0];
        }

        /// <summary>
        /// 指定プレイヤーの現在選択中のキャラクターインデックスを取得。
        /// </summary>
        /// <param name="playerIndex">プレイヤー番号（0 or 1）</param>
        /// <returns>現在のキャラクターインデックス</returns>
        public int GetSelectedCharacterIndex(int playerIndex)
        {
            return playerUIs[playerIndex].currentCharacterIndex;
        }
    }
}
