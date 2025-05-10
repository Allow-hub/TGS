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

        /// <summary>
        /// 攻撃処理を実行する
        /// </summary>
        public IEnumerator ProcessAttack(AttackData attackData, MonoBehaviour coroutineRunner)
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
            // 攻撃位置の計算
            Vector3 attackPosition = CalculateAttackPosition(attackData);
            // デバッグ用に情報を保存
            UpdateDebugInfo(attackPosition, attackData.radius);

            // 攻撃範囲内のコライダーを検出
            Collider[] hitColliders = Physics.OverlapSphere(attackPosition, attackData.radius, attackData.targetLayers);

            bool hitConfirmed = false;
            foreach (var hitCollider in hitColliders)
            {
                // 自分自身は除外
                if (IsOwnCollider(hitCollider))
                    continue;

                // 対戦相手のコントローラーを取得
                Player.CharacterController targetController = GetOpponentController(hitCollider);
                if (targetController == null) continue;
                // BattleJudgeを通じて攻撃対象が有効かどうかを確認
                // if (battleJudge != null && !battleJudge.IsValidAttackTarget(targetController.PlayerID))
                // {
                //     Debug.Log($"Player {targetController.PlayerID} は現在攻撃対象として無効です");
                //     continue;
                // }

                // ガード処理
                if (TryProcessGuard(targetController, hitCollider, attackData))
                {
                    hitConfirmed = true;
                    continue;
                }

                // ダメージ処理
                if (TryProcessDamage(hitCollider, attackData))
                {
                    hitConfirmed = true;
                    // ヒットスタン処理
                    ProcessHitStun(hitCollider, attackData);
                }
            }

            if (hitConfirmed)
            {
                // ヒットエフェクトを生成し、変数に保存
                currentHitEffect = objectPool.GetObject(hitEffectPrefab);
                // Debug.Log(currentHitEffect);
                // ヒット位置にエフェクトを配置
                if (currentHitEffect != null)
                {
                    currentHitEffect.transform.position = attackPosition;
                    
                    // 非同期でエフェクトを返却
                    ReturnEffect().Forget();
                }
                
                comboSystem?.CheckCombos();
            }

            // ヒットボックスの可視化
            VisualizeHitbox(attackPosition, attackData.radius, hitConfirmed);
            
            return hitConfirmed;
        }

        /// <summary>
        /// ヒットエフェクトを一定時間後にプールに戻す非同期メソッド
        /// </summary>
        private async UniTask ReturnEffect()
        {
            if (currentHitEffect == null) return;
            
            // 指定された時間待機
            await UniTask.Delay(TimeSpan.FromSeconds(hitEffectDuration));
            
            // エフェクトをプールに戻す
            if (currentHitEffect != null)
            {
                objectPool.ReturnObject(currentHitEffect);
                currentHitEffect = null;
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
            if (opponentCharacterController == null)
                opponentCharacterController = collider.gameObject.transform.parent.GetComponent<Player.CharacterController>();

            return opponentCharacterController;
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

        /// <summary>
        /// ゲージ増加処理
        /// </summary>
        public void CheckAndAddGauge(AttackData attackData)
        {
            // チャージ可能状態かチェック
            if (characterController.IsChargeEnabled())
            {
                // 基本チャージ量（攻撃の種類や威力に応じて変動可能）
                float chargeAmount = attackData.damage * 0.5f;

                // ゲージ増加処理
                characterController.NotBoolAddSpecialGauge(chargeAmount);

                // 必要に応じてエフェクト表示
                ShowChargeEffect(attackData.damage);
            }
        }

        // エフェクト表示用
        private void ShowChargeEffect(float amount)
        {
            // チャージエフェクトを表示するコード
            Debug.Log($"ゲージチャージ! +{amount * 0.5f}");
        }
    }
}