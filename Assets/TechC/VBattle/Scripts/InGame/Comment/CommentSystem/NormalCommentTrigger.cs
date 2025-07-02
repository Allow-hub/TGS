using UnityEngine;

namespace TechC
{
    public class NormalCommentTrigger : MonoBehaviour
    {
        [HideInInspector] public SpecialCommentType specialCommentType;
        [HideInInspector] public string commentText;

        [Header("固定コメントの設定")]
        [SerializeField] private float freezeDuration = 3f;

        private float freezeTimer = 0f;
        private bool isFrozen = false;
        public bool IsFrozen => isFrozen;


        private void OnTriggerEnter(Collider other)
        {
            if (specialCommentType == SpecialCommentType.Freeze && other.CompareTag("Player") && !isFrozen)
            {
                isFrozen = true;
                freezeTimer = freezeDuration;
            }
        }

        private void Update()
        {
            if (isFrozen)
            {
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f)
                {
                    isFrozen = false;
                }
            }
        }
    }
}