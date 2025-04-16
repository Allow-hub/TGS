using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    [CreateAssetMenu]
    public class CharacterData : ScriptableObject
    {
        [Tooltip("キャラクター名")]
        public string Name;
        [Tooltip("キャラクターのHp")]
        public int Hp;
        [Tooltip("ガードの耐久値")]
        public float GuardPower;
        [Tooltip("ガードの回復速度")]
        public float GuardRecoverySpeed;
        [Tooltip("ガードの回復までのインターバル")]
        public float GuardRecoveryInterval;
        [Tooltip("ガード破壊スタンの時間")]
        public float GuardBreakDuration;
        [Tooltip("移動速度")]
        public float MoveSpeed;
        [Tooltip("加速度")]
        public float Acceleration = 10f;
        [Tooltip("減速度")]
        public float Deceleration = 8f;
        [Tooltip("ジャンプ力")]
        public float JumpForce = 10f;
        [Tooltip("2段ジャンプの力")]
        public float DoubleJumpForce = 8f;
        [Tooltip("空中での移動制御係数")]
        public float AirControlMultiplier = 0.7f;
        [Tooltip("空中での加速度")]
        public float AirAcceleration = 5f;
        [Tooltip("急降下速度")]
        public float FastFallSpeed = 15f;
        [Tooltip("回転速度")]
        public float RotationSpeed = 10f;
    }
}
