using UnityEngine;

namespace TechC
{
    public static class CharacterHelper
    {
        /// <summary>
        /// 各文字を対応するPrefabに変換して処理（例："あ" → a_jp.prefab）
        /// </summary>
        /// <param name="text">表示したい文字列</param>
        /// <param name="parent">生成された文字を入れる親オブジェクト</param>
        public static void ProcessCommentText(string text, Transform parent, Color color)
        {
            float xOffset = 0f;
            float spacing = 0.35f;

            const float PLAYER_TOP_OFFSET = -5.3f;

            const float ROTATE_X_DEGREE = 90f;
            const float ROTATE_Z_DEGREE = 180f;

            foreach (char c in text)
            {
                string prefabName = ConvertCharToPrefabName(c);
                var prefab = CommentFactory.I.GetChar(prefabName);

                if (prefab != null)
                {
                    prefab.transform.SetParent(parent);
                    // Debug.Log(parent);

                    prefab.transform.localPosition = new Vector3(xOffset, 0f, 0f);

                    xOffset += spacing;

                    prefab.transform.Rotate(ROTATE_X_DEGREE, 0, ROTATE_Z_DEGREE);
                    prefab.transform.position = new Vector3(xOffset, parent.transform.position.y, PLAYER_TOP_OFFSET);

                    var meshRenderer = prefab.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        meshRenderer.material.color = color;
                    }
                }


            }
        }

        /// <summary>
        /// 文字とPrefab名の辞書化対応表
        /// </summary>
        private static readonly System.Collections.Generic.Dictionary<char, string> charToPrefabName = new System.Collections.Generic.Dictionary<char, string>
        {
            { 'あ', "a_jp" }, { 'い', "i_jp" }, { 'う', "u_jp" }, { 'え', "e_jp" }, { 'お', "o_jp" },
            { 'か', "ka_jp" }, { 'き', "ki_jp" }, { 'く', "ku_jp" }, { 'け', "ke_jp" }, { 'こ', "ko_jp" },
            { 'が', "ga_jp" }, { 'ぎ', "gi_jp" }, { 'ぐ', "gu_jp" }, { 'げ', "ge_jp" }, { 'ご', "go_jp" },
            { 'さ', "sa_jp" }, { 'し', "shi_jp" }, { 'す', "su_jp" }, { 'せ', "se_jp" }, { 'そ', "so_jp" },
            { 'ざ', "za_jp" }, { 'じ', "ji_jp" }, { 'ず', "zu_jp" }, { 'ぜ', "ze_jp" }, { 'ぞ', "zo_jp" },
            { 'た', "ta_jp" }, { 'ち', "chi_jp" }, { 'つ', "tsu_jp" }, { 'て', "te_jp" }, { 'と', "to_jp" },
            { 'だ', "da_jp" }, { 'ぢ', "di_jp" }, { 'づ', "du_jp" }, { 'で', "de_jp" }, { 'ど', "do_jp" },
            { 'な', "na_jp" }, { 'に', "ni_jp" }, { 'ぬ', "nu_jp" }, { 'ね', "ne_jp" }, { 'の', "no_jp" },
            { 'は', "ha_jp" }, { 'ひ', "hi_jp" }, { 'ふ', "fu_jp" }, { 'へ', "he_jp" }, { 'ほ', "ho_jp" },
            { 'ば', "ba_jp" }, { 'び', "bi_jp" }, { 'ぶ', "bu_jp" }, { 'べ', "be_jp" }, { 'ぼ', "bo_jp" },
            { 'ぱ', "pa_jp" }, { 'ぴ', "pi_jp" }, { 'ぷ', "pu_jp" }, { 'ぺ', "pe_jp" }, { 'ぽ', "po_jp" },
            { 'ま', "ma_jp" }, { 'み', "mi_jp" }, { 'む', "mu_jp" }, { 'め', "me_jp" }, { 'も', "mo_jp" },
            { 'や', "ya_jp" }, { 'ゆ', "yu_jp" }, { 'よ', "yo_jp" },
            { 'ら', "ra_jp" }, { 'り', "ri_jp" }, { 'る', "ru_jp" }, { 'れ', "re_jp" }, { 'ろ', "ro_jp" },
            { 'わ', "wa_jp" }, { 'を', "wo_jp" }, { 'ん', "n_jp" },
            { 'ぁ', "xa_jp" }, { 'ぃ', "xi_jp" }, { 'ぅ', "xu_jp" }, { 'ぇ', "xe_jp" }, { 'ぉ', "xo_jp" },
            { 'ゃ', "xya_jp" }, { 'ゅ', "xyu_jp" }, { 'ょ', "xyo_jp" },
        };

        /// <summary>
        /// 文字列とPrefabを対応させる
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static string ConvertCharToPrefabName(char c)
        {
            if (charToPrefabName.TryGetValue(c, out string prefabName))
            {
                return prefabName;
            }
            else
            {
                return $"unknown_{(int)c}";
            }
        }
    }
}
