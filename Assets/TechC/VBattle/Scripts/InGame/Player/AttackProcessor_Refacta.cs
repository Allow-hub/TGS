using UnityEngine;

namespace TechC
{
    public static class AttackProcessor_Refacta
    {
        public static void ProcessAttack(AttackData attackData, GameObject ownerObj = null)
        {
            if (attackData == null || ownerObj == null) return;

            bool hitOccurred = false;

            switch (attackData.hitDetectionMode)
            {
                case HitDetectionMode.UseSelf:
                    hitOccurred = ProcessUseSelfMode(attackData, ownerObj);
                    break;
                case HitDetectionMode.OverlapSphere:
                    hitOccurred = ProcessOverlapSphereMode(attackData, ownerObj);
                    break;
                case HitDetectionMode.None:
                default:
                    return;
            }

            if (hitOccurred)
            {
                PlaySound(attackData);
            }
        }

        // ==============================
        // 判定モード別の処理
        // ==============================

        private static bool ProcessUseSelfMode(AttackData data, GameObject owner)
        {
            Collider selfCollider = owner.GetComponent<Collider>();
            if (selfCollider == null) return false;

            Collider[] candidates = Physics.OverlapSphere(owner.transform.position, data.radius, data.targetLayers);
            bool hit = false;

            foreach (var targetCol in candidates)
            {
                if (targetCol.gameObject == owner) continue;

                float dist = Vector3.Distance(selfCollider.bounds.center, targetCol.bounds.center);
                if (dist < GetApproxRadius(selfCollider) + GetApproxRadius(targetCol))
                {
                    hit |= HandleHit(data, targetCol);
                }
            }
            return hit;
        }

        private static bool ProcessOverlapSphereMode(AttackData data, GameObject owner)
        {
            Transform t = owner.transform;
            Vector3 center =
                t.position +
                t.right * data.hitboxOffset.x +
                t.up * data.hitboxOffset.y +
                t.forward * data.hitboxOffset.z;

            Collider[] hitColliders = Physics.OverlapSphere(center, data.radius, data.targetLayers);
            AttackVisualizer.I.DrawHitbox(center, data.radius, 0.5f);

            bool hit = false;
            foreach (var targetCol in hitColliders)
            {
                if (targetCol.gameObject == owner) continue;
                hit |= HandleHit(data, targetCol);
            }

            return hit;
        }

        // ==============================
        // ヒット時の処理
        // ==============================

        private static bool HandleHit(AttackData data, Collider targetCol)
        {
            var controller = targetCol.GetComponentInParent<Player.CharacterController>();
            if (controller == null) return false;

            // カウンター処理
            if (TryProcessCounter(controller))
            {
                Debug.Log("Counter triggered");
                return true;
            }

            // ガード処理
            if (TryProcessGuard(controller, targetCol, data))
            {
                Debug.Log("Guard successful");
                return true;
            }

            // ダメージ処理
            if (TryProcessDamage(targetCol, data))
            {
                ApplyKnockback(data, targetCol);
                return true;
            }

            return false;
        }

        // ==============================
        // 個別処理ユーティリティ
        // ==============================

        private static bool TryProcessGuard(Player.CharacterController controller, Collider targetCol, AttackData data)
        {
            if (!controller.GetCharacterState().IsGuardState()) return false;

            IGuardable guardable = targetCol.GetComponentInParent<IGuardable>();
            if (guardable != null)
            {
                var state = controller.GetCharacterState();
                guardable.GuardDamage(data.damage, state.GetCurrentCommand());
                return true;
            }
            return false;
        }

        private static bool TryProcessDamage(Collider targetCol, AttackData data)
        {
            IDamageable damageable = targetCol.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(data.damage);
                Debug.Log($"Damage applied: {data.damage} to {targetCol.name}");
                return true;
            }
            return false;
        }

        private static bool TryProcessCounter(Player.CharacterController controller)
        {
            if (!controller.CanCounter) return false;
            controller.UseCounter();
            return true;
        }

        private static void ApplyKnockback(AttackData data, Collider target)
        {
            // 実装は用途に応じて
            Debug.Log("Applying knockback to " + target.name);
        }

        private static void PlaySound(AttackData data)
        {
            if (data.characterSEType != CharacterSEType.None)
                AudioManager.I.PlayCharacterSE(data.characterType, data.characterSEType);

            if (data.characterVoiceType != CharacterVoiceType.None)
                AudioManager.I.PlayCharacterVoice(data.characterType, data.characterVoiceType);
        }

        private static float GetApproxRadius(Collider col)
        {
            if (col is SphereCollider s) return s.radius * MaxAxis(col.transform.lossyScale);
            if (col is CapsuleCollider c) return c.radius * MaxAxis(col.transform.lossyScale);
            if (col is BoxCollider b) return b.size.magnitude * 0.5f * MaxAxis(col.transform.lossyScale);
            return 0.5f;
        }

        private static float MaxAxis(Vector3 v) => Mathf.Max(v.x, v.y, v.z);
    }
}