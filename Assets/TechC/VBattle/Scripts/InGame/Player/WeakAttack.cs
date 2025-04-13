using System;
using System.Collections;
using TechC.Player;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    [Serializable]
    public  class WeakAttack : MonoBehaviour, IAttackBase
    {
        [SerializeField] protected Player.CharacterController characterController;
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
            if (attackSet == null) return;

            neutralAttackData = attackSet.weakNeutral;
            leftAttackData = attackSet.weakLeft;
            rightAttackData = attackSet.weakRight;
            downAttackData = attackSet.weakDown;
            upAttackData = attackSet.weakUp;
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

        protected virtual void ExecuteAttack(AttackData attackData)
        {
            StartCoroutine(EndAttack(attackData));
        }

        /// <summary>
        /// アニメーションの秒数分経過したら自動でfalseに
        /// </summary>
        /// <param name="attackData"></param>
        /// <returns></returns>
        private IEnumerator EndAttack(AttackData attackData)
        {
            yield return new WaitForSeconds(attackData.attackDuration);
            characterController.SetAnim(attackData.animHash, false);
        }
        /// <summary>
        /// 強制終了時
        /// </summary>
        public virtual void ForceFinish()
        {
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
    }
}
