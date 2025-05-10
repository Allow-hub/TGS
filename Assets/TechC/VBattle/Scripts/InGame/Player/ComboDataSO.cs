using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コンボ定義用ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ComboData", menuName = "TechC/Combat/Combo Data")]
    public class ComboDataSO : ScriptableObject
    {
        public string comboName;
        public string description;

        [System.Serializable]
        public class ComboStep
        {
            public CharacterState.AttackType attackType;
            public AttackManager.AttackStrength attackStrength;
        }
        //public List<string> excludedCommandNames = new List<string> {
        //"MoveCommand", "CrouchCommand"
        //};
        public List<MonoScript> excludedCommandScripts = new();

        public List<ComboStep> sequence = new List<ComboStep>();
        public float timeWindow = 2.0f;
        public float gaugeBonus = 10f;
        public bool requiresCharge = false;
        // エフェクト
        public GameObject effectPrefab;
        public string animationTrigger;

        /// <summary>
        /// 除外対象として有効な Type の一覧を返す
        /// </summary>
        public List<Type> GetExcludedCommandTypes()
        {
            var types = new List<Type>();
            foreach (var script in excludedCommandScripts)
            {
                if (script != null)
                {
                    var type = script.GetClass();
                    if (type != null && typeof(ICommand).IsAssignableFrom(type))
                    {
                        types.Add(type);
                    }
                }
            }
            return types;
        }
    }
}
