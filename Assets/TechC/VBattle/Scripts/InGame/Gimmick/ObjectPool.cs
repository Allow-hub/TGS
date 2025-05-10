using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class ObjectPool : MonoBehaviour
    {
        [Header("Object Pool Settings")]
        [SerializeField] private List<ObjectPoolItem> poolItems; // プール設定リスト

        // 各プレハブごとのオブジェクトプール
        private Dictionary<GameObject, Queue<GameObject>> objectPools = new Dictionary<GameObject, Queue<GameObject>>();

        // インスタンスと元のプレハブを紐づける辞書
        private Dictionary<GameObject, ObjectPoolItem> instanceToPoolItemMap = new Dictionary<GameObject, ObjectPoolItem>();

        private void Awake()
        {
            if (poolItems == null || poolItems.Count == 0)
            {
                Debug.LogError("Object Poolの初期化が不足しています。プールリストを設定してください。");
                return;
            }

            foreach (var poolItem in poolItems)
            {
                InitializePool(poolItem);
            }
        }

        // 指定したプレハブ用のプールを初期化
        private void InitializePool(ObjectPoolItem poolItem)
        {
            if (!objectPools.ContainsKey(poolItem.prefab))
            {
                objectPools[poolItem.prefab] = new Queue<GameObject>();
            }

            for (int i = 0; i < poolItem.initialSize; i++)
            {
                GameObject newObject = CreateNewInstance(poolItem);
                objectPools[poolItem.prefab].Enqueue(newObject);
            }
        }

        // 新しいオブジェクトを作成し、プールに登録
        private GameObject CreateNewInstance(ObjectPoolItem poolItem)
        {
            GameObject newObject = Instantiate(poolItem.prefab);
            newObject.SetActive(false);
            newObject.transform.SetParent(poolItem.parent.transform);
            instanceToPoolItemMap[newObject] = poolItem; // インスタンスとプール情報を紐付け
            return newObject;
        }

        // プレハブからオブジェクトを取得する
        public GameObject GetObject(GameObject prefab)
        {
            if (objectPools.ContainsKey(prefab) && objectPools[prefab].Count > 0)
            {
                GameObject pooledObject = objectPools[prefab].Dequeue();
                pooledObject.SetActive(true);
                return pooledObject;
            }
            else
            {
                // 初期リストに含まれるプレハブか確認
                ObjectPoolItem poolItem = poolItems.Find(item => item.prefab == prefab);
                if (poolItem != null)
                {
                    return CreateNewInstance(poolItem);
                }
                else
                {
                    Debug.LogWarning($"要求されたプレハブ {prefab.name} は ObjectPool に登録されていません。");
                    return null;
                }
            }
        }

        // オブジェクトをプールに返却する
        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false); // 非アクティブ化

            if (instanceToPoolItemMap.TryGetValue(obj, out ObjectPoolItem poolItem))
            {
                obj.transform.SetParent(poolItem.parent.transform); // 元の親オブジェクトに戻す
                objectPools[poolItem.prefab].Enqueue(obj); // プールに追加
            }
            else
            {
                Debug.LogWarning($"オブジェクト {obj.name} はプールに登録されていません。削除します。");
                Destroy(obj);
            }
        }
    }
}
