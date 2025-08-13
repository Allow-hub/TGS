using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TechC.Player.Attack;

namespace TechC.EditorTools
{
   [CustomEditor(typeof(AttackObjectController))]
    public class AttackObjectControllerEditor : PolymorphicListEditor<AttackObjectController, IAttackBehaviour>
    {
        protected override string PropertyName => "behaviours";
    }
}
