#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TechC
{
    [CustomEditor(typeof(AttackSet))]
    public class AttackSetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AttackSet set = (AttackSet)target;

            if (GUILayout.Button("攻撃データを自動設定"))
            {
                AssignAttackDataAutomatically(set);
                EditorUtility.SetDirty(set); // 変更を保存
                AssetDatabase.SaveAssets();
            }
        }

        private void AssignAttackDataAutomatically(AttackSet set)
        {
            var allAttackData = AssetDatabase.FindAssets("t:AttackData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AttackData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(ad => ad.characterType.ToString() == set.characterName) // キャラ一致
                .ToList();

            foreach (var attackData in allAttackData)
            {
                switch (attackData.attackName)
                {
                    // 弱攻撃
                    case "Neutral_W1": set.weakNeutral_1 = attackData; break;
                    case "Neutral_W2": set.weakNeutral_2 = attackData; break;
                    case "Neutral_W3": set.weakNeutral_3 = attackData; break;
                    case "Up_W": set.weakUp = attackData; break;
                    case "Down_W": set.weakDown = attackData; break;
                    case "Left_W": set.weakLeft = attackData; break;
                    case "Right_W": set.weakRight = attackData; break;

                    // 強攻撃
                    case "Neutral_S": set.strongNeutral = attackData; break;
                    case "Up_S": set.strongUp = attackData; break;
                    case "Down_S": set.strongDown = attackData; break;
                    case "Left_S": set.strongLeft = attackData; break;
                    case "Right_S": set.strongRight = attackData; break;

                    // アピール
                    case "Neutral_A": set.appealNeutral = attackData; break;
                    case "Up_A": set.appealUp = attackData; break;
                    case "Down_A": set.appealDown = attackData; break;
                    case "Left_A": set.appealLeft = attackData; break;
                    case "Right_A": set.appealRight = attackData; break;
                }
            }
        }
    }
}
#endif
