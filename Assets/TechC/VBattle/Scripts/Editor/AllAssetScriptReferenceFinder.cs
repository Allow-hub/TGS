using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TechC
{
    /// <summary>
    /// Hierarchy だけでなく、プロジェクト全体から指定した MonoBehaviour スクリプトを参照している
    /// プレハブやシーン内オブジェクトを検索・表示するエディタ拡張ウィンドウ。
    /// スクリプトを選択して「Find」ボタンを押すと、関連する全ての GameObject をリストアップし、
    /// クリックでそのアセットをプロジェクト内から選択・ハイライトできる。
    /// </summary>
    public class AllAssetScriptReferenceFinder : EditorWindow
    {
        private MonoScript targetScript; // 検索対象のスクリプト
        private List<Object> foundObjects = new List<Object>(); // 見つかったオブジェクトのリスト

        [MenuItem("Tools/All Asset Script Reference Finder")]
        public static void OpenWindow()
        {
            GetWindow<AllAssetScriptReferenceFinder>("All Asset Script Reference Finder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Find All Objects/Prefabs with Script", EditorStyles.boldLabel);

            // スクリプトを選択するフィールド
            targetScript = (MonoScript)EditorGUILayout.ObjectField("Script", targetScript, typeof(MonoScript), false);

            if (GUILayout.Button("Find"))
            {
                if (targetScript != null)
                {
                    FindAllAssetsWithScript();
                }
                else
                {
                    Debug.LogWarning("Please select a script to search for.");
                }
            }

            GUILayout.Space(10);

            // 検索結果のリスト表示
            if (foundObjects.Count > 0)
            {
                GUILayout.Label($"Found {foundObjects.Count} objects:", EditorStyles.boldLabel);

                foreach (Object obj in foundObjects)
                {
                    if (GUILayout.Button(AssetDatabase.GetAssetPath(obj) + " / " + obj.name))
                    {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }
            }
        }

        private void FindAllAssetsWithScript()
        {
            foundObjects.Clear();

            // スクリプトの型を取得
            System.Type scriptType = targetScript.GetClass();
            if (scriptType == null || !typeof(MonoBehaviour).IsAssignableFrom(scriptType))
            {
                Debug.LogError("Selected script is not a MonoBehaviour.");
                return;
            }

            // Project内の全アセットパスを取得
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string path in allAssetPaths)
            {
                if (path.EndsWith(".prefab") || path.EndsWith(".unity"))
                {
                    var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var asset in assets)
                    {
                        if (asset is GameObject go)
                        {
                            if (go.GetComponent(scriptType) != null)
                            {
                                foundObjects.Add(go);
                            }
                        }
                    }
                }
            }

            // シーン内の全てのGameObjectも検索
            GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in sceneObjects)
            {
                if (obj.GetComponent(scriptType) != null)
                {
                    foundObjects.Add(obj);
                }
            }

            Debug.Log($"Found {foundObjects.Count} objects with the script {targetScript.name}.");
        }
    }
}
