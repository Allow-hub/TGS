using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using TechC.Player;

namespace TechC
{
    public static class AttackProcessor_Refacta
    {
        private static float hitEffectDuration = 1f;

        public static void ProcessAttack(AttackData attackData,Player.CharacterController characterController, GameObject ownerObj)
        {
            if (attackData == null || ownerObj == null) return;

            bool hitOccurred = attackData.hitDetectionMode switch
            {
                HitDetectionMode.UseSelf => ProcessUseSelfMode(attackData,characterController, ownerObj),
                HitDetectionMode.OverlapSphere => ProcessOverlapSphereMode(attackData,characterController, ownerObj),
                _ => false,
            };

            if (hitOccurred)
            {
                PlaySound(attackData);
            }
        }

        // ==============================
        // 判定モード別の処理
        // ==============================

        private static bool ProcessUseSelfMode(AttackData data,Player.CharacterController characterController ,GameObject owner)
        {
            Collider selfCollider = owner.GetComponent<Collider>();
            if (selfCollider == null) return false;

            var ownerController = characterController;
            var filtered = FilterTargets(
                Physics.OverlapSphere(owner.transform.position, data.radius, data.targetLayers),
                ownerController
            );

            bool hit = false;
            foreach (var (col, ctrl) in filtered)
            {
                float dist = Vector3.Distance(selfCollider.bounds.center, col.bounds.center);
                if (dist < GetApproxRadius(selfCollider) + GetApproxRadius(col))
                {
                    hit |= HandleHit(data, col, ctrl);
                }
            }

            return hit;
        }

        private static bool ProcessOverlapSphereMode(AttackData data,Player.CharacterController characterController ,GameObject owner)
        {
            Transform t = owner.transform;
            Vector3 center = t.position
                + t.right * data.hitboxOffset.x
                + t.up * data.hitboxOffset.y
                + t.forward * data.hitboxOffset.z;

            Collider[] hitColliders = Physics.OverlapSphere(center, data.radius, data.targetLayers);
            AttackVisualizer.I.DrawHitbox(center, data.radius, 0.5f);

            var ownerController = characterController;
            var filtered = FilterTargets(hitColliders, ownerController);

            bool hit = false;
            foreach (var (col, ctrl) in filtered)
            {
                hit |= HandleHit(data, col, ctrl);
            }

            return hit;
        }

        // ==============================
        // フィルタ処理：重複・自分自身の除外
        // ==============================

        private static List<(Collider, Player.CharacterController)> FilterTargets(Collider[] colliders, Player.CharacterController ownerController)
        {
            var results = new List<(Collider, Player.CharacterController)>();
            var seenControllers = new HashSet<Player.CharacterController>();

            foreach (var col in colliders)
            {
                // ✅ 自分のrootと一致するものは無条件に除外
                if (col.transform.root == ownerController?.transform.root) continue;

                var controller = col.GetComponentInParent<Player.CharacterController>();
                if (controller == null) continue;
                if (!seenControllers.Add(controller)) continue;

                results.Add((col, controller));
            }

            return results;
        }

        // ==============================
        // ヒット時の処理
        // ==============================

        private static bool HandleHit(AttackData data, Collider targetCol, Player.CharacterController controller)
        {
            if (controller == null) return false;

            if (TryProcessCounter(controller)) return true;
            if (TryProcessGuard(controller, targetCol, data)) return true;

            if (TryProcessDamage(targetCol, data))
            {
                PlayHitEffect(controller, targetCol.transform.position, data);
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
                guardable.GuardDamage(data.damage, controller.GetCharacterState().GetCurrentCommand());
                return true;
            }

            return false;
        }

        private static bool TryProcessDamage(Collider targetCol, AttackData data)
        {
            IDamageable damageable = targetCol.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                HitStopManager.I.DoHitStop(data.hitStopDuration, data.hitStopTimeScale);
                CameraManager.I.StartShake(data.shakeIntensity, data.shakeDuraion, data.noiseSettings);
                damageable.TakeDamage(data.damage);
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
            Rigidbody rb = target.GetComponentInParent<Rigidbody>();
            if (rb == null) return;

            Vector3 force = data.knockbackDirection.normalized * data.knockback;
            rb.AddForce(force, ForceMode.Impulse);
        }

        private static void PlayHitEffect(MonoBehaviour mono, Vector3 position, AttackData data)
        {
            if (data.hitEffectPrefab == null) return;
            GameObject effect = EffectFactory.I.GetEffectObj(data.hitEffectPrefab);
            if (effect == null) return;

            effect.transform.position = position;

            DelayUtility.StartDelayedActionWithPause(mono, hitEffectDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                if (effect != null)
                {
                    EffectFactory.I.ReturnEffect(effect);
                }
            });
        }

        private static void PlaySound(AttackData data)
        {
            AudioManager.I.PlaySE(SEID.Hit, 0.3f);

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