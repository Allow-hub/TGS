using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ログの可視化を管理するマネージャー
    /// </summary>
    public static class LogManager
    {
        private static LogSettings settings;

        public static void SetSettings(LogSettings logSettings)
        {
            settings = logSettings;
        }

        public static void Log(string message, LogTypeAsset type)
        {
            if (settings == null)
            {
                Debug.LogWarning("LogSettingsが未設定です。LogManager.SetSettings を呼び出してください。");
                return;
            }

            if (type != null && settings.IsEnabled(type))
            {
                Debug.Log($"[{type.typeName}] {message}");
            }
        }
    }
}