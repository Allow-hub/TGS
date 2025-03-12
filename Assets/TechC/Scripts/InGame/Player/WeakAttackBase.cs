using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    public enum WeakAttackMode 
    {
        Neutral,
        FrontNeutral,
        BackNeutral,

    }

    public class WeakAttackBase : MonoBehaviour
    {
        public virtual void NeutraAttack()
        {
        }
    }
}
