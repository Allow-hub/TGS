using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public static class AttackEventBus
    {
        public static event Action<AttackData, Collider> OnAttack;

        public static void RaiseAttack(AttackData attackData, Collider hitCollider)
        {
            OnAttack?.Invoke(attackData, hitCollider);
        }
    }

}
