using Cysharp.Threading.Tasks;
using UnityEngine;
using Windows.Win32.Foundation;
using System.Collections.Generic;

namespace TechC
{
    /// <summary>
    /// キャラ１：あめの強攻撃の実装
    /// </summary>
    public class Ame_StrongAttack : StrongAttack
    {
        [Header("プレハブの参照")]
        [SerializeField] private GameObject swordObj;
        [SerializeField] private GameObject magicCircle;
        [SerializeField] private GameObject iceDataPrefab;
        [SerializeField] private GameObject iceExplosionPrefab;
        [SerializeField] private GameObject iceRosePrefab;
        [SerializeField] private GameObject iceWallPrefab;
        [SerializeField] private GameObject bladeStormPrefab;

        [Header("ニュートラル強")]
        [SerializeField] private float magicDuration = 2f;
        [SerializeField] private TransformRecorder transformRecorder;
        [SerializeField] private CommandHistory commandHistory;
        [SerializeField] private GameObject iceCloneObj; // 分身のプレハブ
        [SerializeField] private float echoTimeInterval = 3.0f; // 再現する時間幅
        private bool isCloneAttacking = false;

        [Header("左強")]
        [SerializeField] private float yOffset = 2;
        [SerializeField] private float leftStrongVelocity;
        [SerializeField] private float explosionDuration = 3f;
        [SerializeField] private Sprite zipSprite;
        [SerializeField] private Vector2 zipSize = new Vector2(200, 200);
        [SerializeField] private float moveSpeed = 10f;

        [SerializeField] private int smooth = 16;
        [SerializeField] private Vector2 windowOffeset = new Vector2(20, -100);
        // 左強攻撃用のデータクラス
        private class LeftAttackData
        {
            public GameObject iceObj;
            public NativeWindow window;
            public int attackId;
            public int count;
            public bool isReadyForExplosion;
        }
        
        private List<LeftAttackData> activeLeftAttacks = new List<LeftAttackData>();
        private int leftAttackIdCounter = 0;

        [Header("右強")]
        [SerializeField] private float iceWallDuration = 5.0f;
        [SerializeField] private float wallOffsetX = 1.5f;
        [SerializeField] private float wallOffsetY = 1.5f;

        [Header("上強")]
        [SerializeField] private float returnStrongUpEffectTime = 3f;
        [SerializeField] private float upwardVelocity = 2.5f;

        /// <summary>
        /// 数秒前の自分が氷で実体化し、攻撃も記録通りなぞってくれる
        /// </summary>
        public override void NeutralAttack()
        {
            if (isCloneAttacking) return;
            base.NeutralAttack();
            GameObject cloneObj = null;

            magicCircle.SetActive(true);
            DelayUtility.StartDelayedActionWithPause(this, magicDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                magicCircle.SetActive(false);
            });
            DelayUtility.StartDelayedActionWithPause(this, neutralAttackData.hitTiming, BattleJudge.I.GetPauseStateFunc, () =>
            {
                cloneObj = Instantiate(iceCloneObj);
                transformRecorder.StartReplayFromSecondsAgo(echoTimeInterval, cloneObj.transform);

                if (commandHistory == null)
                {
                    Debug.LogWarning("CommandHistoryが見つかりませんでした");
                    return;
                }
                var cloneController = cloneObj.GetComponent<Player.CharacterController>();
                cloneController.SetClonePlayerID(characterController.PlayerID);
                if (characterController.GetCharacterState().AttackManager == null) return;
                commandHistory.ReplayAttackCommandsFromSecondsAgo(echoTimeInterval, cloneController.GetCharacterState().AttackManager);
                isCloneAttacking = true;

            });
            DelayUtility.StartDelayedActionWithPause(this, echoTimeInterval, BattleJudge.I.GetPauseStateFunc, () =>
            {
                Destroy(cloneObj);
                isCloneAttacking = false;
            });
        }

        /// <summary>
        /// 氷の魔法を圧縮データにして飛ばす、二回目の入力で解凍
        /// その場で爆発が起こる
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();
            
            // 爆発可能な攻撃データを探す
            var explosionTarget = GetExplosionReadyAttack();
            
            if (explosionTarget != null)
            {
                // 爆発処理
                ExecuteExplosion(explosionTarget);
            }
            else
            {
                // 新しいアイスデータを作成
                CreateNewIceData();
            }

