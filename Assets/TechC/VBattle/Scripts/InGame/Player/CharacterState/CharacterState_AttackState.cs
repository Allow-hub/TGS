using IceMilkTea.StateMachine;
using System.Linq;
using System.Threading;
using TechC.Player.Attack;
using UnityEngine;
using System.Collections;

namespace TechC
{
    public partial class CharacterState
    {
        public enum AttackType
        {
            Neutral, // 他に入力がないとき
            Right,
            Left,
            Down,
            Up,
        }

        public enum AttackStrength
        {
            Weak,
            Strong,
            Air,
            Appeal
        }

        private class AttackState : ImtStateMachine<CharacterState>.State
        {
            // --- 定数と履歴 ---
            private const int PENALTY_THRESHOLD = 3; // 同じ攻撃繰り返し数
            private const float GAUGE_PENALTY = -5f; // ゲージ減少量

            private AttackType lastAttackType = AttackType.Neutral;
            private AttackStrength lastAttackStrength = AttackStrength.Weak;
            private int consecutiveAttackCount = 0;
            private AttackData lastAttackData = null;
            private float lastAttackTime;
            private GameObject lastAttackObject; // 前回の攻撃オブジェクトを保持

            // --- 現在の攻撃情報 ---
            private AttackType attackType;
            private AttackStrength attackStrength;
            private AttackData currentAttackData;
            private CancellationTokenSource attackCTS;
            private float duration;
            private float elapsedTime = 0;
            private bool isEarlyExit = true;
            private bool isCounter;
            private Coroutine appealCoroutine = null;
            private AttackData specialData;

            /// <summary>
            /// アピール中にダメージを受けた時に中断したいのでイベントをもらう
            /// </summary>
            protected internal override void Init()
            {
                Context.characterController.DamageEvent += () =>
                {
                    if (appealCoroutine != null)
                    {
                        Context.characterController.StopCoroutine(appealCoroutine);
                        appealCoroutine = null;
                    }
                };
            }

            protected internal override void Enter()
            {
                Context.characterController.SetAnim(AnimatorParams.IsWalking, false);
                Context.characterController.SetAnim(AnimatorParams.IsRunning, false);
                if (!BattleJudge.I.CanPlayerAttack(Context.characterController.PlayerID))
                {
                    Debug.Log($"攻撃不能状態{Context.characterController.PlayerID}");
                    return;
                }
                attackCTS = new CancellationTokenSource();
                // カウンター再入場処理
                if (isCounter && currentAttackData != null)
                {
                    isCounter = false;
                    duration = currentAttackData.attackDuration;
                    lastAttackTime = Time.time;

                    SetAnimSetting();
                    SetAttackObjSetting();
                    SetupDelayedAttack();
                    return;
                }

                // 通常攻撃処理
                attackType = AttackProcessor_Refacta.CheckAttackType(Context.playerInputManager);
                attackStrength = AttackProcessor_Refacta.CheckAttackStrength(Context.playerInputManager, !Context.characterController.IsGrounded());
                var key = (attackType, attackStrength);

                if (Context.characterController.AttackSet.attackDataMap.TryGetValue(key, out var attackData))
                {
                    currentAttackData = CanChain() ? lastAttackData.nextChain : attackData;
                    duration = currentAttackData.attackDuration;
                    lastAttackTime = Time.time;
                    SetAppeal();
                    SetAnimSetting();
                    SetAttackObjSetting();
                    SetCounterData();
                    SetupDelayedAttack();
                }
                else
                {
                    Debug.LogWarning($"No attack found for: {key}");
                }

                CheckConsecutiveAttacks();
            }

            protected internal override void Update()
            {
                if (BattleJudge.I.IsPaused) return;
                elapsedTime += Time.deltaTime;
                if (elapsedTime > duration)
                {
                    isEarlyExit = false;
                    Context.ChangeNeutralState();
                }
            }

            protected internal override void Exit()
            {
                elapsedTime = 0;

                if (Context != null && Context.anim != null)
                {
                    Context.anim.speed = Context.characterController?.DefaultAnimSpeed ?? 1f;
                    if (currentAttackData != null)
                    {
                        Context.anim.SetBool(currentAttackData.animHash, false);
                        lastAttackData = currentAttackData;
                    }

                    if (isEarlyExit && currentAttackData != null)
                    {
                        Context.anim.SetBool(currentAttackData.animHash, false);
                    }
                }
                else
                {
                    CustomLogger.Warning("Context or Context.anim is null in AttackState.Exit()");
                }

                Context.characterController?.SetCanCounter(false);

                if (attackCTS != null)
                {
                    attackCTS.Cancel();
                    attackCTS.Dispose();
                    attackCTS = null;
                }

                Context.currentCommand = null;

                if (!CanChain())
                {
                    lastAttackObject = null;
                }
            }


            /// <summary>
            ///  攻撃処理実行
            /// </summary>
            private void AttackProcess()
            {
                if (currentAttackData == null) return;
                if (attackCTS == null) return;
                AttackProcessor_Refacta.ProcessAttack(currentAttackData, Context.characterController, Context.characterController.gameObject, attackCTS.Token);
            }

            /// <summary>
            /// アニメーション設定
            /// </summary>
            private void SetAnimSetting()
            {
                if (currentAttackData == null) return;
                Context.anim.speed = currentAttackData.animationSpeed;
                Context.anim.SetBool(currentAttackData.animHash, true);
            }

