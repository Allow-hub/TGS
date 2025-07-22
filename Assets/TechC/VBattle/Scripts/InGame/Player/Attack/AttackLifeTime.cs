using System;
using UnityEngine;

namespace TechC.Player.Attack
{
    [Serializable]
    public class AttackLifeTime : IAttackBehaviour
    {
        [SerializeField] private float lifeTime;
        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
        }

        public void OnUpdate(float deltaTime)
        {
        }
    }
}
