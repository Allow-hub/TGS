using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TechC.Select
{
    public class CharaButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("ホバー開始");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("ホバー終了");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData is UnityEngine.InputSystem.UI.ExtendedPointerEventData extended)
            {
                var device = extended.device;
                if (device != null)
                {
                    string deviceName;

                    if (device is Mouse)
                    {
                        // マウス操作はキーボード扱いにしたい場合
                        deviceName = "Keyboard";
                    }
                    else
                    {
                        // それ以外はデバイスの実名を使う
                        deviceName = device.displayName;
                    }

                    Debug.Log($"選択デバイス: {deviceName}");
                }
                else
                {
                    Debug.Log("デバイス不明");
                }
            }
            else
            {
                Debug.Log("旧InputSystem経由のクリック");
            }
        }
    }
}