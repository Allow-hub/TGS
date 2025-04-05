using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
   // 攻撃セットScriptableObject - 複数の攻撃をグループ化
    [CreateAssetMenu(fileName = "AttackSet", menuName = "TechC/Combat/Attack Set")]
    public class AttackSet : ScriptableObject
    {
        [Header("キャラクター情報")]
        public string characterName;
        
        [Header("弱攻撃セット")]
        public AttackData weakNeutral;
        public AttackData weakUp;
        public AttackData weakDown;
        public AttackData weakLeft;
        public AttackData weakRight;
        
        [Header("強攻撃セット")]
        public AttackData strongNeutral;
        public AttackData strongUp;
        public AttackData strongDown;
        public AttackData strongLeft;
        public AttackData strongRight;
        
        // [Header("空中攻撃セット")]
        // public AttackData airNeutral;
        // public AttackData airUp;
        // public AttackData airDown;
        // public AttackData airForward;
        // public AttackData airBackward;
        
    }
}
