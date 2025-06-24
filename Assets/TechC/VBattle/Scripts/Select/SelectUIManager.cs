using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// キャラクターセレクト画面におけるUI管理クラス。
    /// 各プレイヤーのUIを管理し、ボタン操作によってキャラ変更や決定処理を行う。
    /// </summary>
    public class SelectUIManager : Singleton<SelectUIManager>
    {
        [SerializeField] private GameObject[] characterPrefabs; // 実体として選ばれるキャラクター

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
        }

        public CharacterPick[] CurrentPicks =>currentPicks;
        private CharacterPick[] currentPicks = new CharacterPick[2];
        protected override bool UseDontDestroyOnLoad => false;

        /// <summary>
        /// 初期化時に各UIのボタンに処理を登録する。
        /// </summary>
        protected override void Init()
        {
            base.Init();
            for (int i = 0; i < playerUIs.Length; i++)
            {
                int index = i; // キャプチャ用ローカル変数
                playerUIs[i].ameButton.onClick.AddListener(() => ChangeCharacter(index, 0));
                playerUIs[i].teramiButton.onClick.AddListener(() => ChangeCharacter(index, 1));
                playerUIs[i].pickButton.onClick.AddListener(() => OnCharacterPicked?.Invoke(index)); // ピックボタン
            }

            //どちらもあめで初期化
            ChangeCharacter(0, 0);
            ChangeCharacter(1, 0);

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

            currentPicks[playerIndex] = new CharacterPick
            {
                playerId = playerIndex,
                characterObject = characterPrefabs[characterIndex]
            };
        }

        private void Dicide()
        {
            OnDicidePicked?.Invoke();
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
