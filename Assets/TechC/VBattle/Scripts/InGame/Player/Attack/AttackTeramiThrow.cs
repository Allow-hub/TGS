using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 投げ攻撃：攻撃者を固定し、コライダーを無効化する
    /// </summary>
    [Serializable]
    public class AttackTeramiThrow : IAttackBehaviour
    {
        [SerializeField] private AttackData attackData;
        [SerializeField] private LayerMask groundMask = default;

        private GameObject attackerRoot; // 攻撃者のルートオブジェクト
        private MonoBehaviour runner; // コルーチン実行用
        private const float PinSecs = 0.5f; // 固定時間
        private bool isPinned = false; // 現在固定中かどうか
        private Coroutine currentCoroutine = null; // 実行中のコルーチン
        private List<Collider> disabledColliders = new List<Collider>(); // 無効化したコライダーリスト

        /// <summary>
        /// 攻撃オブジェクトの初期化
        /// </summary>
        /// <param name="ownerAttackObject">攻撃オブジェクト</param>
        public void Initialize(GameObject ownerAttackObject)
        {
            // コルーチン実行用のコンポーネントを取得
            runner = ownerAttackObject.GetComponent<AttackObjectController>();
            
            // 地面マスクが未設定の場合はデフォルト設定
            if (groundMask.value == 0) groundMask = LayerMask.GetMask("Ground");
        }

        /// <summary>
        /// オブジェクト解放時の処理
        /// </summary>
        public void OnRelease() 
        { 
            // 実行中の処理を停止し、状態を復元
            StopCurrentAction();
        }
        
        /// <summary>
        /// 毎フレーム更新（未使用）
        /// </summary>
        /// <param name="dt">デルタタイム</param>
        public void OnUpdate(float dt) { }

        /// <summary>
        /// 攻撃開始時の処理
        /// </summary>
        /// <param name="character">攻撃者キャラクター</param>
        public void Activate(GameObject character)
        {
            attackerRoot = character;
            if (runner != null && attackerRoot != null)
            {
                // 既存の処理があれば停止
                StopCurrentAction();
                
                // 固定状態を開始
                isPinned = true;
                currentCoroutine = runner.StartCoroutine(PinAndDisable(attackerRoot, PinSecs));
            }
        }

        /// <summary>
        /// 他オブジェクトとの衝突判定
        /// </summary>
        /// <param name="other">衝突したコライダー</param>
        public void OnTriggerEnter(Collider other)
        {
            // 無効状態や固定中は処理しない
            if (attackerRoot == null || other == null || runner == null || isPinned) return;

            // 衝突相手のルートオブジェクトを取得
            var victimRoot = other.attachedRigidbody?.gameObject ?? other.transform.root.gameObject;
            
            // 無効オブジェクトや自分自身は除外
            if (!victimRoot || victimRoot == attackerRoot) return;

            // 新しい攻撃として処理開始
            StopCurrentAction();
            isPinned = true;
            currentCoroutine = runner.StartCoroutine(PinAndDisable(attackerRoot, PinSecs));
        }

        /// <summary>
        /// 現在の処理を停止し、状態を復元する
        /// </summary>
        private void StopCurrentAction()
        {
            // 実行中のコルーチンを停止
            if (currentCoroutine != null && runner != null)
            {
                runner.StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
            
            // コライダーを復元し、固定状態を解除
            RestoreColliders();
            isPinned = false;
        }

        /// <summary>
        /// 無効化したコライダーを全て復元する
        /// </summary>
        private void RestoreColliders()
        {
            // 無効化したコライダーを有効に戻す
            foreach (var collider in disabledColliders)
            {
                if (collider) collider.enabled = true;
            }
            // リストをクリア
            disabledColliders.Clear();
        }

        public IEnumerable PinAndDisableNew(GameObject attacker, float duration)
        {
            // 攻撃者が無効なら終了
            if (!attacker) yield break;

            // 攻撃者のコライダーを無効化
            var colliders = attacker.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if (col && col.enabled)
                {
                    col.enabled = false; // コライダーを無効化
                    disabledColliders.Add(col); // 復元用リストに追加
                }
            }

            // 攻撃者のRigidbodyを取得して固定処理
            var rb = attacker.GetComponent<Rigidbody>();
            RigidbodyConstraints originalConstraints = RigidbodyConstraints.None;

            if (!rb)
            {
                // 元の制約を保存
                originalConstraints = rb.constraints;

                // 地面にスナップ（床に吸着）
                if (Physics.Raycast(attacker.transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 5f, groundMask))
                {
                    var pos = attacker.transform.position;
                    pos.y = hit.point.y; // Y座標を地面に合わせる
                    attacker.transform.position = pos;
                }

                // 物理状態を完全固定
                rb.velocity = Vector3.zero; // 速度をゼロに
                rb.angularVelocity = Vector3.zero; // 角速度をゼロに
                rb.useGravity = false; // 重力を無効化
                rb.constraints = RigidbodyConstraints.FreezeAll; // 全軸固定
            }

            yield return PinAndDisableCo(rb,attacker, duration);
        }

        private IEnumerator PinAndDisableCo(Rigidbody rb, GameObject attacker, float duration)
        {
           // 指定時間待機
            yield return new WaitForSeconds(duration);

            // --- 復元処理 ---
            // コライダーを復元
            RestoreColliders();

            // Rigidbodyの物理状態を復元
            if (rb)
            {
                rb.useGravity = true; // 重力を有効化
                // 通常の制約に戻す（Z軸移動とX,Y軸回転を禁止）
                rb.constraints = RigidbodyConstraints.FreezePositionZ |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;

            }

            // 状態をリセット
            isPinned = false;
 
        }

        /// <summary>
        /// 攻撃者を固定し、コライダーを無効化するコルーチン
        /// </summary>
        /// <param name="attacker">攻撃者オブジェクト</param>
        /// <param name="duration">固定時間</param>
        /// <returns>コルーチン</returns>
        private IEnumerator PinAndDisable(GameObject attacker, float duration)
        {
            // 攻撃者が無効なら終了
            if (!attacker) yield break;

            // 攻撃者のコライダーを無効化
            var colliders = attacker.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if (col && col.enabled)
                {
                    col.enabled = false; // コライダーを無効化
                    disabledColliders.Add(col); // 復元用リストに追加
                }
            }

            // 攻撃者のRigidbodyを取得して固定処理
            var rb = attacker.GetComponent<Rigidbody>();
            RigidbodyConstraints originalConstraints = RigidbodyConstraints.None;

            if (rb)
            {
                // 元の制約を保存
                originalConstraints = rb.constraints;

                // 地面にスナップ（床に吸着）
                if (Physics.Raycast(attacker.transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 5f, groundMask))
                {
                    var pos = attacker.transform.position;
                    pos.y = hit.point.y; // Y座標を地面に合わせる
                    attacker.transform.position = pos;
                }

                // 物理状態を完全固定
                rb.velocity = Vector3.zero; // 速度をゼロに
                rb.angularVelocity = Vector3.zero; // 角速度をゼロに
                rb.useGravity = false; // 重力を無効化
                rb.constraints = RigidbodyConstraints.FreezeAll; // 全軸固定
            }

            // 指定時間待機
            yield return new WaitForSeconds(duration);

            // --- 復元処理 ---
            // コライダーを復元
            RestoreColliders();

            // Rigidbodyの物理状態を復元
            if (rb)
            {
                rb.useGravity = true; // 重力を有効化
                // 通常の制約に戻す（Z軸移動とX,Y軸回転を禁止）
                rb.constraints = RigidbodyConstraints.FreezePositionZ |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;

            }

            // 状態をリセット
            isPinned = false;
            currentCoroutine = null;
        }
    }
}