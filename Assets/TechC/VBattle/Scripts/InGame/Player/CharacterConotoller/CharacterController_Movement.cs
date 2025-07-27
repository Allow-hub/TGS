using UnityEngine;
using TechC.CommentSystem;

namespace TechC.Player
{
    /// <summary>
    /// CharacterController_Movement.cs
    /// キャラクターの移動を分離したクラス
    /// </summary>
    public partial class CharacterController
    {
        /// <summary>
        /// ステート側で読み込む移動処理
        /// </summary>
        public void MoveCharacter(float controlMultiplier)
        {
            float horizontalInput = playerInputManager.MoveInput.x;
            Vector3 moveDirection = new Vector3(horizontalInput, 0, 0).normalized;

            // 正面に壁があるかをチェック（Raycast）
            float rayDistance = 1f; // 判定距離
            Vector3 origin = transform.position + Vector3.up * 0.5f; // 足元でなく胸元から前方をチェック
            Vector3 direction = moveDirection;

            if (horizontalInput != 0 && Physics.Raycast(origin, direction, rayDistance, obstacleLayer))
            {
                // 障害物があるので進行しない
                Debug.DrawRay(origin, direction * rayDistance, Color.red, 0.1f); // デバッグ表示
                return;
            }

            Debug.DrawRay(origin, direction * rayDistance, Color.green, 0.1f); // デバッグ表示

            // 接地判定に基づいて異なる挙動を適用
            if (IsGrounded())
            {
                if (hasDoubleJumped)
                {
                    ResetJump();
                }

                GroundMovement(moveDirection, horizontalInput, controlMultiplier);
            }
            else
            {
                AirMovement(moveDirection, horizontalInput);
            }
        }

        /// <summary>
        /// 速度を0にする
        /// </summary>
        public void StopVelocity() => rb.velocity = Vector3.zero;

        /// <summary>
        /// キャラクターに力を加える
        /// </summary>
        public void AddForcePlayer(Vector3 dir, float force, ForceMode forceMode)
            => rb.AddForce(dir * force, forceMode);

        /// <summary>
        /// 地上にいるかどうかを判定する
        /// </summary>
        public bool IsGrounded()
        {
            Vector3 rayOrigin = transform.position + Vector3.up;
            RaycastHit hit;

            return Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, LayerMask.GetMask("Ground"));
        }

        /// <summary>
        /// 地上での移動処理
        /// </summary>
        private void GroundMovement(Vector3 moveDirection, float horizontalInput, float controlMultiplier)
        {
            float groundSpeed = characterData.MoveSpeed * controlMultiplier * GetMultipiler(BuffType.Speed);
            groundSpeed = Mathf.Clamp(groundSpeed, 0f, characterData.MaxGroundSpeed);

            if (Mathf.Abs(horizontalInput) > STOP_THRESHOLD)
            {
                // ★ フリップによる即時回転
                float targetYRotation = horizontalInput > 0 ? RIGHT_FACING_ANGLE : LEFT_FACING_ANGLE;
                transform.rotation = Quaternion.Euler(0, targetYRotation, 0);

                float targetVelocityX = horizontalInput * groundSpeed;
                rb.velocity = new Vector3(targetVelocityX, rb.velocity.y, 0);
            }
            else
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0);
            }
        }


        /// <summary>
        /// 空中での移動処理
        /// </summary>
        private void AirMovement(Vector3 moveDirection, float horizontalInput)
        {
            float airSpeed = characterData.MoveSpeed * GetMultipiler(BuffType.Speed) * characterData.AirControlMultiplier;

            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                transform.forward = new Vector3(Mathf.Sign(horizontalInput), 0, 0);

                float targetVelocityX = horizontalInput * airSpeed;
                float newVelocityX = Mathf.Lerp(rb.velocity.x, targetVelocityX, characterData.AirAcceleration * Time.deltaTime);

                rb.velocity = new Vector3(newVelocityX, rb.velocity.y, 0);
            }

            if (playerInputManager.MoveInput.y < -jumpInputThreshold && rb.velocity.y < 0)
            {
                rb.AddForce(Vector3.down * characterData.FastFallSpeed, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// ジャンプ処理
        /// </summary>
        public void Jump()
        {
            if (IsGrounded())
            {
                AudioManager.I.PlayCharacterSE(characterType, CharacterSEType.Jump);
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * characterData.JumpForce, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// 二段ジャンプ処理
        /// </summary>
        public void DoubleJump()
        {
            if (CanDoubleJump() && !IsGrounded())
            {
                AudioManager.I.PlayCharacterSE(characterType, CharacterSEType.Jump);
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * characterData.DoubleJumpForce, ForceMode.Impulse);
                UseDoubleJump();
            }
        }

        /// <summary>
        /// ジャンプ状態をリセット（着地時に呼び出す）
        /// </summary>
        private void ResetJump()
        {
            hasDoubleJumped = false;
        }

        /// <summary>
        /// 二段ジャンプが可能かどうか
        /// </summary>
        private bool CanDoubleJump() => !hasDoubleJumped;

        /// <summary>
        /// 二段ジャンプを使用済みにする
        /// </summary>
        private void UseDoubleJump() => hasDoubleJumped = true;

    }
}