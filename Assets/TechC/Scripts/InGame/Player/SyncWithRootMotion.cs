using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    public class SyncWithRootMotion : MonoBehaviour
    {
        [SerializeField] private Transform model;      // アニメーションモデル（子オブジェクト）
        [SerializeField] private Rigidbody parentRb;   // 親のRigidbody
        private Vector3 previousPosition;

        private void Start()
        {
            previousPosition = model.position;
        }

        private void LateUpdate()
        {
            model.localPosition = Vector3.zero;
        }
    }
}
