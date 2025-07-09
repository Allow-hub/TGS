using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class SelectCommentManager : MonoBehaviour
    {
        [SerializeField] private List<CommentCategory> commentCategories = new List<CommentCategory>();
        [SerializeField] private GameObject commentPrefab;
        [SerializeField] private RectTransform commentParent;
        [SerializeField] private float spawnInterval = 1.0f;

        [SerializeField] private float minYPosition = -50f;
        [SerializeField] private float maxYPosition = 50f;
        [SerializeField] private float minSpeed = 150f;
        [SerializeField] private float maxSpeed = 250f;

        void Start()
        {
            StartCoroutine(SpawnCommentsRoutine());
        }

        IEnumerator SpawnCommentsRoutine()
        {
            while (true)
            {
                if (HasActiveComments())
                {
                    string message = GetWeightedRandomComment();
                    GameObject obj = Instantiate(commentPrefab, commentParent);
                    float yPos = Random.Range(minYPosition, maxYPosition);
                    float speed = Random.Range(minSpeed, maxSpeed);

                    obj.GetComponent<SelectCommentMover>().Initialize(message, speed, yPos);
                }

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// アクティブなコメントがあるかどうかを確認
        /// </summary>
        /// <returns>アクティブなコメントが存在するかどうか</returns>
        private bool HasActiveComments()
        {
            foreach (var category in commentCategories)
            {
                if (category.isActive && category.comments.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// カテゴリの重み付けに基づいてランダムにコメントを選択する
        /// </summary>
        /// <returns>選択されたコメント文字列</returns>
        private string GetWeightedRandomComment()
        {
            // Step 1: アクティブなカテゴリから重み付きでカテゴリを選択
            CommentCategory selectedCategory = SelectWeightedCategory();

            if (selectedCategory == null || selectedCategory.comments.Count == 0)
            {
                return "デフォルトコメント"; // フォールバック
            }

            // Step 2: 選択されたカテゴリ内からランダムにコメントを選択
            int randomIndex = Random.Range(0, selectedCategory.comments.Count);
            return selectedCategory.comments[randomIndex];
        }

        /// <summary>
        /// 重み付けに基づいてカテゴリを選択する
        /// </summary>
        /// <returns>選択されたカテゴリ</returns>
        private CommentCategory SelectWeightedCategory()
        {
            // アクティブなカテゴリの重みの合計を計算
            int totalWeight = 0;
            foreach (var category in commentCategories)
            {
                if (category.isActive && category.comments.Count > 0)
                {
                    totalWeight += category.categoryWeight;
                }
            }

            if (totalWeight <= 0) return null;

            // ランダムな値を生成
            int randomValue = Random.Range(0, totalWeight);

            // 重み付き選択を実行
            int currentWeight = 0;
            foreach (var category in commentCategories)
            {
                if (category.isActive && category.comments.Count > 0)
                {
                    currentWeight += category.categoryWeight;
                    if (randomValue < currentWeight)
                    {
                        return category;
                    }
                }
            }
            return null; // フォールバック
        }
    }
}