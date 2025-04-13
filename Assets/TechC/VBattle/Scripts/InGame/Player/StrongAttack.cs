using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    [Serializable]
    public  class StrongAttack : MonoBehaviour , IAttackBase
    {
        [SerializeField]
        private AttackSet attackSet;
        [SerializeField]
        protected AttackData neutralAttackData;
        [SerializeField]
        protected AttackData leftAttackData;
        [SerializeField] 
        protected AttackData rightAttackData;
        [SerializeField] 
        protected AttackData downAttackData;
        [SerializeField] 
        protected AttackData upAttackData;

        public void OnValidate()
        {
            neutralAttackData = attackSet.strongNeutral;
            leftAttackData = attackSet.strongLeft;
            rightAttackData = attackSet.strongRight;
            downAttackData = attackSet.strongDown;
            upAttackData = attackSet.strongUp;
        }
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
            Debug.Log($"強攻撃を実行: {attackData.attackName}, ダメージ: {attackData.damage}");
        }
        public float GetDuration(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Neutral:
                    return neutralAttackData.attackDuration;
                case AttackType.Left:
                    return leftAttackData.attackDuration;
                case AttackType.Right:
                    return rightAttackData.attackDuration;
                case AttackType.Down:
                    return downAttackData.attackDuration;
                case AttackType.Up:
                    return upAttackData.attackDuration;
                default:
                    Debug.LogWarning("未定義のAttackTypeが指定されました");
                    return 0f;
            }
        }
        /// <summary>
        /// 強制終了時
        /// </summary>
        public virtual void ForceFinish()
        {
        }
    }
}
