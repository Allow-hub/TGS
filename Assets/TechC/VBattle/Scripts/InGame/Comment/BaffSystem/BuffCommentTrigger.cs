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

        /// <summary>
        /// コメントにPlayerが当たったときにバフの効果とエフェクトを発動する
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (alreadyApplied) return;

            if (other.CompareTag("Player"))
            {
                
                var controller = other.transform.parent.GetComponent<Player.CharacterController>();
                int id = controller.PlayerID;

                /* バフの種類ごとに適用するエフェクトを変える */
                if (buffType == BuffType.Speed)
                {
                    EffectFactory.I.PlayEffect("Speed", id, Quaternion.identity, 3.0f);
                }
                else if (buffType == BuffType.Attack)
                {
                    EffectFactory.I.PlayEffect("Attack", id, Quaternion.identity, 3.0f);
                }


                BuffBase buff = BuffFactory.CreateBuff(buffType);
                if (buff != null)
                {
                    BuffManager buffManager = other.GetComponent<BuffManager>();
                    if (buffManager != null)
                    {
                        buffManager.ApplyBuff(buff);
                    }
                }

                alreadyApplied = true;
                gameObject.SetActive(false);
            }
        }
    }
}
