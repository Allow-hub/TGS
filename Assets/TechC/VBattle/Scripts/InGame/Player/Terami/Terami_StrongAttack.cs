using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// キャラ２：照海の強攻撃の実装
    /// </summary>
    public class Terami_StrongAttack : StrongAttack
    {
        [Header("プレハブの参照")]

        [SerializeField] private GameObject healPrefab; // ニュートラ強：回復エフェクト
        [SerializeField] private GameObject popcornPrefab; // 右強：ポップコーン
        [SerializeField] private GameObject cookiePrefab; // 下強：クッキー
        // [SerializeField] private GameObject giant; // 上強：巨大化の際のエフェクト
        [Header("ニュートラル強")]
        [SerializeField] private float healCooldown = 5;
        [SerializeField] private float returnNeutralEffectTime = 1f;
        private Vector3 cottonCandyScale = new Vector3(0.2f, 0.2f, 0.2f); // 仮の大きさのためモデルができたら調整する

        private float cottonCandyXOffset = 2f;
        private float cottonCandyYOffset = 1f;

        private float yRot;
        private bool canHeal = true;

        // [SerializeField] private float 
        // [Header("左強")]
        [Header("右強")]
        private const int CENTER_INDEX_X_OFFSET = 1;
        private const int INITIAL_DELAY_MULTIPILIER = 1;
        private List<GameObject> generatedPopcorns = new List<GameObject>(); // 生成したポップコーンを管理するリスト
        [SerializeField] private float popcornYOffset = 3f;
        [SerializeField] private float opponentYOffset = 0.5f; // 相手のY方向の補正 
        [SerializeField] private float popcornSpeed = 3f;
        [SerializeField] private float popcornSpacing = 0.5f;
        [SerializeField] private float rightAttackCooldown = 4f; // クールダウン時間
        private Vector3 popcornScale = new Vector3(0.2f, 0.2f, 0.2f); // 仮の大きさのためモデルができたら調整する
        private int maxPopcornNum = 3;
        private float popcornFireInterval = 1f; // 発射間隔
        private float returnRightEffectTime = 5f;
        private bool canFire = true;

        [Header("下強")]
        [SerializeField] private float returnDownEffectTime = 3.0f;

        [Header("上強")]
        [SerializeField] private float scaleMultiplier = 2f;
        [SerializeField] private float moveSpeedMultiplier = 0.5f;
        [SerializeField] private float attackMultiplier = 2f; // 巨大化時の攻撃力倍率
        [SerializeField] private float giantDuration = 2f;
        [SerializeField] private float giantCooldown = 10f;
        private Vector3 originalScale; // プレイヤーの元の大きさ
        private bool isGiant = false;
        private bool isGiantCooldown = false;

        /// <summary>
        /// 回復アイテム（わたあめ）をキャラ２の目の前に出す、一定期間経過後再利用可能
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

            if (!canHeal) return;
            GameObject cottonCandyObj = null;
            canHeal = false;

            // わたあめを生成する処理
            var cottonCandyPos = transform.position;
            yRot = transform.eulerAngles.y;

            if (Mathf.Approximately(yRot, 90f)) // 右向き
            {
                cottonCandyPos = transform.position.AddX(cottonCandyXOffset).AddY(cottonCandyYOffset);
                cottonCandyObj = CharaEffectFactory.I.GetEffectObj(healPrefab, cottonCandyPos, Quaternion.identity);
                cottonCandyObj.transform.localScale = cottonCandyScale;
            }
            else
            {
                cottonCandyPos = transform.position.AddX(-cottonCandyXOffset).AddY(cottonCandyYOffset);
                cottonCandyObj = CharaEffectFactory.I.GetEffectObj(healPrefab, cottonCandyPos, Quaternion.identity);
                cottonCandyObj.transform.localScale = cottonCandyScale;
            }

            var effectSetting = cottonCandyObj.GetComponent<CharaEffect>();
            effectSetting.SetOwnerId(characterController.PlayerID);

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnNeutralEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(cottonCandyObj);
            });

            //わたあめが表示される時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, healCooldown, () =>
            {
                canHeal = true;
            });
        }

        /// <summary>
        /// 左強攻撃はWindowのギミックにするため、一旦保留
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();
        }

        /// <summary>
        /// 頭上にポップコーンを3つ生成し、一定期間後に相手に向かって発射
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();

            if (!canFire) return;

            canFire = false; // 連打防止開始
            GameObject popObj = null;
            generatedPopcorns.Clear();

            // 相手の座標を取得 
            var otherPlayerPos = BattleJudge.I.GetOtherPlayerObjects(characterController.PlayerID)[0].transform.position;
            otherPlayerPos += new Vector3(0, opponentYOffset, 0);

            // 飛び道具の処理
            for (int i = 0; i < maxPopcornNum; i++)
            {
                var pos = transform.position.AddY(popcornYOffset).AddX((i - CENTER_INDEX_X_OFFSET) * popcornSpacing);
                popObj = CharaEffectFactory.I.GetEffectObj(popcornPrefab, pos, Quaternion.identity);

                // オブジェクト初期化
                popObj.transform.localScale = popcornScale; // Prefabの大きさを設定
                var rb = popObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                var effectSetting = popObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID);
                generatedPopcorns.Add(popObj);
            }

            // 1秒おきに発射
            for (int i = 0; i < maxPopcornNum; i++)
            {
                int index = i;
                DelayUtility.StartDelayedAction(this, popcornFireInterval * (i + INITIAL_DELAY_MULTIPILIER), () =>
                {
                    if (index < generatedPopcorns.Count && generatedPopcorns[index] != null)
                    {
                        var rb = generatedPopcorns[index].GetComponent<Rigidbody>();
                        var currentPos = generatedPopcorns[index].transform.position;
                        Vector3 direction = (otherPlayerPos - currentPos).normalized;
                        rb.velocity = direction * popcornSpeed;
                    }
                });
            }

            // エフェクトの返却処理
            DelayUtility.StartDelayedAction(this, returnRightEffectTime, () =>
            {
                foreach (var popcornObj in generatedPopcorns)
                {
                    if (popcornObj != null)
                    {
                        // 返却前に物理演算を停止
                        var rb = popcornObj.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.velocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }
                        CharaEffectFactory.I.ReturnEffectObj(popcornObj);
                    }
                }
                generatedPopcorns.Clear();
            });

            // クールダウンタイマー（連打防止）
            DelayUtility.StartDelayedAction(this, rightAttackCooldown, () =>
            {
                canFire = true;
            });
        }

        /// <summary>
        /// 地面を叩いて、周囲にダメージを与える
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();

            var cookieObjPos = transform.position;

            var cookieEffect = CharaEffectFactory.I.GetEffectObj(cookiePrefab, cookieObjPos, Quaternion.identity);
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnDownEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(cookieEffect);
            });
        }

        /// <summary>
        /// 巨大化して、攻撃力および当たり判定が2倍。移動速度が1/2倍
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();

            if (isGiant || isGiantCooldown) return;

            isGiant = true;
            isGiantCooldown = true;

            originalScale = transform.localScale;

            // Playerに効果を発動させる
            transform.localScale = transform.localScale * scaleMultiplier;
            characterController.AddMultiplier(BuffType.Attack, BuffBase.VoidID, attackMultiplier);
            characterController.AddMultiplier(BuffType.Speed, BuffBase.VoidID, moveSpeedMultiplier);


            // 巨大化解除タイマー。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, giantDuration, () =>
            {
                // Playerに効果を取り消す
                transform.localScale = originalScale;
                characterController.RemoveMultiplier(BuffType.Attack, BuffBase.VoidID, attackMultiplier);
                characterController.RemoveMultiplier(BuffType.Speed, BuffBase.VoidID, moveSpeedMultiplier);

                isGiant = false;
            });

            // 巨大化クールタイム解除タイマー。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, giantCooldown, () =>
            {
                isGiantCooldown = false;
            });

        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
        }
        public override void ForceFinish()
        {
        }
    }
}