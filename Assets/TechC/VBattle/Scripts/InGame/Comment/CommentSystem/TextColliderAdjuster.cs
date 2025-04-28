using UnityEngine;
using TMPro;

namespace TechC
{
    /// <summary>
    /// テキストの長さに応じてboxColliderを生成する
    /// </summary>
    
    /* BoxColliderとTextMeshProUGUIを必須にする */
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(TextMeshProUGUI))]
    
    public class TextColliderAdjuster : MonoBehaviour
    {
        private TextMeshProUGUI textMeshProUGUI;
        private BoxCollider boxCollider;

        void Awake()
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            boxCollider = GetComponent<BoxCollider>();
        }

        void Start()
        {
            UpdateColliderSize();
        }

        private void UpdateColliderSize()
        {
            textMeshProUGUI.ForceMeshUpdate(); /* テキストのサイズを最新の状態にする */
            Bounds bounds = textMeshProUGUI.textBounds;
            boxCollider.size = bounds.size; /* コライダーの大きさをテキストの大きさに合わせる */
            boxCollider.center = bounds.center; /* コライダーの中心位置を、テキストの中心に合わせる */
        }
    }
}