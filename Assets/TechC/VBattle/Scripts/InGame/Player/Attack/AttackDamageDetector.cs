using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player.Attack
{
    public class AttackDamageDetector : IAttackBehaviour
    {
        [SerializeField] private AttackData attackData;
        [SerializeField] private float colliderSize = 3f;
        private Collider col;
        private float currnetTime;
        public void Initialize(GameObject owner)
        {
            col = owner.GetComponent<Collider>();
        }

        public void OnRelease()
        {
            if (col == null) return;
            col.enabled = false;
        }

        public void OnUpdate(float deltaTime)
        {
            if (currnetTime >= attackData.hitTiming)
            {
                if (col == null) return;
                col.enabled = true;   
            }
            currnetTime += deltaTime;
        }
        public void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Hitting");
        }


        public void OnTriggerExit(Collider other) { }
    }
}
