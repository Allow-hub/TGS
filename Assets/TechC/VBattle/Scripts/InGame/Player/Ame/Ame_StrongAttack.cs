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
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private GameObject iceDataPrefab;
        [SerializeField] private GameObject iceExplosionPrefab;
        [SerializeField] private GameObject iceRosePrefab;
        // [Header("ニュートラル強")]
        [Header("左強")]
        [SerializeField] private float magicDuration = 2f;
        [SerializeField] private float yOffset = 2;
        [SerializeField] private float leftStrongVelocity;
        [SerializeField] private float explosionDuration = 3f;
        private GameObject currentIceObj;
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
                magicCircle.SetActive(true);
                DelayUtility.StartDelayedAction(this, magicDuration, () =>
                {
                    magicCircle.SetActive(false);
                });
                var pos = transform.position.AddY(yOffset);
                currentIceObj = CharaEffectFactory.I.GetEffectObj(iceDataPrefab, pos, Quaternion.identity);
                var rb = currentIceObj.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.velocity = transform.forward * leftStrongVelocity;
            }
            else
            {

                var createPos = currentIceObj.transform.position;
                CharaEffectFactory.I.ReturnEffectObj(currentIceObj);
                var explosionObj = CharaEffectFactory.I.GetEffectObj(iceExplosionPrefab, createPos, Quaternion.identity);
                var charaEffectSetting = explosionObj.GetComponent<CharaEffect>();
                charaEffectSetting.SetOwnerId(characterController.PlayerID);
                charaEffectSetting.SetAttackProcessor(attackProcessor);
                DelayUtility.StartDelayedAction(this, explosionDuration, () =>
                {
                    CharaEffectFactory.I.ReturnEffectObj(explosionObj);
                });
                currentCount = 0;
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
