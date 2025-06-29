using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TechC
{
    /// <summary>
    /// 複数枚のプレイヤーパネル用
    /// </summary>
    [System.Serializable]
    public class PlayerSelectUI
    {
        public TMP_Dropdown inputDeviceDropdown;
        public GameObject ameTextImage;//ameのテキストイメージ
        public GameObject teramiTextImage;//teramiのテキストイメージ
        public GameObject ameObj;//ameのモデル
        public GameObject teramiObj;//teramiのモデル
        public Button ameButton;
        public Button teramiButton;
        public Button pickButton;
        public int currentCharacterIndex = 0;
    }
}
