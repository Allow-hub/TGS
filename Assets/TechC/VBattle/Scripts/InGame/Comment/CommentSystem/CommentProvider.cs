using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
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
        [SerializeField, Range(0f, 1f)] private float speedBuffChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float attackBuffChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float mapChangeChance = 0.1f;

        private float totalChance; /* 合計確率 */

        /* バフコメント（Speed） */
        private List<BuffCommentData> speedBuffs;

        /* バフコメント（Attack） */
        private List<BuffCommentData> attackBuffs;

        /* マップ変更用バフコメント */
        private List<BuffCommentData> mapChangeBuffs;

        private void Awake()
        {
            totalChance = normalChance + speedBuffChance + attackBuffChance + mapChangeChance;

            /* 確率が0またはマイナスならデフォルト値に設定 */
            if (totalChance <= 0f)
            {
                normalChance = 0.7f;
                speedBuffChance = 0.1f;
                attackBuffChance = 0.1f;
                mapChangeChance = 0.1f;
                totalChance = 1.0f;
            }

            /* buffCommentsを事前にフィルタリングして分類 */
            speedBuffs = new List<BuffCommentData>();
            attackBuffs = new List<BuffCommentData>();
            mapChangeBuffs = new List<BuffCommentData>();

            foreach (var buff in buffComments)
            {
                if (buff.buffType == BuffType.MapChange)
                {
                    mapChangeBuffs.Add(buff);
                }
                else if (buff.buffType == BuffType.Speed)
                {
                    speedBuffs.Add(buff);
                }
                else if (buff.buffType == BuffType.Attack)
                {
                    attackBuffs.Add(buff);
                }
            }
        }

        /// <summary>
        /// ランダムなコメントを取得するメソッド
        /// </summary>
        /// <returns></returns>
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
            /* Speedバフコメントの確率 */
            else if (randomValue < normalChance + speedBuffChance)
            {
                /* Speedバフコメントをランダムに選択 */
                if (speedBuffs.Count > 0)
                {
                    var buff = speedBuffs[Random.Range(0, speedBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.SpeedBuff, text, buff.buffType);
                }
            }
            /* Attackバフコメントの確率 */
            else if (randomValue < normalChance + speedBuffChance + attackBuffChance)
            {
                /* Attackバフコメントをランダムに選択 */
                if (attackBuffs.Count > 0)
                {
                    var buff = attackBuffs[Random.Range(0, attackBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.AttackBuff, text, buff.buffType);
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
