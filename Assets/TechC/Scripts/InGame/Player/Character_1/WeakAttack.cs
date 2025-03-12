using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    public class WeakAttack : WeakAttackBase
    {
        public override void NeutraAttack()
        {
            base.NeutraAttack();
            Debug.Log("AAa");
        }
    }
}
