using UnityEngine;

namespace TechC
{
    /// <summary>
    /// ウィンドウに当たり判定をつけるためのファクトリー
    /// </summary>
    public class WindowColliderFactory : Singleton<WindowColliderFactory>
    {
        [SerializeField] private ObjectPool objectPool;
        [SerializeField] private GameObject colliderPrefab;
        protected override bool UseDontDestroyOnLoad => false;

        public GameObject GetWindowColliderPrefab()
        {
            var obj = objectPool.GetObject(colliderPrefab);
            if (obj == null)
            {
                Debug.LogError("obj not found.");
                return null;
            }
            return obj;
        }

        public void ReturnWindowCollider(GameObject obj)
        {
            objectPool.ReturnObject(obj);
        }
    }
}
