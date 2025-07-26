using Cinemachine;
using UnityEngine;

namespace TechC
{
    // 攻撃データを保持するScriptableObject
    [CreateAssetMenu(fileName = "AttackData", menuName = "TechC/Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [Header("基本情報")]
        public CharacterType characterType;
        public string attackName;
        public string description;
        public GameObject attackPrefab; // エフェクト・当たり判定含む
        public Vector3 prefabOffset;
        public Vector3 prefabRotation;

        [Header("アニメーション")]
        public string animationTrigger;
        public AnimationClip clip;
        public float animationSpeed = 1f;
        public int animHash => Animator.StringToHash(animationTrigger);
        public float attackDuration;

        [Header("繋ぎ攻撃派生")]
        public AttackData nextChain;
        public bool canChain;
        public float chainThreshold;
        [Header("攻撃特性")]
        public int damage;

        public float knockback;
        [Tooltip("繰り返し攻撃の間隔")]
        public bool canRepeat;
        [Tooltip("繰り返し攻撃の間隔")]
        public float repeatInterval;
        [Tooltip("繰り返し攻撃の時間")]
        public float repeatDuration;

        [Tooltip("攻撃の半径")]
        public float radius;

        [Tooltip("攻撃が可能なレイヤー")]
        public LayerMask targetLayers;

        [Tooltip("攻撃方式")]
        public HitDetectionMode hitDetectionMode = HitDetectionMode.OverlapSphere;
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
        public float shakeIntensity = 0.1f;
        public float shakeDuraion = 0.1f;
        public NoiseSettings noiseSettings;


        [Header("エフェクト")]
        public CharacterSEType characterSEType;
        public CharacterVoiceType characterVoiceType;
    }
    public enum HitDetectionMode
    {
        UseSelf,
        OverlapSphere,
        None
    }
}
