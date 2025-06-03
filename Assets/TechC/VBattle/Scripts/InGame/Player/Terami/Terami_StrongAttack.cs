using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ２：照海の強攻撃の実装
    /// </summary>
    public class Terami_StrongAttack : StrongAttack
    {
        // [Header("プレハブの参照")]
        [Header("ニュートラル強")]
        
        // [SerializeField] private float 
        // [Header("左強")]
        // [Header("右強")]
        // [Header("下強")]
        [Header("上強")]

        [Tooltip("巨大化時の大きさ倍率")]
        [SerializeField] private float scaleMultiplier = 2f; // 巨大化時の大きさ倍率

        [Tooltip("巨大化時の攻撃力倍率")]
        [SerializeField] private float attackMultiplier = 2f; // 巨大化時の攻撃力倍率

        [Tooltip("巨大化時の移動速度倍率（1未満で遅くなる）")]
        [SerializeField] private float moveSpeedMultiplier = 0.5f; // 巨大化時の移動速度倍率（1未満で遅くなる）

        [Tooltip("巨大化の持続時間（秒）")]
        [SerializeField] private float giantDuration = 2f; // 巨大化の持続時間（秒）

        [Tooltip("巨大化のクールダウン時間（秒）")]
        [SerializeField] private float giantCooldown = 10f; // 巨大化のクールダウン時間（秒）

        [Tooltip("プレイヤーの元の大きさ")]
        private Vector3 originalScale; // プレイヤーの元の大きさ
        private bool isGiant = false; // 巨大化かどうか
        private bool isGiantCooldown = false; // クールタイム中かどうか

        /// <summary>
        /// 回復アイテムをキャラ２の目の前に出す、一定期間経過後再利用可能
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

            var player2Hp = BattleJudge.I.GetOtherPlayerObjects(1);


            Debug.Log($"現在のHP：{player2Hp}");
        }

        /// <summary>
        /// 未定
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();
        }

        /// <summary>
        /// 未定
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();
        }

        /// <summary>
        /// 未定
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();
        }

        /// <summary>
        /// 大きさ、攻撃力、当たり判定が２倍になり、移動速度が1/2になる
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();

            if (isGiant || isGiantCooldown) return;

            isGiant = true;
            isGiantCooldown = true;

            originalScale = transform.localScale;

            Debug.Log($"現在の攻撃力倍率：{characterController.GetMultipiler(BuffType.Attack)}");
            Debug.Log($"現在のスピード倍率：{characterController.GetMultipiler(BuffType.Speed)}");
            // Playerに効果を発動させる
            transform.localScale = transform.localScale * scaleMultiplier;
            characterController.AddMultiplier(BuffType.Attack, BuffBase.VoidID, attackMultiplier);
            characterController.AddMultiplier(BuffType.Speed, BuffBase.VoidID, moveSpeedMultiplier);

            Debug.Log($"<color=orange>巨大化後の攻撃力倍率：{characterController.GetMultipiler(BuffType.Attack)}</color>");
            Debug.Log($"<color=orange>巨大化後のスピード倍率：{characterController.GetMultipiler(BuffType.Speed)}</color>");

            // 巨大化解除タイマー
            DelayUtility.StartDelayedAction(this, giantDuration, () =>
            {
                // Playerに効果を取り消す
                transform.localScale = originalScale;
                characterController.RemoveMultiplier(BuffType.Attack, BuffBase.VoidID, 2f);
                characterController.RemoveMultiplier(BuffType.Speed, BuffBase.VoidID, 0.5f);

                Debug.Log($"<color=#00BFFF>巨大化解除の攻撃力倍率：{characterController.GetMultipiler(BuffType.Attack)}</color>");
                Debug.Log($"<color=#00BFFF>巨大化解除のスピード倍率：{characterController.GetMultipiler(BuffType.Speed)}</color>");

                isGiant = false;

            });

            // クールタイム解除タイマー
            DelayUtility.StartDelayedAction(this, giantCooldown, () =>
            {
                isGiantCooldown = false;
            });

        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
        }
        public override void ForceFinish()
        {
        }
    }
}
