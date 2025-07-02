using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コメントを画面上に流す処理
    /// </summary>
    public class CommentDisplay : MonoBehaviour
    {
        [Header("コメントのテキスト用Prefab")]
        [SerializeField] private GameObject commentPrefab;
        [SerializeField] private GameObject speedBuffPrefab;
        [SerializeField] private GameObject attackBuffPrefab;
        [SerializeField] private GameObject mapChangePrefab;

        [Header("コメントが流れるエリア")]
        public RectTransform commentLayer;

        [Header("コメントの設定")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float speed = 100.0f;
        [Header("ランダムなコメントを表示するためのスクリプトを取得")]
        [SerializeField] private CommentProvider commentProvider;

        [Header("特殊コメントの設定")]
        [SerializeField] private float freezeTime = 3f;

        [Header("コメントが出現する場所")]
        public GameObject topRightSpawn;
        public GameObject bottomRightSpawn;
        private float topRightSpawnPosY;
        private float bottomRightSpawnPosY;
        private float spawnPosX;

        [Header("コメントを非表示にする場所")]
        public GameObject topLeftDespawn;
        public GameObject buttonLeftDespawn;

        private float despawnPosX;

        void Start()
        {
            InitSetPositions(); /* コメントを表示 / 非表示にするメソッドを呼ぶ */
            StartCoroutine(FlowComments()); /* コメント流す処理を開始 */
        }

        IEnumerator FlowComments()
        {
            while (true)
            {
                SpawnComment();
                yield return new WaitForSeconds(spawnInterval); /* spawnIntervalの時間待機 */
            }
        }

        /// <summary>
        /// コメントをcommentProviderを通じて発生させる処理
        /// </summary>
        public void SpawnComment()
        {
            var commentData = commentProvider.GetRandomComment();
            const float PLAYER_TOP_OFFSET = -5.3f;


            GameObject comment = CommentFactory.I.GetComment(commentData, GetCommentPrefab(commentData), commentLayer);

            Color commentColor = Color.white;
            switch (commentData.type)
            {
                case CommentType.AttackBuff:
                    commentColor = Color.red;
                    break;
                case CommentType.SpeedBuff:
                    commentColor = Color.blue;
                    break;
                case CommentType.MapChange:
                    commentColor = Color.yellow;
                    break;
            }

            List<GameObject> spawnedChars = AllCharacterHelper.ProcessCommentText(commentData.text, comment.transform, commentColor);

            if (comment == null)
            {
                return;
            }

            if (comment == null) return;

            float randomY = Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
            comment.transform.position = new Vector3(spawnPosX, randomY, PLAYER_TOP_OFFSET);

            StartCoroutine(MoveComment(comment.transform, spawnedChars));
        }

        /// <summary>
        /// コメントを画面上に流す処理
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        IEnumerator MoveComment(Transform trans, List<GameObject> chars)
        {
            // BuffCommentTriggerまたはNormalCommentTriggerのどちらかを取得
            SpecialCommentType specialType = SpecialCommentType.None;
            var buffTrigger = trans.GetComponent<BuffCommentTrigger>();
            if (buffTrigger != null)
                specialType = buffTrigger.specialCommentType;
            else
            {
                var normalTrigger = trans.GetComponent<NormalCommentTrigger>();
                if (normalTrigger != null)
                    specialType = normalTrigger.specialCommentType;
            }

            // 固定コメントなら3秒停止
            if (specialType == SpecialCommentType.Freeze)
            {
                float elapsed = 0f;
                while (elapsed < freezeTime)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // その後、通常通り左に流す
            while (trans.position.x > despawnPosX)
            {
                trans.position += Vector3.left * speed * Time.deltaTime;
                yield return null;
            }
            trans.gameObject.SetActive(false);
            CommentFactory.I.ReturnComment(trans.gameObject);

            foreach (var obj in chars)
            {
                obj.SetActive(false);
                CommentFactory.I.ReturnChar(obj);
            }
        }

        /// <summary>
        /// コメントを発生、消去する座標を取得する
        /// </summary>
        private void InitSetPositions()
        {
            /* コメントを発生させる座標を取得する */
            topRightSpawnPosY = topRightSpawn.transform.position.y;
            bottomRightSpawnPosY = bottomRightSpawn.transform.position.y;
            spawnPosX = topRightSpawn.transform.position.x;

            /* コメントを非表示にする座標を取得する */
            despawnPosX = topLeftDespawn.transform.position.x;
        }

        private GameObject GetCommentPrefab(CommentData commentData)
        {
            switch (commentData.type)
            {
                case CommentType.Normal:
                    return commentPrefab.gameObject;
                case CommentType.AttackBuff:
                    return attackBuffPrefab.gameObject;
                case CommentType.MapChange:
                    return mapChangePrefab.gameObject;
                case CommentType.SpeedBuff:
                    return speedBuffPrefab.gameObject;
                default:
                    return null;
            }
        }
        

        public float GetCurrentSpeed()
        {
            return speed;
        }
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        public void AddSpeed(float amount)
        {
            speed += amount;
        }
    }
}