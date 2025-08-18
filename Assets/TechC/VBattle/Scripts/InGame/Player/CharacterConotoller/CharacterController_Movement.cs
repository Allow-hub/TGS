using UnityEngine;

namespace TechC.Player
{
    /// <summary>
    /// CharacterController_Movement.cs
    /// キャラクターの移動を分離したクラス
    /// </summary>
    public partial class CharacterController
    {

        // 予測着地地点を保持するフィールド
        private Vector3 predictedLandingPoint;
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
            ApplyCustomGravity();

        }
        private void ApplyCustomGravity()
        {
            if (rb.velocity.y > 0) // 上昇中
            {
                if (!playerInputManager.IsJumping)
                {
                    // 可変ジャンプ（短押しなら早く落ちる）
                    rb.velocity += Vector3.down * shortHopGravityScale * Time.deltaTime;
                }
                else
                {
                    // 通常上昇
                    rb.velocity += Vector3.down * gravityScale * Time.deltaTime;
                }
            }
            else if (rb.velocity.y < 0) // 落下中
            {
                rb.velocity += Vector3.down * fallGravityScale * Time.deltaTime;
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

                // 初速を即時反映
                rb.velocity = new Vector3(rb.velocity.x, characterData.JumpForce, rb.velocity.z);

                // 着地予測地点を計算して保存
                CalculatePredictedLandingPoint();
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

                // 今の上昇速度を残しつつ補強
                float newY = Mathf.Max(rb.velocity.y * 0.5f, 0f) + characterData.DoubleJumpForce;
                rb.velocity = new Vector3(rb.velocity.x, newY, rb.velocity.z);

                UseDoubleJump();
                CalculatePredictedLandingPoint();
            }
        }



        /// <summary>
        /// ジャンプ開始時に予測着地地点を計算する
        /// </summary>
        private void CalculatePredictedLandingPoint()
        {
            Vector3 startPosition = transform.position;
            Vector3 initialVelocity = rb.velocity; // ジャンプ直後の速度を使う
            float gravity = Mathf.Abs(Physics.gravity.y); // 重力の絶対値
            float y0 = startPosition.y;
            float vy = initialVelocity.y;

            // y(t) = y0 + vy * t - 0.5 * g * t^2 = 0 となるtを計算する（二次方程式）
            // 0.5 * g * t^2 - vy * t - y0 = 0
            float a = 0.5f * gravity;
            float b = -vy;
            float c = -y0;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                // 解なし（何か異常？）は現在位置を設定しておく
                predictedLandingPoint = startPosition;
                return;
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (b + sqrtDiscriminant) / (2 * a);
            float t2 = (b - sqrtDiscriminant) / (2 * a);

            // 正の解を選択
            float timeToLand = Mathf.Max(t1, t2);
            if (timeToLand < 0)
            {
                predictedLandingPoint = startPosition;
                return;
            }

            // 水平方向の移動距離を計算（速度は一定と仮定）
            float vx = initialVelocity.x;
            float vz = initialVelocity.z;

            float predictedX = startPosition.x + vx * timeToLand;
            float predictedZ = startPosition.z + vz * timeToLand;

            // Yは地面として0をセット（もしくは地形に合わせてRaycastなどで調整可能）
            predictedLandingPoint = new Vector3(predictedX, 0f, predictedZ);

            // デバッグ用に可視化（着地予測位置に青い球を表示）
            Debug.DrawLine(startPosition, predictedLandingPoint, Color.blue, 2f);
            Debug.DrawRay(predictedLandingPoint + Vector3.up * 2, Vector3.down * 2, Color.blue, 2f);
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