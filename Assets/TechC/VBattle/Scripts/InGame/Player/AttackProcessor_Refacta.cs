using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public static class AttackProcessor_Refacta
    {
      /// <summary>
        /// 攻撃判定を実行する共通メソッド
        /// </summary>
        /// <param name="attackData">攻撃データ</param>
        /// <param name="attackerTransform">攻撃者のTransform</param>
        /// <param name="attackerPlayerID">攻撃者のプレイヤーID</param>
        /// <param name="visualizeHitbox">ヒットボックスを可視化するか</param>
        public static void ProcessAttack(AttackData attackData, Transform attackerTransform, int attackerPlayerID, bool visualizeHitbox = false)
        {
            if (attackData == null || attackerTransform == null) return;

            // ヒットボックスの位置計算
            Vector3 hitboxCenter = CalculateHitboxCenter(attackData, attackerTransform);

            // 攻撃判定実行
            ExecuteAttackCheck(attackData, hitboxCenter, attackerPlayerID);

            // 可視化
            if (visualizeHitbox)
            {
                AttackVisualizer.I.DrawHitbox(hitboxCenter, attackData.radius, 1f);
            }
        }

        /// <summary>
        /// ヒットボックスの中心位置を計算
        /// </summary>
        private static Vector3 CalculateHitboxCenter(AttackData attackData, Transform attackerTransform)
        {
            Vector3 offset = attackerTransform.right * attackData.hitboxOffset.x +
                           attackerTransform.up * attackData.hitboxOffset.y +
                           attackerTransform.forward * attackData.hitboxOffset.z;

            return attackerTransform.position + offset;
        }

        /// <summary>
        /// 実際の攻撃判定処理
        /// </summary>
        private static void ExecuteAttackCheck(AttackData attackData, Vector3 center, int attackerPlayerID)
        {
            Collider[] hits = Physics.OverlapSphere(center, attackData.radius, attackData.targetLayers);
            
            foreach (var hit in hits)
            {
                // 自分自身への攻撃チェック
                var opponent = hit.GetComponent<Player.CharacterController>();
                if (opponent?.PlayerID == attackerPlayerID) continue;

                // 攻撃可能対象かチェック
                if (opponent != null && !BattleJudge.I.IsValidAttackTarget(opponent.PlayerID))
                {
                    Debug.Log($"相手は現在無敵");
                    continue;
                }

                // ダメージ処理
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    ProcessDamage(damageable, attackData, hit.transform, attackerPlayerID);
                }
            }
        }

        /// <summary>
        /// ダメージ処理
        /// </summary>
        private static void ProcessDamage(IDamageable target, AttackData attackData, Transform targetTransform, int attackerPlayerID)
        {
            // 基本ダメージ
            target.TakeDamage(attackData.damage);

            // ヒットストップ
            if (attackData.hitStopDuration > 0)
            {
                // HitStopManager.I?.ApplyHitStop(attackData.hitStopDuration, attackData.hitStopTimeScale);
            }

            // ノックバック
            ApplyKnockback(targetTransform, attackData);

            // エフェクト・サウンド
            PlayHitEffects(attackData, targetTransform.position);

            Debug.Log($"攻撃ヒット: {attackData.attackName} -> {target}");
        }

        /// <summary>
        /// ノックバック適用
        /// </summary>
        private static void ApplyKnockback(Transform target, AttackData attackData)
        {
            if (attackData.knockback <= 0) return;

            var rigidbody = target.GetComponent<Rigidbody>();
            if (rigidbody == null) return;

            Vector3 knockbackDirection = attackData.useCustomKnockbackDirection 
                ? attackData.knockbackDirection.normalized 
                : Vector3.forward;

            rigidbody.AddForce(knockbackDirection * attackData.knockback, ForceMode.Impulse);
        }

        /// <summary>
        /// ヒット時のエフェクト・サウンド再生
        /// </summary>
        private static void PlayHitEffects(AttackData attackData, Vector3 hitPosition)
        {
            // サウンド再生
            if (attackData.characterSEType != CharacterSEType.None)
            {
                // AudioManager.PlaySE(attackData.characterSEType);
            }

            // 画面揺れ
            if (attackData.shakeIntensity > 0)
            {
                // CameraShakeManager.Shake(attackData.shakeIntensity, attackData.shakeDuration);
            }
        }
    }
}
