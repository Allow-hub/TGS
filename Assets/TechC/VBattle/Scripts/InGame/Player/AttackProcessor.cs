using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 攻撃処理の実行を担当するクラス
    /// 攻撃判定やヒットチェックなどの処理を集約
    /// </summary>
    public class AttackProcessor
    {
        private GameObject hitEffectPrefab;
        private bool showAttackGizmos = true;
        private Color hitboxColor = new Color(1f, 0f, 0f, 0.5f); // 半透明の赤

        private Vector3 lastAttackPosition;
        private float lastAttackRadius;
        private Player.CharacterController characterController;
        private Player.CharacterController opponentCharacterController;
        private ComboSystem comboSystem;

        // ヒットエフェクトの持続時間（秒）
        private float hitEffectDuration = 1.5f;
        private GameObject currentHitEffect;
        private ObjectPool objectPool;
        private BattleJudge battleJudge;
        public AttackProcessor(Player.CharacterController controller, ComboSystem comboSystem, ObjectPool objectPool, GameObject hitEffectPrefab, BattleJudge battleJudge)
        {
            this.characterController = controller;
            this.comboSystem = comboSystem;
            this.objectPool = objectPool;
            this.hitEffectPrefab = hitEffectPrefab;
            this.battleJudge = battleJudge;
        }
        public void HandleAttack(AttackData attackData, Collider hitCollider)
        {
            if (TryProcessHit(hitCollider, attackData))
            {
                HitConfirmed(hitCollider.transform.position);
            }
        }
        /// <summary>
        /// 攻撃処理を実行する
        /// </summary>
        public IEnumerator ProcessAttack(AttackData attackData)
        {
            yield return new WaitForSeconds(attackData.hitTiming);
            // ヒットチェックを実行し、ヒットした場合にヒットストップを発生させる
            if (PerformAttackHitCheck(attackData))
            {
                // ヒットストップを実行
                HitStopManager.I.DoHitStop(attackData.hitStopDuration, attackData.hitStopTimeScale);
            }
        }

        /// <summary>
        /// ヒットチェックの統括メソッド
        /// </summary>
        private bool PerformAttackHitCheck(AttackData attackData)
        {
            bool hitConfirmed = false;
            Vector3 hitPosition = Vector3.zero;

            if (attackData.useSelfColliderForHitCheck)
            {
                Collider selfCollider = characterController.GetCollider();
                if (selfCollider == null)
                {
                    Debug.LogWarning("自キャラのコライダーが取得できませんでした");
                    return false;
                }

                // 自分のコライダーと接触しているコライダーを取得
                Collider[] overlaps = Physics.OverlapBox(
                    selfCollider.bounds.center,
                    selfCollider.bounds.extents,
                    selfCollider.transform.rotation,
                    attackData.targetLayers
                );

                foreach (var hitCollider in overlaps)
                {
                    if (IsOwnCollider(hitCollider)) continue;

                    if (TryProcessHit(hitCollider, attackData))
                    {
                        hitConfirmed = true;
                        hitPosition = hitCollider.transform.position; 
                    }
                }
            }
            else
            {
                // 通常の OverlapSphere チェック
                Vector3 attackPosition = CalculateAttackPosition(attackData);
                UpdateDebugInfo(attackPosition, attackData.radius);

                Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackData.radius, attackData.targetLayers);
                foreach (var hitCollider in hitColliders)
                {
                    if (IsOwnCollider(hitCollider))
                        continue;
                    if (TryProcessHit(hitCollider, attackData))
                    {
                        hitConfirmed = true;
                        hitPosition = hitCollider.transform.position; 
                    }
                }
            }

            //攻撃が成功したとき
            if (hitConfirmed)
            {
                HitConfirmed(hitPosition);
            }

            //可視化
            if (!attackData.useSelfColliderForHitCheck)
                VisualizeHitbox(CalculateAttackPosition(attackData), attackData.radius, hitConfirmed);

            return hitConfirmed;
        }
        private void HitConfirmed(Vector3 pos)
        {
            currentHitEffect = objectPool.GetObject(hitEffectPrefab);
            if (currentHitEffect != null)
            {
                currentHitEffect.transform.position = pos;
                ReturnEffect(currentHitEffect).Forget();
            }

            comboSystem?.CheckCombos();
        }

        /// <summary>
        /// 対象のコライダーに対して、ガード・ダメージ・ヒットスタンなどを処理する
        /// </summary>
        /// <returns>攻撃がヒットしたかどうか</returns>
        private bool TryProcessHit(Collider hitCollider, AttackData attackData)
        {
            var targetController = GetOpponentController(hitCollider);
            if (targetController == null) return false;

            if (TryProcessGuard(targetController, hitCollider, attackData))
            {
                return true;
            }

            if (TryProcessDamage(hitCollider, attackData))
            {
                ProcessHitStun(hitCollider, attackData);
                return true;
            }

            return false;
        }

        /// <summary>
        /// ヒットエフェクトを一定時間後にプールに戻す非同期メソッド
        /// </summary>
        private async UniTask ReturnEffect(GameObject retrunObj)
        {
            if (retrunObj == null) return;

            // 指定された時間待機
            await UniTask.Delay(TimeSpan.FromSeconds(hitEffectDuration));

            // エフェクトをプールに戻す
            if (retrunObj != null)
            {
                objectPool.ReturnObject(retrunObj);
                retrunObj = null;
                Debug.Log("ヒットエフェクトをプールに返却しました");
            }
        }

        /// <summary>
        /// 攻撃位置の計算
        /// </summary>
        private Vector3 CalculateAttackPosition(AttackData attackData)
        {
            // キャラクターのローカル空間でのオフセット位置を計算
            Vector3 localOffset = attackData.hitboxOffset;

            // キャラクターの向きに合わせてオフセットを回転
            Vector3 worldOffset = characterController.transform.TransformDirection(localOffset);

            // 最終的な攻撃位置を計算
            return characterController.transform.position + worldOffset;
        }

        /// <summary>
        /// デバッグ情報を更新
        /// </summary>
        private void UpdateDebugInfo(Vector3 position, float radius)
        {
            lastAttackPosition = position;
            lastAttackRadius = radius;
        }

        /// <summary>
        /// 自分のコライダーかどうかをチェック
        /// </summary>
        private bool IsOwnCollider(Collider collider)
        {
            return collider.gameObject.GetMostParentComponent<Transform>() == characterController.transform;
        }

        /// <summary>
        /// 対戦相手のコントローラーを取得
        /// </summary>
        private Player.CharacterController GetOpponentController(Collider collider)
        {
            // nullチェック
            if (collider == null)
            {
                Debug.LogWarning("Null collider passed to GetOpponentController");
                return null;
            }

            // コライダーの親オブジェクトを取得
            Transform parentTransform = collider.gameObject.transform.parent;

            // 親オブジェクトのnullチェック
            if (parentTransform == null)
            {
                Debug.LogWarning($"No parent transform found for collider on object: {collider.gameObject.name}");
                return null;
            }

            // CharacterControllerコンポーネントを取得
            Player.CharacterController opponentController = parentTransform.GetComponent<Player.CharacterController>();

            // コンポーネントのnullチェック
            if (opponentController == null)
            {
                Debug.LogWarning($"No CharacterController found on parent of object: {collider.gameObject.name}");
                return null;
            }

            return opponentController;
        }

        /// <summary>
        /// ガード処理を試行
        /// </summary>
        private bool TryProcessGuard(Player.CharacterController targetController, Collider hitCollider, AttackData attackData)
        {
            if (targetController.GetCharacterState().IsGuardState())
            {
                IGuardable guardable = hitCollider.gameObject.transform.parent.GetComponent<IGuardable>();
                if (guardable != null)
                {
                    var opponentState = targetController.GetCharacterState();
                    guardable.GuardDamage(attackData.damage, opponentState.GetCurrentCommand());
                    Debug.Log("対象がガード中です");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ダメージ処理を試行
        /// </summary>
        private bool TryProcessDamage(Collider hitCollider, AttackData attackData)
        {
            IDamageable target = hitCollider.gameObject.transform.parent.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(attackData.damage);
                return true;
            }
            return false;
        }

        /// <summary>
        /// ヒットスタン処理
        /// </summary>
        private void ProcessHitStun(Collider hitCollider, AttackData attackData)
        {
            if (hitCollider.gameObject.transform.parent.TryGetComponent<Player.CharacterController>(out Player.CharacterController characterController))
            {
                // 攻撃者と被攻撃者の情報からHitDataを生成
                HitData hitData = HitDataProcessor.CreateFromAttackData(
                    attackData,
                    this.characterController.transform,
                    hitCollider.transform
                );
                characterController.SetLastHitData(hitData);

                var state = characterController.GetCharacterState();
                state.ChangeDamageState();
            }
        }

        /// <summary>
        /// ヒットボックスの可視化
        /// </summary>
        private void VisualizeHitbox(Vector3 position, float radius, bool hitConfirmed)
        {
            if (showAttackGizmos)
            {
                AttackVisualizer.I.DrawHitbox(
                    position,
                    radius,
                    0.5f, // 表示時間
                    hitConfirmed ? Color.red : hitboxColor // ヒット時は赤く表示
                );
            }
        }
    }
}