using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TechC
{
    public class CommentFactory : Singleton<CommentFactory>
    {
        [SerializeField]
        private ObjectPool commentPool;

        public TMP_Text GetComment(CommentData commentData, GameObject commentPrefab, Transform parent)
        {
            GameObject obj = commentPool.GetObject(commentPrefab);
            // Debug.Log($"現在のcommentPrefabは{commentPrefab}");
            var commentTrigger = obj.GetComponent<BuffCommentTrigger>();
            // Debug.Log(commentTrigger);
            commentTrigger?.Init(commentPool);

            if (obj != null)
            {
                TMP_Text text = obj.GetComponent<TMP_Text>();
                text.text = commentData.text;
                obj.SetActive(true);

                if (commentData.buffType.HasValue)
                {
                    BuffCommentTrigger trigger = obj.GetComponent<BuffCommentTrigger>();
                    if (trigger != null)
                    {
                        trigger.buffType = commentData.buffType.Value;
                    }
                }

                return text;
            }

            return null;
        }


        public void ReturnComment(GameObject comment)
        {
            commentPool.ReturnObject(comment);
        }

        public GameObject GetChar(string charName) =>
            commentPool.GetObjectByName(charName);
    }
}
