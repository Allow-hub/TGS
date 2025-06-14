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
            BattleJudge.I.PausePlayer(opponentCharacter.PlayerID, false);

            if (!isRushing && rb != null)
            {
                isRushing = true;
                hitTargets.Clear();

                Vector3 forwardDirection = transform.forward;
                Vector3 basePosition = transform.position;
                float minHitDistance = rushDistance;
                GameObject firstHitTarget = null;
                bool hitIsTarget = false;

                // 上部・中部・下部の3つのレイで最短ヒット距離を調べる
                Vector3[] checkPositions = new Vector3[]
                {
                    basePosition + Vector3.up * 1.5f,      // 上部
                    basePosition + Vector3.up * 0.75f,     // 中部
                    basePosition,                          // 下部
                };

                for (int i = 0; i < checkPositions.Length; i++)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(checkPositions[i], forwardDirection, out hit, rushDistance, wallLayerMask | targetLayerMask))
                    {
                        // 自分自身に当たった場合は無視
                        if (hit.collider.gameObject == this.gameObject || hit.collider.transform.root == this.transform.root)
                            continue;

                        if (hit.distance < minHitDistance)
                        {
                            minHitDistance = hit.distance;
                            firstHitTarget = hit.collider.gameObject;

                            // 壁かtargetか判定
                            if (((1 << hit.collider.gameObject.layer) & targetLayerMask) != 0)
                            {
                                hitIsTarget = true;
                            }
                            else if (((1 << hit.collider.gameObject.layer) & wallLayerMask) != 0)
                            {
                                hitIsTarget = false;
                            }
                        }
                        Debug.DrawRay(checkPositions[i], forwardDirection * hit.distance, Color.red, 0.2f);
                    }
                    else
                    {
                        Debug.DrawRay(checkPositions[i], forwardDirection * rushDistance, Color.green, 0.1f);
                    }
                }

                // 実際に進む距離を決定
                float actualRushDistance = minHitDistance;
                float rushSpeed = actualRushDistance / rushDuration;
                float elapsedTime = 0f;

                DelayUtility.StartRepeatedActionWithPause(
                    this,
                    rushDuration,
                    raycastInterval,
                    BattleJudge.I.GetPauseStateFunc,
                    () =>
                    {
                        elapsedTime += raycastInterval;
                        Vector3 velocity = forwardDirection * rushSpeed;
                        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

                        // 終了判定
                        if (elapsedTime >= rushDuration)
                        {
                            rb.velocity = Vector3.zero;
                            isRushing = false;

                            // 移動が終わってからヒット処理
                            if (hitIsTarget && firstHitTarget != null)
                            {
                                var opponentController = firstHitTarget.GetComponentInParent<Player.CharacterController>();
                                if (opponentController != null)
                                {
                                    ProcessOpponentHit(opponentController);
                                }
                            }
                        }
                    }
                );
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