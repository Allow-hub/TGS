using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめのアピール実装
    /// </summary>
    public class Ame_Appeal : AppealBase
    {
        [Header("必殺技設定")]
        [SerializeField] private Sprite errorSprite;
        [SerializeField] private float rushDistance = 20f; // 前進距離
        [SerializeField]private float rushDuration = 0.5f; // 前進時間
        [SerializeField]private LayerMask targetLayerMask = 0; // 攻撃対象のレイヤー
        [SerializeField]private LayerMask wallLayerMask = 7; // 壁のレイヤー
        [SerializeField]private float rayLength = 0.5f; // レイの長さ
        [SerializeField]private float wallCheckDistance = 2f; // 壁チェック用レイの長さ
        [SerializeField] private float raycastInterval = 0.1f; // レイキャストの間隔
        private bool isRushing = false;
        private Rigidbody rb;
        private HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 重複ヒット防止

        private void Start()
        {
            // CharacterControllerからRigidbodyを取得
            rb = characterController.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("CharacterControllerにRigidbodyが見つかりません");
            }
        }

        public override void NeutralAttack()
        {
            characterController.NotBoolAddSpecialGauge(100);
            base.NeutralAttack();
        }
        //-------Weak、Strongに合わせたいので使わないが残す-------------------///
        public override void LeftAttack()
        {
            base.LeftAttack();
        }

        public override void RightAttack()
        {
            base.RightAttack();
        }

        public override void DownAttack()
        {
            base.DownAttack();
        }

        public override void UpAttack()
        {
            base.UpAttack();
        }
        //--------------------------------------------------------///
        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
        }

        protected override void ExcuteSpecial()
        {
            base.ExcuteSpecial();
            var opponentCharacter = characterController.OpponentController;
            BattleJudge.I.PausePlayer(opponentCharacter.PlayerID,false);
            if (!isRushing && rb != null)
            {
                isRushing = true;
                hitTargets.Clear();

                // 開始位置と目標位置を設定
                Vector3 forwardDirection = transform.forward;

                float rushSpeed = rushDistance / rushDuration;
                float elapsedTime = 0f;
                // ポーズ対応の繰り返しで前進＋攻撃＋壁判定
                DelayUtility.StartRepeatedActionWithPause(
                    this,
                    rushDuration,
                    raycastInterval,
                    BattleJudge.I.GetPauseStateFunc,
                    () =>
                    {
                        // 前進
                        elapsedTime += raycastInterval;
                        Vector3 velocity = forwardDirection * rushSpeed;
                        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

                        // レイキャスト攻撃
                        PerformRaycastAttacks();
                        // 壁チェック
                        if (CheckWallCollision())
                        {
                            rb.velocity = Vector3.zero;
                            isRushing = false;
                            return;
                        }
                        // 終了判定
                        if (elapsedTime >= rushDuration)
                        {
                            rb.velocity = Vector3.zero;
                            isRushing = false;
                        }
                    }
                );
            }
        }

        /// <summary>
        /// 壁との衝突をチェック
        /// </summary>
        private bool CheckWallCollision()
        {
            Vector3 forwardDirection = transform.forward;
            Vector3 basePosition = transform.position;

            // 上部・中部・下部の3つの位置で壁チェック
            Vector3[] checkPositions = new Vector3[]
            {
                basePosition + Vector3.up * 1.5f,      // 上部
                basePosition + Vector3.up * 0.75f,     // 中部
                basePosition,                          // 下部
            };

            for (int i = 0; i < checkPositions.Length; i++)
            {
                // 壁チェック用レイキャスト
                if (Physics.Raycast(checkPositions[i], forwardDirection, wallCheckDistance, wallLayerMask))
                {
                    // デバッグ用レイ描画（壁検出時は赤色）
                    Debug.DrawRay(checkPositions[i], forwardDirection * wallCheckDistance, Color.red, 0.2f);
                    return true;
                }

                // デバッグ用レイ描画（通常時は緑色）
                Debug.DrawRay(checkPositions[i], forwardDirection * wallCheckDistance, Color.green, 0.1f);
            }

            return false;
        }

        /// <summary>
        /// 上部・中部・下部にレイを飛ばして攻撃判定
        /// </summary>
        private void PerformRaycastAttacks()
        {
            Vector3 forwardDirection = transform.forward;
            Vector3 basePosition = transform.position;

            // 上部・中部・下部の3つのレイを設定
            Vector3[] rayPositions = new Vector3[]
            {
                basePosition + Vector3.up * 1.5f,      // 上部
                basePosition + Vector3.up * 0.75f,     // 中部
                basePosition,                          // 下部
            };

            // デバッグ用のレイ描画色
            Color[] rayColors = new Color[]
            {
                Color.red,    // 上部
                Color.yellow, // 中部
                Color.blue    // 下部
            };

            for (int i = 0; i < rayPositions.Length; i++)
            {
                // レイキャストを実行
                RaycastHit hit;
                if (Physics.Raycast(rayPositions[i], forwardDirection, out hit, rayLength, targetLayerMask))
                {
                    GameObject hitObject = hit.collider.gameObject;

                    // 重複ヒット防止
                    if (!hitTargets.Contains(hitObject))
                    {
                        hitTargets.Add(hitObject);
                        OnRaycastHit(hitObject, hit, i);
                    }
                }

                // デバッグ用レイ描画
                Debug.DrawRay(rayPositions[i], forwardDirection * rayLength, rayColors[i], 0.1f);
            }
        }

        /// <summary>
        /// レイキャストがヒットした時の処理
        /// </summary>
        /// <param name="hitObject">ヒットしたオブジェクト</param>
        /// <param name="hit">レイキャストのヒット情報</param>
        /// <param name="rayIndex">レイのインデックス（0:上部, 1:中部, 2:下部）</param>
        private void OnRaycastHit(GameObject hitObject, RaycastHit hit, int rayIndex)
        {
            // ヒット位置の名前を取得
            string[] rayNames = { "上部", "中部", "下部" };
            Debug.Log($"レイキャスト攻撃ヒット: {hitObject.name} ({rayNames[rayIndex]})");

            // 敵キャラクターの場合の処理
            var opponentController = hitObject.GetComponentInParent<Player.CharacterController>();
            if (opponentController != null)
            {
                // ダメージやノックバックなどの処理
                ProcessOpponentHit(opponentController);
            }
        }

        /// <summary>
        /// 敵キャラクターにヒットした時の処理
        /// </summary>
        private void ProcessOpponentHit(Player.CharacterController opponentController)
        {
            BattleJudge.I.ResumePlayer(opponentController.PlayerID);
            BattleJudge.I.PausePlayers();
            WindowManager.I.PopupWindowWindow(
                WindowFactory.WindowType.Image,
                maxSize: 500,
                tileSize: 200,
                duration: 1f,
                tex: errorSprite
            );
        }

        /// <summary>
        /// 強制終了時
        /// </summary>
        public override void ForceFinish()
        {
            if (isRushing && rb != null)
            {
                StopAllCoroutines();
                rb.velocity = Vector3.zero;
                isRushing = false;
            }
            base.ForceFinish();
        }
    }
}