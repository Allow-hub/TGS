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
    
        protected override bool UseDontDestroyOnLoad => false;
        protected override void Init()
        {
            base.Init();
            effectPool.ForEachInactiveInPool(obj =>
            {
                var charaEffect = obj.GetComponent<CharaEffect>();
                charaEffect?.Init(effectPool);
            });
        }
        
        /// <summary>
        /// 位置だけを指定してエフェクトを再生（回転はデフォルト値）
        /// </summary>
        /// <param name="effectName">エフェクト名</param>
        /// <param name="position">エフェクトの位置</param>
        /// <param name="effectRemainingTime">自動返却までの時間（省略可）</param>
        public void PlayEffect(string effectName, int playerID, Quaternion rotation, float effectRemainingTime = 0f)
        {
            /* ObjectPoolから指定された名前のエフェクトを取得 */
            GameObject effect = effectPool.GetObjectByName(effectName);


            /* エフェクトの位置を回転を設定 */
            // effect.transform.position = position; /* 位置を設定 */
            var obj = BattleJudge.I.GetPlayerObjById(playerID);
            Debug.Log(effect);
            effect.transform.position = obj.transform.position;
            effect.transform.SetParent(obj.transform);
            effect.transform.rotation = rotation; /* 回転を設定 */
            effect.SetActive(true); /* エフェクトを表示 */

            if (effectRemainingTime > 0f)
            {
                StartCoroutine(AutoReturn(effect, effectRemainingTime));
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