using System.Collections.Generic;
using UnityEngine;
using System;

namespace TechC.CommentSystem
{
    /// <summary>
    /// 固定コメントのクラス
    /// </summary>
    [Serializable]
    public class FreezeController
    {
        /// <summary>
        /// コメント本体と文字オブジェクトをプールに返却する
        /// </summary>
        public void ReturnCommentAndChars(GameObject comment, List<GameObject> chars)
        {
            if (chars != null)
            {
                foreach (var obj in chars)
                {
                    if (obj != null && obj.activeInHierarchy)
                    {
                        CommentFactory.I.ReturnChar(obj);
                    }
                }
            }
            if (comment != null && comment.activeInHierarchy)
            {
                CommentFactory.I.ReturnComment(comment);
            }
        }
    }
}
