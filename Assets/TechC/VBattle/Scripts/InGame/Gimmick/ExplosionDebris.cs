using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 壁にぶつかった時破片を飛び散らかす処理
    /// </summary>
    public class ExplosionDebris : MonoBehaviour
    {
        [SerializeField] private GameObject parentObj;
        [SerializeField, ReadOnly] private List<GameObject> childObj;
        [SerializeField, Range(0f, 200f)] private float explosionForceMin = 100f;
        [SerializeField, Range(0f, 200f)] private float explosionForceMax = 500f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float upwardsModifier = 0.5f;
        private void OnValidate()
        {
            if (parentObj == null) return;
            childObj.Clear();
            var childCount = parentObj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                childObj.Add(parentObj.transform.GetChild(i).gameObject);
            }
        }
        private void OnEnable()
        {
            Vector3 explosionPosition = transform.position;

            foreach (var obj in childObj)
            {
                var rb = obj.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = obj.AddComponent<Rigidbody>();
                }
                obj.transform.rotation = Random.rotation;

                float randomForce = Random.Range(explosionForceMin, explosionForceMax);
                rb.AddExplosionForce(randomForce, explosionPosition, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
        }

    }
}