            AudioManager.I.PlayCharacterSE(CharacterType.Ame, CharacterSEType.StrongLeftAttack);
        }

        /// <summary>
        /// 爆発可能な攻撃データを取得
        /// </summary>
        private LeftAttackData GetExplosionReadyAttack()
        {
            foreach (var attack in activeLeftAttacks)
            {
                if (attack.isReadyForExplosion)
                {
                    return attack;
                }
            }
            return null;
        }

        /// <summary>
        /// 新しいアイスデータを作成
        /// </summary>
        private void CreateNewIceData()
        {
            // 新しい攻撃データを作成
            var attackData = new LeftAttackData
            {
                attackId = ++leftAttackIdCounter,
                count = 1,
                isReadyForExplosion = false
            };

            var pos = transform.position.AddY(yOffset);
            attackData.iceObj = CharaEffectFactory.I.GetEffectObj(iceDataPrefab, pos, Quaternion.identity);
            attackData.window = WindowFactory.I.GetWindow(WindowFactory.WindowType.Image);
            
            var imageWindow = attackData.window as ImageWindow;
            var offsetX = windowOffeset.x * characterController.transform.forward.x;
            var windowPos = attackData.iceObj.transform.position.AddX(offsetX).AddY(windowOffeset.y);
            var screenPos = WindowManager.I.WorldToWindowsScreenPosition(windowPos);
            imageWindow.SetImage(zipSprite.texture);
            WindowUtility.MoveWindow((HWND)attackData.window.Hwnd, (int)screenPos.x, (int)screenPos.y);
            WindowUtility.ResizeWindow((HWND)attackData.window.Hwnd, (int)zipSize.x, (int)zipSize.y);
            attackData.window.SetRect();
            
            if (characterController.transform.forward.x < 0)
            {
                WindowUtility.MoveWindowToTargetAsync(attackData.window, -Screen.width, (int)screenPos.y, moveSpeed, smooth, zipSprite.texture).Forget();
            }
            else
            {
                WindowUtility.MoveWindowToTargetAsync(attackData.window, Screen.width, (int)screenPos.y, moveSpeed, smooth, zipSprite.texture).Forget();
            }
            
            RegisterEffect(attackData.iceObj);
            activeLeftAttacks.Add(attackData);

            var rb = attackData.iceObj.GetComponent<Rigidbody>();
            rb.velocity = transform.forward * leftStrongVelocity;
            
            // 少し遅れて爆発可能状態にする
            var capturedAttackData = attackData;
            DelayUtility.StartDelayedActionWithPause(this, 0.5f, BattleJudge.I.GetPauseStateFunc, () =>
            {
                if (capturedAttackData != null && activeLeftAttacks.Contains(capturedAttackData))
                {
                    capturedAttackData.isReadyForExplosion = true;
                }
            });
            
            // 自動クリーンアップ（爆発されなかった場合）
            DelayUtility.StartDelayedActionWithPause(this, explosionDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                if (activeLeftAttacks.Contains(capturedAttackData))
                {
                    CleanupLeftAttackData(capturedAttackData, true);
                }
            });
        }

        /// <summary>
        /// 爆発処理を実行
        /// </summary>
        private void ExecuteExplosion(LeftAttackData explosionTarget)
        {
            if (explosionTarget.window != null)
            {
                WindowUtility.AnimateResizeWindowAsync(explosionTarget.window.Hwnd, 0, 0, 0.3f).Forget();
                DelayUtility.StartDelayedActionWithPause(this, 0.3f, BattleJudge.I.GetPauseStateFunc, () =>
                {
                    if (explosionTarget.window != null)
                    {
                        WindowFactory.I.ReturnWindow(explosionTarget.window);
                        explosionTarget.window = null;
                    }
                });
            }
            
            var createPos = explosionTarget.iceObj.transform.position;
            UnregisterEffect(explosionTarget.iceObj);
            CharaEffectFactory.I.ReturnEffectObj(explosionTarget.iceObj);

            var explosionObj = CharaEffectFactory.I.GetEffectObj(iceExplosionPrefab, createPos, Quaternion.identity);
            RegisterEffect(explosionObj);

            var charaEffectSetting = explosionObj.GetComponent<CharaEffect>();
            charaEffectSetting.SetOwnerId(characterController.PlayerID);
            charaEffectSetting.SetAttackProcessor(attackProcessor);

            DelayUtility.StartDelayedActionWithPause(this, explosionDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                UnregisterEffect(explosionObj);
                CharaEffectFactory.I.ReturnEffectObj(explosionObj);
            });

            // 使用した攻撃データを削除
            activeLeftAttacks.Remove(explosionTarget);
        }

        /// <summary>
        /// 左強攻撃データのクリーンアップ
        /// </summary>
        private void CleanupLeftAttackData(LeftAttackData attackData, bool removeFromList = true)
        {
            if (attackData == null) return;

            if (attackData.iceObj != null)
            {
                UnregisterEffect(attackData.iceObj);
                CharaEffectFactory.I.ReturnEffectObj(attackData.iceObj);
                attackData.iceObj = null;
            }

            if (attackData.window != null)
            {
                WindowFactory.I.ReturnWindow(attackData.window);
                attackData.window = null;
            }

            if (removeFromList)
            {
                activeLeftAttacks.Remove(attackData);
            }
        }

        /// <summary>
        /// 前方に氷の壁を床から飛び出させる
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();
            GameObject iceWallObj = null;

            DelayUtility.StartDelayedActionWithPause(this, rightAttackData.hitTiming, BattleJudge.I.GetPauseStateFunc, () =>
            {
                var wallPos = transform.position.AddX(wallOffsetX).AddY(wallOffsetY);
                iceWallObj = CharaEffectFactory.I.GetEffectObj(iceWallPrefab, wallPos, Quaternion.identity);
                RegisterEffect(iceWallObj);

                var charaEffect = iceWallObj.GetComponent<CharaEffect>();
                charaEffect?.SetOwnerId(characterController.PlayerID);
                charaEffect?.SetAttackProcessor(attackProcessor);
            });

            DelayUtility.StartDelayedActionWithPause(this, iceWallDuration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                if (iceWallObj != null)
                {
                    UnregisterEffect(iceWallObj);
                    CharaEffectFactory.I.ReturnEffectObj(iceWallObj);
                }
            });
        }

        /// <summary>
        /// 下に剣を突き立てて周囲に氷の薔薇を咲かせて範囲攻撃
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();
            ActiveSword(downAttackData.attackDuration);
        }

        /// <summary>
        /// 上に剣を突き出し刃の竜巻を発生させる
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();

            Vector3 spawnPos = transform.position + Vector3.up * yOffset;
            GameObject stormObj = CharaEffectFactory.I.GetEffectObj(bladeStormPrefab, spawnPos, Quaternion.identity);
            RegisterEffect(stormObj);

            float scaleMultiplier = 1f;
            float chance = Random.value;
            if (chance < 0.3f) scaleMultiplier = 1.8f; //30%の確率で大きく
            else if (chance < 0.5f) scaleMultiplier = 0.3f; //20%の確率で小さく

            stormObj.transform.localScale *= scaleMultiplier;

            var rb = stormObj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = transform.up * upwardVelocity;

            var charaEffect = stormObj.GetComponent<CharaEffect>();
            charaEffect?.SetOwnerId(characterController.PlayerID);
            charaEffect?.SetAttackProcessor(attackProcessor);

            DelayUtility.StartDelayedActionWithPause(this, returnStrongUpEffectTime, BattleJudge.I.GetPauseStateFunc, () =>
            {
                UnregisterEffect(stormObj);
                CharaEffectFactory.I.ReturnEffectObj(stormObj);
            });
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
            ActiveSword(attackData.attackDuration);
        }

        private void ActiveSword(float duration)
        {
            swordObj.gameObject.SetActive(true);
            DelayUtility.StartDelayedActionWithPause(this, duration, BattleJudge.I.GetPauseStateFunc, () =>
            {
                swordObj.gameObject.SetActive(false);
            });
        }

        /// <summary>
        /// オブジェクト破棄時のクリーンアップ
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            // 残っている左強攻撃データをすべてクリーンアップ
            foreach (var attackData in activeLeftAttacks)
            {
                CleanupLeftAttackData(attackData, false);
            }
            activeLeftAttacks.Clear();
        }
    }
}