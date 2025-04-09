using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    // 攻撃コマンド基底クラス

    public abstract class AttackCommand : INeutralUsableCommand
    {
        protected IAttackBase attackImplementation;
        protected Player.CharacterController character;
        protected float duration;
        protected float elapsedTime = 0;

        public bool IsFinished => elapsedTime >= duration;

        public AttackCommand(IAttackBase attackImplementation, Player.CharacterController character)
        {
            this.attackImplementation = attackImplementation;
            this.character = character;
        }

        public abstract void Execute();

        public virtual void Undo()
        {
            // 基本的に攻撃のキャンセルはないが、必要に応じて実装
        }

        public virtual void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
        }
    }
}
