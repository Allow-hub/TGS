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
        public enum StrongAttackType
        {
            Normal,//他に入力がないとき
            Right,
            Left,
            Down,
            Up,
        }

        public void NeutraAttack();
        public void RightAttack();
        public void LeftAttack();
        public void DownAttack();
        public void UpAttack();
    }
}
