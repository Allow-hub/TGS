using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class Appeal_BaseCharacter : AppealBase
    {
        public override void NeutralAttack()
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
            // ダメージ処理
            //Debug.Log($"弱攻撃を実行: {attackData.attackName}, ダメージ: {attackData.damage}");
        }

        /// <summary>
        /// 強制終了時
        /// </summary>
        public override void ForceFinish()
        {
        }
    }
}
