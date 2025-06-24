using UnityEngine;

namespace TechC
{

    /// <summary>
    /// 全てのバフの基本処理をまとめた基底クラス
    /// </summary>

    public class BuffBase
    {
        public string buffName { get; protected set; }
        public string description { get; protected set; }
        public float buffDuration { get; protected set; }
        public float remainingTime { get; protected set; }
        public int id { get; private set; }

        // これまでのバフの生成数、識別IDを作成するために使う
        private static int totalBuffCount = VoidID + 1;
        // 予約された無効ID
        public static readonly int VoidID = 0; 

        public BuffBase()
        {
            id = totalBuffCount;
            totalBuffCount++;
        }

        /// <summary>
        /// バフを適用する処理
        /// </summary>
        /// <param name="target"></param>
        public virtual void Apply(GameObject target)
        {
            /* 各バフでオーバーライドして使用する */
        }
        
        /// <summary>
        /// バフを解除する処理
        /// </summary>
        /// <param name="target"></param>
        public virtual void Remove(GameObject target)
        {
            /* 各バフでオーバーライドして使用する */
        }
        
        /// <summary>
        /// バフの残り時間を計算する処理
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="target"></param>

        public virtual void UpdateBuff(float deltaTime, GameObject target)
        {
            remainingTime -= deltaTime;
        }

        /// <summary>
        /// 残り時間を初期化
        /// </summary>
        public void ResetDuration()
        {
            remainingTime = buffDuration;
        }
    }
}
