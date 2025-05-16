using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめのアピール実装
    /// </summary>
    public class Ame_Appeal : AppealBase
    {  public override void NeutralAttack()
        {
            ExecuteAttack(neutralAttackData);
        }

        public override void LeftAttack()
        {
            ExecuteAttack(leftAttackData);
        }

        public override void RightAttack()
        {
            ExecuteAttack(rightAttackData);
        }

        public override void DownAttack()
        {
            ExecuteAttack(downAttackData);
        }

        public override void UpAttack()
        {
            ExecuteAttack(upAttackData);
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);

        }

        /// <summary>
        /// 強制終了時
        /// </summary>
        public override void ForceFinish()
        {
            base .ForceFinish();
        }
    }
}
