using UnityEngine;

namespace TechC
{
    /// <summary>
    ///　キャラ２：照海の弱攻撃の実装
    /// </summary>
    public class Terami_WeakAttack : WeakAttack
    {
        [Header("エフェクトのプレハブや参照")]
        [SerializeField] private GameObject swingPrefab; // 通常攻撃
        [SerializeField] private GameObject marshmallowPrefab; // 左弱用 
        [SerializeField] private GameObject spinPrefab; // 右弱用 
        [SerializeField] private GameObject donutPrefab; // 下弱用
        [SerializeField] private GameObject chocolatePrefab; // 上弱用

        [Header("ニュートラルアタックの設定")]
        [SerializeField] private float swingEffectDistance = 5f;
        [SerializeField] private Quaternion n1Rot;
        [SerializeField] private Quaternion n2Rot;
        [SerializeField] private Quaternion n3Rot;
        private float returnNeutralEffectTime = 1f;
        private Quaternion currentSwingRot;

        [Header("左弱")]
        [SerializeField] private float LeftYOffset = 1f;

        [SerializeField] private float marshmallowThrowSpeed = 10f;
        [SerializeField] private float returnLeftEffectTime = 3.0f;


        [Header("右弱")]
        [SerializeField] private float RightYOffset = 2f;
        [SerializeField] private float returnRightEffectTime = 3.0f;


        [Header("下弱")]
        [SerializeField] private float donutCooldown = 5f;
        [SerializeField] private float donutXOffset = 2f;
        private Vector3 donutScale = new Vector3(1f, 0.1f, 1f); // 仮の大きさのためモデルができたら調整する
        private bool canPlaceDonut = true;

        [Header("上弱")]
        [SerializeField] private float UpYOffset = 5f;
        [SerializeField] private float chocolateFallSpeed = 10f;
        [SerializeField] private float returnUpEffectTime = 5.0f;

        /// <summary>
        /// ゴムベラを前に振る、前方への軽い攻撃。3回目で派生
        /// </summary>
        public override void NeutralAttack()
        {
            base.NeutralAttack();

            //ニュートラルが何段階目かを確かめる
            if (currentNeutral == neutralAttackData_1)
                currentSwingRot = n1Rot;
            else if (currentNeutral == neutralAttackData_2)
                currentSwingRot = n2Rot;
            else if (currentNeutral == neutralAttackData_3)
                currentSwingRot = n3Rot;
            var swObjPos = transform.position.AddY(swingEffectDistance);
            // 向きに応じて回転反転
            if (transform.forward.x < 0)
            {
                currentSwingRot = Quaternion.Euler(0, 180, 0) * currentSwingRot;
            }

            //slashEffectの取得。各段階の回転を反映
            var swObj = CharaEffectFactory.I.GetEffectObj(swingPrefab, swObjPos, currentSwingRot);

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnNeutralEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(swObj);
            });
            AudioManager.I.PlayCharacterSE(CharacterType.Terami,CharacterSEType.WeakNormalAttack_1);
        }

        /// <summary>
        /// マシュマロを投げて相手に当たったら爆発する、飛び道具側の攻撃はCharaEffectが行う
        /// </summary>
        public override void LeftAttack()
        {
            base.LeftAttack();

            GameObject marshObj = null;
            DelayUtility.StartDelayedAction(this, rightAttackData.hitTiming, () =>
            {
                // マシュマロの処理 
                var marshPos = transform.position;
                marshPos = transform.position.AddY(LeftYOffset);
                marshObj = CharaEffectFactory.I.GetEffectObj(marshmallowPrefab, marshPos, Quaternion.identity);
                var effectSetting = marshObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID);
                var rb = marshObj.GetComponent<Rigidbody>();
                // マシュマロをrbで飛ばす 
                rb.velocity = transform.forward * marshmallowThrowSpeed;
            });

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnLeftEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(marshObj);
            });
            AudioManager.I.PlayCharacterSE(CharacterType.Terami,CharacterSEType.WeakLeftAttack);
        }

        /// <summary>
        /// 1回転して敵を蹴る
        /// </summary>
        public override void RightAttack()
        {
            base.RightAttack();

            var spinObjPos = transform.position.AddY(RightYOffset);

            var spinEffect = CharaEffectFactory.I.GetEffectObj(spinPrefab, spinObjPos, Quaternion.identity);
            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnRightEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(spinEffect);
            });
            AudioManager.I.PlayCharacterSE(CharacterType.Terami,CharacterSEType.WeakRightAttack);
        }

        /// <summary>
        /// 下にドーナツホールを設置し、相手が踏むとダメージを受ける（強攻撃から移動）
        /// </summary>
        public override void DownAttack()
        {
            base.DownAttack();

            if (!canPlaceDonut) return;

            canPlaceDonut = false; // 連打防止開始
            GameObject donutObj = null;

            // ドーナツのPrefabを設置
            var donutPos = transform.position.AddX(donutXOffset);
            donutObj = CharaEffectFactory.I.GetEffectObj(donutPrefab, donutPos, Quaternion.identity);
            donutObj.transform.localScale = donutScale;
            var effectSetting = donutObj.GetComponent<CharaEffect>();

            effectSetting.SetAttackProcessor(attackProcessor);
            effectSetting.SetOwnerId(characterController.PlayerID);

            // ドーナツクールタイム解除タイマー。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, donutCooldown, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(donutObj);

                canPlaceDonut = true;
            });
            AudioManager.I.PlayCharacterSE(CharacterType.Terami,CharacterSEType.WeakDownAttack);
        }

        /// <summary>
        /// 相手の上部からチョコを落とす、チョコの攻撃はCharaEffectが行う
        /// </summary>
        public override void UpAttack()
        {
            base.UpAttack();

            // チョコを上から降らせる処理 
            GameObject chocolateObj = null; // チョコのPrefabの初期化 

            DelayUtility.StartDelayedAction(this, rightAttackData.hitTiming, () =>
            {
                // 相手の座標を取得 
                var otherPlayerPos = BattleJudge.I.GetOtherPlayerObjects(characterController.PlayerID)[0].transform.position; // [0]を取得しているのは1vs1限定 
                otherPlayerPos = otherPlayerPos.AddY(UpYOffset); // 高さを追加 

                // お菓子の処理 
                chocolateObj = CharaEffectFactory.I.GetEffectObj(chocolatePrefab, otherPlayerPos, Quaternion.identity);
                var effectSetting = chocolateObj.GetComponent<CharaEffect>();
                effectSetting.SetAttackProcessor(attackProcessor);
                effectSetting.SetOwnerId(characterController.PlayerID);
                var rb = chocolateObj.GetComponent<Rigidbody>();

                rb.velocity = -transform.up * chocolateFallSpeed; // チョコを落下 
            });

            //エフェクトの返却時間分待ったらReturn。実行はヘルパーメソッドで
            DelayUtility.StartDelayedAction(this, returnUpEffectTime, () =>
            {
                CharaEffectFactory.I.ReturnEffectObj(chocolateObj);
            });
            AudioManager.I.PlayCharacterSE(CharacterType.Terami,CharacterSEType.WeakUpAttack);
        }

        protected override void ExecuteAttack(AttackData attackData)
        {
            base.ExecuteAttack(attackData);
        }

    }
}