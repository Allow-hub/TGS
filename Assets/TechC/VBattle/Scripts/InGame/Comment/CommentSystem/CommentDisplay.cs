using System;
using System.Collections;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コメントの表示管理とフリーズ機能を担当
    /// </summary>
    public class CommentDisplay : Singleton<CommentDisplay>
    {
        [SerializeField] private CommentSpawner commentSpawner;
        [SerializeField] private CommentMaterialApplier commentMaterialApplier;
        [SerializeField] private CommentMover commentMover;

        [Header("コメント生成の設定")]
        [SerializeField] private float speed = 100.0f;
        [SerializeField] private float commentInterval = 1.0f;

        [Header("特殊コメントの設定")]
        [SerializeField] private float freezeTime = 3f;
        public bool IsCommentFrozen { get; private set; } = false;

        private bool isSpawning = false;

        protected override bool UseDontDestroyOnLoad => false;

        void Start()
        {
            DelayUtility.StartDelayedAction(this, 0f, () =>
            {
                StartCommentSpawning();
            });
        }

        protected override void Init()
        {
            base.Init();
            commentSpawner.Init();
            commentMover.Init();
        }

        /// <summary>
        /// コメントの自動生成を開始
        /// </summary>
        public void StartCommentSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;
                StartCoroutine(SpawnCommentWithInterval());
            }
        }

        /// <summary>
        /// 指定したインターバルでコメントを生成
        /// </summary>
        private IEnumerator SpawnCommentWithInterval()
        {
            // フリーズ・ポーズ状態を考慮した条件関数
            Func<bool> isPausedFunc = () => IsCommentFrozen || BattleJudge.I.IsPaused;

            // DelayUtilityのポーズ対応版を使用してコメント生成処理を開始
            DelayUtility.StartRepeatedActionWhileWithPause(
                this,
                () => isSpawning,
                commentInterval,
                isPausedFunc,
                () =>
                {
                    // コメントを生成
                    GameObject spawnedComment = commentSpawner.SpawnComment();
                    
                    // 生成されたコメントにマテリアルを適用
                    ApplyMaterialToSpawnedComment(spawnedComment);
                }
            );

            yield break;
        }

        /// <summary>
        /// 生成されたコメントにマテリアルを適用
        /// </summary>
        private void ApplyMaterialToSpawnedComment(GameObject comment)
        {
            if (comment == null) return;

            // CommentSpawnerから最後に生成されたコメントのデータを取得
            var commentData = commentSpawner.GetLastCommentData();
            var characters = commentSpawner.GetLastCharacters();

            // 特殊コメントのチェック
            var freezeCommentTrigger = comment.GetComponent<FreezeCommentTrigger>();
            var spType = SpecialCommentChecker.GetSpecialCommentType(commentData.text);
            Material targetMaterial;

            if (spType != SpecialCommentType.None && freezeCommentTrigger != null)
            {
                // フリーズコメントのマテリアル
                targetMaterial = commentMaterialApplier.GetCommentMaterial(null, spType);
            }
            else
            {
                // 通常コメントのマテリアル
                targetMaterial = commentMaterialApplier.GetCommentMaterial(commentData.type);
            }

            // マテリアルを適用
            commentMaterialApplier.ApplyMaterialToCharacters(characters, targetMaterial);
            
            // 移動処理を開始
            GetMover().StartMoving(comment.transform, characters, freezeCommentTrigger, targetMaterial);
        }

        /// <summary>
        /// CommentMaterialApplierインスタンスを取得
        /// </summary>
        public CommentMaterialApplier GetMaterialApplier()
        {
            return commentMaterialApplier;
        }

        /// <summary>
        /// CommentMoverインスタンスを取得
        /// </summary>
        public CommentMover GetMover()
        {
            return commentMover;
        }

        /// <summary>
        /// FreezeCommentTriggerから直接呼ばれるメソッド
        /// </summary>
        public void OnFreezeTriggered()
        {
            if (!IsCommentFrozen)
            {
                StartCoroutine(FreezeAllCommentsCoroutine());
            }
        }

        private IEnumerator FreezeAllCommentsCoroutine()
        {
            IsCommentFrozen = true;
            yield return new WaitForSeconds(freezeTime);
            IsCommentFrozen = false;
        }

        public float GetCurrentSpeed() => speed;

        public void SetSpeed(float newSpeed) => speed = newSpeed;

        public void AddSpeed(float amount) => speed += amount;
    }
}