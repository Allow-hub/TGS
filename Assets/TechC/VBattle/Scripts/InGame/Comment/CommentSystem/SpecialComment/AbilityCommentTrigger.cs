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
        
        private void OnTriggerEnter(Collider other)
        {
            if (SpecialType == SpecialCommentType.Freeze && other.CompareTag("Player") && !isFrozen)
            {
                isFrozen = true;
                if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
                freezeCoroutine = StartCoroutine(FreezeTimerCoroutine());
            }
        }

        private IEnumerator FreezeTimerCoroutine()
        {
            yield return new WaitForSeconds(freezeDuration);
            isFrozen = false;
        }

        private void OnDisable()
        {
            isFrozen = false;
            if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        }
    }
}