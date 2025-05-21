using System;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    [Serializable]
    public class StrongAttack : AttackBase
    {
        [Header("Components")]
        [SerializeField] private ComboSystem comboSystem;
        [SerializeField] private GameObject hitEffectPrefab;

        [Header("Data")]
        [SerializeField] private AttackSet attackSet;
        [SerializeField, ReadOnly] protected AttackData neutralAttackData;
        [SerializeField, ReadOnly] protected AttackData leftAttackData;
        [SerializeField, ReadOnly] protected AttackData rightAttackData;
        [SerializeField, ReadOnly] protected AttackData downAttackData;
        [SerializeField, ReadOnly] protected AttackData upAttackData;

        // コンポジション - 処理を委譲するコンポーネント
        private AttackProcessor attackProcessor;

        protected override void Awake()
        {
            base.Awake();
            attackProcessor = new AttackProcessor(characterController, comboSystem, objectPool, hitEffectPrefab, battleJudge);
        }

        public void OnValidate()
        {
            if (attackSet == null) return;

            neutralAttackData = attackSet.strongNeutral;
            leftAttackData = attackSet.strongLeft;
            rightAttackData = attackSet.strongRight;
            downAttackData = attackSet.strongDown;
            upAttackData = attackSet.strongUp;
        }

        private void Start()
        {
            InitializeAttackDataMap();
        }

        private void InitializeAttackDataMap()
        {
            RegisterAttackData(AttackType.Neutral, neutralAttackData);
            RegisterAttackData(AttackType.Left, leftAttackData);
            RegisterAttackData(AttackType.Right, rightAttackData);
            RegisterAttackData(AttackType.Down, downAttackData);
            RegisterAttackData(AttackType.Up, upAttackData);
        }

        public override void NeutralAttack()
        {
            ExecuteAttack(neutralAttackData);
        }

        public override void LeftAttack()
        {
            ExecuteAttack(leftAttackData);
        }

        public override void RightAttack()
        {
            ExecuteAttack(rightAttackData);
        }

        public override void DownAttack()
        {
            ExecuteAttack(downAttackData);
        }

        public override void UpAttack()
        {
            ExecuteAttack(upAttackData);
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
            if (attackData.canRepeat)
            {
                DelayUtility.StartRepeatedAction(this, attackData.repeatDuration, attackData.repeatInterval, () =>
                {
                    // 攻撃処理をAttackProcessorに委譲
                    StartCoroutine(attackProcessor.ProcessAttack(attackData, this));
                });
            }
            else
            {
                // 攻撃処理をAttackProcessorに委譲
                StartCoroutine(attackProcessor.ProcessAttack(attackData, this));
            }
        }
    }
}