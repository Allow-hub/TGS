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

        private MaterialPropertyBlock propertyBlock;

        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorPropertyId = Shader.PropertyToID("_EmissionColor");

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
        }

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
        /// 生成された文字オブジェクトリストに Material を適用
        /// </summary>
        public void ApplyMaterialToCharacters(List<GameObject> characters, Material material, Color? overrideColor = null)
        {
            if (characters == null || material == null)
            {
                Debug.LogWarning("characters または material が null です");
                return;
            }

            propertyBlock.Clear();

            // カラー設定
            if (overrideColor.HasValue)
            {
                propertyBlock.SetColor(ColorPropertyId, overrideColor.Value);
            }
            else if (material.HasProperty(ColorPropertyId))
            {
                propertyBlock.SetColor(ColorPropertyId, material.GetColor(ColorPropertyId));
            }

            // エミッション設定（グロー効果など）
            if (material.HasProperty(EmissionColorPropertyId))
            {
                propertyBlock.SetColor(EmissionColorPropertyId, material.GetColor(EmissionColorPropertyId));
            }

            // 各文字オブジェクトに適用
            foreach (var charObj in characters)
            {
                if (charObj == null) continue;

                var renderer = charObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // sharedMaterial をセット（マテリアル複製を防ぐ）
                    renderer.sharedMaterial = material;
                    // PropertyBlockを適用
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }

        /// <summary>
        /// フリーズエフェクト専用の適用メソッド
        /// </summary>
        public void ApplyFreezeEffectToCharacters(List<GameObject> characters, Material originalMaterial)
        {
            if (characters == null)
            {
                Debug.LogWarning("characters が null です");
                return;
            }


            propertyBlock.Clear();

            // フリーズマテリアルの色を適用
            if (freezeCommentMaterial.HasProperty(ColorPropertyId))
            {
                propertyBlock.SetColor(ColorPropertyId, freezeCommentMaterial.GetColor(ColorPropertyId));
            }

            // フリーズ用のエミッション効果
            if (freezeCommentMaterial.HasProperty(EmissionColorPropertyId))
            {
                Color freezeEmission = freezeCommentMaterial.GetColor(EmissionColorPropertyId);
                propertyBlock.SetColor(EmissionColorPropertyId, freezeEmission);
            }

            foreach (var charObj in characters)
            {
                if (charObj == null) continue;

                var renderer = charObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // 元のマテリアルをベースとして使用（シェーダー設定を維持）
                    renderer.sharedMaterial = originalMaterial;
                    // フリーズエフェクトをPropertyBlockで適用
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }

        /// <summary>
        /// フリーズ用マテリアルを取得
        /// </summary>
        public Material GetFreezeMaterial() => freezeCommentMaterial;

    }
}
