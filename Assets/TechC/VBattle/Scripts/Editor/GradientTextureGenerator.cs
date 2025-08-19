using UnityEngine;
using UnityEditor;
using System.IO;
namespace TechC
{
    public class GradientTextureGenerator : EditorWindow
    {
        private Gradient gradient = new Gradient();
        private int textureWidth = 256;
        private int textureHeight = 32;
        private string fileName = "GradientTexture.png";
        private string savePath = "Assets"; // デフォルトはAssets直下

        [MenuItem("Tools/Gradient Texture Generator")]
        public static void ShowWindow()
        {
            GetWindow<GradientTextureGenerator>("Gradient Texture Generator");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Gradient Texture Generator", EditorStyles.boldLabel);
            gradient = EditorGUILayout.GradientField("Gradient", gradient);

            textureWidth = EditorGUILayout.IntField("Width", textureWidth);
            textureHeight = EditorGUILayout.IntField("Height", textureHeight);
            fileName = EditorGUILayout.TextField("File Name", fileName);

            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(80)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Save Folder", savePath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Unity プロジェクト内パスに変換
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        Debug.LogWarning("Assetsフォルダ内に保存してください");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate & Save"))
            {
                GenerateAndSave();
            }
        }

        private void GenerateAndSave()
        {
            Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

            for (int x = 0; x < textureWidth; x++)
            {
                Color col = gradient.Evaluate((float)x / (textureWidth - 1));
                for (int y = 0; y < textureHeight; y++)
                {
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();

            // 保存先パス
            string path = Path.Combine(savePath, fileName);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();
            Debug.Log($"Saved Gradient Texture to: {path}");

            // 保存したテクスチャを選択状態にする
            Texture2D savedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (savedTex != null)
            {
                EditorGUIUtility.PingObject(savedTex);
            }
        }
    }
}