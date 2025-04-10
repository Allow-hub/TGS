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

        void Start()
        {
            StartCoroutine(FlowComments());
        }

        IEnumerator FlowComments()
        {
            while(true)
            {
                SpawnComment();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        public void SpawnComment()
        {
            if (commentData == null || commentData.comment.Length == 0)
            {
                Debug.LogError("コメントデータが設定されてないか、空");
                return;
            }

            int index = Random.Range(0, commentData.comment.Length);
            string commentText = commentData.comment[index];

            TMP_Text comment = Instantiate(commentPrefab, commentLayer);
            comment.text = commentText;

            RectTransform rect = comment.GetComponent<RectTransform>();

            /* 初期位置を右端にする */
            rect.anchoredPosition = new Vector2(commentLayer.rect.width, Random.Range(-commentLayer.rect.height / 2, commentLayer.rect.height / 2));
            
            StartCoroutine(MoveComment(rect));
        }

        IEnumerator MoveComment(RectTransform rect)
        {
            while(rect.anchoredPosition.x > -commentLayer.rect.width)
            {
                rect.anchoredPosition += Vector2.left * speed * Time.deltaTime;
                yield return null;
            }

            Destroy(rect.gameObject); /* 画面外で削除 */
        }
    }
}
