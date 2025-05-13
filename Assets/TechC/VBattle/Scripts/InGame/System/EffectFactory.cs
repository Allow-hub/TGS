using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{

    /// <summary>
    /// エフェクトを生成するファクトリ（シングルトン）
    /// </summary>
    public class EffectFactory : Singleton<EffectFactory>
    {
        [SerializeField]
        private ObjectPool effectPool;


        /// <summary>
        /// 位置だけを指定してエフェクトを再生（回転はデフォルト値）
        /// </summary>
        /// <param name="effectName">エフェクト名</param>
        /// <param name="position">エフェクトの位置</param>
        /// <param name="autoReturnTime">自動返却までの時間（省略可）</param>
        public void PlayEffect(string effectName, Vector3 position, Quaternion rotation, float autoReturnTime = 0f)
        {
            /* ObjectPoolから指定された名前のエフェクトを取得 */
            GameObject effect = effectPool.GetObjectByName(effectName);
            Debug.Log("エフェクトのPrefabを適用メソッド呼ばれた");

            if (effect == null)
            {
                Debug.LogWarning($"エフェクト{effectName}が見つかりません");
                return;
            }

            /* エフェクトの位置を回転を設定 */
            effect.transform.position = position; /* 位置を設定 */
            effect.transform.rotation = rotation; /* 回転を設定 */

            effect.SetActive(true); /* エフェクトを表示 */
            Debug.Log("エフェクトのPrefabを適用");

            if (autoReturnTime > 0f)
            {
                StartCoroutine(AutoReturn(effect, autoReturnTime));
            }
        }


        /// <summary>
        /// 指定時間後にエフェクトをプールに返却する
        /// </summary>
        private IEnumerator AutoReturn(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);

            effectPool.ReturnObject(effect);
        }
            
    }
}