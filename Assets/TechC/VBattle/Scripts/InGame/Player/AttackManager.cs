using UnityEngine;
using static TechC.CharacterState;

namespace TechC
{
    // 攻撃マネージャークラス - 全ての攻撃タイプを管理
    [System.Serializable]
    public class AttackManager
    {
        private BaseInputManager playerInputManager;
        private Player.CharacterController characterController;
        public IAttackBase WeakAttack => weakAttack;
        public IAttackBase StrongAttack => strongAttack;
        public IAttackBase Appeal => appeal;
        // 他の攻撃タイプも追加可能
        private IAttackBase weakAttack;
        private IAttackBase strongAttack;
        private IAttackBase appeal;
        private IAttackBase airAttack;



        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="WeakAttack"></param>
        /// <param name="StrongAttack"></param>
        public void Initialize(IAttackBase WeakAttack, IAttackBase StrongAttack, IAttackBase Appeal, BaseInputManager PlayerInputManager, Player.CharacterController CharacterController)
        {
            weakAttack = WeakAttack;
            strongAttack = StrongAttack;
            appeal = Appeal;
            playerInputManager = PlayerInputManager;
            characterController = CharacterController;
            // airAttack = airAttackImplementation as IAttackBase;

            if (playerInputManager == null) Debug.LogError("playerInputManagerが空です");
            if (characterController == null) Debug.LogError("characterController");
            if (weakAttack == null) Debug.LogError("WeakAttack実装が IAttackBase を実装していません");
            if (strongAttack == null) Debug.LogError("StrongAttack実装が IAttackBase を実装していません");
            if (appeal == null) Debug.LogError("appeal実装が IAttackBase を実装していません");
            // if (airAttack == null) Debug.LogError("AirAttack実装が IAttackBase を実装していません");
        }

        /// <summary>
        /// 攻撃種の設定
        /// </summary>
        /// <param name="attackType"></param>
        public void ExecuteAttack(AttackType attackType)
        {
            // 空中攻撃はまだ

            // 強攻撃
            if (playerInputManager.IsStrongAttacking && strongAttack != null)
            {
                ExecuteSpecificAttack(strongAttack, attackType);
                return;
            }

            // 弱攻撃
            if (playerInputManager.IsWeakAttacking && weakAttack != null)
            {
                ExecuteSpecificAttack(weakAttack, attackType);
                return;
            }

            // アピール
            if (playerInputManager.IsAppealing && appeal != null)
            {
                ExecuteSpecificAttack(appeal, attackType);
                return;
            }

            Debug.LogWarning("攻撃が入力されていません");
        }

        /// <summary>
        /// 攻撃を再現するときに使うオーバーロードメソッド
        /// </summary>
        /// <param name="attackType"></param>
        /// <param name="attackStrength"></param>
        public void ReplayExecuteAttack(AttackType attackType, AttackStrength attackStrength)
        {
            if (weakAttack != null && attackStrength == AttackStrength.Weak)
            {
                ExecuteSpecificAttack(weakAttack, attackType);
                return;
            }
            if (strongAttack != null && attackStrength == AttackStrength.Strong)
            {
                ExecuteSpecificAttack(strongAttack, attackType);
                return;
            }
            if (appeal != null && attackStrength == AttackStrength.Appeal)
            {
                ExecuteSpecificAttack(appeal, attackType);
                return;
            }
        }

        /// <summary>
        /// 向きによって攻撃種を変更
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="attackType"></param>
        private void ExecuteSpecificAttack(IAttackBase attack, AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Neutral:
                    attack.NeutralAttack();
                    break;
                case AttackType.Left:
                    attack.LeftAttack();
                    break;
                case AttackType.Right:
                    attack.RightAttack();
                    break;
                case AttackType.Down:
                    attack.DownAttack();
                    break;
                case AttackType.Up:
                    attack.UpAttack();
                    break;
                default:
                    Debug.LogWarning("未定義のAttackTypeが指定されました");
                    break;
            }
        }

        public enum AttackStrength
        {
            Weak,
            Strong,
            Appeal
        }

        public float GetDuration(AttackType attackType, AttackStrength strength)
        {
            IAttackBase attackImpl;

            switch (strength)
            {
                case AttackStrength.Weak:
                    attackImpl = weakAttack;
                    break;
                case AttackStrength.Strong:
                    attackImpl = strongAttack;
                    break;
                case AttackStrength.Appeal:
                    attackImpl = appeal;
                    break;
                default:
                    Debug.LogWarning("未定義のAttackStrengthが指定されました");
                    attackImpl = weakAttack; // デフォルト
                    break;
            }

            return attackImpl.GetDuration(attackType);
        }
        public AttackData GetAttackData(AttackType attackType, AttackStrength strength)
        {
            IAttackBase attackImpl;
            switch (strength)
            {
                case AttackStrength.Weak:
                    attackImpl = weakAttack;
                    break;
                case AttackStrength.Strong:
                    attackImpl = strongAttack;
                    break;
                case AttackStrength.Appeal:
                    attackImpl = appeal;
                    break;
                default:
                    Debug.LogWarning("未定義のAttackStrengthが指定されました");
                    attackImpl = weakAttack; // デフォルト
                    break;
            }
            return attackImpl.GetAttackData(attackType);
        }

        public void ForceFinish(AttackStrength strength)
        {
            IAttackBase attackImpl;
            switch (strength)
            {
                case AttackStrength.Weak:
                    attackImpl = weakAttack;
                    break;
                case AttackStrength.Strong:
                    attackImpl = strongAttack;
                    break;
                case AttackStrength.Appeal:
                    attackImpl = appeal;
                    break;
                default:
                    Debug.LogWarning("未定義のAttackStrengthが指定されました");
                    attackImpl = weakAttack; // デフォルト
                    break;
            }
            attackImpl.ForceFinish();
        }
    }
}