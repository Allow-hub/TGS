using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ログの名前
    /// </summary>
    [CreateAssetMenu(fileName = "LogType", menuName = "Logging/Log Type")]
    public class LogTypeAsset : ScriptableObject
    {
        public string typeName;
    }
}
