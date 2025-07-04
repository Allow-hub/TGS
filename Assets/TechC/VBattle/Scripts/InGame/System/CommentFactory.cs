using UnityEngine;

namespace TechC
{
    public class CommentFactory : Singleton<CommentFactory>
    {
        [SerializeField] private ObjectPool commentPool;

        [Header("文字とそのPrefabのScriptableObject")]
        [SerializeField] private CharPrefabDatabase charPrefabDatabase;
        protected override bool UseDontDestroyOnLoad => false;

        // 3DText用のスケール定数
        private static readonly Vector3 COMMENT_OBJ_SCALE = new Vector3(0.25f, 0.25f, 0.25f);

        /// <summary>
        /// コメントを取得する
        /// </summary>
        /// <param name="commentData"></param>
        /// <param name="commentPrefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject GetComment(CommentData commentData, GameObject commentPrefab, Transform parent)
        {
            GameObject obj = commentPool.GetObject(commentPrefab);
            obj.transform.localScale = COMMENT_OBJ_SCALE;

            if (commentData.type == CommentType.Normal)
            {
                var abilityTrigger = obj.GetComponent<FreezeCommentTrigger>();
                if (abilityTrigger == null)
                    abilityTrigger = obj.AddComponent<FreezeCommentTrigger>();
                abilityTrigger.GetType().GetProperty("SpecialType").SetValue(abilityTrigger, SpecialCommentChecker.GetSpecialCommentType(commentData.text));
                abilityTrigger.GetType().GetProperty("CommentText").SetValue(abilityTrigger, commentData.text);
            }
            else
            {
                var commentTrigger = obj.GetComponent<BuffCommentTrigger>();
                commentTrigger?.Init(commentPool);
                if (commentTrigger != null)
                {
                    commentTrigger.specialCommentType = SpecialCommentChecker.GetSpecialCommentType(commentData.text);
                    commentTrigger.commentText = commentData.text;
                }
                if (commentData.buffType.HasValue)
                {
                    commentTrigger.buffType = commentData.buffType.Value;
                }
            }
            return obj;
        }


        public void ReturnComment(GameObject comment)
        {
            commentPool.ReturnObject(comment);
        }

        public GameObject GetChar(string charName)
        {

            GameObject charPrefab = null;
            foreach (var entry in charPrefabDatabase.entries)
            {
                if (entry.charText == charName)
                {
                    charPrefab = entry.charPrefab;
                    break;
                }
            }

            if (charPrefab == null)
            {
                Debug.LogError($"その文字はcharPrefabDatabaseに登録されていません: {charName}");
                return null;
            }

            // ObjectPoolから取得
            GameObject charObj = commentPool.GetObject(charPrefab);
            return charObj;
        }

        // 文字オブジェクトをプールに返却
        public void ReturnChar(GameObject charObj)
        {
            commentPool.ReturnObject(charObj);
        }
    }
}
