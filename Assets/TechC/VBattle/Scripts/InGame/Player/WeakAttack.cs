using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TechC.Player;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    [Serializable]
    public class WeakAttack : MonoBehaviour, IAttackBase
    {
        [Header("Reference")]
        [SerializeField] protected Player.CharacterController characterController;
        [SerializeField] private CommandHistory commandHistory;
        [SerializeField] private ComboSystem comboSystem;
        [Header("Data")]
        [SerializeField]
        private AttackSet attackSet;
        [SerializeField]
        protected AttackData neutralAttackData_1, neutralAttackData_2, neutralAttackData_3;
        [SerializeField]
        protected AttackData leftAttackData;
        [SerializeField]
        protected AttackData rightAttackData;
        [SerializeField]
        protected AttackData downAttackData;
        [SerializeField]
        protected AttackData upAttackData;

        [SerializeField] private float nWeakAttackInterval = 1.0f;
        [SerializeField] private bool showAttackGizmos = true;
        [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.5f); // 半透明の赤
        private Vector3 lastAttackPosition;
        private float lastAttackRadius;
        private bool isAttacking = false;
        private int neutralAttackCount = 0;
        private Player.CharacterController opponentCharacterController;
        private readonly Dictionary<AttackType, AttackData> attackDataMap;

        public WeakAttack()
        {
            attackDataMap = new Dictionary<AttackType, AttackData>();
        }
        public void OnValidate()
        {
            if (attackSet == null) return;

            neutralAttackData_1 = attackSet.weakNeutral_1;
            neutralAttackData_2 = attackSet.weakNeutral_2;
            neutralAttackData_3 = attackSet.weakNeutral_3;
            leftAttackData = attackSet.weakLeft;
            rightAttackData = attackSet.weakRight;
            downAttackData = attackSet.weakDown;
            upAttackData = attackSet.weakUp;
        }
        private void Start()
        {
            InitializeAttackDataMap();
        }
        private void InitializeAttackDataMap()
        {
            attackDataMap[AttackType.Neutral] = neutralAttackData_1;
            attackDataMap[AttackType.Left] = leftAttackData;
            attackDataMap[AttackType.Right] = rightAttackData;
            attackDataMap[AttackType.Down] = downAttackData;
            attackDataMap[AttackType.Up] = upAttackData;
        }
        public virtual void NeutralAttack()
        {
            if (commandHistory.WasCommandExecutedRecently<AttackCommand>(nWeakAttackInterval))
            {
                neutralAttackCount++;
                switch (neutralAttackCount)
                {
                    case 1:
                        ExecuteAttack(neutralAttackData_1);
                        break;
                    case 2:
                        ExecuteAttack(neutralAttackData_2);
                        break;
                    case 3:
                        ExecuteAttack(neutralAttackData_3);
                        neutralAttackCount = 0; // コンボ終了でリセット
                        break;
                    default:
                        neutralAttackCount = 0;
                        break;
                }
            }
            else
            {
                neutralAttackCount = 1;
                ExecuteAttack(neutralAttackData_1);
            }
        }

        public virtual void LeftAttack()
        {
            ExecuteAttack(leftAttackData);
        }

        public virtual void RightAttack()
        {
            ExecuteAttack(rightAttackData);
        }

        public virtual void DownAttack()
        {
            ExecuteAttack(downAttackData);
        }

        public virtual void UpAttack()
        {
            ExecuteAttack(upAttackData);
        }

        protected virtual void ExecuteAttack(AttackData attackData)
        {
            characterController.GetAnim().speed = attackData.animationSpeed;
            characterController.SetAnim(attackData.animHash, true);
            StartCoroutine(DelayAttack(attackData));
            StartCoroutine(EndAttack(attackData));
            Debug.Log(attackData.attackName);
        }

        /// <summary>
        /// アニメーションの秒数分経過したら自動でfalseに
        /// </summary>
        /// <param name="attackData"></param>
        /// <returns></returns>
        private IEnumerator EndAttack(AttackData attackData)
        {
            yield return new WaitForSeconds(attackData.attackDuration);
            characterController.SetAnim(attackData.animHash, false);
            characterController.GetAnim().speed = characterController.DefaultAnimSpeed;
        }

    
        private IEnumerator DelayAttack(AttackData attackData)
        {
            yield return new WaitForSeconds(attackData.hitTiming);
            // ヒットチェックを実行し、ヒットした場合にヒットストップを発生させる
            if (PerformAttackHitCheck(attackData))
            {
                // ヒットストップを実行
                HitStopManager.I.DoHitStop(attackData.hitStopDuration, attackData.hitStopTimeScale); // 弱攻撃の場合はfalse
            }
        }

        // ヒットチェックの統括メソッド
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
                // アピール後のチャージ状態でヒットした場合、ゲージを増加
                //CheckAndAddGauge(attackData);
                comboSystem.CheckCombos();
                // コンボ履歴を検証し、特別なコンボの場合は追加ボーナスを付与
                //CheckForSpecialCombos();
            }
            // ヒットボックスの可視化
            VisualizeHitbox(attackPosition, attackData.radius, hitConfirmed);

            return hitConfirmed;
        }

        /// <summary>
        /// 攻撃位置の計算
        /// </summary>
        /// <param name="attackData"></param>
        /// <returns></returns>
        private Vector3 CalculateAttackPosition(AttackData attackData)
        {
            // キャラクターの向いている方向を考慮
            Vector3 attackDirection = characterController.transform.forward;

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
        /// <param name="position"></param>
        /// <param name="radius"></param>
        private void UpdateDebugInfo(Vector3 position, float radius)
        {
            lastAttackPosition = position;
            lastAttackRadius = radius;
            isAttacking = true;
        }

        /// <summary>
        /// 自分のコライダーかどうかをチェック
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private bool IsOwnCollider(Collider collider)
        {
            return collider.gameObject.GetMostParentComponent<Transform>() == characterController.transform;
        }

        /// <summary>
        /// 対戦相手のコントローラーを取得
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        private Player.CharacterController GetOpponentController(Collider collider)
        {
            if (opponentCharacterController == null)
                opponentCharacterController = collider.gameObject.transform.parent.GetComponent<Player.CharacterController>();

            return opponentCharacterController;
        }

        /// <summary>
        /// ガード処理を試行
        /// </summary>
        /// <param name="targetController"></param>
        /// <param name="hitCollider"></param>
        /// <param name="attackData"></param>
        /// <returns></returns>
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
        /// <param name="hitCollider"></param>
        /// <param name="attackData"></param>
        /// <returns></returns>
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
        /// <param name="hitCollider"></param>
        /// <param name="attackData"></param>
        private void ProcessHitStun(Collider hitCollider, AttackData attackData)
        {
            if (hitCollider.gameObject.transform.parent.TryGetComponent<Player.CharacterController>(out Player.CharacterController characterController))
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
            }
        }

        /// <summary>
        /// ヒットボックスの可視化
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="hitConfirmed"></param>
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

        public float GetDuration(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Neutral:
                    return neutralAttackData_1.attackDuration;
                case AttackType.Left:
                    return leftAttackData.attackDuration;
                case AttackType.Right:
                    return rightAttackData.attackDuration;
                case AttackType.Down:
                    return downAttackData.attackDuration;
                case AttackType.Up:
                    return upAttackData.attackDuration;
                default:
                    Debug.LogWarning("未定義のAttackTypeが指定されました");
                    return 0f;
            }
        }
        public AttackData GetAttackData(AttackType attackType)
        {
            if (attackDataMap.TryGetValue(attackType, out var attackData))
            {
                return attackData;
            }

            Debug.LogWarning("未定義のAttackTypeが指定されました");
            return neutralAttackData_1;
        }
        public virtual void ForceFinish()
        {
        }

        private void CheckAndAddGauge(AttackData attackData)
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

        //// コンボ検証メソッド
        //private void CheckForSpecialCombos()
        //{
        //    // コマンド履歴から特定のコンボパターンを検出
        //    if (commandHistory == null) return;

        //    // 例: 3連続攻撃コンボの検出
        //    if (IsThreeAttackCombo())
        //    {
        //        // 特別ボーナスを付与
        //        characterController.NotBoolAddSpecialGauge(10f);
        //        Debug.Log("3連続攻撃コンボ達成! ボーナスゲージ獲得!");
        //    }

        //    // 例: 特殊な方向コンボ（下→中立→上）の検出
        //    if (IsDirectionalCombo())
        //    {
        //        characterController.NotBoolAddSpecialGauge(15f);
        //        Debug.Log("方向コンボ達成! 大きなボーナスゲージ獲得!");
        //    }
        //}

        //// 3連続攻撃コンボの検出
        //private bool IsThreeAttackCombo()
        //{
        //    var history = commandHistory.GetFullHistory(3);
        //    if (history.Count < 3) return false;

        //    // 全てのコマンドが攻撃で、全て成功していることを確認
        //    return history.All(record =>
        //        (record.commandName.Contains("Attack") || record.commandName.Contains("Appeal"))
        //        && record.wasSuccessful);
        //}

        //// 方向コンボの検出
        //private bool IsDirectionalCombo()
        //{
        //    // Type配列を使って特定のコマンドシーケンスをチェック
        //    return commandHistory.CheckCombo(
        //        new Type[] {
        //    //typeof(DownAttackCommand),
        //    typeof(AttackCommand),  // 中立攻撃
        //    //typeof(UpAttackCommand)
        //        },
        //        2.0f  // コマンド間の最大許容時間
        //    );
        //}

        // エフェクト表示用（オプション）
        private void ShowChargeEffect(float amount)
        {
            // チャージエフェクトを表示するコード
            // 例: パーティクルシステムの再生など
            Debug.Log($"ゲージチャージ! +{amount * 0.5f}");
        }
    }
}