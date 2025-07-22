using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 攻撃の各機能を持ったコンポーネントが継承するインターフェース
    /// </summary>
    public interface IAttackBehaviour
    {
        void Initialize(GameObject owner);
        void OnUpdate(float deltaTime);
        void OnRelease();
    }
}