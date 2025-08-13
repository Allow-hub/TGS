using System;
using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 攻撃のオブジェクトにライフタイムを設定し、自分自身プールに返す
    /// </summary>
    [Serializable]
    public class AttackLifeTime : IAttackBehaviour
    {
        [SerializeField] private float lifeTime;
        private float currentLifeTime;
        private GameObject ownerObj;
        public void Initialize(GameObject owner)
        {
            currentLifeTime = lifeTime;
            ownerObj = owner;
        }

        public void OnRelease()
        {
            currentLifeTime = lifeTime;
        }

        public void OnUpdate(float deltaTime)
        {
            if (currentLifeTime <= 0)
            {
                currentLifeTime = 0;
                CharaEffectFactory.I.ReturnEffectObj(ownerObj);
                return;
            }
            currentLifeTime -= deltaTime;
        }
        /// <summary>
        /// Chain攻撃時にLifeTimeを最大値にリセット
        /// </summary>
        public void ResetLifeTime() => currentLifeTime = 0;
    }
}
