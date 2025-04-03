using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    /// <summary>
    /// 弱攻撃のベース
    /// interfaceで実装することで中身の実装を継承側に委ねる
    /// </summary>
    public interface IWeakAttackBase
    {
        public enum WeakAttackType
        {
            Normal,//他に入力がないとき
            Right,
            Left,
            Down,
            Up, 
        }

        public  void NeutraAttack();
        public void RightAttack();
        public void LeftAttack();
        public void DownAttack();
        public void UpAttack();
    }
}
