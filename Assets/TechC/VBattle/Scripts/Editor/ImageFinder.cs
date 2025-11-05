using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImageUsageAnalyzer : EditorWindow
{
    private string targetImageFolder = "Assets/Textures/UI";
    private bool includeSubfolders = true;
    private Vector2 scrollPosition;
    private List<ImageAssetInfo> analysisResults = new List<ImageAssetInfo>();
    private bool hasAnalyzed = false;

    [MenuItem("Window/Tools/Image Usage Analyzer")]
    public static void ShowWindow()
    {
        GetWindow<ImageUsageAnalyzer>("Image Usage Analyzer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Image Usage Analyzer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Compare images in folder vs scene usage to find deletable assets", MessageType.Info);
        EditorGUILayout.Space();

        // Folder Settings
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Image Folder Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        targetImageFolder = EditorGUILayout.TextField("Image Folder", targetImageFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Image Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                targetImageFolder = "Assets" + path.Substring(Application.dataPath.Length);
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

        // Analyze button
        GUI.enabled = activeScene.isLoaded && Directory.Exists(targetImageFolder);
        if (GUILayout.Button("Analyze Image Usage", GUILayout.Height(30)))
        {
            AnalyzeImageUsage();
        }
        GUI.enabled = true;

        if (!Directory.Exists(targetImageFolder))
        {
            EditorGUILayout.HelpBox($"Folder not found: {targetImageFolder}", MessageType.Error);
        }

        // Results
        if (hasAnalyzed)
        {
            EditorGUILayout.Space();
            DisplayResults();
        }
    }

    private void AnalyzeImageUsage()
    {
        analysisResults.Clear();
        hasAnalyzed = false;

        Debug.Log($"<color=cyan>Analyzing image usage...</color>");
        Debug.Log($"<color=cyan>Target Folder: {targetImageFolder}</color>");

        try
        {
            // Step 1: Get all image assets in target folder
            var folderImages = GetImageAssetsInFolder();
            Debug.Log($"<color=cyan>Found {folderImages.Count} image assets in folder</color>");

            if (folderImages.Count == 0)
            {
                Debug.Log($"<color=yellow>No image assets found in {targetImageFolder}</color>");
                hasAnalyzed = true;
                Repaint();
                return;
            }

            // Step 2: Get all sprites used in current scene
            var usedSpritesInScene = GetUsedSpritesInScene();
            Debug.Log($"<color=cyan>Found {usedSpritesInScene.Count} unique sprites used in current scene</color>");

            // Step 3: Check each image asset
            foreach (var imageAsset in folderImages)
            {
                bool isUsedInScene = IsImageUsedInScene(imageAsset, usedSpritesInScene);
                
                var info = new ImageAssetInfo
                {
                    assetName = imageAsset.name,
                    assetPath = AssetDatabase.GetAssetPath(imageAsset),
                    texture = imageAsset,
                    isUsedInScene = isUsedInScene,
                    usageDetails = isUsedInScene ? GetUsageDetails(imageAsset, usedSpritesInScene) : new List<string>()
                };

                analysisResults.Add(info);
            }

            // Sort: unused first
            analysisResults = analysisResults.OrderBy(i => i.isUsedInScene).ThenBy(i => i.assetName).ToList();

            // Log results
            LogAnalysisResults();
            hasAnalyzed = true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during analysis: {ex.Message}");
        }
        
        Repaint();
    }

    private List<Texture2D> GetImageAssetsInFolder()
    {
        var images = new List<Texture2D>();
        
        if (!Directory.Exists(targetImageFolder))
            return images;

        // Find all texture assets in the folder
        var searchInFolders = includeSubfolders ? 
            new string[] { targetImageFolder } : 
            new string[] { targetImageFolder };

        var guids = AssetDatabase.FindAssets("t:Texture2D", searchInFolders);
        
        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // If not including subfolders, filter out subdirectory assets
            if (!includeSubfolders)
            {
                var relativePath = assetPath.Substring(targetImageFolder.Length + 1);
                if (relativePath.Contains('/')) continue;
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture != null)
            {
                images.Add(texture);
            }
        }

        return images;
    }

    private Dictionary<Sprite, List<string>> GetUsedSpritesInScene()
    {
        var usedSprites = new Dictionary<Sprite, List<string>>();
        var activeScene = SceneManager.GetActiveScene();
        
        if (!activeScene.isLoaded)
            return usedSprites;

        var allGameObjects = activeScene.GetRootGameObjects();
        
        foreach (var rootGO in allGameObjects)
        {
            CollectUsedSprites(rootGO, usedSprites);
        }

        return usedSprites;
    }

    private void CollectUsedSprites(GameObject go, Dictionary<Sprite, List<string>> usedSprites)
    {
        // Check Image components
        var images = go.GetComponents<Image>();
        foreach (var image in images)
        {
            if (image != null && image.sprite != null)
            {
                string objectPath = GetGameObjectPath(go);
                
                if (!usedSprites.ContainsKey(image.sprite))
                {
                    usedSprites[image.sprite] = new List<string>();
                }
                
                if (!usedSprites[image.sprite].Contains(objectPath))
                {
                    usedSprites[image.sprite].Add(objectPath);
                }
            }
        }

        // Check SpriteRenderer components
        var spriteRenderers = go.GetComponents<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                string objectPath = GetGameObjectPath(go);
                
                if (!usedSprites.ContainsKey(spriteRenderer.sprite))
                {
                    usedSprites[spriteRenderer.sprite] = new List<string>();
                }
                
                if (!usedSprites[spriteRenderer.sprite].Contains(objectPath))
                {
                    usedSprites[spriteRenderer.sprite].Add(objectPath);
                }
            }
        }

        // Check children recursively
        for (int i = 0; i < go.transform.childCount; i++)
        {
            CollectUsedSprites(go.transform.GetChild(i).gameObject, usedSprites);
        }
    }

    private bool IsImageUsedInScene(Texture2D texture, Dictionary<Sprite, List<string>> usedSprites)
    {
        // Check if any sprite from this texture is used
        foreach (var sprite in usedSprites.Keys)
        {
            if (sprite.texture == texture)
            {
                return true;
            }
        }
        return false;
    }

    private List<string> GetUsageDetails(Texture2D texture, Dictionary<Sprite, List<string>> usedSprites)
    {
        var details = new List<string>();
        
        foreach (var kvp in usedSprites)
        {
            if (kvp.Key.texture == texture)
            {
                details.AddRange(kvp.Value);
            }
        }
        
        return details.Distinct().ToList();
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

    private void LogAnalysisResults()
    {
        var unusedCount = analysisResults.Count(i => !i.isUsedInScene);
        var usedCount = analysisResults.Count - unusedCount;

        Debug.Log($"<color=cyan>=== IMAGE USAGE ANALYSIS RESULTS ===</color>");
        Debug.Log($"<color=green>Images used in scene: {usedCount}</color>");
        Debug.Log($"<color=red>Images not used in scene: {unusedCount}</color>");

        if (unusedCount > 0)
        {
            Debug.Log($"<color=red>=== UNUSED IMAGES (Safe to delete) ===</color>");
            foreach (var unused in analysisResults.Where(i => !i.isUsedInScene))
            {
                Debug.Log($"<color=red>• {unused.assetName} ({unused.assetPath})</color>");
            }
        }
    }

    private void DisplayResults()
    {
        var unusedCount = analysisResults.Count(i => !i.isUsedInScene);
        var usedCount = analysisResults.Count - unusedCount;

        if (analysisResults.Count == 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("No image assets found in the target folder.", EditorStyles.helpBox);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.LabelField($"Results: {usedCount} Used, {unusedCount} Unused", EditorStyles.boldLabel);

        if (unusedCount == 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("✅ All images in the folder are being used in the current scene!", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.HelpBox($"{unusedCount} images are not used in the current scene and can potentially be deleted.", MessageType.Warning);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var result in analysisResults)
        {
            // Only show unused images (deletable ones)
            if (result.isUsedInScene) continue;

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(result.assetName, EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.LabelField("UNUSED", GUILayout.Width(80));

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = result.texture;
                EditorGUIUtility.PingObject(result.texture);
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Image Asset", 
                    $"Are you sure you want to delete '{result.assetName}'?\n\nPath: {result.assetPath}", 
                    "Delete", "Cancel"))
                {
                    AssetDatabase.DeleteAsset(result.assetPath);
                    AssetDatabase.Refresh();
                    
                    Debug.Log($"<color=orange>Deleted image asset: {result.assetName}</color>");
                    
                    // Re-run analysis
                    AnalyzeImageUsage();
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"Path: {result.assetPath}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("⚠️ Not used in current scene - Safe to delete", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            GUI.backgroundColor = originalColor;
        }

        EditorGUILayout.EndScrollView();

        // Bulk delete button
        if (unusedCount > 0)
        {
            EditorGUILayout.Space();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button($"🗑️ Delete All {unusedCount} Unused Images", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete All Unused Images", 
                    $"Are you sure you want to delete all {unusedCount} unused image assets?\n\nThis action cannot be undone!", 
                    "Delete All", "Cancel"))
                {
                    int deletedCount = 0;
                    var unusedImages = analysisResults.Where(i => !i.isUsedInScene).ToList();
                    
                    foreach (var unused in unusedImages)
                    {
                        AssetDatabase.DeleteAsset(unused.assetPath);
                        deletedCount++;
                        Debug.Log($"<color=orange>Deleted image asset: {unused.assetName}</color>");
                    }
                    
                    AssetDatabase.Refresh();
                    Debug.Log($"<color=orange>Successfully deleted {deletedCount} unused image assets!</color>");
                    
                    // Clear results
                    analysisResults.Clear();
                    hasAnalyzed = false;
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }

    private class ImageAssetInfo
    {
        public string assetName;
        public string assetPath;
        public Texture2D texture;
        public bool isUsedInScene;
        public List<string> usageDetails;
    }
}