using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// コメントのマテリアル適用処理のみを担当
    /// </summary>
    [Serializable]
    public class CommentMaterialApplier
    {
        [Header("コメントのマテリアル")]
        [SerializeField] private Material normalCommentMaterial;
        [SerializeField] private Material speedBuffCommentMaterial;
        [SerializeField] private Material attackBuffCommentMaterial;
        [SerializeField] private Material mapChangeCommentMaterial;
        [SerializeField] private Material freezeCommentMaterial;

        /// <summary>
        /// コメントタイプに応じたMaterialを取得
        /// </summary>
        public Material GetCommentMaterial(CommentType? commentType, SpecialCommentType? specialCommentType = SpecialCommentType.None)
        {
            if (commentType != null)
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
            else if (specialCommentType != SpecialCommentType.None)
            {
                switch (specialCommentType)
                {
                    case SpecialCommentType.Freeze:
                        return freezeCommentMaterial;
                }
            }
            return normalCommentMaterial;
        }

        /// <summary>
        /// 生成された文字オブジェクトリストにMaterialを適用
        /// </summary>
        public void ApplyMaterialToCharacters(List<GameObject> characters, Material material)
        {
            if (characters == null || material == null)
            {
                Debug.LogWarning("characters または material が null です");
                return;
            }

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
        /// 単一の文字オブジェクトにMaterialを適用
        /// </summary>
        public void ApplyMaterialToCharacter(GameObject character, Material material)
        {
            var meshRenderer = character.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
            }
        }

        /// <summary>
        /// フリーズ用マテリアルを取得
        /// </summary>
        public Material GetFreezeMaterial()
        {
            return freezeCommentMaterial;
        }
    }
}
