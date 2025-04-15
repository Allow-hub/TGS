using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class BuffManager : MonoBehaviour
    {
        /* バフを保持するリスト */
        private List<BuffBase> activeBuffs = new List<BuffBase>();

        void Update()
{
    /* バフリストの更新処理 */
    for (int i = activeBuffs.Count - 1; i >= 0; i--)
    {
        BuffBase buff = activeBuffs[i];
        buff.UpdateBuff(Time.deltaTime, gameObject);

        // バフの時間が終わったら削除
        if (buff.remainingTime <= 0)
        {
            buff.Remove(gameObject);
            activeBuffs.RemoveAt(i);
        }
    }
}


        /* バフを追加する */
        public void ApplyBuff(BuffBase buff)
        {
            foreach (BuffBase activeBuff in activeBuffs)
            {
                if (activeBuff.GetType() == buff.GetType())
                {
                    /* 同じバフを適用した際は時間をリセットして再適用する */
                    activeBuff.ResetDuration();
                    return;
                }
            }

            activeBuffs.Add(buff);
            buff.ResetDuration();
            buff.Apply(gameObject);
        }

        /* バフを解除する */
        public void RemoveBuff(BuffBase buff)
        {
            if (activeBuffs.Contains(buff))
            {
                buff.Remove(gameObject);
                activeBuffs.Remove(buff);
            }
        }
    }
}
