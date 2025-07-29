using UnityEngine;

namespace TechC.Player.Attack
{
    public class AttackDamageDetector : IAttackBehaviour
    {
        [SerializeField] private AttackData attackData;
        [SerializeField] private Collider col;
        private float currnetTime;
        private GameObject ownerObj;
        private GameObject character;
        public void Initialize(GameObject owner)
        {
            ownerObj = owner;
            col.enabled = false;
        }
        public void Activate(GameObject character)
        {
            if (col == null) return;
            col.enabled = false;
            this.character = character;
            currnetTime = 0f;
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
            AttackProcessor_Refacta.ProcessAttack(attackData, character.GetComponent<CharacterController>(), ownerObj);
        }
    }
}
