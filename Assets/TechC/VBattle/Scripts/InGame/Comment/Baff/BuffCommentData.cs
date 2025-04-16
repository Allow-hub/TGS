using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    [CreateAssetMenu(fileName = "BuffCommentData", menuName = "TechC/Comment/Buff")]
    public class BuffCommentData : ScriptableObject
    {
        public BuffType buffType;
        [TextArea]
        public string[] comments;
    }
}
