using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TechC
{
    public class CommentDisplay : MonoBehaviour
    {
        [Header("通常コメントデータ")]
        public NormalCommentData commentData;
        [Header("バフコメントデータ")]
        public List<BuffCommentData> buffComments;

        [Header("コメントのテキスト用Prefab")]
        public TMP_Text commentPrefab;
        public TMP_Text buffCommentPrefab;
        public TMP_Text mapChangePrefab;

        [Header("コメントが流れるエリア")]
        public RectTransform commentLayer;

        [Header("コメントの設定")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float speed = 100.0f;
        
        [Header("コメントの出現確率")]
        [SerializeField, Range(0f, 1f), Tooltip("通常コメントの確率 (0.0-1.0)")] 
        private float normalCommentChance = 0.7f; /* 通常コメントの確率 */
        [SerializeField, Range(0f, 1f), Tooltip("通常バフコメントの確率 (0.0-1.0)")] 
        private float buffCommentChance = 0.2f; /* 通常バフの確率 */
        [SerializeField, Range(0f, 1f), Tooltip("マップ変更コメントの確率 (0.0-1.0)")] 
        private float mapChangeChance = 0.1f; /* マップ変更バフの確率 */

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

        /* 確率の合計（正規化用） */
        private float totalChance = 1.0f;

        void Start()
        {
            InitSetPositions(); /* コメントを表示 / 非表示にするメソッドを呼ぶ */
            NormalizeChances(); /* コメント確率を正規化する */
            StartCoroutine(FlowComments()); /* コメント流す処理を開始 */
        }

        /* 確率の合計を1.0に正規化する */
        private void NormalizeChances()
        {
            totalChance = normalCommentChance + buffCommentChance + mapChangeChance;
            if (totalChance <= 0f)
            {
                /* 全ての確率が0以下の場合、デフォルト値に設定 */
                normalCommentChance = 0.7f;
                buffCommentChance = 0.2f;
                mapChangeChance = 0.1f;
                totalChance = 1.0f;
            }
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
            float randomValue = Random.value * totalChance; /* 正規化された範囲内のランダム値 */
            
            if (randomValue < normalCommentChance) /* 通常コメントの確率 */
            {
                /* 通常コメント */
                SpawnNormalComment();
            }
            else if (randomValue < normalCommentChance + buffCommentChance) /* 通常バフコメントの確率を加算 */
            {
                if (buffComments != null && buffComments.Count > 0)
                {
                    /* 通常のバフコメントだけをフィルタリングするリストを作成 */
                    List<BuffCommentData> normalBuffs = new List<BuffCommentData>();
                    foreach (BuffCommentData buff in buffComments)
                    {
                        if (buff.buffType != BuffType.MapChange)
                        {
                            normalBuffs.Add(buff);
                        }
                    }

                    if (normalBuffs.Count > 0)
                    {
                        /* ランダムにバフコメントをデータから選ぶ */
                        int buffIndex = Random.Range(0, normalBuffs.Count);
                        BuffCommentData selectedBuff = normalBuffs[buffIndex];

                        /* ランダムにバフテキストを選択する */
                        int textIndex = Random.Range(0, selectedBuff.comments.Length);
                        string buffText = selectedBuff.comments[textIndex];

                        /* バフコメントのPrefabを流す */
                        TMP_Text comment = Instantiate(buffCommentPrefab, commentLayer);
                        comment.text = buffText;

                        /* 選ばれたバフタイプをコメントに反映（プレハブの初期値を上書き） */
                        BuffCommentTrigger trigger = comment.GetComponent<BuffCommentTrigger>();
                        if (trigger != null)
                        {
                            trigger.buffType = selectedBuff.buffType;
                        }

                        RectTransform rect = comment.GetComponent<RectTransform>();
                        float randomY = Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
                        rect.anchoredPosition = new Vector2(spawnPosX, randomY);

                        StartCoroutine(MoveComment(rect));
                    }
                    else
                    {
                        /* 通常のバフがない場合は通常コメントを流す */
                        SpawnNormalComment();
                    }
                }
                else
                {
                    /* バフコメントデータがない場合は通常コメントを流す */
                    SpawnNormalComment();
                }
            }
            else /* マップ変更コメントの確率 */
            {
                if (buffComments != null && buffComments.Count > 0)
                {
                    /* MapChangeバフだけをフィルタリングするリストを作成 */
                    List<BuffCommentData> mapChangeBuffs = new List<BuffCommentData>();
                    foreach (BuffCommentData buff in buffComments)
                    {
                        if (buff.buffType == BuffType.MapChange)
                        {
                            mapChangeBuffs.Add(buff);
                        }
                    }

                    if (mapChangeBuffs.Count > 0)
                    {
                        /* ランダムにMapChangeバフコメントをデータから選ぶ */
                        int buffIndex = Random.Range(0, mapChangeBuffs.Count);
                        BuffCommentData selectedBuff = mapChangeBuffs[buffIndex];

                        /* ランダムにバフテキストを選択する */
                        int textIndex = Random.Range(0, selectedBuff.comments.Length);
                        string buffText = selectedBuff.comments[textIndex];

                        /* MapChange専用のPrefabを流す */
                        TMP_Text comment = Instantiate(mapChangePrefab, commentLayer);
                        comment.text = buffText;

                        /* 選ばれたバフタイプをコメントに反映（プレハブの初期値を上書き） */
                        BuffCommentTrigger trigger = comment.GetComponent<BuffCommentTrigger>();
                        if (trigger != null)
                        {
                            trigger.buffType = selectedBuff.buffType;
                        }

                        RectTransform rect = comment.GetComponent<RectTransform>();
                        float randomY = Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
                        rect.anchoredPosition = new Vector2(spawnPosX, randomY);

                        StartCoroutine(MoveComment(rect));
                    }
                    else
                    {
                        /* MapChangeバフがない場合は通常コメントを流す */
                        SpawnNormalComment();
                    }
                }
                else
                {
                    /* バフコメントデータがない場合は通常コメントを流す */
                    SpawnNormalComment();
                }
            }
        }

        /* 通常コメントを生成するヘルパーメソッド（コードの重複を減らすため） */
        private void SpawnNormalComment()
        {
            int index = Random.Range(0, commentData.comment.Length);
            string commentText = commentData.comment[index];

            TMP_Text comment = Instantiate(commentPrefab, commentLayer);
            comment.text = commentText;

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

            // Debug.Log("コメントを発生させる上のy座標は：" + topRightSpawnPosY);
            // Debug.Log("コメントを発生させる下のy座標は：" + bottomRightSpawnPosY);
            // Debug.Log("spawnPosX座標は" + spawnPosX);

            // Debug.Log("コメントを非表示にさせる上のy座標は：" + topRightSpawnPosY);
            // Debug.Log("コメントを非表示にさせる下のy座標は：" + bottomRightSpawnPosY);
            // Debug.Log("despawnPosX座標は" + spawnPosX);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Playerがコメントにぶつかった。");
            }
        }
    }
}