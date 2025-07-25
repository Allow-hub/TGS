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
        public List<SpecialCommentData> specialComments; // 新規追加：特殊コメントデータ


        [Header("コメントの出現確率")]
        [SerializeField, Range(0f, 1f)] private float normalChance = 0.7f;
        [SerializeField, Range(0f, 1f)] private float speedBuffChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float attackBuffChance = 0.2f;
        [SerializeField, Range(0f, 1f)] private float mapChangeChance = 0.1f;
        [SerializeField, Range(0f, 1f)] private float specialCommentChance = 0.1f; // 新規追加：特殊コメントの確率


        private float totalChance; /* 合計確率 */


        /* バフコメント（Speed） */
        private List<BuffCommentData> speedBuffs;

        /* バフコメント（Attack） */
        private List<BuffCommentData> attackBuffs;

        /* マップ変更用バフコメント */
        private List<BuffCommentData> mapChangeBuffs;

        /* 特殊コメント */
        private List<SpecialCommentData> specialCommentList;

        private void Awake()
        {

            totalChance = normalChance + speedBuffChance + attackBuffChance + mapChangeChance + specialCommentChance;


            /* 確率が0またはマイナスならデフォルト値に設定 */
            if (totalChance <= 0f)
            {
                normalChance = 0.7f;
                speedBuffChance = 0.1f;
                attackBuffChance = 0.1f;
                mapChangeChance = 0.05f;
                specialCommentChance = 0.05f;
                totalChance = 1.0f;
            }


            /* buffCommentsを事前にフィルタリングして分類 */
            speedBuffs = new List<BuffCommentData>();
            attackBuffs = new List<BuffCommentData>();
            mapChangeBuffs = new List<BuffCommentData>();
            specialCommentList = new List<SpecialCommentData>();

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

            /* SpecialCommentDataをリスト化 */
            if (specialComments != null)
            {
                foreach (var special in specialComments)
                {
                    specialCommentList.Add(special);
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

            float threshold = 0f;

            // 通常コメント
            threshold += normalChance;
            if (randomValue < threshold)
            {
                string text = normalComments.comment[Random.Range(0, normalComments.comment.Length)];
                return new CommentData(CommentType.Normal, text, null);
            }

            // Speedバフコメント
            threshold += speedBuffChance;
            if (randomValue < threshold)
            {
                if (speedBuffs.Count > 0)
                {
                    var buff = speedBuffs[Random.Range(0, speedBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.SpeedBuff, text, buff.buffType);
                }
            }

            // Attackバフコメント
            threshold += attackBuffChance;
            if (randomValue < threshold)
            {
                if (attackBuffs.Count > 0)
                {
                    var buff = attackBuffs[Random.Range(0, attackBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.AttackBuff, text, buff.buffType);
                }
            }

            // マップ変更コメント
            threshold += mapChangeChance;
            if (randomValue < threshold)
            {
                if (mapChangeBuffs.Count > 0)
                {
                    var buff = mapChangeBuffs[Random.Range(0, mapChangeBuffs.Count)];
                    string text = buff.comments[Random.Range(0, buff.comments.Length)];
                    return new CommentData(CommentType.MapChange, text, buff.buffType);
                }
            }

            // 特殊コメント
            threshold += specialCommentChance;
            if (randomValue < threshold)
            {
                if (specialComments != null && specialComments.Count > 0)
                {
                    // SpecialCommentEntry[]の全要素をリスト化
                    var allEntries = new List<SpecialCommentData.SpecialCommentEntry>();
                    foreach (var data in specialComments)
                    {
                        if (data != null && data.comments != null)
                        {
                            allEntries.AddRange(data.comments);
                        }
                    }
                    if (allEntries.Count > 0)
                    {
                        var entry = allEntries[Random.Range(0, allEntries.Count)];
                        return new CommentData(CommentType.Special, entry.comment, null, entry.specialType);
                    }
                }
            }

            // fallback（通常コメント）
            string fallback = normalComments.comment[Random.Range(0, normalComments.comment.Length)];
            return new CommentData(CommentType.Normal, fallback, null);
        }
    }
}
