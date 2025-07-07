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
        [Tooltip("ガード中に毎フレーム耐久値を減少する値")]
        public float GuardDecreasePower;
        [Tooltip("ガードの回復速度")]
        public float GuardRecoverySpeed;
        [Tooltip("ガードの回復までのインターバル")]
        public float GuardRecoveryInterval;
        [Tooltip("ガード破壊スタンの時間")]
        public float GuardBreakDuration;
        [Tooltip("移動速度")]
        public float MoveSpeed;
        [Tooltip("最大地上移動速度")]
        public float MaxGroundSpeed = 10f;

        [Tooltip("加速度")]
        public float Acceleration = 10f;
        [Tooltip("減速度")]
        public float Deceleration = 8f;

        /* ===============================
         * TODO: バランス調整が終わり次第このフラグを消すこと
         * ・trueになったらQuickStopMultiplierも消してCharacterDataの値を直接変更すること
         * =============================== */
        [Tooltip("瞬間方向転換の有効/無効（true=格闘ゲーム風、false=もともとの移動方法）")]
        public bool UseInstantTurn = true;
        [Tooltip("瞬間停止時の減速倍率（UseInstantTurn=trueの場合）")]
        public float QuickStopMultiplier = 10f;
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
