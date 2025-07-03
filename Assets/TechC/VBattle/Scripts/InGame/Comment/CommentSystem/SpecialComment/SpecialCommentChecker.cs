using System.Collections.Generic;

namespace TechC
{
    /// <summary>
    /// 特殊コメントの判定クラス
    /// </summary>
    public static class SpecialCommentChecker
    {
        // キーワードとタイプのマッピング
        private static readonly Dictionary<string, SpecialCommentType> keywordMap = new()
        {
            { "草", SpecialCommentType.Grass },
            { "固定", SpecialCommentType.Freeze },
        };

        /// <summary>
        /// コメントが特殊コメントかどうかを判別し、Typeを返す
        /// </summary>
        public static SpecialCommentType GetSpecialCommentType(string commentText)
        {
            if (string.IsNullOrEmpty(commentText))
                return SpecialCommentType.None;

            foreach (var kvp in keywordMap)
            {
                if (commentText.Contains(kvp.Key))
                    return kvp.Value;
            }
            return SpecialCommentType.None;
        }
    }
}
