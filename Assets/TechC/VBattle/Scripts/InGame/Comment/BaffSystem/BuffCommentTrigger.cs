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

        private void OnTriggerEnter(Collider other)
        {
            if (alreadyApplied) return;

            if (other.CompareTag("Player"))
            {
                // Debug.Log($"バフ{buffType}が発動した");
                var controller=other.transform.parent.GetComponent<Player.CharacterController>();
                int id = controller.PlayerID;
                EffectFactory.I.PlayEffect("Attack",id, Quaternion.identity, 3.0f);
                Debug.Log(other);

                BuffBase buff = BuffFactory.CreateBuff(buffType);
                if (buff != null)
                {
                    BuffManager buffManager = other.GetComponent<BuffManager>();
                    if(buffManager != null)
                    {
                        buffManager.ApplyBuff(buff);
                    }

                    // buff.Apply(other.gameObject);
                }

                alreadyApplied = true;
                gameObject.SetActive(false);
            }
        }
    }
}
