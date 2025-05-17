using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラのエフェクトはすべてこれがついている必要がある
    /// </summary>
    public class CharaEffect : MonoBehaviour
    {
        /// 自分が所属するオブジェクトプール
        private ObjectPool objectPool;

        /// <summary>
        /// ファクトリー側で呼ぶ初期化メソッド
        /// </summary>
        /// <param name="objectPool"></param>
        public void Init(ObjectPool objectPool)
        {
            this.objectPool = objectPool;
        }
    }
}
