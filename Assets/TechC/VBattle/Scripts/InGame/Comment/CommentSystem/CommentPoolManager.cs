using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コメント用のゲームオブジェクトをオブジェクトプールから取得し、  
    /// 指定されたテキストやバフ情報をセットして返す
    /// また、使用後のコメントオブジェクトをプールに返却する機能も持つ
    /// </summary>
    public class CommentPoolManager : MonoBehaviour
    {
        public List<ObjectPoolItem> poolItems;
        private ObjectPool commentPool;

        /* ひらがな1文字をキーにObjectPoolItemを取得する辞書 */
        private Dictionary<string, ObjectPoolItem> poolDictionary;


        void Awake()
        {
            /* 辞書の初期化 */
            poolDictionary = new Dictionary<string, ObjectPoolItem>();

            commentPool = GetComponent<ObjectPool>();
            foreach (var item in poolItems)
            {
                if (!poolDictionary.ContainsKey(item.name))
                {
                    poolDictionary.Add(item.name, item);
                    Debug.Log($"プールに登録:{item.name}");
                }
                else
                {
                    Debug.LogWarning($"プールの名前が重複しています{item.name}");
                }
            }
        }

        public ObjectPoolItem GetPoolItem(string kana)
        {
            if (poolDictionary.TryGetValue(kana, out ObjectPoolItem item))
            {
                return item;
            }
            else
            {
                Debug.LogWarning($"該当するプールがありません: {kana}");
                return null;
            }
        }
    }
}
