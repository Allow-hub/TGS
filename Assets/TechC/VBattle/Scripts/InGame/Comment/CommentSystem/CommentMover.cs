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
            CommentDisplay.I.StartCoroutine(MoveComment(trans, chars, freezeCommentTrigger, originalMaterial));
        }

        /// <summary>
        /// コメントを画面上に流す処理
        /// </summary>
        private IEnumerator MoveComment(Transform trans, List<GameObject> chars, FreezeCommentTrigger freezeCommentTrigger, Material originalMaterial)
        {
            bool freezeMaterialApplied = false;
            var materialApplier = CommentDisplay.I.GetMaterialApplier();

            while (trans.position.x > despawnPosX)
            {
                if (!trans.gameObject.activeInHierarchy)
                {
                    yield break;
                }

                // 全コメントのフリーズ状態をチェック
                if (CommentDisplay.I.IsCommentFrozen)
                {
                    if (!freezeMaterialApplied)
                    {
                        materialApplier.ApplyMaterialToCharacters(chars, materialApplier.GetFreezeMaterial());
                        freezeMaterialApplied = true;
                    }
                    yield return null;
                    continue;
                }
                else if (freezeMaterialApplied)
                {
                    materialApplier.ApplyMaterialToCharacters(chars, originalMaterial);
                    freezeMaterialApplied = false;
                }

                // コメントを左に移動
                trans.position += Vector3.left * CommentDisplay.I.GetCurrentSpeed() * Time.deltaTime;
                yield return null;
            }

            // コメントをプールに返却
            ReturnComment(trans.gameObject, chars);
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
