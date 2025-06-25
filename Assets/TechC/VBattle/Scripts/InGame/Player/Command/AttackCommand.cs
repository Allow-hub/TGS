using TechC.Player;

namespace TechC
{
    /// <summary>
    /// 攻撃コマンド基底クラス
    /// </summary>
    public class AttackCommand : INeutralUsableCommand
    {
        private CharacterState characterState;
        private Player.CharacterController characterController;
        protected float duration;
        protected float elapsedTime = 0;
        protected bool isForceFinished = false;
        public bool IsFinished => isForceFinished || elapsedTime >= duration;

        // 攻撃の種類と強さを明示的に保持
        public CharacterState.AttackType Type { get; protected set; } = CharacterState.AttackType.Neutral;
        public AttackManager.AttackStrength Strength { get; protected set; } = AttackManager.AttackStrength.Weak;

        public AttackCommand(CharacterState characterState, CharacterController characterController)
        {
            this.characterState = characterState;
            this.characterController = characterController;
        }

        /// <summary>
        /// 攻撃タイプを設定するメソッド
        /// </summary>
        /// <param name="type"></param>
        public void SetAttackType(CharacterState.AttackType type)
        {
            Type = type;
        }

        /// <summary>
        /// 攻撃強度を設定するメソッド
        /// </summary>
        /// <param name="strength"></param>
        public void SetAttackStrength(AttackManager.AttackStrength strength)
        {
            Strength = strength;
        }

        public void Execute()
        {
            if (characterController.hasComment)
            {
                characterController.InvokeCommentEvent();
                return;
            }

            characterState.ChangeAttackState();
        }

        public virtual void Undo()
        {
            // 基本的に攻撃のキャンセルはないが、必要に応じて実装
        }

        public void ExecuteAttack(CharacterState.AttackType attackType, AttackManager.AttackStrength attackStrength, AttackManager attackManager)
        {
            attackManager.ReplayExecuteAttack(attackType, attackStrength);
        }
        public virtual void ForceFinish()
        {
            elapsedTime = 0;
            isForceFinished = true;
        }

        /// <summary>
        /// 攻撃の種類と強さを文字列で取得（コンボ検出で使用）
        /// </summary>
        /// <returns></returns>
        public string GetCommandSignature()
        {
            return $"{Strength}_{Type}";
        }
    }
}