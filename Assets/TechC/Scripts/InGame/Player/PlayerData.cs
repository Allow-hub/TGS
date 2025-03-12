using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.Player
{
    [CreateAssetMenu]
    public class PlayerData : ScriptableObject
    {
        public string Name;

        public int Hp;
        public float GardPower;
        public float MoveSpeed;
        public float JumpPower;

    }
}
