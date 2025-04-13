using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace TechC
{
    [Serializable]
    public class WeakAttack_BaseCharacter : WeakAttack
    {
        // WeakAttack_BaseCharacterクラスに追加
        [SerializeField] private bool showAttackGizmos = true;
        [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.5f); // 半透明の赤
        private Vector3 lastAttackPosition;
        private float lastAttackRadius;
        private bool isAttacking = false;
        public override void NeutralAttack()
        {
            base.NeutralAttack();
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
            characterController.SetAnim(attackData.animHash, true);
            Debug.Log("オーバーライド," + attackData.attackName + attackData.damage);
            StartCoroutine(DelayAttack(attackData));
        }


        public override void ForceFinish()
        {
            base.ForceFinish();
            StopAllCoroutines();
        }

        private IEnumerator DelayAttack(AttackData attackData)
        {
            yield return new WaitForSeconds(attackData.hitTiming);
            // ヒットチェックを実行し、ヒットした場合にヒットストップを発生させる
            if (CheckHit(attackData))
            {
                // ヒットストップを実行
                HitStopManager.I.DoHitStop(attackData.hitStopDuration, attackData.hitStopTimeScale); // 弱攻撃の場合はfalse
            }
        }
        // ヒットチェックメソッド（攻撃の当たり判定を処理）
        private bool CheckHit(AttackData attackData)
        {
            // キャラクターの向いている方向を考慮
            Vector3 attackDirection = characterController.transform.forward;

            // キャラクターのローカル空間でのオフセット位置を計算
            Vector3 localOffset = attackData.hitboxOffset;

            // キャラクターの向きに合わせてオフセットを回転
            Vector3 worldOffset = characterController.transform.TransformDirection(localOffset);

            // 最終的な攻撃位置を計算
            Vector3 attackPosition = characterController.transform.position + worldOffset;

            // デバッグ用にヒットボックス情報を保存
            lastAttackPosition = attackPosition;
            lastAttackRadius = attackData.radius;
            isAttacking = true;

            // 攻撃範囲内のコライダーを検出
            Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackData.radius, attackData.targetLayers);

            bool hitConfirmed = false;
            foreach (var hitCollider in hitColliders)
            {
                // 自分自身は除外
                if (hitCollider.transform == characterController.transform)
                    continue;

                // ヒットしたオブジェクトにダメージを与える
                IDamageable target = hitCollider.gameObject.transform.parent.GetComponent<IDamageable>();
                if (target != null)
                {
                    //Debug.Log($"ヒット検出: {hitCollider.name}にダメージ {attackData.damage} を与えました");
                    target.TakeDamage(attackData.damage);
                    hitConfirmed = true;

                    //ヒットスタンのデータを生成
                    if(hitCollider.gameObject.transform.parent.TryGetComponent<Player.CharacterController>(out Player.CharacterController characterController))
                    {
                        // 攻撃者と被攻撃者の情報からHitDataを生成
                        HitData hitData = HitDataProcessor.CreateFromAttackData(
                            attackData,
                            gameObject.transform,
                            hitCollider.transform
                        ); 
                        characterController.SetLastHitData(hitData);

                        var state = characterController.GetCharacterState();
                        state.ChangeDamageState();
                        // ターゲットにHitDataを設定
                    }
                }
            }

            // ヒットボックスを可視化
            if (showAttackGizmos)
            {
                AttackVisualizer.I.DrawHitbox(
                    attackPosition,
                    attackData.radius,
                    0.5f, // 表示時間
                    hitConfirmed ? Color.red : hitboxColor // ヒット時は赤く表示
                );
            }
            return hitConfirmed;
        }
    }
}
