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
        
        [Header("攻撃特性")]
        public int damage;
        public float knockback;
        public float attackRange;
        public LayerMask targetLayers;
        
        [Header("エフェクト")]
        public AudioClip soundEffect;
        public GameObject effectPrefab;
        public Vector3 effectOffset;
        
        [Header("追加効果")]
        public bool causeStun;
        public float stunDuration;
        public StatusEffectType statusEffect;
        public float statusEffectDuration;
        
        public enum StatusEffectType { None, Burn, Freeze, Poison, Stun }
    }
}
