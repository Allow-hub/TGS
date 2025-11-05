using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonoBehaviourUnusedFinder : EditorWindow
{
    private string targetFolderPath = "Assets/TechC/VBattle/Scripts/Select";
    private bool includeSubfolders = true;
    private Vector2 scrollPosition;
    private List<UnusedMonoBehaviourInfo> unusedScripts = new List<UnusedMonoBehaviourInfo>();
    private bool hasAnalyzed = false;

    [MenuItem("Window/Tools/MonoBehaviour Unused Finder")]
    public static void ShowWindow()
    {
        GetWindow<MonoBehaviourUnusedFinder>("MonoBehaviour Unused Finder");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("MonoBehaviour Unused Finder", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Find MonoBehaviour scripts not attached in current scene", MessageType.Info);
        EditorGUILayout.Space();

        // Folder Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Target Folder Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        targetFolderPath = EditorGUILayout.TextField("Script Folder", targetFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Script Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                targetFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
        EditorGUILayout.EndVertical();

        // Current Scene Info
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Current Scene", EditorStyles.boldLabel);
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.isLoaded)
        {
            EditorGUILayout.LabelField($"Scene: {activeScene.name}");
            EditorGUILayout.LabelField($"Path: {activeScene.path}");
        }
        else
        {
            EditorGUILayout.HelpBox("No scene is currently loaded!", MessageType.Warning);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Find button
        GUI.enabled = activeScene.isLoaded && Directory.Exists(targetFolderPath);
        if (GUILayout.Button("Find Unused MonoBehaviour Scripts", GUILayout.Height(30)))
        {
            FindUnusedMonoBehaviours();
        }
        GUI.enabled = true;

        if (!Directory.Exists(targetFolderPath))
        {
            EditorGUILayout.HelpBox($"Folder not found: {targetFolderPath}", MessageType.Error);
        }

        // Results
        if (hasAnalyzed)
        {
            EditorGUILayout.Space();
            DisplayResults();
        }
    }

    private void FindUnusedMonoBehaviours()
    {
        unusedScripts.Clear();
        hasAnalyzed = false;

        Debug.Log($"<color=cyan>Searching for unused MonoBehaviour scripts in: {targetFolderPath}</color>");

        try
        {
            // Step 1: Get all MonoBehaviour scripts in target folder
            var monoBehaviourScripts = GetMonoBehaviourScriptsInFolder();
            Debug.Log($"<color=cyan>Found {monoBehaviourScripts.Count} MonoBehaviour scripts in folder</color>");

            if (monoBehaviourScripts.Count == 0)
            {
                Debug.Log($"<color=yellow>No MonoBehaviour scripts found in {targetFolderPath}</color>");
                hasAnalyzed = true;
                Repaint();
                return;
            }

            // Step 2: Get scripts used in current scene
            var usedScriptsInScene = GetUsedMonoBehavioursInScene();
            Debug.Log($"<color=cyan>Found {usedScriptsInScene.Count} MonoBehaviour types used in current scene</color>");

            // Step 3: Find unused scripts
            foreach (var script in monoBehaviourScripts)
            {
                if (!usedScriptsInScene.Contains(script.className))
                {
                    unusedScripts.Add(script);
                }
            }

            // Step 4: Log results
            if (unusedScripts.Count > 0)
            {
                Debug.Log($"<color=red>=== UNUSED MONOBEHAVIOUR SCRIPTS (Safe to delete) ===</color>");
                foreach (var unused in unusedScripts)
                {
                    Debug.Log($"<color=red>• {unused.scriptName} ({unused.filePath})</color>");
                }
                Debug.Log($"<color=red>Total unused: {unusedScripts.Count} scripts</color>");
            }
            else
            {
                Debug.Log($"<color=green>All MonoBehaviour scripts in {targetFolderPath} are being used in the current scene!</color>");
            }

            hasAnalyzed = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during analysis: {ex.Message}");
        }
        
        Repaint();
    }

    private List<UnusedMonoBehaviourInfo> GetMonoBehaviourScriptsInFolder()
    {
        var monoBehaviourScripts = new List<UnusedMonoBehaviourInfo>();
        
        if (!Directory.Exists(targetFolderPath))
            return monoBehaviourScripts;

        var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var csFiles = Directory.GetFiles(targetFolderPath, "*.cs", searchOption);

        foreach (var filePath in csFiles)
        {
            string assetPath = filePath;
            if (filePath.StartsWith(Application.dataPath))
            {
                assetPath = "Assets" + filePath.Substring(Application.dataPath.Length);
            }

            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (monoScript != null)
            {
                var scriptClass = monoScript.GetClass();
                if (scriptClass != null && typeof(MonoBehaviour).IsAssignableFrom(scriptClass))
                {
                    monoBehaviourScripts.Add(new UnusedMonoBehaviourInfo
                    {
                        scriptName = monoScript.name,
                        className = scriptClass.Name,
                        filePath = assetPath,
                        scriptType = scriptClass
                    });
                }
            }
        }

        return monoBehaviourScripts;
    }

    private HashSet<string> GetUsedMonoBehavioursInScene()
    {
        var usedScripts = new HashSet<string>();
        var activeScene = SceneManager.GetActiveScene();
        
        if (!activeScene.isLoaded)
            return usedScripts;

        var allGameObjects = activeScene.GetRootGameObjects();
        
        foreach (var rootGO in allGameObjects)
        {
            CollectUsedMonoBehaviours(rootGO, usedScripts);
        }

        return usedScripts;
    }

    private void CollectUsedMonoBehaviours(GameObject go, HashSet<string> usedScripts)
    {
        // Check components on this GameObject
        var components = go.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component != null)
            {
                usedScripts.Add(component.GetType().Name);
            }
        }

        // Check children recursively
        for (int i = 0; i < go.transform.childCount; i++)
        {
            CollectUsedMonoBehaviours(go.transform.GetChild(i).gameObject, usedScripts);
        }
    }

    private void DisplayResults()
    {
        if (unusedScripts.Count == 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("✅ No unused MonoBehaviour scripts found!", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("All scripts in the target folder are being used in the current scene.", EditorStyles.helpBox);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.LabelField($"🗑️ Unused MonoBehaviour Scripts: {unusedScripts.Count}", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("These scripts are not attached to any GameObject in the current scene and can be safely deleted.", MessageType.Warning);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var unused in unusedScripts)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            // Script name with red background
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            EditorGUILayout.LabelField(unused.scriptName, EditorStyles.boldLabel, GUILayout.Width(200));
            GUI.backgroundColor = originalColor;

            EditorGUILayout.LabelField("UNUSED", GUILayout.Width(80));

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(unused.filePath);
                if (asset != null)
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Script", 
                    $"Are you sure you want to delete '{unused.scriptName}'?\n\nPath: {unused.filePath}", 
                    "Delete", "Cancel"))
                {
                    AssetDatabase.DeleteAsset(unused.filePath);
                    AssetDatabase.Refresh();
                    
                    Debug.Log($"<color=orange>Deleted script: {unused.scriptName}</color>");
                    
                    // Re-run analysis
                    FindUnusedMonoBehaviours();
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"Path: {unused.filePath}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("⚠️ Safe to delete - Not attached to any GameObject in current scene", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        // Bulk delete button
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button($"🗑️ Delete All {unusedScripts.Count} Unused Scripts", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Delete All Unused Scripts", 
                $"Are you sure you want to delete all {unusedScripts.Count} unused MonoBehaviour scripts?\n\nThis action cannot be undone!", 
                "Delete All", "Cancel"))
            {
                int deletedCount = 0;
                foreach (var unused in unusedScripts)
                {
                    AssetDatabase.DeleteAsset(unused.filePath);
                    deletedCount++;
                    Debug.Log($"<color=orange>Deleted script: {unused.scriptName}</color>");
                }
                
                AssetDatabase.Refresh();
                Debug.Log($"<color=orange>Successfully deleted {deletedCount} unused MonoBehaviour scripts!</color>");
                
                // Clear results
                unusedScripts.Clear();
                hasAnalyzed = false;
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private class UnusedMonoBehaviourInfo
    {
        public string scriptName;
        public string className;
        public string filePath;
        public Type scriptType;
    }
}