using UnityEngine;
using UnityEditor;

namespace TechC
{
    /// <summary>
    /// シーン内のMissing Scriptを持つGameObjectを検索して一覧表示するツール
    /// </summary>
    public class MissingScriptFinder : EditorWindow
    {
        private Vector2 scrollPos;
        private string result = "";

        [MenuItem("Tools/Missing Script Finder")]
        public static void ShowWindow()
        {
            GetWindow<MissingScriptFinder>("Missing Script Finder");
        }

        private void OnGUI()
        {

            if (GUILayout.Button("Missing Scriptを検索"))
            {
                FindMissingScripts();
            }

            EditorGUILayout.LabelField("検索結果:", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.TextArea(result);
            EditorGUILayout.EndScrollView();
        }

        private void FindMissingScripts()
        {
            int goCount = 0;
            int componentsCount = 0;
            int missingCount = 0;
            result = "";

            GameObject[] gos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (GameObject go in gos)
            {
                goCount++;
                Component[] components = go.GetComponents<Component>();
                componentsCount += components.Length;

                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        missingCount++;
                        string path = GetGameObjectPath(go);
                        string log = $"Missing Script発見: {path}\n";
                        Debug.Log(log, go);
                        result += log;
                    }
                }
            }

            result += $"合計検索対象: {goCount}個のGameObject, {componentsCount}個のコンポーネント\n";
            result += $"Missing Script合計: {missingCount}個\n";
        }

        private string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }
    }
}
