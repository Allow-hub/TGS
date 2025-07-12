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
        }

        /// <summary>
        /// コメントをcommentProviderを通じて発生させる処理
        /// </summary>
        /// <returns>生成されたコメントのGameObject</returns>
        public GameObject SpawnComment()
        {
            var commentData = commentProvider.GetRandomComment();
            GameObject comment = CommentFactory.I.GetComment(commentData, GetCommentPrefab(commentData));

            List<GameObject> spawnedChars = AllCharacterHelper.ProcessCommentText(commentData.text, comment.transform, Color.white);

            float randomY = UnityEngine.Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
            comment.transform.position = new Vector3(spawnPosX, randomY, PLAYER_TOP_OFFSET);

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
