using UnityEngine;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TechC.Player.Attack
{
    /// <summary>
    /// あめの必殺技
    /// </summary>
    [System.Serializable]
    public class AttackAmeSpecial : IAttackBehaviour
    {
        [Header("必殺技の設定")]
        [SerializeField] private Sprite errorSprite;
        [SerializeField] private Sprite specialSprite;
        [SerializeField] private float hideScreenDelay = 1.5f;
        [SerializeField] private float resetScreenDelay = 1f;

        [SerializeField] private float popupWindowDurtaion = 1f;
        [SerializeField] private float windowMoveSpeed_1 = 0.2f;

        [SerializeField] private float windowMoveSpeed_2 = 0.2f;
        [SerializeField] private float windowResizeSpeed_1 = 0.2f;
        [SerializeField] private float windowResizeSpeed_2 = 0.2f;
        private bool canMove = false;
        public bool CanMove => canMove;
        System.Func<bool> CanMoveFunc => () => CanMove;
        //// ObjControllerの設定
        private GameObject ownerObj;
        private CharacterController ownerCharacter;
        public void Initialize(GameObject owner)
        {
            ownerObj = owner;
        }

        public void OnRelease()
        {
            // ownerObj = null;
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Activate(GameObject character)
        {
            ownerCharacter = character.GetComponent<CharacterController>();
        }
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                // 壁に当たったら何もせず終了
                CharaEffectFactory.I.ReturnEffectObj(ownerObj);
            }
            else
            {
                var characterController = other.transform.root.GetComponent<CharacterController>();
                if (characterController == null) return;
                if (characterController.PlayerID == ownerCharacter.PlayerID) return;// 自分自身への接触は無視
                ProcessOpponentHit(characterController);
                CharaEffectFactory.I.ReturnEffectObj(ownerObj);
            }
        }

        /// <summary>
        /// 敵キャラクターにヒットした時の処理
        /// サイズを変更したときに再描画する処理がWindowProcで走っているので画像が上書き差てしまう問題あり
        /// </summary>
        private void ProcessOpponentHit(Player.CharacterController opponentController)
        {
                        //少し待って必殺技状態に変更しステージを変える
            DelayUtility.StartDelayedAction(ownerCharacter, hideScreenDelay, () =>
            {
                BattleJudge.I.SetIsUlting(true);
            });

            DelayUtility.StartDelayedActionWithPause(ownerCharacter, hideScreenDelay + resetScreenDelay, () => !WindowManager.I.AllreadyPopup, () =>
            {
                WindowManager.I.ResetWindow(true);
                WindowManager.I.ResetAllreasyPopup();
                BattleJudge.I.ResumePlayers();
                GameManager.I.SetActiveSceneRoot(true, "CutIn");
            });
            BattleJudge.I.PausePlayer(ownerCharacter.PlayerID, false);
            //ウィンドウで画面を隠す
            WindowManager.I.PopupWindowWindow(
                WindowFactory.WindowType.Image,
                maxSize: 500,
                duration: popupWindowDurtaion,
                tex: errorSprite
            );

            // if (GameManager.I.CanConectWifi)
            // {
            //     var webWindowParent = WindowFactory.I.GetWindow(WindowFactory.WindowType.Web);
            //     var webWindow = webWindowParent as WebWindow;
            //     WindowUtility.ResizeWindow(webWindow.WebWindowHwnd, Screen.width, Screen.height);
            //     //画面上部にいったんWebWindowを隠す
            //     WindowUtility.MoveWindow(webWindow.WebWindowHwnd, 0, -Screen.height);
            //     webWindow.SetUrl("https://www.youtube.com/watch?v=CBYSqKn1vpQ");

            //     webWindow.SetRect();
            //     //画面が隠れるのを待ち、Youtubeを上から降ろす
            //     DelayUtility.StartDelayedAction(this, hideScreenDelay, () =>
            //     {
            //         canMove = true; // ウィンドウを動かせるようにする
            //         DelayUtility.StartRepeatedActionWhile(this, CanMoveFunc, 0.05f, () =>
            //         {
            //             // ウィンドウの位置を下に移動
            //             WindowUtility.MoveWindowToTargetPosition(webWindow.WebWindowHwnd, 0, Screen.height, 2000f);
            //             if (WindowUtility.GetWindowRect(webWindow.WebWindowHwnd).Y >= 0)
            //             {
            //                 Debug.Log("WebWindowが画面内に戻りました。");
            //                 canMove = false; // ウィンドウが画面内に戻ったら停止
            //             }
            //         });
            //     });
            // }
            // else
            // {
            DelayUtility.StartDelayedAction(ownerCharacter, hideScreenDelay, () =>
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
                DelayUtility.StartRepeatedActionWhileWithPause(ownerCharacter, CanMoveFunc, 0.05f, () => !WindowManager.I.AllreadyPopup, () =>
                {
                    image.SetImage(specialSprite.texture);
                    PInvoke.SetForegroundWindow((HWND)imageWindow.Hwnd);
                    if (!canResizeHeight)
                    {
                        WindowUtility.MoveWindowToTargetPosition((HWND)imageWindow.Hwnd, Screen.width / 2, 0, windowMoveSpeed_1);
                        WindowUtility.AnimateResizeWindow((HWND)imageWindow.Hwnd, 1, Screen.height, windowResizeSpeed_1);
                    }
                    // Y座標が0になったら、縦伸ばし完了と判定
                    if (TransformHelper.IsCloseTo(WindowUtility.GetWindowRect((HWND)imageWindow.Hwnd).Y, 0))
                    {
                        canResizeHeight = true;
                        WindowUtility.AnimateResizeWindow(
                            (HWND)imageWindow.Hwnd,
                            Screen.width,
                            Screen.height,
                            windowResizeSpeed_2
                        );

                        WindowUtility.MoveWindowToTargetPosition((HWND)imageWindow.Hwnd, 0, 0, windowMoveSpeed_2);
                        if (TransformHelper.IsCloseTo(WindowUtility.GetWindowRect((HWND)imageWindow.Hwnd).X, 0))
                        {
                            canMove = false;
                            DelayUtility.StartDelayedAction(ownerCharacter, 1.5f, () =>
                            {
                                WindowManager.I.ResetWindow(false, imageWindow);
                            });
                        }
                    }
                });
            });
            // }
        }

    }
}