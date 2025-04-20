using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class MapChangeManager : MonoBehaviour
    {
        public static MapChangeManager Instance;

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

        public void ChangeMap()
        {
            Debug.Log("ChangeMapメソッドが呼ばれました。");
            /* ここに具体的にマップ変化の処理を書く */
        }

    }
}
