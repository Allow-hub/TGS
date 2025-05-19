using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめの弱攻撃の実装
    /// </summary>
    public class Ame_WeakAttack : WeakAttack
    {
        #region  エフェクトのプレハブや参照
        [SerializeField] private GameObject sword;
        [SerializeField] private GameObject slash;
        [SerializeField] private GameObject flyingSlash;
        #endregion

        #region ニュートラルアタックの設定
        [SerializeField] private float slashEffectDistance = 2f;
        [SerializeField] private Quaternion n1Rot;
        [SerializeField] private Quaternion n2Rot;
        [SerializeField] private Quaternion n3Rot;
        private float returnNeutralEffectTime = 3f;
        private Quaternion currentSlashRot;
        #endregion
        public override void NeutralAttack()
        {
            base.NeutralAttack();
            if (currentNeutral == neutralAttackData_1)
                currentSlashRot = n1Rot;
            else if (currentNeutral == neutralAttackData_2)
                currentSlashRot = n2Rot;
            else if (currentNeutral == neutralAttackData_3)
                currentSlashRot = n3Rot;
            var slObjPos = new Vector3(transform.position.x, transform.position.y + slashEffectDistance, transform.position.z);
            var slObj = CharaEffectFactory.I.GetEffectObj(slash, slObjPos, currentSlashRot);

            DelayUtility.StartDelayedAction(this, returnNeutralEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(slObj);
            });
        }

        public override void LeftAttack()
        {
            base.LeftAttack();
        }

        public override void RightAttack()
        {
            base.RightAttack();
        }

        public override void DownAttack()
        {
            base.DownAttack();
        }

        public override void UpAttack()
        {
            base.UpAttack();
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);

            sword.SetActive(true);
            StartCoroutine(SwordDisActive(attackData));
        }

        private IEnumerator SwordDisActive(AttackData attackData)
        {
            yield return new WaitForSeconds(attackData.attackDuration);
            sword.SetActive(false);
        }
    }
}