using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    /// <summary>
    /// WeakAttackとStrongAttackの共通機能を持つ基底クラス
    /// </summary>
    [Serializable]
    public abstract class AttackBase : MonoBehaviour, IAttackBase
    {
        [Header("Reference")]
        [SerializeField] protected Player.CharacterController characterController;
        [SerializeField] protected ObjectPool objectPool;
        [SerializeField] protected BattleJudge battleJudge;
        private readonly Dictionary<AttackType, AttackData> attackDataMap = new Dictionary<AttackType, AttackData>();
        protected bool isAttacking = false;
        protected List<GameObject> activeEffects = new List<GameObject>();

        #region IAttackBaseのメソッド
        public abstract void NeutralAttack();
        public abstract void LeftAttack();
        public abstract void RightAttack();
        public abstract void DownAttack();
        public abstract void UpAttack();

        public virtual void ForceFinish()
        {
            isAttacking = false;
            StopAllCoroutines();
        }

        public float GetDuration(AttackType attackType)
        {
            if (attackDataMap.TryGetValue(attackType, out var attackData))
            {
                return attackData.attackDuration;
            }

            Debug.LogWarning("未定義のAttackTypeが指定されました");
            return 0f;
        }

        public AttackData GetAttackData(AttackType attackType)
        {
            if (attackDataMap.TryGetValue(attackType, out var attackData))
            {
                return attackData;
            }

            Debug.LogWarning("未定義のAttackTypeが指定されました");
            return null;
        }
        #endregion

        protected virtual void Awake()
        {
            objectPool = GameObject.FindWithTag("EffectPool").GetComponent<ObjectPool>();
            battleJudge = GameObject.FindWithTag("BattleJadge").GetComponent<BattleJudge>();
        }
        /// <summary>
        /// 攻撃データをマップに登録
        /// </summary>
        protected void RegisterAttackData(AttackType attackType, AttackData attackData)
        {
            if (attackDataMap.ContainsKey(attackType))
            {
                attackDataMap[attackType] = attackData;
            }
            else
            {
                attackDataMap.Add(attackType, attackData);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var obj in activeEffects)
            {
                if (obj != null)
                {
                    CharaEffectFactory.I.ReturnEffectObj(obj);
                }
            }
            activeEffects.Clear();
        }

        protected void RegisterEffect(GameObject effect)
        {
            if (effect != null && !activeEffects.Contains(effect))
                activeEffects.Add(effect);
        }

        protected void UnregisterEffect(GameObject effect)
        {
            if (effect != null)
                activeEffects.Remove(effect);
        }
        /// <summary>
        /// 攻撃を実行する共通処理
        /// </summary>
        protected virtual void ExecuteAttack(AttackData attackData)
        {
            if (isAttacking) return;
            isAttacking = true;

            characterController.GetAnim().speed = attackData.animationSpeed;
            characterController.SetAnim(attackData.animHash, true);
            CustomLogger.Info("アタックデータ:" + attackData.name, "comboCheck");
            DelayUtility.StartDelayedActionWithPause(this, attackData.attackDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                isAttacking = false;
                characterController.SetAnim(attackData.animHash, false);
                characterController.GetAnim().speed = characterController.DefaultAnimSpeed;
                // Debug.Log("攻撃終了: " + attackData.name);
            });
        }
    }
}