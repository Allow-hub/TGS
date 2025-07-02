using UnityEngine;

namespace TechC
{
    public class NormalCommentTrigger : MonoBehaviour
    {
        public SpecialCommentType specialCommentType;
        [HideInInspector] public string commentText;
    }
}