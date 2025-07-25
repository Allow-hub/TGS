using IceMilkTea.StateMachine;
using UnityEngine;
namespace TechC
{
    public partial class CharacterState
    {
        public enum AttackType
        {
            Neutral,//他に入力がないとき
            Right,
            Left,
            Down,
            Up,
        }

        public enum AttackStrength
        {
            Weak,
            Strong,
            Appeal
        }

        private class AttackState : ImtStateMachine<CharacterState>.State
        {

            // 攻撃履歴を保持する静的変数
            private AttackType lastAttackType = AttackType.Neutral;
            private AttackStrength lastAttackStrength = AttackStrength.Weak;
            private int consecutiveAttackCount = 0;
            private AttackType attackType;
            private AttackStrength attackStrength;
            private AttackData currentAttackData;
            private float duration;
            private float elapsedTime = 0;
            private bool isEarlyExit = true;
            private AttackData lastAttackData = null;
            private float lastAttackTime;

            // 同じ攻撃を何回繰り返すとゲージ減少が始まるか
            private const int PENALTY_THRESHOLD = 3;
            // ゲージ減少量
            private const float GAUGE_PENALTY = -5f;

            protected internal override void Enter()
            {
                if (!BattleJudge.I.CanPlayerAttack(Context.characterController.PlayerID))
                {
                    Debug.Log("攻撃不能状態");
                    return;
                }
                attackType = Context.CheckAttackType();
                attackStrength = Context.CheckAttackStrength();
                var key = (attackType, attackStrength);
                if (Context.characterController.AttackSet.attackDataMap.TryGetValue(key, out var attackData))
                {
                    if (CanChain())
                    {
                    }
                    else
                    {
                        currentAttackData = attackData;
                    }

                    duration = attackData.attackDuration;
                    lastAttackTime = Time.time;
                    SetAnimSetting();
                    SetAttackObjSetting();
                    DelayUtility.StartDelayedActionWithPause(Context.characterController, currentAttackData.hitTiming, BattleJudge.I.GetPauseStateFunc, SpawnHitbox);
                }
                else
                {
                    Debug.LogWarning($"No attack found for: {key}");
                }
                // 同じ攻撃の連続使用をチェック
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
                Context.anim.speed = Context.characterController.DefaultAnimSpeed;
                if (currentAttackData != null)
                {
                    Context.anim.SetBool(currentAttackData.animHash, false);
                    lastAttackData = currentAttackData;
                }
                else
                {
                    CustomLogger.Warning($"AttackData is null for type {attackType} and strength {attackStrength}");
                }
                //もし攻撃時間がたたずに他ステートから割り込まれたときに強制終了のメソッドを呼ぶ

                if (isEarlyExit)
                {
                    Context.anim.SetBool(currentAttackData.animHash, false);
                    Debug.Log("Early");
                }
                Context.currentCommand = null;
            }

            private void SetAnimSetting()
            {
                if (currentAttackData == null) return;
                Context.anim.speed = currentAttackData.animationSpeed;
                Context.anim.SetBool(currentAttackData.animHash, true);
            }

            private void SetAttackObjSetting()
            {
                if (currentAttackData == null) return;
                if (currentAttackData.attackPrefab == null) return;
                var obj = CharaEffectFactory.I.GetEffectObj(currentAttackData.attackPrefab);
                var t = Context.characterController.transform;

                // ローカル空間の offset をワールド空間へ変換
                var offset =
                    t.right * currentAttackData.prefabOffset.x +
                    t.up * currentAttackData.prefabOffset.y +
                    t.forward * currentAttackData.prefabOffset.z;

                var pos = t.position + offset;
                obj.transform.position = pos;

                var rot = currentAttackData.prefabRotation;

                // 向きによる Y軸反転（左向きのとき）
                if (t.forward.x < 0)
                {
                    rot.y = 180 - rot.y;
                }
                obj.transform.rotation = Quaternion.Euler(rot);
            }


            /// <summary>
            /// 同じ攻撃の連続使用をチェックし、必要に応じてゲージを減らす
            /// </summary>
            private void CheckConsecutiveAttacks()
            {
                if (attackType == lastAttackType && attackStrength == lastAttackStrength)
                {
                    if (lastAttackData != currentAttackData) return;
                    // 同じ攻撃が連続で使われている
                    consecutiveAttackCount++;

                    // しきい値を超えたらペナルティを適用
                    if (consecutiveAttackCount >= PENALTY_THRESHOLD)
                    {
                        // ゲージを減少させる
                        var characterController = Context.characterController;
                        if (characterController != null)
                        {
                            // ここでゲージを減少（設定により調整可能）
                            characterController.NotBoolAddSpecialGauge(GAUGE_PENALTY);
                            Debug.Log($"同じ攻撃を{consecutiveAttackCount}回連続で使用: ゲージを{GAUGE_PENALTY}減少");
                        }
                    }
                }
                else
                {
                    // 異なる攻撃に変わったらリセット
                    consecutiveAttackCount = 1;
                    lastAttackType = attackType;
                    lastAttackStrength = attackStrength;
                }
            }

            private void SpawnHitbox()
            {
                if (currentAttackData == null) return;

                var t = Context.characterController.transform;

                Vector3 offset =
                    t.right * currentAttackData.hitboxOffset.x +
                    t.up * currentAttackData.hitboxOffset.y +
                    t.forward * currentAttackData.hitboxOffset.z;

                Vector3 center = t.position + offset;

                // 当たり判定
                Collider[] hits = Physics.OverlapSphere(center, currentAttackData.radius, currentAttackData.targetLayers);
                foreach (var hit in hits)
                {
                    var damageable = hit.GetComponent<IDamageable>();
                    var opponent = hit.GetComponent<Player.CharacterController>();
                    //自分への接触チェック
                    if (opponent?.PlayerID == Context.characterController.PlayerID) continue;

                    if (damageable != null)
                    {
                        var targetController = Context.characterController.OpponentController;
                        if (targetController != null &&
                            !BattleJudge.I.IsValidAttackTarget(targetController.PlayerID))
                        {
                            Debug.Log($"相手は現在無敵");
                            continue; // 無敵状態などの場合はスキップ
                        }
                        damageable.TakeDamage(currentAttackData.damage);
                    }
                }
                AttackVisualizer.I.DrawHitbox(center, currentAttackData.radius, 1f);
            }

            private bool CanChain()
            {
                if (lastAttackData == null) return false;
                if (!lastAttackData.canChain) return false;
                if (lastAttackData.nextChain == null) return false;
                if (Time.time - lastAttackTime > lastAttackData.chainThreshold) return false;
                currentAttackData = lastAttackData.nextChain;
                return true;
            }
        }
    }
}