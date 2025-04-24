using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コメントタイプを定義
    /// </summary>
    public enum CommentType { Normal, Buff, MapChange }

    public class CommentData
    {
        public CommentType type;
        public string text;
        public BuffType? buffType;

        /* コンストラクタでコメントタイプ、テキスト、バフタイプを設定 */
        public CommentData(CommentType type, string text, BuffType? buffType)
        {
            this.type = type;
            this.text = text;
            this.buffType = buffType;
        }
    }
    /// <summary>
    /// ランダムにコメントを選び提供する
    /// </summary>
    public class CommentProvider : MonoBehaviour
    {
        [Header("コメントデータ")]
        public NormalCommentData normalComments;
        public List<BuffCommentData> buffComments;

        [Header("コメントの出現確率")]
        [SerializeField, Range(0f, 1f)] private float normalChance = 0.7f;
        [SerializeField, Range(0f, 1f)] private float buffChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float mapChangeChance = 0.1f;

        private float totalChance; /* 合計確率 */

        /* バフコメント（MapChange以外） */
        private List<BuffCommentData> normalBuffs;

        /* マップ変更用バフコメント */
        private List<BuffCommentData> mapChangeBuffs;

        private void Awake()
        {
            totalChance = normalChance + buffChance + mapChangeChance;

            /* 確率が0またはマイナスならデフォルト値に設定 */
            if (totalChance <= 0f)
            {
                normalChance = 0.7f;
                buffChance = 0.2f;
                mapChangeChance = 0.1f;
                totalChance = 1.0f;
            }

            /* buffCommentsを事前にフィルタリングして分類 */
            normalBuffs = new List<BuffCommentData>();
            mapChangeBuffs = new List<BuffCommentData>();

            foreach (var buff in buffComments)
            {
                if (buff.buffType == BuffType.MapChange)
                {
                    mapChangeBuffs.Add(buff);
                }
                else
                {
                    normalBuffs.Add(buff);
                }
            }
        }

        /* ランダムなコメントを取得するメソッド */
        public CommentData GetRandomComment()
        {
            /* ランダムな値を計算 */
            float randomValue = Random.value * totalChance;

            /* 通常コメントの確率 */
            if (randomValue < normalChance)
            {
                string text = normalComments.comment[Random.Range(0, normalComments.comment.Length)];
                return new CommentData(CommentType.Normal, text, null);
            }
            /* バフコメントの確率 */
            else if (randomValue < normalChance + buffChance)
            {
                /* MapChange以外のバフコメントをランダムに選択 */
                if (normalBuffs.Count > 0)
                {
                    var buff = normalBuffs[Random.Range(0, normalBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.Buff, text, buff.buffType);
                }
            }
            /* マップ変更コメントの確率 */
            else
            {
                /* マップ変更用バフコメントをランダムに選択 */
                if (mapChangeBuffs.Count > 0)
                {
                    var buff = mapChangeBuffs[Random.Range(0, mapChangeBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.MapChange, text, buff.buffType);
                }
            }

            /* デフォルト fallback（通常コメント） */
            string fallback = normalComments.comment[Random.Range(0, normalComments.comment.Length)];
            return new CommentData(CommentType.Normal, fallback, null);
        }
    }

}
