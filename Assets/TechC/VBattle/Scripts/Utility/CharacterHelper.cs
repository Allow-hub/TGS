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
        public static void ProcessCommentText(string text, Transform parent)
        {
            float xOffset = 0f;         
            float spacing = 2.5f;

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

                    // Debug.Log($"文字 '{c}' → プレハブ '{prefabName}' を生成しました");
                }
                else
                {
                    // Debug.LogWarning($"文字 '{c}' に対応するプレハブ '{prefabName}' が見つかりませんでした");
                }
            }
        }

        /// <summary>
        /// かな文字とローマ字の対応表
        /// </summary>
        private static readonly (string kana, string roman)[] kanaPairs = new (string, string)[]
        {
            ("あ", "a"), ("い", "i"), ("う", "u"), ("え", "e"), ("お", "o"),
            ("か", "ka"), ("き", "ki"), ("く", "ku"), ("け", "ke"), ("こ", "ko"),
            ("が", "ga"), ("ぎ", "gi"), ("ぐ", "gu"), ("げ", "ge"), ("ご", "go"),
            ("さ", "sa"), ("し", "shi"), ("す", "su"), ("せ", "se"), ("そ", "so"),
            ("ざ", "za"), ("じ", "ji"), ("ず", "zu"), ("ぜ", "ze"), ("ぞ", "zo"),
            ("た", "ta"), ("ち", "chi"), ("つ", "tsu"), ("て", "te"), ("と", "to"),
            ("だ", "da"), ("ぢ", "di"), ("づ", "du"), ("で", "de"), ("ど", "do"),
            ("な", "na"), ("に", "ni"), ("ぬ", "nu"), ("ね", "ne"), ("の", "no"),
            ("は", "ha"), ("ひ", "hi"), ("ふ", "fu"), ("へ", "he"), ("ほ", "ho"),
            ("ば", "ba"), ("び", "bi"), ("ぶ", "bu"), ("べ", "be"), ("ぼ", "bo"),
            ("ぱ", "pa"), ("ぴ", "pi"), ("ぷ", "pu"), ("ぺ", "pe"), ("ぽ", "po"),
            ("ま", "ma"), ("み", "mi"), ("む", "mu"), ("め", "me"), ("も", "mo"),
            ("や", "ya"), ("ゆ", "yu"), ("よ", "yo"),
            ("ら", "ra"), ("り", "ri"), ("る", "ru"), ("れ", "re"), ("ろ", "ro"),
            ("わ", "wa"), ("を", "wo"), ("ん", "n"),
            ("ぁ", "xa"), ("ぃ", "xi"), ("ぅ", "xu"), ("ぇ", "xe"), ("ぉ", "xo"),
            ("ゃ", "xya"), ("ゅ", "xyu"), ("ょ", "xyo"),
            ("きゃ", "kya"), ("きゅ", "kyu"), ("きょ", "kyo"),
            ("ぎゃ", "gya"), ("ぎゅ", "gyu"), ("ぎょ", "gyo"),
            ("しゃ", "sha"), ("しゅ", "shu"), ("しょ", "sho"),
            ("じゃ", "ja"), ("じゅ", "ju"), ("じょ", "jo"),
            ("ちゃ", "cha"), ("ちゅ", "chu"), ("ちょ", "cho"),
            ("にゃ", "nya"), ("にゅ", "nyu"), ("にょ", "nyo"),
            ("ひゃ", "hya"), ("ひゅ", "hyu"), ("ひょ", "hyo"),
            ("びゃ", "bya"), ("びゅ", "byu"), ("びょ", "byo"),
            ("ぴゃ", "pya"), ("ぴゅ", "pyu"), ("ぴょ", "pyo"),
            ("みゃ", "mya"), ("みゅ", "myu"), ("みょ", "myo"),
            ("りゃ", "rya"), ("りゅ", "ryu"), ("りょ", "ryo"),
        };


        /// <summary>
        /// 文字列とPrefabを対応させる
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static string ConvertCharToPrefabName(char c)
        {
            switch (c)
            {
                case 'あ': return "a_jp";
                case 'い': return "i_jp";
                case 'う': return "u_jp";
                case 'え': return "e_jp";
                case 'お': return "o_jp";
                case 'か': return "ka_jp";
                case 'き': return "ki_jp";
                case 'く': return "ku_jp";
                case 'け': return "ke_jp";
                case 'こ': return "ko_jp";
                case 'が': return "ga_jp";
                case 'ぎ': return "gi_jp";
                case 'ぐ': return "gu_jp";
                case 'げ': return "ge_jp";
                case 'ご': return "go_jp";
                case 'さ': return "sa_jp";
                case 'し': return "shi_jp";
                case 'す': return "su_jp";
                case 'せ': return "se_jp";
                case 'そ': return "so_jp";
                case 'ざ': return "za_jp";
                case 'じ': return "ji_jp";
                case 'ず': return "zu_jp";
                case 'ぜ': return "ze_jp";
                case 'ぞ': return "zo_jp";
                case 'た': return "ta_jp";
                case 'ち': return "chi_jp";
                case 'つ': return "tsu_jp";
                case 'て': return "te_jp";
                case 'と': return "to_jp";
                case 'だ': return "da_jp";
                case 'ぢ': return "di_jp";
                case 'づ': return "du_jp";
                case 'で': return "de_jp";
                case 'ど': return "do_jp";
                case 'な': return "na_jp";
                case 'に': return "ni_jp";
                case 'ぬ': return "nu_jp";
                case 'ね': return "ne_jp";
                case 'の': return "no_jp";
                case 'は': return "ha_jp";
                case 'ひ': return "hi_jp";
                case 'ふ': return "fu_jp";
                case 'へ': return "he_jp";
                case 'ほ': return "ho_jp";
                case 'ば': return "ba_jp";
                case 'び': return "bi_jp";
                case 'ぶ': return "bu_jp";
                case 'べ': return "be_jp";
                case 'ぼ': return "bo_jp";
                case 'ぱ': return "pa_jp";
                case 'ぴ': return "pi_jp";
                case 'ぷ': return "pu_jp";
                case 'ぺ': return "pe_jp";
                case 'ぽ': return "po_jp";
                case 'ま': return "ma_jp";
                case 'み': return "mi_jp";
                case 'む': return "mu_jp";
                case 'め': return "me_jp";
                case 'も': return "mo_jp";
                case 'や': return "ya_jp";
                case 'ゆ': return "yu_jp";
                case 'よ': return "yo_jp";
                case 'ら': return "ra_jp";
                case 'り': return "ri_jp";
                case 'る': return "ru_jp";
                case 'れ': return "re_jp";
                case 'ろ': return "ro_jp";
                case 'わ': return "wa_jp";
                case 'を': return "wo_jp";
                case 'ん': return "n_jp";
                case 'ぁ': return "xa_jp";
                case 'ぃ': return "xi_jp";
                case 'ぅ': return "xu_jp";
                case 'ぇ': return "xe_jp";
                case 'ぉ': return "xo_jp";
                case 'ゃ': return "xya_jp";
                case 'ゅ': return "xyu_jp";
                case 'ょ': return "xyo_jp";
                default:
                    return $"unknown_{(int)c}";
            }
        }
    }
}
