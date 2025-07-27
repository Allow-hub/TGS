namespace TechC.CommentSystem
{
   /// <summary>
    /// コメントタイプを定義
    /// </summary>
    public enum CommentType { Normal, SpeedBuff, AttackBuff, MapChange, Special }

    public class CommentData
    {
        public CommentType type;
        public string text;
        public BuffType? buffType;
        public SpecialCommentType? specialType;

        /* コンストラクタでコメントタイプ、テキスト、バフタイプを設定 */
        public CommentData(CommentType type, string text, BuffType? buffType, SpecialCommentType? specialType = null)
        {
            this.type = type;
            this.text = text;
            this.buffType = buffType;
            this.specialType = specialType;
        }
    }
}
