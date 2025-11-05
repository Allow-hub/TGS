using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 投げ攻撃：攻撃者のみを一時固定し、床にスナップ & コライダー無効化。
    /// 解除タイミングは DelayUtility（ポーズ停止対応）で管理。
    /// ノックバック等は他システムに委ね、ここでは一切行わない。
    /// </summary>
    [Serializable]
    public class AttackTeramiThrow : IAttackBehaviour
    {
        [Header("Config")]
        [SerializeField] private AttackData attackData;
        [SerializeField] private LayerMask groundMask = default;
        [SerializeField] private float pinSeconds = 0.5f;        // 固定時間
        [SerializeField] private bool pinOnActivate = false;     // 攻撃開始直後に固定するか（通常はfalse）
        [SerializeField] private bool pinOnHit = true;           // ヒット時のみ固定するか（通常はtrue）
        [SerializeField] private bool preventReentryWhilePinned = true; // 固定中の再入防止

        // 実行基盤
        private GameObject attackerRoot;
        private MonoBehaviour runner; // AttackObjectController を想定
        private bool isPinned = false;
        private Coroutine delayHandle = null;

        // 復帰用
        private readonly List<Collider> disabledColliders = new();
        private Rigidbody pinnedRb = null;
        private RigidbodyConstraints originalConstraints = RigidbodyConstraints.None;

        // ===== IAttackBehaviour =====
        public void Initialize(GameObject ownerAttackObject)
        {
            runner = ownerAttackObject.GetComponent<AttackObjectController>();
            if (!runner) runner = ownerAttackObject.GetComponent<MonoBehaviour>(); // 最低限の保険

            if (groundMask.value == 0)
                groundMask = LayerMask.GetMask("Ground");
        }

        public void OnRelease()
        {
            StopCurrentAction(); // 無効化時などに必ず完全復帰
        }

        public void OnUpdate(float dt) { }

        public void Activate(GameObject character)
        {
            attackerRoot = character;
            if (!runner || !attackerRoot) return;

            if (pinOnActivate)
            {
                StopCurrentAction();
                BeginPin(attackerRoot, pinSeconds);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!pinOnHit) return;
            if (!runner || !attackerRoot || !other) return;
            if (isPinned && preventReentryWhilePinned) return;

            // 自分自身は無視
            var victimRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.transform.root.gameObject;
            if (!victimRoot || victimRoot == attackerRoot) return;

            // ここでは victim には一切触れない（ノックバック等は別システムに任せる）
            StopCurrentAction();
            BeginPin(attackerRoot, pinSeconds);
        }

        // ===== 実装詳細 =====
        private void BeginPin(GameObject attacker, float duration)
        {
            if (!attacker) return;
            if (preventReentryWhilePinned && isPinned) return;

            isPinned = true;

            // Rigidbody 先処理（床スナップ→完全固定）
            pinnedRb = attacker.GetComponent<Rigidbody>();
            if (pinnedRb != null)
            {
                originalConstraints = pinnedRb.constraints;

                // 床にスフィアキャストでスナップ（精度重視）
                if (Physics.SphereCast(attacker.transform.position + Vector3.up * 0.5f, 0.2f,
                                       Vector3.down, out var hit, 5f, groundMask))
                {
                    var pos = attacker.transform.position;
                    pos.y = hit.point.y;
                    attacker.transform.position = pos;
                }

                // 完全固定
                pinnedRb.velocity = Vector3.zero;
                pinnedRb.angularVelocity = Vector3.zero;
                pinnedRb.useGravity = false;
                pinnedRb.constraints = RigidbodyConstraints.FreezeAll;
            }

            // 次にコライダー全OFF（必要なら除外リストを設けて調整）
            DisableAllColliders(attacker);

            delayHandle = TechC.DelayUtility.StartDelayedActionWithPause(
                runner,
                duration,
                // ポーズ判定はプロジェクト都合で差し替え
                () => Time.timeScale == 0f,
                OnDelayFinishedRestore
            );
        }

        private void OnDelayFinishedRestore()
        {
            RestoreColliders();

            if (pinnedRb != null)
            {
                pinnedRb.useGravity = true;
                pinnedRb.constraints = originalConstraints; // 完全復元
                pinnedRb = null;
            }

            isPinned = false;
            delayHandle = null;
        }

        private void StopCurrentAction()
        {
            // 遅延解除のキャンセル
            if (delayHandle != null && runner)
            {
                runner.StopCoroutine(delayHandle);
                delayHandle = null;
            }

            // 復帰（未復帰ならここで戻す）
            RestoreColliders();

            if (pinnedRb != null)
            {
                pinnedRb.useGravity = true;
                pinnedRb.constraints = originalConstraints;
                pinnedRb = null;
            }

            isPinned = false;
        }

        private void DisableAllColliders(GameObject target)
        {
            disabledColliders.Clear();
            var cols = target.GetComponentsInChildren<Collider>(true);
            foreach (var col in cols)
            {
                if (col && col.enabled)
                {
                    col.enabled = false;
                    disabledColliders.Add(col);
                }
            }
        }

        private void RestoreColliders()
        {
            if (disabledColliders.Count == 0) return;
            foreach (var col in disabledColliders)
            {
                if (col) col.enabled = true;
            }
            disabledColliders.Clear();
        }
    }
}
