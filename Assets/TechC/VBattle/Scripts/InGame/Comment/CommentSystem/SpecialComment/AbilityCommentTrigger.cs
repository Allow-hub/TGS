using UnityEngine;
using System.Collections;

namespace TechC
{
    /// <summary>
    /// 特殊能力付きコメントのトリガー
    /// </summary>
    public class AbilityCommentTrigger : MonoBehaviour
    {
        public SpecialCommentType SpecialType { get; private set; }
        public string CommentText { get; private set; }

        [Header("固定コメントの設定")]
        [SerializeField] private float freezeDuration = 3f;

        private Coroutine freezeCoroutine;
        private bool isFrozen = false;
        public bool IsFrozen => isFrozen;

        // 1度だけ停止用フラグ
        private bool hasFrozenOnce = false;

        private void OnTriggerEnter(Collider other)
        {
            if (SpecialType == SpecialCommentType.Freeze && other.CompareTag("Player") && !isFrozen && !hasFrozenOnce)
            {
                isFrozen = true;
                hasFrozenOnce = true;
                if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
                freezeCoroutine = StartCoroutine(FreezeTimerCoroutine());

                // シングルトンのCommentDisplayに直接通知
                if (CommentDisplay.I != null)
                {
                    CommentDisplay.I.OnFreezeTriggered(this);
                }
            }
        }

        private IEnumerator FreezeTimerCoroutine()
        {
            yield return new WaitForSeconds(freezeDuration);
            isFrozen = false;
        }

        // コメント再利用時に明示的にリセットしたい場合はこのメソッドを呼ぶ
        public void ResetFreezeState()
        {
            isFrozen = false;
            hasFrozenOnce = false;
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        }

        private void OnDisable()
        {
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
            isFrozen = false;
        }

        public void SetSpecialType(SpecialCommentType type)
        {
            SpecialType = type;
        }
    }
}