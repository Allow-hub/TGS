using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class TestPlayer :MonoBehaviour , IDamageable
    {
        public void Des()
        {
        }

        public void TakeDamage(float damage)
        {
            Debug.Log("DADA");
        }
    }
}
