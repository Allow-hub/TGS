using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コメントを画面上に流す処理
    /// </summary>
    public class CommentDisplay : Singleton<CommentDisplay>
    {

        [Header("コメントのテキスト用Prefab")]
        [SerializeField] private GameObject commentPrefab;
        [SerializeField] private GameObject speedBuffPrefab;
        [SerializeField] private GameObject attackBuffPrefab;
        [SerializeField] private GameObject mapChangePrefab;

        [Header("コメントのマテリアル")]
        [SerializeField] Material normalCommentMaterial;
        [SerializeField] Material speedBuffCommentMaterial;
        [SerializeField] Material attackBuffCommentMaterial;
        [SerializeField] Material mapChangeCommentMaterial;
        [SerializeField] Material freezeCommentMaterial;

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

            Material commentMaterial = GetCommentMaterial(commentData.type);

            List<GameObject> spawnedChars = AllCharacterHelper.ProcessCommentText(commentData.text, comment.transform, Color.white);

            ApplyMaterialToCharacters(spawnedChars, commentMaterial);

            var abilityTrigger = comment.GetComponent<AbilityCommentTrigger>();

            if (comment == null)
            {
                return;
            }

            // AbilityCommentTriggerの特殊タイプを設定
            if (abilityTrigger != null && commentData.text.Contains("固定"))
            {
                abilityTrigger.SetSpecialType(SpecialCommentType.Freeze);
            }

            float randomY = Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
            comment.transform.position = new Vector3(spawnPosX, randomY, PLAYER_TOP_OFFSET);

            StartCoroutine(MoveComment(comment.transform, spawnedChars));
        }

        /// <summary>
        /// コメントタイプに応じたMaterialを取得
        /// </summary>
        private Material GetCommentMaterial(CommentType commentType)
        {
            switch (commentType)
            {
                case CommentType.AttackBuff:
                    return attackBuffCommentMaterial;
                case CommentType.SpeedBuff:
                    return speedBuffCommentMaterial;
                case CommentType.MapChange:
                    return mapChangeCommentMaterial;
                case CommentType.Normal:
                default:
                    return normalCommentMaterial;
            }
        }

        /// <summary>
        /// 生成された文字オブジェクトリストにMaterialを適用
        /// </summary>
        private void ApplyMaterialToCharacters(List<GameObject> characters, Material material)
        {
            foreach (var charObj in characters)
            {
                if (charObj == null) continue;

                var meshRenderer = charObj.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.material = material;
                }
            }
        }

        /// <summary>
        /// コメントを画面上に流す処理
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        IEnumerator MoveComment(Transform trans, List<GameObject> chars)
        {
            AbilityCommentTrigger abilityCommentTrigger = trans.GetComponent<AbilityCommentTrigger>();
            bool freezeMaterialApplied = false;

            while (trans.position.x > despawnPosX)
            {
                /* 「固定」コメントで停止中ならfreezeCommentMaterialを適用 */
                if (abilityCommentTrigger != null && abilityCommentTrigger.SpecialType == SpecialCommentType.Freeze && abilityCommentTrigger.IsFrozen)
                {
                    if (!freezeMaterialApplied)
                    {
                        ApplyMaterialToCharacters(chars, freezeCommentMaterial);
                        freezeMaterialApplied = true;
                    }
                    yield return null;
                    continue;
                }
                else if (freezeMaterialApplied)
                {
                    ApplyMaterialToCharacters(chars, normalCommentMaterial);
                    freezeMaterialApplied = false;
                }

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

        /// <summary>
        /// AbilityCommentTriggerから直接呼ばれるメソッド
        /// </summary>
        public void OnFreezeTriggered(AbilityCommentTrigger trigger)
        {
            Debug.Log("FreezeコメントがPlayerに当たりました（CommentDisplay経由）");
            // 必要ならここで追加の処理が可能
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