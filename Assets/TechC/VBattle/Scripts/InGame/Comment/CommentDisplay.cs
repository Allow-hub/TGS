using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TechC
{
    public class CommentDisplay : MonoBehaviour
    {
        [Header("コメントデータ")]
        public NormalCommentData commentData;

        [Header("コメントのテキスト用Prefab")]
        public TMP_Text commentPrefab;

        [Header("コメントが流れるエリア")]
        public RectTransform commentLayer;

        [Header("コメントの設定")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float speed = 100.0f;

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
            if (commentData == null || commentData.comment.Length == 0)
            {
                Debug.LogError("コメントデータが設定されてないか、空");
                return;
            }

            /* コメントデータの中からランダムに一つ選択 */
            int index = Random.Range(0, commentData.comment.Length);
            string commentText = commentData.comment[index];
            // Debug.Log("表示するコメント：" + commentText);

            /* Prefabから新しいコメントオブジェクトを生成 */
            TMP_Text comment = Instantiate(commentPrefab, commentLayer);
            comment.text = commentText;
            // Debug.Log("実際に設定されたコメント：" + comment.text);

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
            rect.gameObject.SetActive(false); /* どっちを使用しよう */
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

            // Debug.Log("コメントを発生させる上のy座標は：" + topRightSpawnPosY);
            // Debug.Log("コメントを発生させる下のy座標は：" + bottomRightSpawnPosY);
            // Debug.Log("spawnPosX座標は" + spawnPosX);

            // Debug.Log("コメントを非表示にさせる上のy座標は：" + topRightSpawnPosY);
            // Debug.Log("コメントを非表示にさせる下のy座標は：" + bottomRightSpawnPosY);
            // Debug.Log("despawnPosX座標は" + spawnPosX);
        }

        private void OnCollisionEnter(Collision other)
        {
            if(other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Playerがコメントにぶつかった。");
            }
        }
    }
}
