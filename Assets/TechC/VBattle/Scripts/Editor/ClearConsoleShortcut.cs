using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace TechC
{
    public static class ClearConsoleShortcut
    {
        [MenuItem("Tools/Clear Console %#k")] // Ctrl + Shift + K (Windows) / Cmd + Shift + K (Mac)
        public static void ClearConsole()
        {
            // ConsoleWindow 型の取得
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
    }

}
