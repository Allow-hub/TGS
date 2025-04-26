using UnityEngine;
using TMPro;

namespace TechC
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(TextMeshPro))]
    public class TextColliderAdjuster : MonoBehaviour
    {
        private TextMeshPro textMeshPro;
        private BoxCollider boxCollider;

        void Awake()
        {
            textMeshPro = GetComponent<TextMeshPro>();
            boxCollider = GetComponent<BoxCollider>();
        }

        void Start()
        {
            UpdateColliderSize();
        }

        private void UpdateColliderSize()
        {
            textMeshPro.ForceMeshUpdate(); /* テキストのサイズを最新の状態にする */
            Bounds bounds = textMeshPro.textBounds;
            boxCollider.size = bounds.size; /* コライダーの大きさをテキストの大きさに合わせる */
            boxCollider.center = bounds.center; /* コライダーの中心位置を、テキストの中心に合わせる */
        }
    }
}
