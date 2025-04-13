using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    // 攻撃コマンド基底クラス

    public  class AttackCommand : INeutralUsableCommand
    {
        private CharacterState characterState;
        protected float duration;
        protected float elapsedTime = 0;
        protected bool isForceFinished = false;
        public bool IsFinished =>isForceFinished || elapsedTime >= duration;

        public AttackCommand(CharacterState characterState)
        {
            this.characterState = characterState;
        }

        public  void Execute()
        {
            characterState.ChangeAttackState();
        }

        public virtual void Undo()
        {
            // 基本的に攻撃のキャンセルはないが、必要に応じて実装
        }

        //public virtual void Update(float deltaTime)
        //{
        //    elapsedTime += deltaTime;
        //}

        public virtual void ForceFinish()
        {
            elapsedTime = 0;
        }
    }
}
