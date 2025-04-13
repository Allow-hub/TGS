using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TechC.Player;
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
        // 他の攻撃タイプも追加可能
        private IAttackBase weakAttack;
        private IAttackBase strongAttack;
        private IAttackBase airAttack;


        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="WeakAttack"></param>
        /// <param name="StrongAttack"></param>
        public void Initialize(IAttackBase WeakAttack,IAttackBase StrongAttack,BaseInputManager PlayerInputManager, Player.CharacterController CharacterController)
        {
            weakAttack = WeakAttack;
            strongAttack =StrongAttack;
            playerInputManager =PlayerInputManager;
            characterController = CharacterController;
            // airAttack = airAttackImplementation as IAttackBase;
            
            if (playerInputManager == null) Debug.LogError("playerInputManagerが空です");
            if (characterController == null) Debug.LogError("characterController");
            if (weakAttack == null) Debug.LogError("WeakAttack実装が IAttackBase を実装していません");
            if (strongAttack == null) Debug.LogError("StrongAttack実装が IAttackBase を実装していません");
            // if (airAttack == null) Debug.LogError("AirAttack実装が IAttackBase を実装していません");
        }

        //// コマンドを作成して返すファクトリメソッド
        //public ICommand CreateAttackCommand(CharacterState.AttackType attackType, bool isWeak, float duration)
        //{
        //    Debug.Log("CreateCommand");
        //    IAttackBase attackImpl = isWeak ? weakAttack : strongAttack;
        //    switch (attackType)
        //    {
        //        case CharacterState.AttackType.Neutral:
        //            return isWeak
        //                ? new WeakNeutralAttackCommand(attackImpl, characterController, 1)
        //                : new WeakNeutralAttackCommand(attackImpl, characterController, 1);
        //        case CharacterState.AttackType.Left:
        //            return isWeak
        //                ? new WeakNeutralAttackCommand(attackImpl, characterController, 1)
        //                : new WeakNeutralAttackCommand(attackImpl, characterController, 1);
        //        // 他の方向も同様に
        //        default:
        //            Debug.LogWarning("未定義のAttackTypeが指定されました");
        //            return isWeak
        //                ? new WeakNeutralAttackCommand(attackImpl, characterController, 1)
        //                : new WeakNeutralAttackCommand(attackImpl, characterController, 1);
        //    }
        //    return null;
        //}



        /// <summary>
        /// 攻撃種の設定
        /// </summary>
        /// <param name="attackType"></param>
        /// <param name="context"></param>
        public void ExecuteAttack(AttackType attackType, CharacterState context)
        {

            // 空中攻撃はまだ

            //float attackDuration = 1.0f; // デフォルト持続時間

            // 強攻撃
            if (playerInputManager.IsStrongAttacking && strongAttack != null)
            {
                //var command = CreateAttackCommand(attackType, false, attackDuration);
                //command?.Execute();
                ExecuteSpecificAttack(strongAttack, attackType);
                return;
            }

            // 弱攻撃
            if (playerInputManager.IsWeakAttacking && weakAttack != null)
            {
                //var command = CreateAttackCommand(attackType, true, attackDuration);
                //command?.Execute();
                ExecuteSpecificAttack(weakAttack, attackType);
                return;
            }

            Debug.LogWarning("攻撃が入力されていません");
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

        public float GetDuration(AttackType attackType, bool isWeak)
        {
            IAttackBase attackImpl = isWeak ? weakAttack : strongAttack;
            return attackImpl.GetDuration(attackType);
        }
    }
}
