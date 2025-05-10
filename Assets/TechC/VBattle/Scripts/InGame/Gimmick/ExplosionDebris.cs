using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 壁にぶつかった時に破片を飛び散らかす処理
    /// </summary>
    public class ExplosionDebris : MonoBehaviour
    {
        [System.Serializable]
        public struct TransformSnapshot
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public TransformSnapshot(Transform t)
            {
                position = t.localPosition;
                rotation = t.localRotation;
                scale = t.localScale;
            }

            public void ApplyTo(Transform t)
            {
                t.localPosition = position;
                t.localRotation = rotation;
                t.localScale = scale;
            }
        }

        [SerializeField] private GameObject parentObj;

        [SerializeField, ReadOnly] private List<GameObject> childObj = new List<GameObject>();
        [SerializeField, ReadOnly] private List<Transform> childTrans = new List<Transform>();
        [SerializeField, ReadOnly] private List<TransformSnapshot> originalTransforms = new List<TransformSnapshot>();

        [SerializeField, Range(0f, 200f)] private float explosionForceMin = 100f;
        [SerializeField, Range(0f, 200f)] private float explosionForceMax = 500f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float upwardsModifier = 0.5f;

        private Transform initPos;

        private void OnValidate()
        {
            if (parentObj == null) return;

            childObj.Clear();
            childTrans.Clear();
            originalTransforms.Clear();

            int childCount = parentObj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parentObj.transform.GetChild(i);
                childObj.Add(child.gameObject);
                childTrans.Add(child);
                originalTransforms.Add(new TransformSnapshot(child));
            }
        }

        private void OnEnable()
        {
            Vector3 explosionPosition = transform.position;

            for (int i = 0; i < childObj.Count; i++)
            {
                GameObject obj = childObj[i];
                Transform t = childTrans[i];
                TransformSnapshot snapshot = originalTransforms[i];

                // Transformを初期状態に戻す
                snapshot.ApplyTo(t);

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb == null) rb = obj.AddComponent<Rigidbody>();

                // 物理状態をリセット
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // ランダムな回転を適用
                t.rotation = Random.rotation;

                // 爆発力を加える
                float randomForce = Random.Range(explosionForceMin, explosionForceMax);
                rb.AddExplosionForce(randomForce, explosionPosition, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 半透明のオレンジ
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }

    }
}
