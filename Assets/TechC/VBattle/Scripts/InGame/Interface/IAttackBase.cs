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
        /// <summary>
        /// ニュートラル攻撃
        /// </summary>
        void NeutralAttack();

        /// <summary>
        /// 左入力攻撃
        /// </summary>
        void LeftAttack();

        /// <summary>
        /// 右入力攻撃
        /// </summary>
        void RightAttack();

        /// <summary>
        /// 下入力攻撃
        /// </summary>
        void DownAttack();

        /// <summary>
        /// 上入力攻撃
        /// </summary>
        void UpAttack();

        /// <summary>
        /// 攻撃種を受け取りその攻撃の時間をDataから受ける
        /// </summary>
        /// <param name="attackType"></param>
        /// <returns></returns>
        float GetDuration(CharacterState.AttackType attackType);

        /// <summary>
        /// 強制終了
        /// </summary>
        void ForceFinish();
    }
}
