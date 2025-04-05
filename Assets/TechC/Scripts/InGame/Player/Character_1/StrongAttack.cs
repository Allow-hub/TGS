using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class StrongAttack : MonoBehaviour, IAttackBase
    {
        private AttackData neutralAttackData;
        private AttackData leftAttackData;
        private AttackData rightAttackData;
        private AttackData downAttackData;
        private AttackData upAttackData;

        private AudioSource audioSource;
        private ParticleSystem chargeEffect;

        public void NeutralAttack()
        {
            StartCoroutine(ChargedAttack(neutralAttackData));
        }

        public void LeftAttack()
        {
            StartCoroutine(ChargedAttack(leftAttackData));
        }

        public void RightAttack()
        {
            StartCoroutine(ChargedAttack(rightAttackData));
        }

        public void DownAttack()
        {
            StartCoroutine(ChargedAttack(downAttackData));
        }

        public void UpAttack()
        {
            StartCoroutine(ChargedAttack(upAttackData));
        }

        private IEnumerator ChargedAttack(AttackData attackData)
        {
            // // 溜め時間
            float chargeTime = 0.5f;
            yield return new WaitForSeconds(chargeTime);

          
            Debug.Log($"強攻撃を実行: {attackData.attackName}, ダメージ: {attackData.damage}");
        }
    }
}
