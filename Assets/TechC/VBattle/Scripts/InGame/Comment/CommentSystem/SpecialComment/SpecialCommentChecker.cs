using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 特殊コメントのクラス
    /// </summary>
    public class SpecialCommentChecker : MonoBehaviour
    {
        /// <summary>
        /// コメントが特殊コメントかどうかを判別し、Typeを返す
        /// </summary>
        /// <param name="commentText"></param>
        /// <returns></returns>
        public SpecialCommentType GetSpecialCommentType(string commentText)
        {
            if (string.IsNullOrEmpty(commentText))
            {
                return SpecialCommentType.None;
            }

            if (commentText == "草")
            {
                return SpecialCommentType.Grass;
            }
            return SpecialCommentType.None; // 当てはまってなかったらNone 
        }
    }
}
