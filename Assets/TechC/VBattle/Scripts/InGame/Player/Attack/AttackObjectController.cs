using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player.Attack
{
    /// <summary>
    /// 各攻撃のオブジェクトの管理クラス
    /// それぞれの機能を組み立てて実行する
    /// </summary>
    public class AttackObjectController : MonoBehaviour
    {
        [SerializeReference] private List<IAttackBehaviour> behaviours;
    }
}
