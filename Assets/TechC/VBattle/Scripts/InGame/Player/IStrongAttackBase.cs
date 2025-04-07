using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    /// <summary>
    /// 強攻撃のベース
    /// </summary>
    public interface IStrongAttackBase
    {
        public void NeutralAttack();
        public void RightAttack();
        public void LeftAttack();
        public void DownAttack();
        public void UpAttack();
    }
}
