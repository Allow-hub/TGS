using UnityEngine;

namespace TechC
{
    [CreateAssetMenu(fileName = "New Stage Data", menuName = "TechC/Stage Data", order = 1)]
    public class StageData:ScriptableObject
    {
        [Header("基本情報")]
        public string stageName = "New Stage";
        public Sprite stageSprite;
        public Vector2 spriteScale = new Vector2(10, 10);
    }
}
