using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class ExplosionPopcone : MonoBehaviour
    {
        [Header("Explosion Settings")]
        [SerializeField] private float explosionForce = 500f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float upwardsModifier = 0.0f;
        [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
        [SerializeField] private Vector3 explosionOffset = Vector3.zero;
        [SerializeField] private bool includeInactiveChildren = false;

        // キャッシュ
        private Rigidbody[] childRigidbodies;

        private void Awake()
        {
            CacheChildRigidbodies();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // エディタでプロパティ変更時にキャッシュを更新
            CacheChildRigidbodies();
        }
#endif

        private void CacheChildRigidbodies()
        {
            childRigidbodies = GetComponentsInChildren<Rigidbody>(includeInactiveChildren);
        }

        private void OnEnable()
        {
            if (childRigidbodies == null || childRigidbodies.Length == 0)
                CacheChildRigidbodies();

            Vector3 explosionPos = transform.position + explosionOffset;

            foreach (var rb in childRigidbodies)
            {
                if (rb == null) continue;

                // 線形の爆風
                rb.AddExplosionForce(explosionForce, explosionPos, explosionRadius, upwardsModifier, forceMode);

                float torqueAmount = explosionForce * 0.02f; // 調整値
                rb.AddTorque(Random.insideUnitSphere * torqueAmount, ForceMode.Impulse);
            }
        }
    }
}
