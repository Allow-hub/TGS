using UnityEngine;
using UnityEditor;
using System.IO;
using TechC;  // ObjectPoolやObjectPoolItemの名前空間に合わせて
public class ObjectPoolAutoAdder : EditorWindow
{
    public ObjectPool targetPool;
    public GameObject parentObject;  // 追加: 親オブジェクトを指定するフィールド
    private string prefabFolderPath = "Assets/TechC/VBattle/Models/3DText/hiragana";

    [MenuItem("Tools/ObjectPool Auto Adder")]
    public static void ShowWindow()
    {
        GetWindow<ObjectPoolAutoAdder>("ObjectPool Auto Adder");
    }

    void OnGUI()
    {
        GUILayout.Label("Add Prefabs from Folder to ObjectPool", EditorStyles.boldLabel);

        targetPool = (ObjectPool)EditorGUILayout.ObjectField("Target ObjectPool", targetPool, typeof(ObjectPool), true);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

        prefabFolderPath = EditorGUILayout.TextField("Prefab Folder Path", prefabFolderPath);

        if (GUILayout.Button("Add all Prefabs from folder"))
        {
            if (targetPool == null)
            {
                Debug.LogError("Target ObjectPool is not set.");
                return;
            }

            if (parentObject == null)
            {
                Debug.LogWarning("Parent Object is not set. Using targetPool GameObject as parent.");
                parentObject = targetPool.gameObject;
            }

            AddPrefabsToPool();
        }
    }

    void AddPrefabsToPool()
    {
        string fullPath = Application.dataPath.Replace("Assets", "") + prefabFolderPath;

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"Folder not found: {prefabFolderPath}");
            return;
        }

        string[] prefabFiles = Directory.GetFiles(fullPath, "*.prefab", SearchOption.TopDirectoryOnly);

        if (prefabFiles.Length == 0)
        {
            Debug.LogWarning("No prefab files found in folder.");
            return;
        }

        int addedCount = 0;

        foreach (string filePath in prefabFiles)
        {
            string assetPath = "Assets" + filePath.Replace(Application.dataPath, "").Replace('\\', '/');
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null)
            {
                Debug.LogWarning($"Could not load prefab at {assetPath}");
                continue;
            }

            ObjectPoolItem newItem = new ObjectPoolItem(prefab.name, prefab, parentObject, 5);

            targetPool.AddPoolItem(newItem);

            addedCount++;
        }

        EditorUtility.SetDirty(targetPool);
        AssetDatabase.SaveAssets();

        Debug.Log($"{addedCount} prefab(s) added to the ObjectPool.");
    }
}
