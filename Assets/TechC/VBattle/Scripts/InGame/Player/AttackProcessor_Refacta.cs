using UnityEngine;

namespace TechC
{
    public static class AttackProcessor_Refacta
    {
        /// <summary>
        /// 攻撃の実行を行う公開メソッド
        /// </summary>
        /// <param name="attackData"></param>
        public static void ProcessAttack(AttackData attackData, GameObject ownerObj = null)
        {
            if (!CheckHit(attackData, ownerObj)) return;
            PlaySound(attackData);
        }

        /// <summary>
        /// 攻撃のヒット判定をチェックする
        /// </summary>
        /// <param name="attackData"></param>
        /// <returns></returns>
        private static bool CheckHit(AttackData attackData, GameObject ownerObj = null)
        {
            if (attackData == null || attackData.hitDetectionMode == HitDetectionMode.None) return false;
            if (ownerObj == null) return false;

            Collider selfCollider = ownerObj.GetComponent<Collider>();
            if (selfCollider == null) return false;

            bool hitOccurred = false;

            switch (attackData.hitDetectionMode)
            {
                case HitDetectionMode.UseSelf:
                    {
                        // 自身の位置を中心に候補を取得(近くの当たりの候補)
                        Collider[] candidates = Physics.OverlapSphere(
                            ownerObj.transform.position,
                            attackData.radius,
                            attackData.targetLayers);

                        // 近くの当たりの候補から、実際に当たったものをチェック
                        foreach (var other in candidates)
                        {
                            if (other.gameObject == ownerObj) continue;

                            float dist = Vector3.Distance(selfCollider.bounds.center, other.bounds.center);
                            float r1 = GetApproxRadius(selfCollider);
                            float r2 = GetApproxRadius(other);

                            if (dist < (r1 + r2))
                            {
                                // 当たった場合の処理
                                var damageable = other.GetComponent<IDamageable>();
                                if (damageable != null)
                                {
                                    ApplyDamage(attackData, damageable);
                                    ApplyKnockback(attackData, other);
                                    hitOccurred = true;
                                }
                            }
                        }

                        return hitOccurred;
                    }

                case HitDetectionMode.OverlapSphere:
                    {
                        // オフセット考慮して中心位置計算
                        Transform t = ownerObj.transform;
                        Vector3 center =
                            t.position +
                            t.right * attackData.hitboxOffset.x +
                            t.up * attackData.hitboxOffset.y +
                            t.forward * attackData.hitboxOffset.z;

                        Collider[] hitColliders = Physics.OverlapSphere(
                            center,
                            attackData.radius,
                            attackData.targetLayers);
                        AttackVisualizer.I.DrawHitbox(center, attackData.radius, 0.5f);

                        foreach (var collider in hitColliders)
                        {
                            if (collider.gameObject == ownerObj) continue;

                            var damageable = collider.GetComponent<IDamageable>();
                            if (damageable != null)
                            {
                                ApplyDamage(attackData, damageable);
                                ApplyKnockback(attackData, collider);
                                hitOccurred = true;
                            }
                        }

                        return hitOccurred;
                    }

                case HitDetectionMode.None:
                default:
                    return false;
            }
        }

        /// <summary>
        /// 攻撃のサウンドやエフェクトを再生する
        /// </summary>
        private static void PlaySound(AttackData attackData)
        {
            if (attackData == null) return;
            // サウンドの再生処理
            if (attackData.characterSEType != CharacterSEType.None)
            {
                // サウンドエフェクトの再生
                AudioManager.I.PlayCharacterSE(attackData.characterType, attackData.characterSEType);
            }
            if (attackData.characterVoiceType != CharacterVoiceType.None)
            {
                // キャラクターボイスの再生
                AudioManager.I.PlayCharacterVoice(attackData.characterType, attackData.characterVoiceType);
            }
        }

        /// <summary>ノックバックを適用する</summary>
        private static void ApplyDamage(AttackData attackData, IDamageable target)
        {
            if (attackData == null || target == null) return;
            target.TakeDamage(attackData.damage);
            Debug.Log("Damage applied: " + attackData.damage);
        }

        /// <summary>
        /// ノックバックを適用する
        /// </summary>
        /// <param name="attackData"></param>
        /// <param name="target"></param>
        private static void ApplyKnockback(AttackData attackData, Collider target)
        {
            // 実装は必要に応じて拡張
            Debug.Log("Applying knockback to: " + target.name);
        }

        private static float GetApproxRadius(Collider collider)
        {
            if (collider is SphereCollider sphere)
                return sphere.radius * MaxAxis(collider.transform.lossyScale);

            if (collider is CapsuleCollider capsule)
                return capsule.radius * MaxAxis(collider.transform.lossyScale);

            if (collider is BoxCollider box)
                return box.size.magnitude * 0.5f * MaxAxis(collider.transform.lossyScale);

            // その他の複雑な形状には安全なデフォルト
            return 0.5f;
        }

        private static float MaxAxis(Vector3 v) => Mathf.Max(v.x, v.y, v.z);

    }
}
