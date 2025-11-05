using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptReferenceFinder : EditorWindow
{
    MonoScript targetScript;
    string typeNameFilter = "";
    bool includeOpenScenes = true;
    bool includeAllSceneAssets = false; // not implemented heavy scan
    bool includePrefabs = true;
    bool includeAssets = true;
    bool includeCodeSearch = true;
    bool includeMissingScripts = true;

    List<ResultEntry> sceneResults = new List<ResultEntry>();
    List<ResultEntry> prefabResults = new List<ResultEntry>();
    List<ResultEntry> assetResults = new List<ResultEntry>();
    List<CodeResult> codeResults = new List<CodeResult>();

    Vector2 scroll;

    bool isSearching = false;
    bool cancelRequested = false;

    [MenuItem("Window/Tools/Script Reference Finder")]
    public static void OpenWindow()
    {
        var w = GetWindow<ScriptReferenceFinder>("Script Ref Finder");
        w.minSize = new Vector2(600, 400);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Script / Type Reference Finder", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");
        targetScript = (MonoScript)EditorGUILayout.ObjectField("Target Script (MonoScript)", targetScript, typeof(MonoScript), false);
        EditorGUILayout.LabelField("or enter type name (partial or full):");
        typeNameFilter = EditorGUILayout.TextField("Type name", typeNameFilter);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        includeOpenScenes = EditorGUILayout.ToggleLeft("Search open scenes", includeOpenScenes);
        includePrefabs = EditorGUILayout.ToggleLeft("Search prefab assets", includePrefabs);
        includeAssets = EditorGUILayout.ToggleLeft("Search other assets (ScriptableObjects etc.)", includeAssets);
        includeCodeSearch = EditorGUILayout.ToggleLeft("Search .cs files (text search)", includeCodeSearch);
        includeMissingScripts = EditorGUILayout.ToggleLeft("Report missing scripts in scenes/prefabs", includeMissingScripts);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !isSearching;
        if (GUILayout.Button("Search"))
        {
            StartSearch();
        }
        GUI.enabled = isSearching;
        if (GUILayout.Button("Cancel"))
        {
            cancelRequested = true;
        }
        GUI.enabled = true;
        if (GUILayout.Button("Clear Results"))
        {
            ClearResults();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (isSearching)
        {
            EditorGUILayout.HelpBox("Searching... (progress bar shown) - you can cancel", MessageType.Info);
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawResultsSection("Scene Results", sceneResults);
        DrawResultsSection("Prefab Results", prefabResults);
        DrawResultsSection("Asset Reference Results", assetResults);
        DrawCodeResultsSection("Code Results", codeResults);
        EditorGUILayout.EndScrollView();
    }

    void DrawResultsSection(string title, List<ResultEntry> list)
    {
        EditorGUILayout.LabelField($"{title} ({list.Count})", EditorStyles.boldLabel);
        foreach (var r in list)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField(r.summary, GUILayout.Width(position.width - 220));
            if (GUILayout.Button("Ping", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(r.unityObject);
            }
            if (GUILayout.Button("Select", GUILayout.Width(80)))
            {
                Selection.activeObject = r.unityObject;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawCodeResultsSection(string title, List<CodeResult> list)
    {
        EditorGUILayout.LabelField($"{title} ({list.Count})", EditorStyles.boldLabel);
        foreach (var c in list)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"{c.path} (line {c.line})", GUILayout.Width(position.width - 220));
            if (GUILayout.Button("Open", GUILayout.Width(80)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(c.path);
                if (asset != null)
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(c.path);
                    AssetDatabase.OpenAsset(obj);
                }
                else
                {
                    // Fallback: open with OS
                    EditorUtility.RevealInFinder(c.path);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void StartSearch()
    {
        ClearResults();
        cancelRequested = false;
        isSearching = true;
        try
        {
            Type targetType = ResolveTargetType();
            if (targetType == null)
            {
                if (targetScript == null && string.IsNullOrEmpty(typeNameFilter))
                {
                    EditorUtility.DisplayDialog("Error", "Please specify a MonoScript or type name.", "OK");
                    isSearching = false;
                    return;
                }
                // Continue: will still search for references to the MonoScript asset if set
            }

            // Run heavy work in coroutine-like fashion using EditorApplication.update
            EditorApplication.update += () => SearchCoroutineStep(targetType);
        }
        catch (Exception ex)
        {
            Debug.LogError("Search start failed: " + ex);
            isSearching = false;
        }
    }

    int coroutineState = 0;
    Type coroutineTargetType;
    string[] allPrefabGuids;
    string[] allAssetGuids;
    string[] allCsFiles;
    int idxGlob = 0;

    void SearchCoroutineStep(Type resolvedType)
    {
        try
        {
            if (cancelRequested)
            {
                EditorApplication.update -= () => SearchCoroutineStep(resolvedType);
                FinishSearch();
                return;
            }

            if (coroutineState == 0)
            {
                coroutineTargetType = resolvedType;
                idxGlob = 0;
                // prepare lists
                if (includePrefabs)
                {
                    allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
                }
                else allPrefabGuids = new string[0];

                if (includeAssets)
                {
                    allAssetGuids = AssetDatabase.FindAssets(""); // all assets (could be heavy)
                }
                else allAssetGuids = new string[0];

                if (includeCodeSearch)
                {
                    // enumerate .cs under Assets
                    try
                    {
                        var projectPath = Application.dataPath;
                        allCsFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                                             .Select(p => "Assets" + p.Substring(projectPath.Length)).ToArray();
                    }
                    catch
                    {
                        allCsFiles = new string[0];
                    }
                }
                else allCsFiles = new string[0];

                coroutineState = 10;
            }

            // 10: Scan open scenes
            if (coroutineState == 10)
            {
                if (includeOpenScenes)
                {
                    int sceneCount = EditorSceneManager.sceneCount;
                    for (int si = 0; si < sceneCount; si++)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Scanning scenes", $"Scene {si + 1}/{sceneCount}", (float)si / sceneCount))
                        {
                            cancelRequested = true;
                            break;
                        }
                        Scene scene = EditorSceneManager.GetSceneAt(si);
                        if (!scene.isLoaded) continue;
                        var roots = scene.GetRootGameObjects();
                        foreach (var root in roots)
                            TraverseGameObject(root, sceneResults, scene.path, coroutineTargetType);
                    }
                }
                EditorUtility.ClearProgressBar();
                coroutineState = 20;
            }

            if (cancelRequested)
            {
                EditorApplication.update -= () => SearchCoroutineStep(resolvedType);
                FinishSearch();
                return;
            }

            // 20: Scan prefab assets
            if (coroutineState == 20)
            {
                int total = allPrefabGuids.Length;
                for (int i = 0; i < total; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Scanning prefabs", $"Prefab {i + 1}/{total}", (float)i / Math.Max(1, total)))
                    {
                        cancelRequested = true;
                        break;
                    }
                    string path = AssetDatabase.GUIDToAssetPath(allPrefabGuids[i]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null) continue;
                    // Use GetComponentsInChildren on the prefab asset
                    var comps = prefab.GetComponentsInChildren<Component>(true);
                    foreach (var c in comps)
                    {
                        if (c == null)
                        {
                            if (includeMissingScripts)
                            {
                                // We cannot determine which missing script it was here; report prefab has missing script
                                AddPrefabMissingScript(path);
                                break;
                            }
                        }
                        else if (coroutineTargetType != null && coroutineTargetType.IsAssignableFrom(c.GetType()))
                        {
                            var go = c.gameObject;
                            prefabResults.Add(new ResultEntry
                            {
                                summary = $"Prefab: {path} -> {GetGameObjectPath(go)} (component {c.GetType().Name})",
                                unityObject = prefab
                            });
                            break;
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
                coroutineState = 30;
            }

            if (cancelRequested)
            {
                EditorApplication.update -= () => SearchCoroutineStep(resolvedType);
                FinishSearch();
                return;
            }

            // 30: Scan assets (serialized properties)
            if (coroutineState == 30)
            {
                int total = allAssetGuids.Length;
                for (int i = 0; i < total; i++)
                {
                    if (i % 50 == 0)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Scanning assets (serialized refs)", $"Asset {i + 1}/{total}", (float)i / Math.Max(1, total)))
                        {
                            cancelRequested = true;
                            break;
                        }
                    }

                    string path = AssetDatabase.GUIDToAssetPath(allAssetGuids[i]);
                    if (string.IsNullOrEmpty(path)) continue;
                    // skip script files and meta files (we'll handle code separately)
                    if (path.EndsWith(".cs") || path.EndsWith(".js") || path.EndsWith(".dll") || path.EndsWith(".meta")) continue;

                    UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                    if (obj == null) continue;

                    try
                    {
                        SerializedObject so = new SerializedObject(obj);
                        var prop = so.GetIterator();
                        while (prop.Next(true))
                        {
                            if (prop.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                var refObj = prop.objectReferenceValue;
                                if (refObj == null) continue;
                                // If the reference is a GameObject or Component, check its components
                                if (refObj is GameObject goRef)
                                {
                                    var comps = goRef.GetComponentsInChildren<Component>(true);
                                    foreach (var c in comps)
                                    {
                                        if (c != null && coroutineTargetType != null && coroutineTargetType.IsAssignableFrom(c.GetType()))
                                        {
                                            assetResults.Add(new ResultEntry
                                            {
                                                summary = $"Asset: {path} -> serialized GameObject '{goRef.name}' contains component {c.GetType().Name}",
                                                unityObject = obj
                                            });
                                            goto nextAsset; // break out to next asset (we found a ref)
                                        }
                                    }
                                }
                                else if (refObj is Component compRef)
                                {
                                    if (coroutineTargetType != null && coroutineTargetType.IsAssignableFrom(compRef.GetType()))
                                    {
                                        assetResults.Add(new ResultEntry
                                        {
                                            summary = $"Asset: {path} -> serialized Component '{compRef.GetType().Name}'",
                                            unityObject = obj
                                        });
                                        goto nextAsset;
                                    }
                                }
                                else if (refObj is MonoScript ms)
                                {
                                    // direct reference to the script asset
                                    if (targetScript != null && ms == targetScript)
                                    {
                                        assetResults.Add(new ResultEntry
                                        {
                                            summary = $"Asset: {path} -> directly references script asset {ms.name}",
                                            unityObject = obj
                                        });
                                        goto nextAsset;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Some asset types can't be serialized via SerializedObject; ignore
                    }

                nextAsset:
                    continue;
                }
                EditorUtility.ClearProgressBar();
                coroutineState = 40;
            }

            if (cancelRequested)
            {
                EditorApplication.update -= () => SearchCoroutineStep(resolvedType);
                FinishSearch();
                return;
            }

            // 40: Code search (.cs text)
            if (coroutineState == 40 && includeCodeSearch)
            {
                int total = allCsFiles.Length;
                string searchToken = null;
                if (coroutineTargetType != null)
                {
                    searchToken = coroutineTargetType.FullName ?? coroutineTargetType.Name;
                }
                else if (targetScript != null)
                {
                    var cls = targetScript.GetClass();
                    if (cls != null) searchToken = cls.FullName ?? cls.Name;
                    else searchToken = targetScript.name;
                }
                else if (!string.IsNullOrEmpty(typeNameFilter))
                {
                    searchToken = typeNameFilter;
                }
                if (!string.IsNullOrEmpty(searchToken))
                {
                    for (int i = 0; i < total; i++)
                    {
                        if (i % 50 == 0)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar("Scanning code files", $"File {i + 1}/{total}", (float)i / Math.Max(1, total)))
                            {
                                cancelRequested = true;
                                break;
                            }
                        }
                        string path = allCsFiles[i];
                        try
                        {
                            string fullPath = Path.GetFullPath(path);
                            string text = File.ReadAllText(fullPath);
                            // quick contains first
                            if (text.Contains(searchToken))
                            {
                                // find first occurrence line number
                                int idx = text.IndexOf(searchToken, StringComparison.Ordinal);
                                int line = 1;
                                for (int p = 0; p < idx && p < text.Length; p++)
                                    if (text[p] == '\n') line++;
                                codeResults.Add(new CodeResult { path = path, line = line });
                            }
                        }
                        catch { }
                    }
                }
                EditorUtility.ClearProgressBar();
                coroutineState = 50;
            }

            // finish
            EditorApplication.update -= () => SearchCoroutineStep(resolvedType);
            FinishSearch();
        }
        catch (Exception ex)
        {
            Debug.LogError("Search failed: " + ex);
            EditorApplication.update -= () => SearchCoroutineStep(resolvedType);
            FinishSearch();
        }
    }

    void FinishSearch()
    {
        isSearching = false;
        cancelRequested = false;
        coroutineState = 0;
        coroutineTargetType = null;
        allPrefabGuids = null;
        allAssetGuids = null;
        allCsFiles = null;
        EditorUtility.ClearProgressBar();
        Repaint();
    }

    void ClearResults()
    {
        sceneResults.Clear();
        prefabResults.Clear();
        assetResults.Clear();
        codeResults.Clear();
    }

    void AddPrefabMissingScript(string prefabPath)
    {
        // ensure we don't spam multiple entries for same prefab
        if (!prefabResults.Exists(r => r.summary.Contains(prefabPath)))
        {
            prefabResults.Add(new ResultEntry
            {
                summary = $"Prefab: {prefabPath} -> contains missing script",
                unityObject = AssetDatabase.LoadMainAssetAtPath(prefabPath)
            });
        }
    }

    void TraverseGameObject(GameObject go, List<ResultEntry> outList, string contextPath, Type targetType)
    {
        if (cancelRequested) return;
        var comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null)
            {
                if (includeMissingScripts)
                {
                    outList.Add(new ResultEntry
                    {
                        summary = $"Scene: {contextPath} -> {GetGameObjectPath(go)} has missing script",
                        unityObject = go
                    });
                }
            }
            else if (targetType != null && targetType.IsAssignableFrom(c.GetType()))
            {
                outList.Add(new ResultEntry
                {
                    summary = $"Scene: {contextPath} -> {GetGameObjectPath(go)} (component {c.GetType().Name})",
                    unityObject = go
                });
            }
        }
        // recurse
        for (int i = 0; i < go.transform.childCount; i++)
        {
            TraverseGameObject(go.transform.GetChild(i).gameObject, outList, contextPath, targetType);
            if (cancelRequested) return;
        }
    }

    Type ResolveTargetType()
    {
        Type t = null;
        if (targetScript != null)
        {
            t = targetScript.GetClass();
            if (t == null)
            {
                // maybe compilation not finished or class in different assembly, try resolving by name
                string name = targetScript.name;
                t = FindTypeByName(name);
            }
        }
        if (t == null && !string.IsNullOrEmpty(typeNameFilter))
        {
            // Try full name first then by simple name
            t = Type.GetType(typeNameFilter);
            if (t == null)
            {
                t = FindTypeByName(typeNameFilter);
            }
        }
        return t;
    }

    Type FindTypeByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        // try partial match across loaded assemblies
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types = null;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var ty in types)
            {
                if (ty.Name == name || ty.FullName == name || (ty.FullName != null && ty.FullName.EndsWith("." + name)))
                    return ty;
            }
        }
        return null;
    }

    static string GetGameObjectPath(GameObject go)
    {
        string p = go.name;
        Transform t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            p = t.name + "/" + p;
        }
        return p;
    }

    // result types
    class ResultEntry
    {
        public string summary;
        public UnityEngine.Object unityObject;
    }

    class CodeResult
    {
        public string path;
        public int line;
    }
}