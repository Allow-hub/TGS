using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Metadata;

namespace TechC
{
    [CreateAssetMenu(fileName = "NormalComment", menuName = "TechC/Comment/Normal")]
    public class NormalCommentData : ScriptableObject
    {
        public string [] comment;
    }
}
