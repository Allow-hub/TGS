using UnityEngine;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// 複数枚のプレイヤーパネル用
    /// </summary>
    [System.Serializable]
    public class PlayerSelectUI
    {
        public GameObject characterTextImages;//キャラのテキストイメージをまとめたペアレント
        public Button leftButton;
        public Button rightButton;
        public Button pickButton;
        public int currentCharacterIndex = 0;
    }
}
