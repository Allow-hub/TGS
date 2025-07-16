using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コメントの移動処理のみを担当
    /// </summary>
    [Serializable]
    public class CommentMover
    {
        [Header("コメントを非表示にする場所")]
        [SerializeField] private Transform topLeftDespawn;
        [SerializeField] private Transform buttonLeftDespawn;
        private float despawnPosX;

        /// <summary>
        /// コメントを非表示にする座標を初期化
        /// </summary>
        public void Init()
        {
            despawnPosX = topLeftDespawn.transform.position.x;
        }

        /// <summary>
        /// コメントを画面上に流す処理を開始
        /// </summary>
        public void StartMoving(Transform trans, List<GameObject> chars, FreezeCommentTrigger freezeCommentTrigger, Material originalMaterial)
        {
            // ポーズ中のみ停止（フリーズ中は継続してマテリアル適用を行う）
            Func<bool> isPausedFunc = () => BattleJudge.I.IsPaused;

            // DelayUtilityのポーズ対応版を使用してコメント移動処理を開始
            DelayUtility.StartRepeatedActionWhileWithPause(
                CommentDisplay.I,
                () => trans.gameObject.activeInHierarchy && trans.position.x > despawnPosX,
                Time.fixedDeltaTime,
                isPausedFunc,
                () => MoveCommentFrame(trans, chars, freezeCommentTrigger, originalMaterial)
            );
        }

        /// <summary>
        /// 1フレーム分のコメント移動処理
        /// </summary>
        private void MoveCommentFrame(Transform trans, List<GameObject> chars, FreezeCommentTrigger freezeCommentTrigger, Material originalMaterial)
        {
            if (!trans.gameObject.activeInHierarchy)
            {
                return;
            }

            var materialApplier = CommentDisplay.I.GetMaterialApplier();

            // フリーズ状態に応じてマテリアルを適用
            if (CommentDisplay.I.IsCommentFrozen)
            {
                materialApplier.ApplyMaterialToCharacters(chars, materialApplier.GetFreezeMaterial());
                // フリーズ中は移動処理を停止
                return;
            }
            else
            {
                materialApplier.ApplyMaterialToCharacters(chars, originalMaterial);
            }

            // 通常時のみ移動処理を実行
            trans.position += Vector3.left * CommentDisplay.I.GetCurrentSpeed() * Time.deltaTime;

            // 画面外に出た場合はプールに返却
            if (trans.position.x <= despawnPosX)
            {
                ReturnComment(trans.gameObject, chars);
            }
        }

        /// <summary>
        /// コメントと文字オブジェクトをプールに返却
        /// </summary>
        private void ReturnComment(GameObject comment, List<GameObject> chars)
        {
            /* コメントの文字を先にPoolに返却する */
            foreach (var obj in chars)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                    CommentFactory.I.ReturnChar(obj);
                }
            }

            /* コメントそのものをPoolに返却する */
            if (comment.activeInHierarchy)
            {
                comment.SetActive(false);
                CommentFactory.I.ReturnComment(comment);
            }
        }
    }
}
