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
        [SerializeField] private GameObject[] characterPrefabs; // 実体として選ばれるキャラクター（任意）

        [SerializeField] private Button dicisionButton;//ゲームを開始するボタン
        [SerializeField] private PlayerSelectUI[] playerUIs; // [0]=P1, [1]=P2 のUI情報を保持

        [SerializeField] private Sprite[] characterIcons; // キャラ選択時に表示するアイコン画像の一覧

        // キャラ変更時に通知するイベント: (playerIndex, characterIndex)
        public System.Action<int, int> OnCharacterChanged;

        // キャラ決定時に通知するイベント: (playerIndex)
        public System.Action<int> OnCharacterPicked;
        public System.Action OnDicidePicked;
        /// <summary>
        /// 各プレイヤーの現在の選択状態（インデックスとオブジェクト）を保持する構造体。
        /// </summary>
        public struct CharacterPick
        {
            public int characterIndex;
            public GameObject characterObject;
        }

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
                playerUIs[i].leftButton.onClick.AddListener(() => ChangeCharacter(index, -1)); // 左ボタン
                playerUIs[i].rightButton.onClick.AddListener(() => ChangeCharacter(index, 1)); // 右ボタン
                playerUIs[i].pickButton.onClick.AddListener(() => OnCharacterPicked?.Invoke(index)); // 決定ボタン
                currentPicks[i].characterIndex = 0;
                currentPicks[i].characterObject = characterPrefabs.Length > 0 ? characterPrefabs[0] : null;


            }
            dicisionButton.onClick.AddListener(() => Dicide());
        }

        /// <summary>
        /// 指定プレイヤーの選択キャラクターを変更する。
        /// </summary>
        /// <param name="playerIndex">プレイヤー番号（0 or 1）</param>
        /// <param name="direction">変更方向（-1 = 左, 1 = 右）</param>
        private void ChangeCharacter(int playerIndex, int direction)
        {
            var ui = playerUIs[playerIndex];

            // インデックスを循環させてキャラクター選択
            // ui.currentCharacterIndex = (ui.currentCharacterIndex + direction + characterIcons.Length) % characterIcons.Length;
            if (direction == -1)
            {
                ui.teramiTextImage.SetActive(false);
                ui.ameTextImage.SetActive(true);
                ui.teramiObj.SetActive(false);
                ui.ameObj.SetActive(true);

            }
            else
            {
                ui.teramiTextImage.SetActive(true);
                ui.ameTextImage.SetActive(false);
                ui.teramiObj.SetActive(true);
                ui.ameObj.SetActive(false);

            }
            // キャラクター画像を更新

            // キャラ変更イベントを発火
            // OnCharacterChanged?.Invoke(playerIndex, ui.currentCharacterIndex);
        }

        private void Dicide()
        {
            OnDicidePicked.Invoke();
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
