using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめのアピール実装
    /// </summary>
    public class Ame_Appeal : AppealBase
    {
        [Header("必殺技設定")]
        [SerializeField] private GameObject ultPrefab;
        [SerializeField] private Sprite errorSprite;
        [SerializeField] private Sprite specialSprite;
        [SerializeField] private float rushDistance = 20f; // 前進距離
        [SerializeField] private float rushDuration = 0.5f; // 前進時間
        [SerializeField] private LayerMask targetLayerMask = 0; // 攻撃対象のレイヤー
        [SerializeField] private LayerMask wallLayerMask = 7; // 壁のレイヤー
        [SerializeField] private float raycastInterval = 0.1f; // レイキャストの間隔
        [SerializeField] private float hideScreenDelay = 1.5f;
        [SerializeField] private float resetScreenDelay = 1f;
        [SerializeField] private float popupWindowDurtaion = 1f;

        [Header("Windowアニメーション")]
        [SerializeField] private float windowMoveSpeed_1 = 0.2f;

        [SerializeField] private float windowMoveSpeed_2 = 0.2f;
        [SerializeField] private float windowResizeSpeed_1 = 0.2f;
        [SerializeField] private float windowResizeSpeed_2 = 0.2f;
        private bool isRushing = false;
        private Rigidbody rb;
        private HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 重複ヒット防止
        private bool canMove = false;
        public bool CanMove => canMove;
        System.Func<bool> CanMoveFunc => () => CanMove;

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
            // characterController.NotBoolAddSpecialGauge(100);//デバッグ用
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

            //少し待って必殺技状態に変更しステージを変える
            DelayUtility.StartDelayedAction(this, hideScreenDelay, () =>
            {
                base.ExcuteSpecial();
            });

            DelayUtility.StartDelayedActionWithPause(this, hideScreenDelay + resetScreenDelay, () => !WindowManager.I.AllreadyPopup, () =>
            {
                WindowManager.I.ResetWindow(true);
                WindowManager.I.ResetAllreasyPopup();
                BattleJudge.I.ResumePlayers();
                GameManager.I.SetActiveSceneRoot(true, "CutIn");
                
            });
        }

        /// <summary>
        /// 敵キャラクターにヒットした時の処理
        /// </summary>
        private void ProcessOpponentHit(Player.CharacterController opponentController)
        {
            BattleJudge.I.PausePlayer(characterController.PlayerID, false);
            //ウィンドウで画面を隠す
            WindowManager.I.PopupWindowWindow(
                WindowFactory.WindowType.Image,
                maxSize: 500,
                tileSize: 200,
                duration: popupWindowDurtaion,
                tex: errorSprite
            );

            if (GameManager.I.CanConectWifi)
            {
                var webWindowParent = WindowFactory.I.GetWindow(WindowFactory.WindowType.Web);
                var webWindow = webWindowParent as WebWindow;
                WindowUtility.ResizeWindow(webWindow.WebWindowHwnd, Screen.width, Screen.height);
                //画面上部にいったんWebWindowを隠す
                WindowUtility.MoveWindow(webWindow.WebWindowHwnd, 0, -Screen.height);
                webWindow.SetUrl("https://www.youtube.com/watch?v=CBYSqKn1vpQ");

                webWindow.SetRect();
                //画面が隠れるのを待ち、Youtubeを上から降ろす
                DelayUtility.StartDelayedAction(this, hideScreenDelay, () =>
                {
                    canMove = true; // ウィンドウを動かせるようにする
                    DelayUtility.StartRepeatedActionWhile(this, CanMoveFunc, 0.05f, () =>
                    {
                        // ウィンドウの位置を下に移動
                        WindowUtility.MoveWindowToTargetPosition(webWindow.WebWindowHwnd, 0, Screen.height, 2000f);
                        if (WindowUtility.GetWindowRect(webWindow.WebWindowHwnd).Y >= 0)
                        {
                            Debug.Log("WebWindowが画面内に戻りました。");
                            canMove = false; // ウィンドウが画面内に戻ったら停止
                        }
                    });
                });
            }
            else
            {
                DelayUtility.StartDelayedAction(this, hideScreenDelay, () =>
                {
                    var imageWindow = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
                    WindowUtility.ResizeWindow((HWND)imageWindow.Hwnd, 0, 0);
                    WindowUtility.MoveWindow((HWND)imageWindow.Hwnd, Screen.width / 2, Screen.height / 2);
                    imageWindow.SetRect();

                    var image = imageWindow as ImageWindow;
                    image.SetImage(specialSprite.texture, imageWindow.Width, imageWindow.Height);

                    canMove = true;
                    bool canResizeHeight = false;
                    // Step 1: 縦にアニメーション（高さを伸ばす）
                    DelayUtility.StartRepeatedActionWhileWithPause(this, CanMoveFunc, 0.05f, () => !WindowManager.I.AllreadyPopup, () =>
                    {
                        PInvoke.SetForegroundWindow((HWND)imageWindow.Hwnd);
                        if (!canResizeHeight)
                        {
                            WindowUtility.MoveWindowToTargetPosition((HWND)imageWindow.Hwnd, Screen.width / 2, 0, windowMoveSpeed_1);
                            WindowUtility.AnimateResizeWindow((HWND)imageWindow.Hwnd, 10, Screen.height, windowResizeSpeed_1);
                        }
                        // Y座標が0になったら、縦伸ばし完了と判定
                        if (TransformHelper.IsCloseTo(WindowUtility.GetWindowRect((HWND)imageWindow.Hwnd).Y, 0))
                        {
                            canResizeHeight = true;
                            WindowUtility.AnimateResizeWindow(
                                (HWND)imageWindow.Hwnd,
                                Screen.width,
                                Screen.height,
                                windowResizeSpeed_1
                            );

                            WindowUtility.MoveWindowToTargetPosition((HWND)imageWindow.Hwnd, 0, 0, windowMoveSpeed_2);
                            if (TransformHelper.IsCloseTo(WindowUtility.GetWindowRect((HWND)imageWindow.Hwnd).X, 0))
                            {
                                canMove = false;
                                DelayUtility.StartDelayedAction(this, 1.5f, () =>
                                {
                                    WindowManager.I.ResetWindow(false, imageWindow);
                                });
                            }
                        }
                    });
                });
            }

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