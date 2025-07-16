using UnityEngine;

namespace TechC.Player
{
    /// <summary>
    /// CharacterController_Movement.cs
    //　キャラクターの移動を分離したクラス
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
        /// 地上での移動処理
        /// </summary>
        private void GroundMovement(Vector3 moveDirection, float horizontalInput, float controlMultiplier)
        {
            float groundSpeed = characterData.MoveSpeed * controlMultiplier * GetMultipiler(BuffType.Speed);
            groundSpeed = Mathf.Clamp(groundSpeed, 0f, characterData.MaxGroundSpeed);

            if (Mathf.Abs(horizontalInput) > STOP_THRESHOLD)
            {
                // キャラクターの向きをY軸回転でスムーズに変更
                float targetYRotation = horizontalInput > 0 ? RIGHT_FACING_ANGLE : LEFT_FACING_ANGLE;
                float currentYRotation = transform.eulerAngles.y;

                // 現在の向きと目標の向きが大きく異なる場合のみ回転を実行
                float angleDifference = Mathf.DeltaAngle(currentYRotation, targetYRotation);

                // 既に正しい方向を向いている場合は回転しない（許容誤差5度）
                if (Mathf.Abs(angleDifference) > ROTATION_TOLERANCE)
                {
                    float rotationStep = characterData.RotationSpeed * RIGHT_FACING_ANGLE * Time.deltaTime;

                    if (Mathf.Abs(angleDifference) > MICRO_ROTATION_THRESHOLD)
                    {
                        float newYRotation = currentYRotation + Mathf.Sign(angleDifference) * Mathf.Min(rotationStep, Mathf.Abs(angleDifference));
                        transform.rotation = Quaternion.Euler(0, newYRotation, 0);
                    }
                    else
                    {
                        // 微小な差の場合は完全に目標角度に合わせる
                        transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
                    }
                }


                float targetVelocityX = horizontalInput * groundSpeed;
                rb.velocity = new Vector3(targetVelocityX, rb.velocity.y, 0);


            }
            else
            {
                // 入力が止まった時：最終的に完全に左右どちらかを向くように調整
                float currentYRotation = transform.eulerAngles.y;

                // 現在の角度から最も近い目標角度（90度または-90度）を決定
                float targetYRotation;
                float diffTo90 = Mathf.Abs(Mathf.DeltaAngle(currentYRotation, RIGHT_FACING_ANGLE));
                float diffToMinus90 = Mathf.Abs(Mathf.DeltaAngle(currentYRotation, LEFT_FACING_ANGLE));

                targetYRotation = diffTo90 < diffToMinus90 ? RIGHT_FACING_ANGLE : LEFT_FACING_ANGLE;

                // 目標角度への最終調整
                float angleDifference = Mathf.DeltaAngle(currentYRotation, targetYRotation);
                if (Mathf.Abs(angleDifference) > FINAL_ROTATION_THRESHOLD) // 1度以上のずれがある場合のみ調整
                {
                    float rotationStep = characterData.RotationSpeed * RIGHT_FACING_ANGLE * Time.deltaTime;
                    float newYRotation = currentYRotation + Mathf.Sign(angleDifference) * Mathf.Min(rotationStep, Mathf.Abs(angleDifference));
                    transform.rotation = Quaternion.Euler(0, newYRotation, 0);
                }
                else if (Mathf.Abs(angleDifference) > MICRO_ROTATION_THRESHOLD)
                {
                    // 微小な差の場合は完全に目標角度に合わせる
                    transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
                }



                if (Mathf.Abs(rb.velocity.x) > STOP_THRESHOLD)
                {
                    float deceleratedX = Mathf.MoveTowards(rb.velocity.x, 0f, characterData.Deceleration * Time.fixedDeltaTime);
                    rb.velocity = new Vector3(deceleratedX, rb.velocity.y, 0);
                }
                else
                {
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0);
                }


                // 従来の減速処理
                if (Mathf.Abs(rb.velocity.x) > STOP_THRESHOLD)
                {
                    float deceleratedX = Mathf.MoveTowards(rb.velocity.x, 0f, characterData.Deceleration * Time.fixedDeltaTime);
                    rb.velocity = new Vector3(deceleratedX, rb.velocity.y, 0);
                }

            }
        }

        /// <summary>
        /// 空中での移動処理
        /// </summary>
        private void AirMovement(Vector3 moveDirection, float horizontalInput)
        {
            // 空中での移動速度（地上より制限される）
            float airSpeed = characterData.MoveSpeed * GetMultipiler(BuffType.Speed) * characterData.AirControlMultiplier;
            // 空中での水平移動（制限付き）
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // キャラクターの向きを変更
                transform.forward = new Vector3(Mathf.Sign(horizontalInput), 0, 0);

                // 空中でも方向転換可能だが、地上より制限される
                float targetVelocityX = horizontalInput * airSpeed;
                float newVelocityX = Mathf.Lerp(rb.velocity.x, targetVelocityX, characterData.AirAcceleration * Time.deltaTime);

                rb.velocity = new Vector3(newVelocityX, rb.velocity.y, 0);
            }

            // 空中でのファストフォール（急降下）- スマブラの特徴的な動き
            // 垂直入力が十分に下向きの場合のみ発動
            if (playerInputManager.MoveInput.y < -jumpInputThreshold && rb.velocity.y < 0)
            {
                rb.AddForce(Vector3.down * characterData.FastFallSpeed, ForceMode.Acceleration);
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
    }
}
