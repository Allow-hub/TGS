using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// バフコメントがプレイヤーと衝突した際にバフを適用するトリガークラス
    /// </summary>
    public class BuffCommentTrigger : MonoBehaviour
    {
        public BuffType buffType;
        private bool alreadyApplied = false;

        private ObjectPool objectPool;

        /// <summary>
        /// 疑似的なコンストラクタ
        /// </summary>
        /// <param name="objectPool"></param>
        public void Init(ObjectPool objectPool)
        {
            this.objectPool = objectPool;
            // Debug.Log("Init");
        }

        /// <summary>
        /// コメントにPlayerが当たったときにバフの効果とエフェクトを発動する
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (alreadyApplied) return;

            if (other.CompareTag("Player"))
            {
                // Debug.Log("Buffコメントにあたった");
                BuffBase buff = BuffFactory.CreateBuff(buffType);

                if (buff != null)
                {
                    BuffManager buffManager = other.GetComponentInParent<BuffManager>();
                    if (buffManager != null)
                    {
                        buffManager.ApplyBuff(buff);
                    }
                }


                var controller = other.transform.parent.GetComponent<Player.CharacterController>();
                int id = controller.PlayerID;

                float effectTime = buff.remainingTime; /*バフのエフェクトの継続時間にバフの効果の時間を代入 */

                /* バフの種類ごとに適用するエフェクトを変える */
                switch (buffType)
                {
                    case BuffType.Speed:
                        EffectFactory.I.PlayEffect("Speed", id, Quaternion.identity, effectTime);
                        break;
                    case BuffType.Attack:
                        EffectFactory.I.PlayEffect("Attack", id, Quaternion.identity, effectTime);
                        break;
                    // 必要であれば他のバフタイプも追加できます
                    default:
                        Debug.LogWarning($"未対応のバフタイプ: {buffType}");
                        break;
                }

                alreadyApplied = true;
                objectPool.ReturnObject(gameObject);
            }
        }
    }
}
