using UnityEngine;
using TMPro;

namespace TechC
{
    /// <summary>
    /// テキストの長さに応じてBoxColliderを生成する（負のサイズは除外）
    /// </summary>
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
            textMeshProUGUI.ForceMeshUpdate(); // テキストのサイズを更新
            Bounds bounds = textMeshProUGUI.textBounds;

            Vector3 size = bounds.size;
            Vector3 center = bounds.center;

            // 各軸のサイズを絶対値化（負のサイズを防止）
            size = new Vector3(
                Mathf.Abs(size.x),
                Mathf.Abs(size.y),
                Mathf.Abs(size.z)
            );

            boxCollider.size = size;
            boxCollider.center = center;
        }
    }
}
