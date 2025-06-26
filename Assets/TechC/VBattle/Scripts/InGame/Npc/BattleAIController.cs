using System.Collections;
using System.Collections.Generic;
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

            yield return new WaitForSeconds(0.3f);

            inputManager.OnMove(Vector2.zero, false, true);
        }

        /// <summary>
        /// 後退行動
        /// </summary>
        private IEnumerator PerformRetreat()
        {
            Vector2 direction = -GetDirectionToOpponent();
            inputManager.OnMove(direction, true, false);

            yield return new WaitForSeconds(0.3f);

            inputManager.OnMove(Vector2.zero, false, true);
        }

        /// <summary>
        /// 攻撃行動
        /// </summary>
        private IEnumerator PerformAttack()
        {
            // ランダムで弱攻撃か強攻撃を選択
            if (Random.Range(0f, 1f) < 0.7f)
            {
                Vector2 direction = GetDirectionToOpponent();
                inputManager.OnMove(direction, true, false);
                //攻撃はInputManagerのMoveInputの値によって攻撃派生をしているので
                //攻撃の直前でどの派生かを選んでください
                inputManager.OnWeakAttack(true, false);
                yield return new WaitForSeconds(0.2f);
                inputManager.OnWeakAttack(false, true);
                inputManager.OnMove(Vector2.zero, false, true);

            }
            else
            {
                Vector2 direction = GetDirectionToOpponent();
                inputManager.OnMove(direction, true, false);

                inputManager.OnStrongAttack(true, false);
                yield return new WaitForSeconds(0.3f);
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
            yield return new WaitForSeconds(0.5f);
            inputManager.OnGuard(false, true);
        }

        /// <summary>
        /// ジャンプ行動
        /// </summary>
        private IEnumerator PerformJump()
        {
            inputManager.OnJump(true, false);
            yield return new WaitForSeconds(0.1f);
            inputManager.OnJump(false, true);
        }

        /// <summary>
        /// しゃがみ行動
        /// </summary>
        private IEnumerator PerformCrouch()
        {
            inputManager.OnCrouch(true, false);
            yield return new WaitForSeconds(0.3f);
            inputManager.OnCrouch(false, true);
        }

        /// <summary>
        /// 待機行動
        /// </summary>
        private IEnumerator PerformWait()
        {
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 相手への方向ベクトルを取得
        /// </summary>
        private Vector2 GetDirectionToOpponent()
        {
            Vector3 direction = opponent.position - transform.position;
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
    }
}
