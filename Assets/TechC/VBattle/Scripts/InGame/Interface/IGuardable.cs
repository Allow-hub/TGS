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
        public void GuardDamage(float damage);


        /// <summary>
        /// ガードを破壊されたときの処理
        /// </summary>
        public void GuardBreak();
    }
}
