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
        [SerializeField] private GameObject sword;
        [SerializeField] private GameObject slash;
        [SerializeField] private GameObject flyingSlash;

        public override async void NeutralAttack()
        {
            base.NeutralAttack();
            // var slObj=CharaEffectFactory.I.GetEffectObj(slash, sword.transform.position, Quaternion.identity);
            // await DelayUtility.RunAfterDelay(3f, () =>
            // {
            //     CharaEffectFactory.I.ReturnEffectObj(slObj);
            // });
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