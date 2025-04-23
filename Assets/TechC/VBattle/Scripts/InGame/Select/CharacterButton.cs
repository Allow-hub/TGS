using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class CharacterButton : MonoBehaviour
    {
        [SerializeField] private int characterIndex;  // このボタンが操作するキャラのインデックス
        [SerializeField] private Button button;       // このボタンのUIコンポーネント
        [SerializeField] private CharacterSelectionManager manager;  // CharacterSelectionManagerへの参照

        void Start()
        {
            // ボタンがクリックされた時に呼ばれる
            button.onClick.AddListener(OnButtonClicked);
        }

        // ボタンがクリックされたときに呼ばれる
        void OnButtonClicked()
        {
            manager.SelectCharacter(characterIndex);  // キャラを選択
        }
    }

}
