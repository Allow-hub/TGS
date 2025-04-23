using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    // ヒットデータを処理するクラス（AttackDataからHitDataを生成）
    public class HitDataProcessor
    {
        // AttackDataからHitDataを生成
        public static HitData CreateFromAttackData(AttackData attackData, Transform attacker, Transform target)
        {
            Vector3 direction;

            // カスタム方向を使うかどうかで分岐
            if (attackData.useCustomKnockbackDirection)
            {
                // AttackDataで定義されたカスタム方向を使用
                // 攻撃者の向きに合わせて方向を回転させる
                direction = attacker.TransformDirection(attackData.knockbackDirection).normalized;
            }
            else
            {
                // 従来通り、攻撃者から被攻撃者への方向を計算
                direction = (target.position - attacker.position).normalized;
            }

            // Y方向の成分を調整（打ち上げ攻撃の場合は上向きに）
            if (attackData.isLaunchAttack)
            {
                // カスタム方向を使う場合は、Y成分のみを調整
                if (attackData.useCustomKnockbackDirection)
                {
                    // Y成分が小さい場合は強制的に上向き成分を追加
                    if (direction.y < 0.3f)
                    {
                        direction = new Vector3(direction.x, Mathf.Max(direction.y, 0.5f), direction.z).normalized;
                    }
                }
                else
                {
                    // 従来の計算方法
                    direction = new Vector3(direction.x, 0.5f, direction.z).normalized;
                }
            }


            // HitDataを生成
            return new HitData(
                attackData.damage,
                attackData.hitStunDuration,
                direction,
                attackData.knockback,
                attackData.hitStunLevel,
                attackData.isLaunchAttack,
                attackData.canTech,
                attackData.canDI,
                attackData.diInfluence,
                attackData.hitStopDuration
            );
        }
    }
}
