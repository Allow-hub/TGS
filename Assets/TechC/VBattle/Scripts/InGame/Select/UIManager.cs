using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject confirmButton;  // 決定ボタン
        [SerializeField] private CharacterSelectionManager manager;  // キャラクター選択管理

        // 決定ボタンが押された時に呼ばれる
        public void ConfirmSelection()
        {
            Debug.Log("キャラクター選択完了!");
            // ここでゲームスタートシーンに遷移するなどの処理を追加
        }

        // キャラが選択されていない状態でも決定ボタンが表示されないようにする
        public void ShowConfirmButton(bool show)
        {
            confirmButton.SetActive(show);
        }
    }

}
