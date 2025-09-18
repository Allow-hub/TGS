using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TechC.Select
{
    public class CharaButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // ホバー開始時
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("ホバー開始");
        }

        // ホバー終了時
        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("ホバー終了");
        }
    }
}
