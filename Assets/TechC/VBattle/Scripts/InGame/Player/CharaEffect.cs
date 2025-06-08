using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラのエフェクトはすべてこれがついている必要がある
    /// </summary>
    public class CharaEffect : MonoBehaviour
    {
        [SerializeField] private AttackData attackData;
        /// 自分が所属するオブジェクトプール
        private ObjectPool objectPool;
        private AttackProcessor attackProcessor;
        private int ownerId;

        [SerializeField] private bool canHeal;
        [SerializeField] private bool canSelfReturn;
        private float healAmount = 50f;

        /// <summary>
        /// ファクトリー側で呼ぶ初期化メソッド
        /// </summary>
        /// <param name="objectPool"></param>
        public void Init(ObjectPool objectPool)
        {
            this.objectPool = objectPool;
        }

        /// <summary>
        /// 攻撃側のIDを設定（自キャラの攻撃が自分に当たらないように）
        /// </summary>
        /// <param name="id">Player.CharacterControllerのPlayerId</param>
        public void SetOwnerId(int id) => ownerId = id;
        public void SetAttackProcessor(AttackProcessor attackProcessor) => this.attackProcessor = attackProcessor;

        public void SetHealAmount(float value) => healAmount = value;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            var opponentController = other.gameObject.GetComponentInParent<Player.CharacterController>();
            var opponentId = opponentController.PlayerID;
            
            if (ownerId == opponentId)
            {
                if (!canHeal) return;
                opponentController.HealHp(healAmount);
            }
            else
            {
                // ドレイン系の攻撃が増える場合、拡張が必要
                attackProcessor.HandleAttack(attackData, other);
            }

            if (canSelfReturn)
            {
                CharaEffectFactory.I.ReturnEffectObj(gameObject);
            }
        }
    }
}