            /// <summary>
            /// 攻撃オブジェクト生成設定
            /// </summary>
            private void SetAttackObjSetting()
            {
                if (currentAttackData == null || currentAttackData.attackPrefab == null) return;
                DelayUtility.StartDelayedActionWithPause(
                    Context.characterController,
                    currentAttackData.hitTiming,
                    BattleJudge.I.GetPauseStateFunc,
                    () =>
                    {
                        var obj = CharaEffectFactory.I.GetEffectObj(currentAttackData.attackPrefab);
                        var t = Context.characterController.transform;

                        Vector3 spawnPosition;

                        // Chain攻撃の場合、前回のオブジェクトの現在位置を使用
                        if (CanChain() && lastAttackObject != null && currentAttackData.isChainPos)
                        {
                            spawnPosition = lastAttackObject.transform.position;

                            // Chain攻撃時のオフセットを適用
                            var offset = lastAttackObject.transform.right * currentAttackData.prefabOffset.x +
                                         lastAttackObject.transform.up * currentAttackData.prefabOffset.y +
                                         lastAttackObject.transform.forward * currentAttackData.prefabOffset.z;
                            spawnPosition += offset;
                            if (lastAttackObject == null) return;

                            var controller = lastAttackObject.GetComponent<AttackObjectController>();
                            //FirstOrDefaultは最初に用件を満たすものを返す
                            var lifeTime = controller?.Behaviours.FirstOrDefault(b => b is AttackLifeTime) as AttackLifeTime;
                            lifeTime?.ResetLifeTime();
                        }
                        else
                        {
                            // 通常攻撃の場合、キャラクター基準の位置
                            var offset = t.right * currentAttackData.prefabOffset.x +
                                         t.up * currentAttackData.prefabOffset.y +
                                         t.forward * currentAttackData.prefabOffset.z;
                            spawnPosition = t.position + offset;
                        }

                        obj.transform.position = spawnPosition;

                        var rot = currentAttackData.prefabRotation;
                        if (t.forward.x < 0) rot.y = 180 - rot.y;
                        obj.transform.rotation = Quaternion.Euler(rot);

                        var attackObjController = obj.GetComponent<AttackObjectController>();
                        attackObjController?.SetPlayer(Context.characterController.PlayerID, Context.characterController.gameObject);

                        // 現在のオブジェクトを記録
                        lastAttackObject = obj;
                    }
                );

            }

            /// <summary>
            /// カウンター攻撃用設定
            /// </summary>
            private void SetCounterData()
            {
                if (!currentAttackData.isCounter) return;

                Context.characterController.SetCanCounter(true);
                Context.characterController.SetCounterAction(() =>
                {
                    if (currentAttackData != null)
                    {
                        Context.anim.SetBool(currentAttackData.animHash, false);
                    }
                    isCounter = true;
                    currentAttackData = currentAttackData.nextChain;
                    Context.ChangeAttackState();
                });
            }

            /// <summary>
            /// アピールかどうかの識別
            /// アピールの場合エネルギーチャージ状態へ移行
            /// </summary>
            private void SetAppeal()
            {
                if (!currentAttackData.isAppeal) return;
                if (Context.characterController.CanSpecialAttack())
                {
                    currentAttackData = specialData;
                    return;
                }
                appealCoroutine = null;
                appealCoroutine = Context.characterController.StartCoroutine(SuccessAppeal(currentAttackData.attackDuration, currentAttackData.chargeDuration));
                //アピール実行時一回だけ必殺技のデータを入れる
                //アピールを一回でも実行しないと必殺技がnullになるがそもそもアピールをしないと必殺技がたまり切らないと思うのでこの実装で
                //必殺技のデータをCharacterControllerやResourcesから受け取るのが責任の分離的に嫌なので
                if (specialData == null)
                    specialData = currentAttackData.nextChain;
            }

            private IEnumerator SuccessAppeal(float dur, float chargeDur)
            {
                yield return new WaitForSeconds(dur);
                Context.characterController.ChangeCanCharge(true);
                Context.characterController.StartCoroutine(ResetAppeal(chargeDur));
            }

            private IEnumerator ResetAppeal(float dur)
            {
                yield return new WaitForSeconds(dur);
                Context.characterController.ChangeCanCharge(false);
            }

            /// <summary>
            /// 攻撃の連続使用チェックとペナルティ
            /// </summary>
            private void CheckConsecutiveAttacks()
            {
                if (attackType == lastAttackType && attackStrength == lastAttackStrength)
                {
                    if (lastAttackData != currentAttackData) return;
                    consecutiveAttackCount++;
                    if (consecutiveAttackCount >= PENALTY_THRESHOLD)
                    {
                        Context.characterController?.NotBoolAddSpecialGauge(GAUGE_PENALTY);
                    }
                }
                else
                {
                    consecutiveAttackCount = 1;
                    lastAttackType = attackType;
                    lastAttackStrength = attackStrength;
                }
            }

            /// <summary>
            /// チェイン攻撃可能かを判定
            /// </summary>
            /// <returns></returns>
            private bool CanChain()
            {
                if (lastAttackData == null || !lastAttackData.canChain || lastAttackData.nextChain == null) return false;
                if (Time.time - lastAttackTime > lastAttackData.chainThreshold) return false;
                return true;
            }

            /// <summary>
            /// ヒットタイミング処理の遅延実行
            /// </summary>
            private void SetupDelayedAttack()
            {
                DelayUtility.StartDelayedActionWithPause(
                    Context.characterController,
                    currentAttackData.hitTiming,
                    BattleJudge.I.GetPauseStateFunc,
                    AttackProcess
                );
            }
        }
    }
}