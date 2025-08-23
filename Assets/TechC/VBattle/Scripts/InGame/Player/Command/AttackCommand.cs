using System.Linq;
using TechC.Player;
using TechC.Player.Attack;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 攻撃コマンド基底クラス
    /// </summary>
    public class AttackCommand : INeutralUsableCommand
    {
        private CharacterState characterState;
        private Player.CharacterController characterController;
        private Player.CharacterController cloneController;
        protected float duration;
        protected float elapsedTime = 0;
        protected bool isForceFinished = false;
        public bool IsFinished => isForceFinished || elapsedTime >= duration;
        private float lastAttackTime = 0;
        private AttackData currentAttackData;
        private bool isCounter;
        private AttackData lastAttackData = null;
        private GameObject lastAttackObject; // 前回の攻撃オブジェクトを保持


        // 攻撃の種類と強さを明示的に保持
        public CharacterState.AttackType Type { get; protected set; } = CharacterState.AttackType.Neutral;
        public CharacterState.AttackStrength Strength { get; protected set; } = CharacterState.AttackStrength.Weak;

        public AttackCommand(CharacterState characterState, Player.CharacterController characterController)
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
        public void SetAttackStrength(CharacterState.AttackStrength strength)
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

        public void RePlayAttack(CharacterState.AttackType attackType, CharacterState.AttackStrength attackStrength, Player.CharacterController controller)
        {
            cloneController = controller;
            if (isCounter && currentAttackData != null)
            {
                isCounter = false;
                duration = currentAttackData.attackDuration;
                lastAttackTime = Time.time;

                SetAnimSetting();
                SetAttackObjSetting();
                SetupDelayedAttack();
                return;
            }

            var key = (attackType, attackStrength);

            if (cloneController.AttackSet.attackDataMap.TryGetValue(key, out var attackData))
            {
                currentAttackData = CanChain() ? lastAttackData.nextChain : attackData;
                duration = currentAttackData.attackDuration;
                lastAttackTime = Time.time;

                SetAnimSetting();
                SetAttackObjSetting();
                SetCounterData();
                SetupDelayedAttack();
            }
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

        /// <summary>
        ///  攻撃処理実行
        /// </summary>
        private void AttackProcess()
        {
            if (currentAttackData == null) return;
            AttackProcessor_Refacta.ProcessAttack(currentAttackData, characterController, cloneController.gameObject);
            lastAttackData = currentAttackData;
        }

        /// <summary>
        /// アニメーション設定
        /// </summary>
        private void SetAnimSetting()
        {
            if (currentAttackData == null) return;
            cloneController.GetAnim().speed = currentAttackData.animationSpeed;
            Debug.Log($"AttackCommand: SetAnimSetting: {currentAttackData.animationTrigger}, Speed: {currentAttackData.animationSpeed}");
            cloneController.GetAnim().SetBool(currentAttackData.animHash, true);
        }

        /// <summary>
        /// 攻撃オブジェクト生成設定
        /// </summary>
        private void SetAttackObjSetting()
        {
            if (currentAttackData == null || currentAttackData.attackPrefab == null) return;

            var obj = CharaEffectFactory.I.GetEffectObj(currentAttackData.attackPrefab);
            var t = cloneController.transform;

            Vector3 spawnPosition;

            // Chain攻撃の場合、前回のオブジェクトの現在位置を使用
            if (CanChain() && lastAttackObject != null && currentAttackData.isChainPos)
            {
                spawnPosition = lastAttackObject.transform.position;

                // Chain攻撃時のオフセットを適用
                var offset = lastAttackObject.transform.right * currentAttackData.prefabOffset.x +
                             lastAttackObject.transform.up * currentAttackData.prefabOffset.y +
                             lastAttackObject.transform.forward * currentAttackData.prefabOffset.z;
                spawnPosition += offset;
                if (lastAttackObject == null) return;

                var controller = lastAttackObject.GetComponent<AttackObjectController>();
                //FirstOrDefaultは最初に用件を満たすものを返す
                var lifeTime = controller?.Behaviours.FirstOrDefault(b => b is AttackLifeTime) as AttackLifeTime;
                lifeTime?.ResetLifeTime();
            }
            else
            {
                // 通常攻撃の場合、キャラクター基準の位置
                var offset = t.right * currentAttackData.prefabOffset.x +
                             t.up * currentAttackData.prefabOffset.y +
                             t.forward * currentAttackData.prefabOffset.z;
                spawnPosition = t.position + offset;
            }

            obj.transform.position = spawnPosition;

            var rot = currentAttackData.prefabRotation;
            if (t.forward.x < 0) rot.y = 180 - rot.y;
            obj.transform.rotation = Quaternion.Euler(rot);

            var attackObjController = obj.GetComponent<AttackObjectController>();
            attackObjController?.SetPlayer(characterController.PlayerID, cloneController.gameObject);

            // 現在のオブジェクトを記録
            lastAttackObject = obj;
        }

        /// <summary>
        /// カウンター攻撃用設定
        /// </summary>
        private void SetCounterData()
        {
            if (!currentAttackData.isCounter) return;

            cloneController.SetCanCounter(true);
            cloneController.SetCounterAction(() =>
            {
                if (currentAttackData != null)
                {
                    cloneController.GetAnim().SetBool(currentAttackData.animHash, false);
                }
                isCounter = true;
                currentAttackData = currentAttackData.nextChain;
                cloneController.GetCharacterState().ChangeAttackState();
            });
        }
        /// <summary>
        /// チェイン攻撃可能かを判定
        /// </summary>
        /// <returns></returns>
        private bool CanChain()
        {
            if (lastAttackData == null || !lastAttackData.canChain || lastAttackData.nextChain == null) return false;
            if (Time.time - lastAttackTime > lastAttackData.chainThreshold) return false;
            return true;
        }

        /// <summary>
        /// ヒットタイミング処理の遅延実行
        /// </summary>
        private void SetupDelayedAttack()
        {
            DelayUtility.StartDelayedActionWithPause(
                cloneController,
                currentAttackData.hitTiming,
                BattleJudge.I.GetPauseStateFunc,
                AttackProcess
            );
        }
    }
}