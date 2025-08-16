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
        [Header("攻撃データ一覧")]
        public List<AttackEntry> attacks;
        public Dictionary<(CharacterState.AttackType, CharacterState.AttackStrength), AttackData> attackDataMap;
        private void OnEnable()
        {
            attackDataMap = new Dictionary<(CharacterState.AttackType, CharacterState.AttackStrength), AttackData>();

            foreach (var entry in attacks)
            {
                var key = (entry.type, entry.strength);
                if (!attackDataMap.ContainsKey(key))
                {
                    attackDataMap.Add(key, entry.attackData);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key in AttackSet: {key}");
                }
            }
        }


    }
    [System.Serializable]
    public struct AttackEntry
    {
        public string attackName;
        public CharacterState.AttackType type;
        public CharacterState.AttackStrength strength;
        public AttackData attackData;
    }
}
