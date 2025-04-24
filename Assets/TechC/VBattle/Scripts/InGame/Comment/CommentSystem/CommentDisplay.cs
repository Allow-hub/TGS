using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TechC
{
    /// <summary>
    /// コメントを画面上に流す処理
    /// </summary>
    public class CommentDisplay : MonoBehaviour
    {
        [Header("コメントのテキスト用Prefab")]
        [SerializeField] private TMP_Text commentPrefab;
        [SerializeField] private TMP_Text buffCommentPrefab;
        [SerializeField] private TMP_Text mapChangePrefab;

        [Header("コメントが流れるエリア")]
        public RectTransform commentLayer;

        [Header("コメントの設定")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float speed = 100.0f;
        [Header("ランダムなコメントを表示するためのスクリプトを取得")]
        [SerializeField] private CommentProvider commentProvider;

        [Header("コメントが出現する場所")]
        /* コメントが出現する場所 */
        public GameObject topRightSpawn;
        public GameObject bottomRightSpawn;
        private float topRightSpawnPosY;
        private float bottomRightSpawnPosY;
        private float spawnPosX;

        /* コメントが消滅する場所 */
        [Header("コメントを非表示にする場所")]
        public GameObject topLeftDespawn;
        public GameObject buttonLeftDespawn;
        private float topLeftDespawnPosY;
        private float buttonLeftDespawnPosY;
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

        public void SpawnComment()
        {
            var commentData = commentProvider.GetRandomComment();

            TMP_Text prefab;
            switch (commentData.type)
            {
                case CommentType.Normal:
                    prefab = commentPrefab;
                    break;
                case CommentType.Buff:
                    prefab = buffCommentPrefab;
                    break;
                case CommentType.MapChange:
                    prefab = mapChangePrefab;
                    break;
                default:
                    prefab = commentPrefab;
                    break;
            }

            TMP_Text comment = Instantiate(prefab, commentLayer);
            comment.text = commentData.text;

            if (commentData.buffType.HasValue)
            {
                BuffCommentTrigger trigger = comment.GetComponent<BuffCommentTrigger>();
                if (trigger != null)
                {
                    trigger.buffType = commentData.buffType.Value;
                }
            }

            RectTransform rect = comment.GetComponent<RectTransform>();
            float randomY = Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
            rect.anchoredPosition = new Vector2(spawnPosX, randomY);

            StartCoroutine(MoveComment(rect));
        }

        IEnumerator MoveComment(RectTransform rect)
        {
            while (rect.anchoredPosition.x > despawnPosX) /* 左端まで */
            {
                rect.anchoredPosition += Vector2.left * speed * Time.deltaTime;
                yield return null; /* 次のフレームまで待機 */
            }
            rect.gameObject.SetActive(false);
        }

        /* 最初にコメントを表示 / 非表示にする座標を取得する */
        private void InitSetPositions()
        {
            /* コメントを発生させる座標を取得する */
            topRightSpawnPosY = topRightSpawn.transform.position.y;
            bottomRightSpawnPosY = bottomRightSpawn.transform.position.y;
            spawnPosX = topRightSpawn.transform.position.x;

            /* コメントを非表示にする座標を取得する */
            topLeftDespawnPosY = topLeftDespawn.transform.position.y;
            buttonLeftDespawnPosY = buttonLeftDespawn.transform.position.y;
            despawnPosX = topLeftDespawn.transform.position.x;
        }
    }
}