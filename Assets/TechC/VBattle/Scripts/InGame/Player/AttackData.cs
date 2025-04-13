using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    // 攻撃データを保持するScriptableObject
   [CreateAssetMenu(fileName = "AttackData", menuName = "TechC/Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [Header("基本情報")]
        public string attackName;
        public string description;
        
        [Header("アニメーション")]
        public string animationTrigger;
        public float animationSpeed = 1f;
        public int animHash => Animator.StringToHash(animationTrigger);
        public float attackDuration;
        
        [Header("攻撃特性")]
        public int damage;
        public float knockback;
        public float range;      // 攻撃の届く距離
        public float radius;     // 攻撃の半径
        public LayerMask targetLayers;
        public Vector3 hitboxOffset; // キャラクターからの相対位置
        public float hitTiming;     //当たり判定の発生タイミング
        public float hitStopDuration; // ヒットストップの持続時間
        public float hitStopTimeScale; // ヒットストップ中の時間スケール

        [Header("ノックバック設定")]
        public Vector3 knockbackDirection = Vector3.forward; // 吹っ飛ぶ方向を定義（デフォルトは前方）
        public bool useCustomKnockbackDirection = false; // カスタム方向を使用するかのフラグ

        [Header("エフェクト")]
        public AudioClip soundEffect;
        public GameObject effectPrefab;
        public Vector3 effectOffset;
        
        [Header("ヒットスタン設定")]
        public float hitStunDuration = 0.5f;   // ヒットスタンの持続時間
        public int hitStunLevel = 0;          // 0=軽い、1=中、2=強い
        public bool isLaunchAttack = false;   // 打ち上げ攻撃か
        public bool canTech = true;           // 受け身可能か
        public bool canDI = true;             // 方向操作(DI)可能か
        public float diInfluence = 0.3f;      // DIの影響度

        public enum StatusEffectType { None, Burn, Freeze, Poison, Stun }
    }
}
