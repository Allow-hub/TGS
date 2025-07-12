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
        [SerializeField] CommentSpawner commentSpawner;
        // [Header("コメントのテキスト用Prefab")]
        // [SerializeField] private GameObject commentPrefab;
        // [SerializeField] private GameObject speedBuffPrefab;
        // [SerializeField] private GameObject attackBuffPrefab;
        // [SerializeField] private GameObject mapChangePrefab;

        // [Header("コメントのマテリアル")]
        // [SerializeField] Material normalCommentMaterial;
        // [SerializeField] Material speedBuffCommentMaterial;
        // [SerializeField] Material attackBuffCommentMaterial;
        // [SerializeField] Material mapChangeCommentMaterial;
        // [SerializeField] Material freezeCommentMaterial;

        // [Header("コメントの設定")]
        // [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float speed = 100.0f;
        // [Header("ランダムなコメントを表示するためのスクリプトを取得")]
        // [SerializeField] private CommentProvider commentProvider;

        // [Header("特殊コメントの設定")]
        // [SerializeField] private float freezeTime = 3f;
        // public bool IsCommentFrozen { get; private set; } = false;

        // [Header("コメントが出現する場所")]
        // public GameObject topRightSpawn;
        // public GameObject bottomRightSpawn;
        // private float topRightSpawnPosY;
        // private float bottomRightSpawnPosY;
        // private float spawnPosX;

        // [Header("コメントを非表示にする場所")]
        // public GameObject topLeftDespawn;
        // public GameObject buttonLeftDespawn;
        // private float despawnPosX;

        protected override bool UseDontDestroyOnLoad => false;

        void Start()
        {
            // InitSetPositions(); /* コメントを表示 / 非表示にするメソッドを呼ぶ */
            // StartCoroutine(FlowComments()); /* コメント流す処理を開始 */
            DelayUtility.StartDelayedAction(this, 0.5f,()=> {
                commentSpawner.SpawnComment();
            });
        }

        protected override void Init()
        {
            base.Init();
            commentSpawner.Init();
        }
        // IEnumerator FlowComments()
        // {
        //     while (true)
        //     {
        //         SpawnComment();
        //         yield return new WaitForSeconds(spawnInterval); /* spawnIntervalの時間待機 */
        //     }
        // }

        // /// <summary>
        // /// コメントをcommentProviderを通じて発生させる処理
        // /// </summary>
        // public void SpawnComment()
        // {
        //     if (IsCommentFrozen) return;
        //     var commentData = commentProvider.GetRandomComment();
        //     const float PLAYER_TOP_OFFSET = -5.3f;

        //     GameObject comment = CommentFactory.I.GetComment(commentData, GetCommentPrefab(commentData));

        //     var sp = comment.GetComponent<FreezeCommentTrigger>();
        //     var spType = SpecialCommentChecker.GetSpecialCommentType(commentData.text);
        //     Material commentMaterial;

        //     if (spType != SpecialCommentType.None && sp != null)
        //         commentMaterial = GetCommentMaterial(null, sp.SpecialType);
        //     else
        //         commentMaterial = GetCommentMaterial(commentData.type);

        //     List<GameObject> spawnedChars = AllCharacterHelper.ProcessCommentText(commentData.text, comment.transform, Color.white);
        //     // 元のマテリアルを適用してから、そのマテリアルを保持
        //     ApplyMaterialToCharacters(spawnedChars, commentMaterial);

        //     float randomY = Random.Range(bottomRightSpawnPosY, topRightSpawnPosY);
        //     comment.transform.position = new Vector3(spawnPosX, randomY, PLAYER_TOP_OFFSET);

        //     var freezeCommentTrigger = comment.GetComponent<FreezeCommentTrigger>();

        //     // commentMaterialを直接渡す
        //     StartCoroutine(MoveComment(comment.transform, spawnedChars, freezeCommentTrigger, commentMaterial));
        // }


        // /// <summary>
        // /// コメントタイプに応じたMaterialを取得
        // /// </summary>
        // private Material GetCommentMaterial(CommentType? commentType, SpecialCommentType? specialCommentType = SpecialCommentType.None)
        // {
        //     if (commentType != null)
        //     {
        //         switch (commentType)
        //         {
        //             case CommentType.AttackBuff:
        //                 return attackBuffCommentMaterial;
        //             case CommentType.SpeedBuff:
        //                 return speedBuffCommentMaterial;
        //             case CommentType.MapChange:
        //                 return mapChangeCommentMaterial;
        //             case CommentType.Normal:
        //             default:
        //                 return normalCommentMaterial;
        //         }
        //     }
        //     else if (specialCommentType != SpecialCommentType.None)
        //     {
        //         switch (specialCommentType)
        //         {
        //             case SpecialCommentType.Freeze:
        //                 return freezeCommentMaterial;
        //         }
        //     }
        //     return normalCommentMaterial;
        // }

        // /// <summary>
        // /// 生成された文字オブジェクトリストにMaterialを適用
        // /// </summary>
        // private void ApplyMaterialToCharacters(List<GameObject> characters, Material material)
        // {
        //     foreach (var charObj in characters)
        //     {
        //         if (charObj == null) continue;

        //         var meshRenderer = charObj.GetComponent<MeshRenderer>();
        //         if (meshRenderer != null)
        //         {
        //             meshRenderer.material = material;
        //         }
        //     }
        // }

        // /// <summary>
        // /// コメントを画面上に流す処理
        // /// </summary>
        // IEnumerator MoveComment(Transform trans, List<GameObject> chars, FreezeCommentTrigger freezeCommentTrigger, Material originalMaterial)
        // {
        //     bool freezeMaterialApplied = false;

        //     while (trans.position.x > despawnPosX)
        //     {
        //         // 全コメントのフリーズ状態をチェック（メソッド名変更）
        //         if (IsCommentFrozen)
        //         {
        //             if (!freezeMaterialApplied)
        //             {
        //                 ApplyMaterialToCharacters(chars, freezeCommentMaterial);
        //                 freezeMaterialApplied = true;
        //             }
        //             yield return null;
        //             continue;
        //         }
        //         else if (freezeMaterialApplied)
        //         {
        //             ApplyMaterialToCharacters(chars, originalMaterial);
        //             freezeMaterialApplied = false;
        //         }

        //         trans.position += Vector3.left * speed * Time.deltaTime;
        //         yield return null;
        //     }

        //     trans.gameObject.SetActive(false);
        //     CommentFactory.I.ReturnComment(trans.gameObject);

        //     foreach (var obj in chars)
        //     {
        //         obj.SetActive(false);
        //         CommentFactory.I.ReturnChar(obj);
        //     }
        // }


        // /// <summary>
        // /// コメントを発生、消去する座標を取得する
        // /// </summary>
        // private void InitSetPositions()
        // {
        //     /* コメントを発生させる座標を取得する */
        //     topRightSpawnPosY = topRightSpawn.transform.position.y;
        //     bottomRightSpawnPosY = bottomRightSpawn.transform.position.y;
        //     spawnPosX = topRightSpawn.transform.position.x;

        //     /* コメントを非表示にする座標を取得する */
        //     despawnPosX = topLeftDespawn.transform.position.x;
        // }

        // private GameObject GetCommentPrefab(CommentData commentData)
        // {
        //     switch (commentData.type)
        //     {
        //         case CommentType.Normal:
        //             return commentPrefab.gameObject;
        //         case CommentType.AttackBuff:
        //             return attackBuffPrefab.gameObject;
        //         case CommentType.MapChange:
        //             return mapChangePrefab.gameObject;
        //         case CommentType.SpeedBuff:
        //             return speedBuffPrefab.gameObject;
        //         default:
        //             return null;
        //     }
        // }

        // /// <summary>
        // /// FreezeCommentTriggerから直接呼ばれるメソッド
        // /// </summary>
        // public void OnFreezeTriggered()
        // {
        //     // 既にフリーズ中でない場合のみフリーズ開始
        //     if (!IsCommentFrozen)
        //     {
        //         StartCoroutine(FreezeAllCommentsCoroutine());
        //     }
        // }

        // private IEnumerator FreezeAllCommentsCoroutine()
        // {
        //     IsCommentFrozen = true;
        //     yield return new WaitForSeconds(freezeTime);
        //     IsCommentFrozen = false;
        // }

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