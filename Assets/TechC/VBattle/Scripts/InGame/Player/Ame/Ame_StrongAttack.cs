using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめの強攻撃の実装
    /// </summary>
    public class Ame_StrongAttack : StrongAttack
    {
        [Header("プレハブの参照")]
        [SerializeField] private GameObject swordObj;
        [SerializeField] private GameObject iceDataPrefab;
        [SerializeField] private GameObject iceRosePrefab;
        // [Header("ニュートラル強")]
        [Header("左強")]
        [SerializeField] private float leftStrongVelocity;
        private float elapsedTime;
        private const int MAXCOUNT = 2;
        private int currentCount;
        private bool OnleftStrong = false;  
        // [Header("右強")]
        // [Header("下強")]
        // [Header("上強")]


        /// <summary>
        /// 数秒前の自分が氷で実体化し、攻撃も記録通りなぞってくれる
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

        }

        /// <summary>
        /// 氷の魔法を圧縮データにして飛ばす、二回目の入力で解凍
        /// その場で爆発が起こる
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();
            currentCount++;
            if (currentCount < MAXCOUNT)
            {
                var obj = CharaEffectFactory.I.GetEffectObj(iceDataPrefab);
                var rb = obj.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.velocity = transform.forward * leftStrongVelocity;
            }
        }

        /// <summary>
        /// 未定
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();
        }

        /// <summary>
        /// 下に剣を突き立てて周囲に氷の薔薇を咲かせて範囲攻撃
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();
            ActiveSword(downAttackData.attackDuration);
        }

        /// <summary>
        /// 未定
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
        }

        private void ActiveSword(float duration)
        {
            swordObj.gameObject.SetActive(true);
            DelayUtility.StartDelayedAction(this, duration, () =>
            {
                swordObj.gameObject.SetActive(false);
            });
        }
    }
}
