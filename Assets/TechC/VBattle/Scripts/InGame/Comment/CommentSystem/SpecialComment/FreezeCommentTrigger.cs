using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 「固定」コメントがプレイヤーに接触した際に、フリーズイベントを発火するコンポーネント
    /// </summary>
    public class FreezeCommentTrigger : MonoBehaviour
    {

        /* ===============================
         * TODO: NormalCommentにスクリプトをアタッチしないように修正する
         * =============================== */
        public SpecialCommentType SpecialType { get; private set; }
        public string CommentText { get; private set; }

        /// <summary>
        /// 特殊コメントタイプの設定
        /// </summary>
        /// <param name="type"></param>
        public void SetSpecialType(SpecialCommentType type)
        {
            SpecialType = type;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (SpecialType == SpecialCommentType.Freeze && other.CompareTag("Player"))
            {
                if (CommentDisplay.I != null)
                {
                    // CommentDisplay.I.OnFreezeTriggered();
                }

                foreach (Transform child in transform)
                {
                    CommentFactory.I.ReturnChar(child.gameObject);
                    // child.gameObject.SetActive(false);
                }

                /* 固定コメントを非表示いして、Poolに返却する */
                // gameObject.SetActive(false);
                CommentFactory.I.ReturnComment(gameObject);
            }
        }
    }
}