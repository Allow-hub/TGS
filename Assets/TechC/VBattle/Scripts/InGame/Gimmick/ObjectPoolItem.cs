using System;
using UnityEngine;

namespace TechC
{
    [Serializable]
    public class ObjectPoolItem
    {
        public string name;            // 識別用の名前（任意）
        public GameObject prefab;      // プールするプレハブ
        public GameObject parent;      // 生成されたオブジェクトを格納する親オブジェクト
        public int initialSize;        // 初期プールサイズ

        // コンストラクタ（オプション）
        public ObjectPoolItem(string name, GameObject prefab, GameObject parent, int initialSize)
        {
            this.name = name;
            this.prefab = prefab;
            this.parent = parent;
            this.initialSize = initialSize;
        }
    }
}
