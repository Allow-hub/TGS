using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ログの表示設定
    /// </summary>
    public class LogInitializer : MonoBehaviour
    {
        [SerializeField] private LogSettings logSettings;

        void Awake()
        {
            LogManager.SetSettings(logSettings);
        }
    }

}
