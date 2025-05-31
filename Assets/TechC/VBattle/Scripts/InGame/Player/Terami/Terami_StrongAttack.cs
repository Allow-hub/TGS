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
        // [Header("ニュートラル強")]
        // [Header("左強")]
        // [Header("右強")]
        // [Header("下強")]
        // [Header("上強")]


        /// <summary>
        /// 回復アイテムをキャラ２の目の前に出す、一定期間経過後再利用可能
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

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
