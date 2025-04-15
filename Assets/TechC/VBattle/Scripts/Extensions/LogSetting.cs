using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ログの可視化設定
    /// </summary>
    [CreateAssetMenu(fileName = "LogSettings", menuName = "Logging/Log Settings")]
    public class LogSettings : ScriptableObject
    {
        [System.Serializable]
        public class LogToggle
        {
            public LogTypeAsset logType;
            public bool isEnabled;
        }

        public LogToggle[] logToggles;

        public bool IsEnabled(LogTypeAsset type)
        {
            foreach (var toggle in logToggles)
            {
                if (toggle.logType == type)
                    return toggle.isEnabled;
            }
            return false; // デフォルト非表示
        }
    }

}
