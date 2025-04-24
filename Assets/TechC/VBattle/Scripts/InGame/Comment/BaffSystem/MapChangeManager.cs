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
        /* インスタンスの定義 */
        public static MapChangeManager Instance;

        /* Mapを格納する */
        public GameObject [] mapObjects; /* 複数入れれるように */

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

        public void ChangeMap(int mapIndex)
        {
            Debug.Log("ChangeMapメソッドが呼ばれました。");

            for (int i = 0; i < mapObjects.Length; i++)
            {
                mapObjects[i].SetActive(false);
            }
           
            if (mapIndex >= 0 && mapIndex < mapObjects.Length)
            {
                mapObjects[mapIndex].SetActive(true);
            }
            else if(mapIndex != -1)
            {
                Debug.LogWarning("無効なマップインデックスが指定されました: " + mapIndex);
            }
        }

    }
}
