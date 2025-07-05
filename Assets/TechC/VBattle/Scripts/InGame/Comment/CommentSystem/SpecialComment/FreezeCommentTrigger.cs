using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 「固定」コメントがプレイヤーに接触した際に、フリーズイベントを発火するコンポーネント
    /// </summary>
    public class FreezeCommentTrigger : MonoBehaviour
    {
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
                    CommentDisplay.I.OnFreezeTriggered();
                }
            }
        }
    }
}