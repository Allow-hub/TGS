using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class SelectPickAnim : MonoBehaviour
    {
        [SerializeField] private float animDelay = 1f;
        [SerializeField] private float appearDelay = 1.2f;
        [SerializeField] private GameObject ameObj;
        [SerializeField] private GameObject teramiObj;
        private int animName = Animator.StringToHash("IsShowingPannel");
        public void PlayAnim(int id)
        {
            if (id == 0) return;
            GameObject obj = ameObj;
            if (id == 1)
            {
                obj = ameObj;
            }
            else if (id ==2)
            {
                obj = teramiObj;
            }
            var anim = obj?.GetComponentInChildren<Animator>();
            DelayUtility.StartDelayedAction(this, appearDelay, () => obj?.SetActive(true));

            DelayUtility.StartDelayedAction(this, appearDelay + animDelay, () => anim?.SetBool(animName, true));
        }
    }
}
