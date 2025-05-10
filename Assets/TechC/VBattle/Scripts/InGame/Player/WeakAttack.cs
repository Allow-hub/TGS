using System;
using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    [Serializable]
    public class WeakAttack : AttackBase
    {
        [Header("Components")]
        [SerializeField] private BaseInputManager inputManager;
        [SerializeField] private CommandHistory commandHistory;
        [SerializeField] private ComboSystem comboSystem;
        [SerializeField] private GameObject hitEffectPrefab;

        [Header("Data")]
        [SerializeField] private AttackSet attackSet;
        [SerializeField, ReadOnly] protected AttackData neutralAttackData_1, neutralAttackData_2, neutralAttackData_3;
        [SerializeField, ReadOnly] protected AttackData leftAttackData;
        [SerializeField, ReadOnly] protected AttackData rightAttackData;
        [SerializeField, ReadOnly] protected AttackData downAttackData;
        [SerializeField, ReadOnly] protected AttackData upAttackData;

        // コンポジション - 処理を委譲するコンポーネント
        private AttackProcessor attackProcessor;
        private NeutralComboChecker neutralComboChecker;

        protected override void Awake()
        {
            base.Awake();
            attackProcessor = new AttackProcessor(characterController, comboSystem, objectPool,hitEffectPrefab,battleJudge);
            neutralComboChecker = new NeutralComboChecker(commandHistory);
        }

        public void OnValidate()
        {
            if (attackSet == null) return;

            neutralAttackData_1 = attackSet.weakNeutral_1;
            neutralAttackData_2 = attackSet.weakNeutral_2;
            neutralAttackData_3 = attackSet.weakNeutral_3;
            leftAttackData = attackSet.weakLeft;
            rightAttackData = attackSet.weakRight;
            downAttackData = attackSet.weakDown;
            upAttackData = attackSet.weakUp;
        }

        private void Start()
        {
            InitializeAttackDataMap();
        }

        private void InitializeAttackDataMap()
        {
            RegisterAttackData(AttackType.Neutral, neutralAttackData_1);
            RegisterAttackData(AttackType.Left, leftAttackData);
            RegisterAttackData(AttackType.Right, rightAttackData);
            RegisterAttackData(AttackType.Down, downAttackData);
            RegisterAttackData(AttackType.Up, upAttackData);
        }

        public override void NeutralAttack()
        {
            // ニュートラルコンボチェッカーに次の攻撃データを取得
            AttackData nextAttack = neutralComboChecker.GetNextNeutralAttackData(
                neutralAttackData_1,
                neutralAttackData_2,
                neutralAttackData_3
            );
            CustomLogger.Info("ニュートラル番号"+nextAttack.name,"comboCheck");
            ExecuteAttack(nextAttack);
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
            // 攻撃処理をAttackProcessorに委譲
            StartCoroutine(attackProcessor.ProcessAttack(attackData, this));
        }


        public override void ForceFinish()
        {
            base.ForceFinish();
            neutralComboChecker.ResetCombo();
        }
    }
}