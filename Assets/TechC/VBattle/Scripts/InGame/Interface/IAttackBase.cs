using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// すべての攻撃種の基底クラス
    /// </summary>
    public interface IAttackBase
    {
        void NeutralAttack();
        void LeftAttack();
        void RightAttack();
        void DownAttack();
        void UpAttack();
    }
}
