using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 複数のマップを管理し、指定されたマップに切り替えるためのシングルトンマネージャー
    /// </summary>
    public class MapChangeManager : MonoBehaviour
    {
        // シングルトンインスタンス
        public static MapChangeManager Instance;

        // マップオブジェクトのリスト
        public List<GameObject> mapObjects;

        // 現在のマップインデックス
        [SerializeField]
        private int currentMapIndex = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Debug.Log("インスタンスが設定できました。");
            }
            else
            {
                Destroy(gameObject);
                // Debug.LogError("インスタンスが重複している");
            }
        }

        /// <summary>
        /// 現在のマップインデックスに基づいてマップを切り替える
        /// </summary>
        public void ChangeMap()
        {
            // Debug.Log("ChangeMapメソッドが呼ばれました。");

            // すべてのマップを非表示にする
            foreach (GameObject map in mapObjects)
            {
                map.SetActive(false);
            }

            if (currentMapIndex >= mapObjects.Count)
                currentMapIndex = 0;
            // 指定されたマップのみ表示
            if (currentMapIndex >= 0 && currentMapIndex < mapObjects.Count)
            {
                mapObjects[currentMapIndex].SetActive(true);
                // Debug.Log(mapObjects[currentMapIndex].name);
            }
            else if (currentMapIndex != -1)
            {
                // Debug.LogWarning("無効なマップインデックスが指定されました: " + currentMapIndex);
            }
            currentMapIndex++;

        }

        /// <summary>
        /// 外部からマップインデックスを指定して変更する
        /// </summary>
        public void SetMapIndex()
        {
            currentMapIndex++;
            ChangeMap();
        }
    }
}
