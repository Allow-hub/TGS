using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public abstract  class WeakAttack : MonoBehaviour, IAttackBase
    {
        protected AttackData neutralAttackData;
        protected AttackData leftAttackData;
        protected AttackData rightAttackData;
        protected AttackData downAttackData;
        protected AttackData upAttackData;


        public virtual void NeutralAttack()
        {
            ExecuteAttack(neutralAttackData);
        }

        public virtual void LeftAttack()
        {
            ExecuteAttack(leftAttackData);
        }

        public virtual void RightAttack()
        {
            ExecuteAttack(rightAttackData);
        }

        public virtual void DownAttack()
        {
            ExecuteAttack(downAttackData);
        }

        public virtual void UpAttack()
        {
            ExecuteAttack(upAttackData);
        }

        // privateからprotectedに変更して子クラスからアクセス可能にする
        protected virtual void ExecuteAttack(AttackData attackData)
        {
            // ダメージ処理
            Debug.Log($"弱攻撃を実行: {attackData.attackName}, ダメージ: {attackData.damage}");
        }
    }
}
