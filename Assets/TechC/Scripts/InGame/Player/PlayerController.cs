using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private WeakAttackBase weakAttackBase;
        private void OnEnable()
        {
            playerInputManager.weakAttackAction += WeakAttack;
        }

        private void OnDisable()
        {
            playerInputManager.weakAttackAction -= WeakAttack;  
        }

        private void WeakAttack(WeakAttackMode attackMode)
        {
            weakAttackBase.NeutraAttack();
        }

        private void StrongAttack()
        {

        }
    }
}
