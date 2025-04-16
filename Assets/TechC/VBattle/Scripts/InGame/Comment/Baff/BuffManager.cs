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

                /* バフの時間が終わったら削除 */
                if (buff.remainingTime <= 0)
                {
                    RemoveBuff(buff);
                    // Debug.Log($"[Removed] {buff.GetType().Name} を削除しました。現在の数: {activeBuffs.Count}");
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
                    // Debug.Log($"[Apply] {buff.GetType().Name} が再適用され、時間がリセットされました。");
                    return;
                }
            }

            activeBuffs.Add(buff);
            buff.ResetDuration();
            buff.Apply(gameObject);
            // Debug.Log($"[Apply] {buff.GetType().Name} が activeBuffs に追加されました。現在の数: {activeBuffs.Count}");
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
