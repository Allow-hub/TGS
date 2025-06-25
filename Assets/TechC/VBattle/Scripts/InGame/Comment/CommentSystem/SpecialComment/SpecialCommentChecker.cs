namespace TechC
{
    /// <summary>
    /// 特殊コメントのクラス
    /// </summary>
    public static class SpecialCommentChecker
    {
        /// <summary>
        /// コメントが特殊コメントかどうかを判別し、Typeを返す
        /// </summary>
        /// <param name="commentText"></param>
        /// <returns></returns>
        public static SpecialCommentType GetSpecialCommentType(string commentText)
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
