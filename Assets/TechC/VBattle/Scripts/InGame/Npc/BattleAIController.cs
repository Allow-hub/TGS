using System.Collections;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 対戦AI制御クラス
    /// </summary>
    public class BattleAIController : MonoBehaviour
    {
        [Header("AI設定")]
        private Transform opponent; // 相手のTransform
        [SerializeField] private NpcInputManager inputManager; // 入力管理
        [SerializeField] private BattleAIStrategy strategy; // 戦略管理

        [Header("行動設定")]
        [SerializeField] private float actionInterval = 0.5f; // 行動間隔
        [SerializeField] private float reactionTime = 0.1f; // 反応時間

        [Header("デバッグ")]
        [SerializeField] private bool showDebugInfo = true;

        [Header("難易度")]
        [SerializeField] private EnemyDifficulty difficulty = EnemyDifficulty.Easy;

        [Header("【重要】各行動の時間（※難易度をDEBUGにすることで反映）")]
        [Tooltip("接近行動の継続時間（秒）")]
        [SerializeField] private float approachTime = 0.3f;      // 接近
        [Tooltip("後退行動の継続時間（秒）")]
        [SerializeField] private float retreatTime = 0.3f;       // 後退
        [Tooltip("弱攻撃の入力継続時間（秒）")]
        [SerializeField] private float weakAttackTime = 0.15f;   // 弱攻撃
        [Tooltip("強攻撃の入力継続時間（秒）")]
        [SerializeField] private float strongAttackTime = 0.3f;  // 強攻撃
        [Tooltip("ガードの継続時間（秒）")]
        [SerializeField] private float guardTime = 0.3f;         // ガード
        [Tooltip("ジャンプの入力継続時間（秒）")]
        [SerializeField] private float jumpTime = 0.12f;         // ジャンプ
        [Tooltip("しゃがみの継続時間（秒）")]
        [SerializeField] private float crouchTime = 0.25f;       // しゃがみ
        [Tooltip("待機行動の継続時間（秒）")]
        [SerializeField] private float waitTime = 0.25f;         // 待機

        [Header("攻撃方向の確率（通常時）")]
        [Tooltip("左方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseLeftPercent = 25f;

        [Tooltip("右方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseRightPercent = 25f;

        [Tooltip("上方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseUpPercent = 25f;

        [Tooltip("下方向への攻撃確率（通常時）")]
        [SerializeField, ReadOnly] private float baseDownPercent = 25f;

        // 攻撃方向の確率（優遇時）
        [Header("攻撃方向の確率（優遇時）")]

        [SerializeField] private float preferLeftPercent = 40f;
        [SerializeField] private float preferRightPercent = 40f;
        [SerializeField] private float lessLeftPercent = 10f;
        [SerializeField] private float lessRightPercent = 10f;

        private float lastActionTime;
        private BattleRange currentRange;
        private AIActionType currentAction;
        private bool isExecutingAction;

        private void Start()
        {
            if (inputManager == null)
                inputManager = GetComponent<NpcInputManager>();

            if (strategy == null)
                strategy = GetComponent<BattleAIStrategy>();

            if (opponent == null)
            {
                //二人対戦の場合1p(人)対2p（AI）の構図になる
                opponent = BattleJudge.I.GetPlayerObjById(1).transform;
            }

            ApplyDifficultySettings();
        }

        private void Update()
        {
            if (opponent == null || inputManager == null || strategy == null)
                return;

            UpdateBattleRange();

            if (Time.time - lastActionTime >= actionInterval && !isExecutingAction)
            {
                ExecuteAIAction();
                lastActionTime = Time.time;
            }
        }

        /// <summary>
        /// 戦闘距離を更新
        /// </summary>
        private void UpdateBattleRange()
        {
            float distance = Vector3.Distance(transform.position, opponent.position);
            //距離に応じて重みの変化
            currentRange = strategy.GetBattleRange(distance);
        }

        /// <summary>
        /// AIの行動を実行
        /// </summary>
        private void ExecuteAIAction()
        {
            currentAction = strategy.SelectAction(currentRange);
            CustomLogger.Info($"{currentAction}", AIWeightUtility.NPCLOGTAG);
            StartCoroutine(PerformAction(currentAction));
        }

        /// <summary>
        /// 選択された行動を実行
        /// </summary>
        private IEnumerator PerformAction(AIActionType actionType)
        {
            isExecutingAction = true;

            // 反応時間の遅延
            yield return new WaitForSeconds(reactionTime);

            switch (actionType)
            {
                case AIActionType.Approach:
                    yield return StartCoroutine(PerformApproach());
                    break;

                case AIActionType.Retreat:
                    yield return StartCoroutine(PerformRetreat());
                    break;

                case AIActionType.Attack:
                    yield return StartCoroutine(PerformAttack());
                    break;

                case AIActionType.Guard:
                    yield return StartCoroutine(PerformGuard());
                    break;

                case AIActionType.Jump:
                    yield return StartCoroutine(PerformJump());
                    break;

                case AIActionType.Crouch:
                    yield return StartCoroutine(PerformCrouch());
                    break;

                case AIActionType.Wait:
                    yield return StartCoroutine(PerformWait());
                    break;
            }

            isExecutingAction = false;
        }

        /// <summary>
        /// 接近行動
        /// </summary>
        private IEnumerator PerformApproach()
        {
            Vector2 direction = GetDirectionToOpponent();
            inputManager.OnMove(direction, true, false);

            yield return new WaitForSeconds(approachTime);

            inputManager.OnMove(Vector2.zero, false, true);
        }

        /// <summary>
        /// 後退行動
        /// </summary>
        private IEnumerator PerformRetreat()
        {
            Vector2 direction = -GetDirectionToOpponent();
            inputManager.OnMove(direction, true, false);

            yield return new WaitForSeconds(retreatTime);

            inputManager.OnMove(Vector2.zero, false, true);
        }

        /// <summary>
        /// 攻撃行動
        /// </summary>
        private IEnumerator PerformAttack()
        {
            const float WEAK_ATTACK_CHANCE = 0.7f;

            // ランダムで弱攻撃か強攻撃を選択
            if (Random.Range(0f, 1f) < WEAK_ATTACK_CHANCE)
            {
                Vector2 direction = GetAttackDirection();
                inputManager.OnMove(direction, true, false);

                inputManager.OnWeakAttack(true, false);
                yield return new WaitForSeconds(weakAttackTime);
                inputManager.OnWeakAttack(false, true);
                inputManager.OnMove(Vector2.zero, false, true);
            }
            else
            {
                Vector2 direction = GetAttackDirection();
                inputManager.OnMove(direction, true, false);

                inputManager.OnStrongAttack(true, false);
                yield return new WaitForSeconds(strongAttackTime);
                inputManager.OnStrongAttack(false, true);
                inputManager.OnMove(Vector2.zero, false, true);
            }
        }

        /// <summary>
        /// ガード行動
        /// </summary>
        private IEnumerator PerformGuard()
        {
            inputManager.OnGuard(true, false);
            yield return new WaitForSeconds(guardTime);
            inputManager.OnGuard(false, true);
        }

        /// <summary>
        /// ジャンプ行動
        /// </summary>
        private IEnumerator PerformJump()
        {
            inputManager.OnJump(true, false);
            float attackDelay = jumpTime * 0.5f; // ジャンプ中盤で攻撃入力
            yield return new WaitForSeconds(attackDelay);
            // ジャンプ中に上下攻撃を一定確率で発動
            if (Random.value < 0.6f) // 難易度で確率調整も可
            {
                Vector2 dir = Vector2.up;
                inputManager.OnMove(dir, true, false);
                if (Random.value < 0.7f)
                {
                    inputManager.OnWeakAttack(true, false);
                    yield return new WaitForSeconds(weakAttackTime);
                    inputManager.OnWeakAttack(false, true);
                }
                else
                {
                    inputManager.OnStrongAttack(true, false);
                    yield return new WaitForSeconds(strongAttackTime);
                    inputManager.OnStrongAttack(false, true);
                }
                inputManager.OnMove(Vector2.zero, false, true);
            }
            yield return new WaitForSeconds(jumpTime - attackDelay);
            inputManager.OnJump(false, true);
        }

        /// <summary>
        /// しゃがみ行動
        /// </summary>
        private IEnumerator PerformCrouch()
        {
            inputManager.OnCrouch(true, false);
            float attackDelay = crouchTime * 0.5f; // しゃがみ中盤で攻撃入力
            yield return new WaitForSeconds(attackDelay);
            // しゃがみ中に下攻撃を一定確率で発動
            if (Random.value < 0.6f)
            {
                Vector2 dir = Vector2.down;
                inputManager.OnMove(dir, true, false);
                if (Random.value < 0.7f)
                {
                    inputManager.OnWeakAttack(true, false);
                    yield return new WaitForSeconds(weakAttackTime);
                    inputManager.OnWeakAttack(false, true);
                }
                else
                {
                    inputManager.OnStrongAttack(true, false);
                    yield return new WaitForSeconds(strongAttackTime);
                    inputManager.OnStrongAttack(false, true);
                }
                inputManager.OnMove(Vector2.zero, false, true);
            }
            yield return new WaitForSeconds(crouchTime - attackDelay);
            inputManager.OnCrouch(false, true);
        }

        /// <summary>
        /// 待機行動
        /// </summary>
        private IEnumerator PerformWait()
        {
            yield return new WaitForSeconds(waitTime);
        }

        /// <summary>
        /// 相手への方向ベクトルを取得
        /// </summary>
        private Vector2 GetDirectionToOpponent()
        {
            Vector3 direction = opponent.position - transform.position;
            return new Vector2(Mathf.Sign(direction.x), 0);
        }

        /// <summary>
        /// 攻撃する方向ベクトルをランダムに取得する
        /// </summary>
        /// <returns></returns>
        private Vector2 GetAttackDirection()
        {
            float dx = opponent.position.x - transform.position.x;

            /* 初期値として設定 */
            float leftPercent = baseLeftPercent;
            float rightPercent = baseRightPercent;
            float upPercent = baseUpPercent;
            float downPercent = baseDownPercent;

            if (dx < 0)
            {
                leftPercent = preferLeftPercent;
                rightPercent = lessRightPercent;
            }
            else if (dx > 0)
            {
                rightPercent = preferRightPercent;
                leftPercent = lessLeftPercent;
            }

            float total = leftPercent + rightPercent + upPercent + downPercent;

            /* 0〜totalの中からランダムな値を取得 */
            float rand = Random.Range(0f, total);

            if (rand < leftPercent) return Vector2.left;
            rand -= leftPercent;
            if (rand < rightPercent) return Vector2.right;
            rand -= rightPercent;
            if (rand < upPercent) return Vector2.up;
            return Vector2.down;
        }

        /// <summary>
        /// 基本方向を取得
        /// </summary>
        private Vector2 GetBaseDirection(float leftPercent, float rightPercent, float upPercent, float downPercent)
        {
            float total = leftPercent + rightPercent + upPercent + downPercent;
            float rand = Random.Range(0f, total);

            if (rand < leftPercent) return Vector2.left;
            rand -= leftPercent;
            if (rand < rightPercent) return Vector2.right;
            rand -= rightPercent;
            if (rand < upPercent) return Vector2.up;
            return Vector2.down;
        }

        /// <summary>
        /// 難易度に応じて、CPUのパラメータを変更する
        /// </summary>

        private void ApplyDifficultySettings()
        {
            switch (difficulty)
            {
                case EnemyDifficulty.Debug:
                    break;
                case EnemyDifficulty.Easy:
                    actionInterval = 0.8f;
                    reactionTime = 0.3f;
                    approachTime = 0.4f;
                    retreatTime = 0.4f;
                    weakAttackTime = 0.18f;
                    strongAttackTime = 0.35f;
                    guardTime = 0.35f;
                    jumpTime = 0.22f;
                    crouchTime = 0.36f;
                    waitTime = 0.35f;
                    strategy.SetPersonality(0.7f, 1.2f, 0.8f);
                    break;
                case EnemyDifficulty.Normal:
                    actionInterval = 0.5f;
                    reactionTime = 0.1f;
                    approachTime = 0.3f;
                    retreatTime = 0.3f;
                    weakAttackTime = 0.15f;
                    strongAttackTime = 0.3f;
                    guardTime = 0.3f;
                    jumpTime = 0.16f;
                    crouchTime = 0.28f;
                    waitTime = 0.25f;
                    strategy.SetPersonality(1.0f, 1.0f, 1.0f);
                    break;
            }
        }
    }
}
