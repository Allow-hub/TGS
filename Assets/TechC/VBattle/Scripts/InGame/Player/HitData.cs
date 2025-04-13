using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    // ヒットデータを管理するクラス
    public class HitData
    {
        // 基本パラメータ
        public float damage { get; private set; }
        public float hitStunDuration { get; private set; }
        public Vector3 knockbackDirection { get; private set; }
        public float knockbackForce { get; private set; }
        public int hitStunLevel { get; private set; }
        public bool isLaunchAttack { get; private set; }

        // 追加パラメータ
        public bool canTech { get; private set; }
        public bool canDI { get; private set; }
        public float diInfluence { get; private set; }
        public float hitStopDuration { get; private set; }

        // コンストラクタ
        public HitData(
            float damage,
            float hitStunDuration,
            Vector3 knockbackDirection,
            float knockbackForce,
            int hitStunLevel,
            bool isLaunchAttack,
            bool canTech,
            bool canDI,
            float diInfluence,
            float hitStopDuration)
        {
            this.damage = damage;
            this.hitStunDuration = hitStunDuration;
            this.knockbackDirection = knockbackDirection.normalized;
            this.knockbackForce = knockbackForce;
            this.hitStunLevel = hitStunLevel;
            this.isLaunchAttack = isLaunchAttack;
            this.canTech = canTech;
            this.canDI = canDI;
            this.diInfluence = diInfluence;
            this.hitStopDuration = hitStopDuration;
        }

        // スマブラのように蓄積ダメージに基づいてノックバック力を調整
        public float CalculateKnockbackForce(float accumulatedDamage)
        {
            // 例: 基本ノックバック + (蓄積ダメージ * 0.1f)
            return knockbackForce + (accumulatedDamage * 0.1f);
        }
    }
}
