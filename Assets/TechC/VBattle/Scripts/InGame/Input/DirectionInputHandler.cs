using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TechC
{
    /// <summary>
    /// 入力方向を表す列挙型
    /// </summary>
    public enum InputDirection
    {
        None,       // 入力なし
        Up,         // 上
        UpRight,    // 右上
        Right,      // 右
        DownRight,  // 右下
        Down,       // 下
        DownLeft,   // 左下
        Left,       // 左
        UpLeft      // 左上
    }

    /// <summary>
    /// 方向入力を処理するヘルパークラス
    /// </summary>
    public static class DirectionInputHandler
    {
        /// <summary>
        /// スナップされた方向を取得
        /// </summary>
        /// <param name="input"></param>
        /// <param name="deadzone"></param>
        /// <returns></returns>
        public static InputDirection GetSnappedDirection(Vector2 input, float deadzone = 0.3f)
        {
            // 入力が閾値以下なら None を返す
            if (input.magnitude <= deadzone)
                return InputDirection.None;

            // 角度を計算（度）- 0度は右方向
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            // 負の角度を 0-360 の範囲に変換
            if (angle < 0) angle += 360f;

            // 角度から8方向の入力を決定
            if (angle >= 337.5f || angle < 22.5f)
                return InputDirection.Right;
            else if (angle >= 22.5f && angle < 67.5f)
                return InputDirection.UpRight;
            else if (angle >= 67.5f && angle < 112.5f)
                return InputDirection.Up;
            else if (angle >= 112.5f && angle < 157.5f)
                return InputDirection.UpLeft;
            else if (angle >= 157.5f && angle < 202.5f)
                return InputDirection.Left;
            else if (angle >= 202.5f && angle < 247.5f)
                return InputDirection.DownLeft;
            else if (angle >= 247.5f && angle < 292.5f)
                return InputDirection.Down;
            else
                return InputDirection.DownRight;
        }

        /// <summary>
        /// 入力をスナップして返す（SnapDirectionProcessor と同様の処理）
        /// </summary>
        /// <param name="input"></param>
        /// <param name="digitalNormalized"></param>
        /// <param name="deadzone"></param>
        /// <returns></returns>
        public static Vector2 GetSnappedVector(Vector2 input, bool digitalNormalized = false, float deadzone = 0.3f)
        {
            // 入力が閾値以下ならゼロベクトルを返す
            if (input.magnitude <= deadzone)
                return Vector2.zero;

            // 角度を取得（ラジアン）
            float angle = Mathf.Atan2(input.y, input.x);
            
            // 角度を8方向（45度ごと）にスナップ
            angle = Mathf.Round(angle / (Mathf.PI / 4)) * (Mathf.PI / 4);
            
            // 入力の大きさを決定
            float magnitude = input.magnitude;
            if (digitalNormalized)
                magnitude = 1.0f;
                
            // 角度からベクトルを計算して返す
            return new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ).normalized * magnitude;
        }
        
        /// <summary>
        /// 特定の方向かどうかをチェック
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsDirection(InputDirection current, InputDirection target)
        {
            return current == target;
        }
        
        /// <summary>
        /// 垂直方向（上または下）かどうかをチェック
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsVerticalDirection(InputDirection direction)
        {
            return direction == InputDirection.Up || direction == InputDirection.Down;
        }
        
        /// <summary>
        /// ジャンプ可能な方向（上のみ）かどうかをチェック
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsJumpDirection(InputDirection direction)
        {
            return direction == InputDirection.Up;
        }
        
        /// <summary>
        /// しゃがみ可能な方向（下のみ）かどうかをチェック
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsCrouchDirection(InputDirection direction)
        {
            return direction == InputDirection.Down;
        }
    }
}