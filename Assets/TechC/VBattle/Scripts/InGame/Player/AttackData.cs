using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        [System.Serializable]
        public class Data
        {
            public CharacterState.AttackType attackType;
            public AttackManager.AttackStrength attackStrength;
        }

        public Data data;

        [Header("アニメーション")]
        public string animationTrigger;
        public float animationSpeed = 1f;
        public int animHash => Animator.StringToHash(animationTrigger);
        public float attackDuration;
        
        [Header("攻撃特性")]
        public int damage;

        public float knockback;

        [Tooltip("攻撃の半径")]
        public float radius;
        
        [Tooltip("攻撃が可能なレイヤー")]
        public LayerMask targetLayers;

        [Tooltip("キャラクターからの相対位置")]
        public Vector3 hitboxOffset;

        [Tooltip("当たり判定の発生タイミング")]
        public float hitTiming;

        [Tooltip("ヒットストップの持続時間")]
        public float hitStopDuration;

        [Tooltip("ヒットストップ中の時間スケール")]
        public float hitStopTimeScale; 

        [Header("ノックバック設定")]

        [Tooltip("吹っ飛ぶ方向を定義（デフォルトは前方）")]
        public Vector3 knockbackDirection = Vector3.forward;

        [Tooltip("カスタム方向を使用するかのフラグ")]
        public bool useCustomKnockbackDirection = false; 

        [Header("エフェクト")]
        public AudioClip soundEffect;
        public GameObject effectPrefab;
        public Vector3 effectOffset;
        
        [Header("ヒットスタン設定")]

        [Tooltip("ヒットスタンの持続時間")]
        public float hitStunDuration = 0.5f;   
        public int hitStunLevel = 0;          // 0=軽い、1=中、2=強い
        public bool isLaunchAttack = false;   // 打ち上げ攻撃か
        public bool canTech = true;           // 受け身可能か
        public bool canDI = true;             // 方向操作(DI)可能か
        public float diInfluence = 0.3f;      // DIの影響度

        public enum StatusEffectType { None, Burn, Freeze, Poison, Stun }
    }
}
