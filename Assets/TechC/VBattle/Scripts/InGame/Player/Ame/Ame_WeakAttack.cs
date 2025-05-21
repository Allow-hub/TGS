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
        [Header("エフェクトのプレハブや参照")]
        [SerializeField] private GameObject sword;
        [SerializeField] private GameObject slash;
        [SerializeField] private GameObject flyingSlash;
        [SerializeField] private GameObject flower;

        [Header("ニュートラルアタックの設定")]
        [SerializeField] private float slashEffectDistance = 2f;
        [SerializeField] private Quaternion n1Rot;
        [SerializeField] private Quaternion n2Rot;
        [SerializeField] private Quaternion n3Rot;
        private float returnNeutralEffectTime = 3f;
        private Quaternion currentSlashRot;
        [Header("左弱")]

        [Header("右弱")]
        [SerializeField] private float xOffset = 3f;
        [SerializeField] private float flyingSlashSpeed = 5f;
        private float returnRightEffectTime = 3f;

        [Header("下弱")]
        //スライディング時の変化後の自分の当たり判定
        [SerializeField] private Vector3 changeHitBox;
        [SerializeField] private float chageColliderSpeed = 10f;
        [SerializeField] private float slidingSpeed = 5f;
        private float returnDownEffectTime = 2f;


        [Header("上弱")]
        private float returnUpEffectTime = 3f;
        

        public override void NeutralAttack()
        {
            base.NeutralAttack();

            //ニュートラルが何段階目かを確かめる
            if (currentNeutral == neutralAttackData_1)
                currentSlashRot = n1Rot;
            else if (currentNeutral == neutralAttackData_2)
                currentSlashRot = n2Rot;
            else if (currentNeutral == neutralAttackData_3)
                currentSlashRot = n3Rot;
            var slObjPos = transform.position.AddY(slashEffectDistance);
            // 向きに応じて回転反転
            if (transform.forward.x < 0) 
            {
                currentSlashRot = Quaternion.Euler(0, 180, 0) * currentSlashRot;
            }

            //slashEffectの取得。各段階の回転を反映
            var slObj = CharaEffectFactory.I.GetEffectObj(slash, slObjPos, currentSlashRot);

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
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
            GameObject slObj = null;
            DelayUtility.StartDelayedAction(this, rightAttackData.hitTiming, () =>
            {
                //飛び道具の処理
                var pos = transform.position.AddX(xOffset);
                slObj = CharaEffectFactory.I.GetEffectObj(flyingSlash, pos, Quaternion.identity);
                var effectSetting = slObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID); 
                var rb = slObj.GetComponent<Rigidbody>();
                //斬撃をrbで飛ばす
                rb.velocity = transform.forward * flyingSlashSpeed;
            });
            
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnRightEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(slObj);
            });
        }

        public override void DownAttack()
        {
            base.DownAttack();
            characterController.StopVelocity();
            characterController.AddForcePlayer(transform.forward, slidingSpeed, ForceMode.Impulse);
            characterController.ChangeHitCollider(changeHitBox, chageColliderSpeed);
            characterController.ChangeColliderTrigger(true);
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnDownEffectTime, () =>
            {
                // CharaEffectFactory.I.ReturnEffectObj(flowerEffect);
                characterController.ResetHitCollider(chageColliderSpeed);
                characterController.ChangeColliderTrigger(false);
            });
        }


        public override void UpAttack()
        {
            base.UpAttack();
            var flowerEffect = CharaEffectFactory.I.GetEffectObj(flower, transform.up, Quaternion.identity);

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnUpEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(flowerEffect);
            });
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