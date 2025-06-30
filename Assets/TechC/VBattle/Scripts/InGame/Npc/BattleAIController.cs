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

        [Header("各行動の時間")]
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

                // ここで攻撃方向をログ出力
                CustomLogger.Info($"[Npc] WeakAttack Direction: {DirectionToString(direction)}", AIWeightUtility.NPCLOGTAG);

                inputManager.OnWeakAttack(true, false);
                yield return new WaitForSeconds(weakAttackTime);
                inputManager.OnWeakAttack(false, true);
                inputManager.OnMove(Vector2.zero, false, true);

            }
            else
            {
                Vector2 direction = GetAttackDirection();
                inputManager.OnMove(direction, true, false);

                // ここで攻撃方向をログ出力
                CustomLogger.Info($"[Npc] StrongAttack Direction: {DirectionToString(direction)}", AIWeightUtility.NPCLOGTAG);

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
            yield return new WaitForSeconds(jumpTime);
            inputManager.OnJump(false, true);
        }

        /// <summary>
        /// しゃがみ行動
        /// </summary>
        private IEnumerator PerformCrouch()
        {
            inputManager.OnCrouch(true, false);
            yield return new WaitForSeconds(crouchTime);
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
            const int DIRECTION_COUNT = 4;
            int dir = Random.Range(0, DIRECTION_COUNT);

            switch (dir)
            {
                case 0: return Vector2.right;   // 右
                case 1: return Vector2.left;    // 左
                case 2: return Vector2.up;      // 上
                case 3: return Vector2.down;    // 下
                default: return Vector2.right;
            }
        }

        /* ===============================
         * TODO: 攻撃方向を分かりやすくする補助メソッド
         * 不要になったら削除すること
         * =============================== */
        private string DirectionToString(Vector2 dir)
        {
            if (dir == Vector2.right) return "Right";
            if (dir == Vector2.left) return "Left";
            if (dir == Vector2.up) return "Up";
            if (dir == Vector2.down) return "Down";
            return dir.ToString();
        }
    }
}
