using System.Collections;
using System.Collections.Generic;
using TechC.Player;
using UnityEngine;

namespace TechC
{
    // 攻撃マネージャークラス - 全ての攻撃タイプを管理
    [System.Serializable]
    public class AttackManager
    {
        private PlayerInputManager playerInputManager;
        private PlayerController playerController;
        
        // 他の攻撃タイプも追加可能
        private IAttackBase weakAttack;
        private IAttackBase strongAttack;
        private IAttackBase airAttack;


        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="WeakAttack"></param>
        /// <param name="StrongAttack"></param>
        public void Initialize(IAttackBase WeakAttack,IAttackBase StrongAttack,PlayerInputManager PlayerInputManager,PlayerController PlayerController  )
        {
            weakAttack = WeakAttack;
            strongAttack =StrongAttack;
            playerInputManager =PlayerInputManager;
            playerController = PlayerController;

            // airAttack = airAttackImplementation as IAttackBase;
            
            if (playerInputManager == null) Debug.LogError("playerInputManagerが空です");
            if (playerController == null) Debug.LogError("playerControllerが空です");
            if (weakAttack == null) Debug.LogError("WeakAttack実装が IAttackBase を実装していません");
            if (strongAttack == null) Debug.LogError("StrongAttack実装が IAttackBase を実装していません");
            // if (airAttack == null) Debug.LogError("AirAttack実装が IAttackBase を実装していません");
        }
        
        /// <summary>
        /// 攻撃種の設定
        /// </summary>
        /// <param name="attackType"></param>
        /// <param name="context"></param>
        public void ExecuteAttack(CharacterState.AttackType attackType, CharacterState context)
        {
            // 空中攻撃（優先度高） ※地上でないときのみ
            // if (playerInputManager.IsAirAttacking && !context.isGrounded && _airAttack != null)
            // {
            //     ExecuteSpecificAttack(_airAttack, attackType);
            // }

            // 強攻撃
            if (playerInputManager.IsStrongAttacking && strongAttack != null)
            {
                ExecuteSpecificAttack(strongAttack, attackType);
            }

            // 弱攻撃
            if (playerInputManager.IsWeakAttacking && weakAttack != null)
            {
                ExecuteSpecificAttack(weakAttack, attackType);
            }
        }

        /// <summary>
        /// 向きによって攻撃種を変更
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="attackType"></param>
        private void ExecuteSpecificAttack(IAttackBase attack, CharacterState.AttackType attackType)
        {
            switch (attackType)
            {
                case CharacterState.AttackType.Neutral:
                    attack.NeutralAttack();
                    break;
                case CharacterState.AttackType.Left:
                    attack.LeftAttack();
                    break;
                case CharacterState.AttackType.Right:
                    attack.RightAttack();
                    break;
                case CharacterState.AttackType.Down:
                    attack.DownAttack();
                    break;
                case CharacterState.AttackType.Up:
                    attack.UpAttack();
                    break;
                default:
                    Debug.LogWarning("未定義のAttackTypeが指定されました");
                    break;
            }
        }
    }
}
