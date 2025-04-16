using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
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
