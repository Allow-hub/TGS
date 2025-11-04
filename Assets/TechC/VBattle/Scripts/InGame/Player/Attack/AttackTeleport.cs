using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player.Attack
{
    [System.Serializable]
    public class AttackTeleport : IAttackBehaviour
    {
        [SerializeField] private Vector3 teleportPos;
        [SerializeField] private Vector2 xRange = new Vector2(-7.5f, 7.5f);
        [SerializeField] private float delay = 3f;
        [SerializeField] private bool random;

        private float elapsed;
        private GameObject character;
        private bool teleported = false;

        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
            elapsed = 0f;
            character = null;
            teleported = false;
        }

        public void OnUpdate(float deltaTime)
        {
            if (teleported) return;
            elapsed += deltaTime;
            if (elapsed < delay) return;

            Vector3 target;
            if (random)
            {
                float x = Random.Range(xRange.x, xRange.y);
                // y,z は設定された teleportPos を使う
                target = new Vector3(x, teleportPos.y, teleportPos.z);
            }
            else
            {
                target = teleportPos;
            }

            character.transform.position = target;
            teleported = true;
        }

        public void Activate(GameObject character)
        {
            this.character = character;
        }
    }
}
