using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class WeakAttackCommand : AttackCommand
    {
        public WeakAttackCommand(IAttackBase attackImpl, Player.CharacterController character)
      : base(attackImpl, character) { }

        public override void Execute()
        {
            attackImplementation.NeutralAttack();
        }
    }
}
