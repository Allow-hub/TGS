using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 通常コメントを管理する ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NormalComment", menuName = "TechC/Comment/Special")]

    public class SpecialCommentData : ScriptableObject
    {
        public string[] comment;
        public SpecialCommentType specialType;
    }
}
