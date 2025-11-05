using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnusedScriptChecker : EditorWindow
{
    private string targetFolderPath = "Assets/Scripts";
    private bool includeSubfolders = true;
    private bool checkCurrentSceneOnly = true; // デフォルトを現在のシーンのみに変更
    private bool checkAllScenes = false;
    private bool checkPrefabs = false; // デフォルトをfalseに変更（軽量化）
    private bool verbose = false;
    
    private Vector2 scrollPosition;
    private bool isChecking = false;
    private List<ScriptAnalysisResult> results = new List<ScriptAnalysisResult>();

    [MenuItem("Window/Tools/Unused Script Checker")]
    public static void ShowWindow()
    {
        GetWindow<UnusedScriptChecker>("Unused Script Checker");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Unused Script Checker", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        targetFolderPath = EditorGUILayout.TextField("Target Folder", targetFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                targetFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
        
        // シーン検索オプション
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Search Options", EditorStyles.boldLabel);
        
        checkCurrentSceneOnly = EditorGUILayout.Toggle("Current Scene Only (Fast)", checkCurrentSceneOnly);
        if (checkCurrentSceneOnly)
        {
            checkAllScenes = false;
            EditorGUILayout.HelpBox($"Will search in currently open scene: {GetCurrentSceneName()}", MessageType.Info);
        }
        
        GUI.enabled = !checkCurrentSceneOnly;
        checkAllScenes = EditorGUILayout.Toggle("All Scene Assets (Slow)", checkAllScenes);
        GUI.enabled = true;
        
        checkPrefabs = EditorGUILayout.Toggle("Check Prefabs", checkPrefabs);
        verbose = EditorGUILayout.Toggle("Verbose Logging", verbose);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 現在のシーン情報を表示
        if (checkCurrentSceneOnly)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Current Scene Info", EditorStyles.boldLabel);
            
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.isLoaded)
            {
                EditorGUILayout.LabelField($"Scene Name: {activeScene.name}");
                EditorGUILayout.LabelField($"Scene Path: {activeScene.path}");
                EditorGUILayout.LabelField($"Root Objects: {activeScene.rootCount}");
                
                // シーン内のGameObject数を表示
                int totalGameObjects = CountGameObjectsInScene(activeScene);
                EditorGUILayout.LabelField($"Total GameObjects: {totalGameObjects}");
            }
            else
            {
                EditorGUILayout.HelpBox("No scene is currently loaded!", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Check button
        GUI.enabled = !isChecking && Directory.Exists(targetFolderPath);
        if (GUILayout.Button(isChecking ? "Checking..." : "Check for Unused Scripts", GUILayout.Height(30)))
        {
            StartCheck();
        }
        GUI.enabled = true;

        if (isChecking)
        {
            EditorGUILayout.HelpBox("Checking scripts... Please wait.", MessageType.Info);
        }

        // Results
        if (results.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Results ({results.Count} scripts analyzed)", EditorStyles.boldLabel);
            
            var unusedCount = results.Count(r => r.IsUnused);
            var usedCount = results.Count - unusedCount;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Unused: {unusedCount}", GUILayout.Width(100));
            EditorGUILayout.LabelField($"Used: {usedCount}", GUILayout.Width(100));
            if (GUILayout.Button("Clear Results"))
            {
                results.Clear();
            }
            EditorGUILayout.EndHorizontal();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var result in results.OrderBy(r => r.IsUnused ? 0 : 1)) // 未使用を上に表示
            {
                DrawResultEntry(result);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private string GetCurrentSceneName()
    {
        var activeScene = SceneManager.GetActiveScene();
        return activeScene.isLoaded ? activeScene.name : "No Scene Loaded";
    }

    private int CountGameObjectsInScene(Scene scene)
    {
        int count = 0;
        var rootObjects = scene.GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            count += CountGameObjectsRecursive(root);
        }
        return count;
    }

    private int CountGameObjectsRecursive(GameObject go)
    {
        int count = 1; // 自分自身
        for (int i = 0; i < go.transform.childCount; i++)
        {
            count += CountGameObjectsRecursive(go.transform.GetChild(i).gameObject);
        }
        return count;
    }

    private void DrawResultEntry(ScriptAnalysisResult result)
    {
        Color originalColor = GUI.backgroundColor;
        
        if (result.IsUnused)
        {
            GUI.backgroundColor = Color.green;
        }
        else if (result.HasSceneReferences || result.HasCodeReferences)
        {
            GUI.backgroundColor = Color.red;
        }
        else
        {
            GUI.backgroundColor = Color.yellow;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField(result.ClassName, EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField($"Scene: {(result.HasSceneReferences ? "Yes" : "No")}", GUILayout.Width(80));
        EditorGUILayout.LabelField($"Code: {(result.HasCodeReferences ? "Yes" : "No")}", GUILayout.Width(80));
        
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(result.FilePath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (result.IsUnused)
        {
            EditorGUILayout.LabelField("Status: UNUSED - Safe to delete", EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.LabelField($"Status: IN USE - {result.GetUsageDescription()}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.LabelField($"Path: {result.FilePath}", EditorStyles.miniLabel);
        
        // 詳細な参照情報を表示
        if (result.SceneReferences.Count > 0)
        {
            EditorGUILayout.LabelField($"Found in: {string.Join(", ", result.SceneReferences)}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndVertical();
        
        GUI.backgroundColor = originalColor;
    }

    private async void StartCheck()
    {
        isChecking = true;
        results.Clear();
        
        try
        {
            Debug.Log($"<color=cyan>Starting unused script check in: {targetFolderPath}</color>");
            
            if (checkCurrentSceneOnly)
            {
                var activeScene = SceneManager.GetActiveScene();
                if (!activeScene.isLoaded)
                {
                    Debug.LogError("No scene is currently loaded!");
                    return;
                }
                Debug.Log($"<color=cyan>Checking current scene only: {activeScene.name}</color>");
            }
            
            // Get all .cs files
            var csFiles = GetCsFiles();
            Debug.Log($"<color=cyan>Found {csFiles.Count} .cs files to analyze</color>");

            // Analyze each file
            for (int i = 0; i < csFiles.Count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Analyzing Scripts", 
                    $"Analyzing {Path.GetFileName(csFiles[i])} ({i + 1}/{csFiles.Count})", 
                    (float)i / csFiles.Count))
                {
                    break;
                }

                var result = AnalyzeScript(csFiles[i]);
                if (result != null)
                {
                    results.Add(result);
                    
                    if (result.IsUnused)
                    {
                        Debug.Log($"<color=green>UNUSED SCRIPT FOUND: {result.ClassName} ({result.FilePath}) - Safe to delete!</color>");
                    }
                    else if (verbose)
                    {
                        Debug.Log($"<color=yellow>Script in use: {result.ClassName} - {result.GetUsageDescription()}</color>");
                    }
                }
            }

            // Summary
            var unusedCount = results.Count(r => r.IsUnused);
            var usedCount = results.Count - unusedCount;
            
            Debug.Log($"<color=cyan>Analysis complete! {unusedCount} unused scripts found out of {results.Count} total.</color>");
            
            if (unusedCount > 0)
            {
                Debug.Log($"<color=green>=== UNUSED SCRIPTS (Safe to delete) ===</color>");
                foreach (var unused in results.Where(r => r.IsUnused))
                {
                    Debug.Log($"<color=green>• {unused.ClassName} ({unused.FilePath})</color>");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during script analysis: {ex.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            isChecking = false;
            Repaint();
        }
    }

    private List<string> GetCsFiles()
    {
        var files = new List<string>();
        
        if (!Directory.Exists(targetFolderPath))
        {
            Debug.LogWarning($"Directory not found: {targetFolderPath}");
            return files;
        }

        var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        
        try
        {
            var foundFiles = Directory.GetFiles(targetFolderPath, "*.cs", searchOption);
            files.AddRange(foundFiles.Where(f => !f.EndsWith(".meta")));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting .cs files: {ex.Message}");
        }

        return files;
    }

    private ScriptAnalysisResult AnalyzeScript(string filePath)
    {
        try
        {
            // Convert to Unity asset path
            string assetPath = filePath;
            if (filePath.StartsWith(Application.dataPath))
            {
                assetPath = "Assets" + filePath.Substring(Application.dataPath.Length);
            }

            // Load as MonoScript
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            if (monoScript == null)
            {
                if (verbose) Debug.LogWarning($"Could not load MonoScript for: {assetPath}");
                return null;
            }

            var scriptClass = monoScript.GetClass();
            if (scriptClass == null)
            {
                if (verbose) Debug.LogWarning($"Could not get class for script: {assetPath}");
                return null;
            }

            var result = new ScriptAnalysisResult
            {
                FilePath = assetPath,
                ClassName = scriptClass.Name,
                FullClassName = scriptClass.FullName ?? scriptClass.Name,
                ScriptType = scriptClass
            };

            // Check scene references
            result.HasSceneReferences = CheckSceneReferences(scriptClass, result);

            // Check code references
            result.HasCodeReferences = CheckCodeReferences(scriptClass, assetPath);

            return result;
        }
        catch (Exception ex)
        {
            if (verbose) Debug.LogWarning($"Error analyzing script {filePath}: {ex.Message}");
            return null;
        }
    }

    private bool CheckSceneReferences(Type scriptClass, ScriptAnalysisResult result)
    {
        try
        {
            bool hasReferences = false;

            // Check current scene only
            if (checkCurrentSceneOnly)
            {
                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.isLoaded)
                {
                    if (HasScriptInScene(activeScene, scriptClass, result))
                    {
                        hasReferences = true;
                    }
                }
            }
            // Check all open scenes
            else if (checkAllScenes)
            {
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);
                    if (scene.isLoaded && HasScriptInScene(scene, scriptClass, result))
                    {
                        hasReferences = true;
                    }
                }
            }

            // Check prefabs if enabled
            if (checkPrefabs)
            {
                var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
                foreach (var guid in prefabGuids)
                {
                    var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null && HasScriptInPrefab(prefab, scriptClass, result))
                    {
                        hasReferences = true;
                    }
                }
            }

            return hasReferences;
        }
        catch (Exception ex)
        {
            if (verbose) Debug.LogWarning($"Error checking scene references for {scriptClass.Name}: {ex.Message}");
            return false;
        }
    }

    private bool HasScriptInScene(Scene scene, Type scriptClass, ScriptAnalysisResult result)
    {
        var rootObjects = scene.GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            if (HasScriptInGameObject(root, scriptClass, result, $"Scene '{scene.name}'"))
            {
                return true;
            }
        }
        return false;
    }

    private bool HasScriptInPrefab(GameObject prefab, Type scriptClass, ScriptAnalysisResult result)
    {
        return HasScriptInGameObject(prefab, scriptClass, result, "Prefab");
    }

    private bool HasScriptInGameObject(GameObject go, Type scriptClass, ScriptAnalysisResult result, string context)
    {
        bool found = false;
        
        // Check this GameObject
        var components = go.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp != null && comp.GetType() == scriptClass)
            {
                string objectPath = GetGameObjectPath(go);
                result.SceneReferences.Add($"{context}: {objectPath}");
                found = true;
            }
        }

        // Check children recursively
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (HasScriptInGameObject(go.transform.GetChild(i).gameObject, scriptClass, result, context))
            {
                found = true;
            }
        }

        return found;
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

    private bool CheckCodeReferences(Type scriptClass, string excludePath)
    {
        try
        {
            // Get all .cs files in the project
            var allCsFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Equals(excludePath, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            string className = scriptClass.Name;
            string fullClassName = scriptClass.FullName ?? className;

            foreach (var filePath in allCsFiles)
            {
                try
                {
                    string content = File.ReadAllText(filePath);
                    
                    // Simple text-based search for class references
                    if (HasClassReference(content, className, fullClassName))
                    {
                        return true;
                    }
                }
                catch
                {
                    // Skip files that can't be read
                    continue;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            if (verbose) Debug.LogWarning($"Error checking code references for {scriptClass.Name}: {ex.Message}");
            return false;
        }
    }

    private bool HasClassReference(string content, string className, string fullClassName)
    {
        // Remove comments and strings to avoid false positives
        content = RemoveCommentsAndStrings(content);

        // Patterns to look for class usage
        var patterns = new[]
        {
            $@"\b{Regex.Escape(className)}\b",
            $@"\b{Regex.Escape(fullClassName)}\b",
            $@"typeof\s*\(\s*{Regex.Escape(className)}\s*\)",
            $@"typeof\s*\(\s*{Regex.Escape(fullClassName)}\s*\)",
            $@"new\s+{Regex.Escape(className)}\s*\(",
            $@"<\s*{Regex.Escape(className)}\s*>",
            $@":\s*{Regex.Escape(className)}\b",
            $@"{Regex.Escape(className)}\s*\.",
        };

        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private string RemoveCommentsAndStrings(string code)
    {
        // Remove single-line comments
        code = Regex.Replace(code, @"//.*?$", "", RegexOptions.Multiline);
        
        // Remove multi-line comments
        code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        // Remove string literals
        code = Regex.Replace(code, @""".*?""", "", RegexOptions.Singleline);
        code = Regex.Replace(code, @"'.*?'", "", RegexOptions.Singleline);
        
        return code;
    }

    private class ScriptAnalysisResult
    {
        public string FilePath;
        public string ClassName;
        public string FullClassName;
        public Type ScriptType;
        public bool HasSceneReferences;
        public bool HasCodeReferences;
        public List<string> SceneReferences = new List<string>();

        public bool IsUnused => !HasSceneReferences && !HasCodeReferences;

        public string GetUsageDescription()
        {
            var usages = new List<string>();
            if (HasSceneReferences) usages.Add("Scene/Prefab");
            if (HasCodeReferences) usages.Add("Code");
            return string.Join(", ", usages);
        }
    }
}