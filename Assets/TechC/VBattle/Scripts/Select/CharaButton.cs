using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TechC.Select
{
    /// <summary>
    /// キャラピックのイベントを送受信
    /// </summary>
    public class CharaButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        [SerializeField] private Image p1DisplayImage;
        [SerializeField] private Image p2DisplayImage;

        [SerializeField] private Sprite p1CharaSprite;       // このボタンで選べるキャラのサムネ
        [SerializeField] private Sprite p2CharaSprite;       // このボタンで選べるキャラのサムネ
        [SerializeField] private GameObject pickCharaPrefab;// このボタンで選べるキャラ

        private void OnValidate()
        {
#if UNITY_EDITOR
            // 名前で自動割り当て
            if (p1DisplayImage == null)
            {
                var obj = GameObject.Find("p1DisplayImage").GetComponent<Image>();
                if (obj != null) p1DisplayImage = obj;
            }

            if (p2DisplayImage == null)
            {
                var obj = GameObject.Find("p2DisplayImage").GetComponent<Image>();
                if (obj != null) p2DisplayImage = obj;
            }
#endif
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);
            int id = SelectUIManagerFix.I.SetCharacterPick(device, pickCharaPrefab);
            ChangePickThumbnail(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var (device, deviceName) = ResolveDevice(eventData);

            if (device != null)
            {
                int id = SelectUIManagerFix.I.SetCharacterPick(device, pickCharaPrefab);
                DicidePick(id);

                Debug.Log($"クリック - デバイス: {deviceName}");
            }
            else
            {
                Debug.Log("旧InputSystem経由のクリック");
            }
        }

        /// <summary>
        /// PointerEventData から InputDevice と名前を取り出す
        /// </summary>
        private (InputDevice, string) ResolveDevice(PointerEventData eventData)
        {
            if (eventData is UnityEngine.InputSystem.UI.ExtendedPointerEventData extended)
            {
                var device = extended.device;
                if (device is Mouse)
                {
                    // マウスはKeyboard扱いにする
                    return (Keyboard.current, "Keyboard");
                }
                else if (device != null)
                {
                    return (device, device.displayName);
                }
                else
                {
                    return (null, "不明");
                }
            }
            return (null, "旧InputSystem");
        }

        /// <summary>
        /// カーソルを合わせた時サムネイルを変更
        /// </summary>
        /// <param name="id"></param>
        private void ChangePickThumbnail(int id)
        {
            if (id == 0) return; // 無効なID

            if (id == 1) // 1P
            {
                // p1DisplayImage に選んだキャラの画像を反映
                p1DisplayImage.sprite = p1CharaSprite;
            }
            else // 2P
            {
                p2DisplayImage.sprite = p2CharaSprite;
            }
        }

        private void DicidePick(int id)
        {
            if (id == 0) return; // 無効なID

            if (id == 1) // 1P
            {
            }
            else // 2P
            {
            }
        }
    }
}