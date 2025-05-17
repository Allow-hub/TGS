using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラクターのエフェクト管理用ファクトリー
    /// </summary>
    public class CharaEffectFactory : Singleton<CharaEffectFactory>
    {
        [SerializeField] private ObjectPool objectPool;
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
            objectPool.ForEachInactiveInPool(obj =>
            {
                var charaEffect = obj.GetComponent<CharaEffect>();
                charaEffect?.Init(objectPool);
            });
        }

        /// <summary>
        /// エフェクトを取得
        /// </summary>
        /// <param name="prefab">エフェクトのプレハブ</param>
        /// <returns></returns>
        public GameObject GetEffectObj(GameObject prefab)=>objectPool.GetObject(prefab);

        /// <summary>
        /// エフェクトを位置と回転を指定したうえで取得
        /// </summary>
        /// <param name="prefab">エフェクトのプレハブ</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">初期回転</param>
        /// <returns></returns>
        public GameObject GetEffectObj(GameObject prefab, Vector3 position, Quaternion rotation) => objectPool.GetObject(prefab, position, rotation);

        public void ReturnEffectObj(GameObject obj)=>objectPool.ReturnObject(obj);
    }
}
