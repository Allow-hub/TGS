using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    [CreateAssetMenu]
    public class CharacterData : ScriptableObject
    {
        public string Name;

        public int Hp;
        public float GardPower;
        public float MoveSpeed;
        public float JumpPower;
        public float Acceleration = 10f;      // 加速度
        public float Deceleration = 8f;       // 減速度
        public float JumpForce = 10f;         // ジャンプ力
        public float DoubleJumpForce = 8f;    // 二段ジャンプの力
        public float AirControlMultiplier = 0.7f; // 空中での移動制御係数
        public float AirAcceleration = 5f;    // 空中での加速度
        public float FastFallSpeed = 15f;     // 急降下速度
        public float RotationSpeed = 10f;     // 回転速度
    }
}
