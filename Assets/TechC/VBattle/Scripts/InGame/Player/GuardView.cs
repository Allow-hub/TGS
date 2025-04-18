using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechC
{
    /// <summary>
    /// ガードのオブジェクトの見た目
    /// ガードの耐久値に合わせて見た目を小さくしたりする
    /// </summary>
    public class GuardView : MonoBehaviour
    {
        [SerializeField] private Player.CharacterController characterController;
        [Tooltip("ガードの最大サイズ")]
        [SerializeField] private Vector3 maxScale;
        [Tooltip("ガードの最小サイズの比率(0～1)")]
        [SerializeField] private float minScaleRatio = 0.5f;

        private float lastGuardPower;

        private void Start()
        {
            lastGuardPower = characterController.GetCharacterData().GuardPower;
            transform.localScale = maxScale;
        }

        private void Update()
        {
            if (characterController.GetGuardPower() == lastGuardPower) return;

            // ガード値の割合を計算（0.0～1.0）
            var guardRatio = characterController.GetGuardPower() / characterController.GetCharacterData().GuardPower;

            // 最小比率を考慮したスケール計算
            // guardRatio=0のとき minScaleRatio、guardRatio=1のとき 1.0 になるように線形補間
            var scaleFactor = minScaleRatio + (1f - minScaleRatio) * guardRatio;

            // ガード値の割合に応じてサイズを変更
            var newScale = new Vector3(
                maxScale.x * scaleFactor,
                maxScale.y * scaleFactor,
                maxScale.z * scaleFactor
            );

            transform.localScale = newScale;
            lastGuardPower = characterController.GetGuardPower();
        }
    }
}