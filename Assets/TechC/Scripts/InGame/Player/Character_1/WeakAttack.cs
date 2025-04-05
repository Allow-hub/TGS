using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public  class WeakAttack : MonoBehaviour, IAttackBase
    {
        private AttackData neutralAttackData;
        private AttackData leftAttackData;
        private AttackData rightAttackData;
        private AttackData downAttackData;
        private AttackData upAttackData;

        private Transform attackOrigin;
        private Animator animator;
        public void NeutralAttack()
        {
            ExecuteAttack(neutralAttackData);
        }

        public void LeftAttack()
        {
            ExecuteAttack(leftAttackData);
        }

        public void RightAttack()
        {
            ExecuteAttack(rightAttackData);
        }

        public void DownAttack()
        {
            ExecuteAttack(downAttackData);
        }

        public void UpAttack()
        {
            ExecuteAttack(upAttackData);
        }

        private void ExecuteAttack(AttackData attackData)
        {
            // ダメージ処理（具体的な実装はここに記述）
            Debug.Log($"弱攻撃を実行: {attackData.attackName}, ダメージ: {attackData.damage}");
        }
    }
}
