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
        public bool IsFrozen { get; private set; } = false;

                private bool hasFrozenOnce = false; // 1度だけ停止用フラグ

        // 固定コメントとしてセット
        public void SetSpecialType(SpecialCommentType type)
        {
            SpecialType = type;
        }

        private void OnTriggerEnter(Collider other)
        {
            // 1回目だけフリーズ、それ以降は何もしない
            if (SpecialType == SpecialCommentType.Freeze && other.CompareTag("Player") && !hasFrozenOnce)
            {
                IsFrozen = true;
                hasFrozenOnce = true;
                if (CommentDisplay.I != null)
                {
                    CommentDisplay.I.OnFreezeTriggered(this);
                }
            }
        }

        // フリーズ解除（CommentDisplayから呼ばれる）
        public void ResetFreezeState()
        {
            IsFrozen = false;
        }

        // 再利用時にリセット
        public void ResetAllState()
        {
            IsFrozen = false;
            hasFrozenOnce = false;
        }
    }
}
