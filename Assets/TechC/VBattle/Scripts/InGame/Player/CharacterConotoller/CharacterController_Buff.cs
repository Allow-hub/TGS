using System.Linq;
using UnityEngine;
using TechC.CommentSystem;

namespace TechC.Player
{
    /// <summary>
    /// CharacterController_Buff.cs
    /// バフ関連の分離クラス
    /// </summary>
    public partial class CharacterController
    {
        /// <summary>
        /// バフの適用（バフの種類,乗算の数値）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void AddMultiplier(BuffType type, int buffId, float value)
        {
            var dic = multiplierEntries[type];

            if (dic.ContainsKey(buffId))
                dic[buffId] = value;
            else
                dic.Add(buffId, value);

            // multipliersの値を再計算
            UpdateMultiplier(type);
        }

        /// <summary>
        /// バフの解除（バフの種類,除算の数値）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void RemoveMultiplier(BuffType type, int buffId, float value)
        {
            var dic = multiplierEntries[type];

            if (dic.ContainsKey(buffId))
                dic.Remove(buffId);
            else
            {
                Debug.LogWarning($"登録されていないバフを失効しようとしました:{type}/{buffId}");
                return;
            }

            // multipliersの値を再計算
            UpdateMultiplier(type);
        }

        /// <summary>
        /// 指定されたバフタイプの最終倍率を計算して更新
        /// </summary>
        /// <param name="type"></param>
        private void UpdateMultiplier(BuffType type)
        {
            var dic = multiplierEntries[type];

            // すべてのバフを乗算で適用
            float finalMultiplier = 1.0f;
            foreach (var buff in dic.ToArray())
            {
                finalMultiplier *= buff.Value;
            }

            multipliers[type] = finalMultiplier;
        }

        /// <summary>
        /// multipliers に type が 存在するなら、その値（value）を返す
        /// 存在しないなら、デフォルト値の 1.0f を返す
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public float GetMultipiler(BuffType type) =>
            multipliers.TryGetValue(type, out var value) ? value : 1.0f;
    }
}
