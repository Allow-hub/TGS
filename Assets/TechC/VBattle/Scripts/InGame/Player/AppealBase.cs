using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    /// <summary>
    /// アピールは現状Neutral以外使わない予定
    /// </summary>
    [Serializable]
    public class AppealBase : MonoBehaviour, IAttackBase
    {
        [SerializeField] private Player.CharacterController characterController;
        [SerializeField]
        private AttackSet attackSet;
        [SerializeField,ReadOnly]
        protected AttackData neutralAttackData;
        [SerializeField, ReadOnly]
        protected AttackData leftAttackData;
        [SerializeField, ReadOnly]
        protected AttackData rightAttackData;
        [SerializeField, ReadOnly]
        protected AttackData downAttackData;
        [SerializeField, ReadOnly]
        protected AttackData upAttackData;
        [SerializeField] private BaseInputManager inputManager;

        [Tooltip("必殺技のチャージが可能な期間"), SerializeField]
        protected float canChargeDuration = 10f;

        private readonly Dictionary<AttackType, AttackData> attackDataMap;

        public AppealBase()
        {
            attackDataMap = new Dictionary<AttackType, AttackData>();
        }
        private void Start()
        {
            InitializeAttackDataMap();
        }

        public void OnValidate()
        {
            neutralAttackData = attackSet.appealNeutral;
            leftAttackData = attackSet.appealLeft;
            rightAttackData = attackSet.appealRight;
            downAttackData = attackSet.appealDown;
            upAttackData = attackSet.appealUp;

        }
        private void InitializeAttackDataMap()
        {
            attackDataMap[AttackType.Neutral] = neutralAttackData;
            attackDataMap[AttackType.Left] = leftAttackData;
            attackDataMap[AttackType.Right] = rightAttackData;
            attackDataMap[AttackType.Down] = downAttackData;
            attackDataMap[AttackType.Up] = upAttackData;
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
            characterController.GetAnim().speed = attackData.animationSpeed;
            characterController.GetAnim().SetBool(attackData.animHash, true);
            StartCoroutine(Charge(attackData));
        }
        /// <summary>
        /// 必殺技のチャージを可能に
        /// </summary>
        /// <returns></returns>
        private IEnumerator Charge(AttackData attackData)
        {
            yield return new WaitForSeconds(attackData.attackDuration);
            characterController.ChangeCanCharge(true);
        }
        /// <summary>
        /// 強制終了時
        /// </summary>
        public virtual void ForceFinish()
        {
            characterController.ChangeCanCharge(false);
            Debug.Log("AN");

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
        public AttackData GetAttackData(AttackType attackType)
        {
            if (attackDataMap.TryGetValue(attackType, out var attackData))
            {
                return attackData;
            }

            Debug.LogWarning("未定義のAttackTypeが指定されました");
            return neutralAttackData;
        }
    }
}
