using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class CharacterSelectionManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] characterPrefabs;   // キャラのプレハブを格納
        [SerializeField] private Transform displayArea;           // キャラを表示するエリア
        [SerializeField] private GameObject confirmButton;        // 決定ボタン
        [SerializeField] private UnityEngine.UI.Text characterNameText;  // 選択したキャラの名前を表示するUIテキスト

        private GameObject currentCharacter;    // 現在選択されているキャラクター

        // キャラが選択された時に呼ばれる
        public void SelectCharacter(int index)
        {
            // 前のキャラを削除
            if (currentCharacter != null)
            {
                Destroy(currentCharacter);
            }

            // 新しいキャラを表示
            currentCharacter = Instantiate(characterPrefabs[index], displayArea.position, Quaternion.identity);
            currentCharacter.transform.SetParent(displayArea);

            // キャラ名を表示
            characterNameText.text = "選択キャラ: " + characterPrefabs[index].name;

            // 決定ボタンをアクティブに
            confirmButton.SetActive(true);
        }
    }
}