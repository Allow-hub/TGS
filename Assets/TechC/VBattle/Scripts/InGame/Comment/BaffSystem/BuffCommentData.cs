using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// バフコメントデータを格納するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "BuffCommentData", menuName = "TechC/Comment/Buff")]
    public class BuffCommentData : ScriptableObject
    {
        [Header("バフの種類")]
        public BuffType buffType;
        [TextArea]
        public string[] comments;

        [Header("エフェクトのプレハブ")]
        public GameObject effectPrefab;
    }
}
