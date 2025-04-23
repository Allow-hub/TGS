using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ガードが可能なことを契約させるインターフェース
    /// </summary>
    public interface IGuardable
    {

        /// <summary>
        /// ガードにダメージを与える処理
        /// </summary>
        /// <param name="damage"></param>
        public void GuardDamage(float damage, ICommand guardCommand);


        /// <summary>
        /// ガードを破壊されたときの処理
        /// </summary>
        public void GuardBreak(ICommand guardCommand);

        /// <summary>
        /// ガードの回復
        /// </summary>
        /// <param name="value"></param>
        public void HealGuardPower(float value);

        /// <summary>
        /// 回復が可能かどうか
        /// </summary>
        /// <returns></returns>
        public bool CanHeal();
    }
}
