using System;
using System.Collections;
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
        /// <summary>
        /// 強制終了時
        /// </summary>
        public virtual void ForceFinish()
        {
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
                if (hitCollider.gameObject.GetMostParentComponent<Transform>() == characterController.transform)
                    continue;

                if (characterController.GetCharacterState().IsGuardState())
                {
                    IGuardable guardable = hitCollider.gameObject.transform.parent.GetComponent<IGuardable>();
                    if (guardable != null)
                    {
                        guardable.GuardDamage(attackData.damage);
                        Debug.Log("対象がガード中です");
                        return true;
                    }
                }


                // ヒットしたオブジェクトにダメージを与える
                IDamageable target = hitCollider.gameObject.transform.parent.GetComponent<IDamageable>();
                if (target != null)
                {
                    //Debug.Log($"ヒット検出: {hitCollider.name}にダメージ {attackData.damage} を与えました");
                    target.TakeDamage(attackData.damage);
                    hitConfirmed = true;

                    //ヒットスタンのデータを生成
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


        public float GetDuration(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Neutral:
                    ///
                    ///ニュートラル攻撃の時間要修正
                    ///
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
    }
}
