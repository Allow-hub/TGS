using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    [Serializable]
    public class CommentSpawner
    {
        [SerializeField] private CommentProvider commentProvider;

        [Header("コメントのテキスト用Prefab")]
        [SerializeField] private GameObject commentPrefab;
        [SerializeField] private GameObject speedBuffPrefab;
        [SerializeField] private GameObject attackBuffPrefab;
        [SerializeField] private GameObject mapChangePrefab;

        [Header("コメントが出現する場所")]
        [SerializeField] private Transform topRightSpawnPos;
        [SerializeField] private Transform bottomRightSpawnPos;
        private float topRightSpawnPosY = 5.0f;
        private float bottomRightSpawnPosY = -5.0f;
        private float spawnPosX = 10.0f;

        private const float PLAYER_TOP_OFFSET = -5.3f;
        private bool isInitialized = false;

        public void Init()
        {
            topRightSpawnPosY = topRightSpawnPos.position.y;
            bottomRightSpawnPosY = bottomRightSpawnPos.position.y;
            spawnPosX = topRightSpawnPos.position.x;
            isInitialized = true;
        }

        /// <summary>
        /// コメントをcommentProviderを通じて発生させる処理
        /// </summary>
        /// <returns>生成されたコメントのGameObject</returns>
        public GameObject SpawnComment()
        {
            if (!isInitialized)
            {
                Init();
            }

            var commentData = commentProvider.GetRandomComment();
            GameObject comment = CommentFactory.I.GetComment(commentData, GetCommentPrefab(commentData));

            // 特殊コメントの処理
            var sp = comment.GetComponent<FreezeCommentTrigger>();
            var spType = SpecialCommentChecker.GetSpecialCommentType(commentData.text);
            Material commentMaterial;

            var materialApplier = CommentDisplay.I.GetMaterialApplier();

            if (spType != SpecialCommentType.None && sp != null)
                commentMaterial = materialApplier.GetCommentMaterial(null, sp.SpecialType);
            else
                commentMaterial = materialApplier.GetCommentMaterial(commentData.type);

            List<GameObject> spawnedChars = AllCharacterHelper.ProcessCommentText(commentData.text, comment.transform, Color.white);
            
            // マテリアルを適用
            materialApplier.ApplyMaterialToCharacters(spawnedChars, commentMaterial);

            float randomY = UnityEngine.Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
            comment.transform.position = new Vector3(spawnPosX, randomY, PLAYER_TOP_OFFSET);

            var freezeCommentTrigger = comment.GetComponent<FreezeCommentTrigger>();

            // 移動処理を開始
            CommentDisplay.I.GetMover().StartMoving(comment.transform, spawnedChars, freezeCommentTrigger, commentMaterial);

            return comment;
        }

        private GameObject GetCommentPrefab(CommentData commentData)
        {
            switch (commentData.type)
            {
                case CommentType.Normal:
                    return commentPrefab;
                case CommentType.AttackBuff:
                    return attackBuffPrefab;
                case CommentType.MapChange:
                    return mapChangePrefab;
                case CommentType.SpeedBuff:
                    return speedBuffPrefab;
                default:
                    return commentPrefab;
            }
        }
    }
}
