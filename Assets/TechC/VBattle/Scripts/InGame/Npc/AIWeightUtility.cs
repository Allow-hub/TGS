using UnityEngine;

namespace TechC
{
    /// <summary>
    /// 重み付けリストのユーティリティクラス
    /// 重み付け計算や正規化などの便利機能を提供
    /// </summary>
    public static class AIWeightUtility
    {
        public static string NPCLOGTAG = "npc";
        /// <summary>
        /// 重み付けリストの総重量を計算
        /// </summary>
        /// <param name="weights">重み付けリスト</param>
        /// <returns>総重量</returns>
        public static float GetTotalWeight(System.Collections.Generic.List<AIActionWeight> weights)
        {
            float total = 0f;
            foreach (var weight in weights)
            {
                total += weight.weight;
            }
            return total;
        }
        
        /// <summary>
        /// 重み付けリストを正規化（合計を1.0にする）
        /// </summary>
        /// <param name="weights">重み付けリスト</param>
        public static void NormalizeWeights(System.Collections.Generic.List<AIActionWeight> weights)
        {
            float total = GetTotalWeight(weights);
            
            if (total <= 0f)
            {
                // 全ての重みが0の場合、均等に分配
                float equalWeight = 1f / weights.Count;
                foreach (var weight in weights)
                {
                    weight.weight = equalWeight;
                }
            }
            else
            {
                // 総重量で各重みを割って正規化
                foreach (var weight in weights)
                {
                    weight.weight /= total;
                }
            }
        }
        
        /// <summary>
        /// 重み付けに基づいてランダムに行動を選択
        /// </summary>
        /// <param name="weights">重み付けリスト</param>
        /// <returns>選択された行動タイプ</returns>
        public static AIActionType SelectWeightedAction(System.Collections.Generic.List<AIActionWeight> weights)
        {
            if (weights == null || weights.Count == 0)
                return AIActionType.Wait;
            
            float totalWeight = GetTotalWeight(weights);
            if (totalWeight <= 0f)
                return weights[0].actionType;
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var weight in weights)
            {
                currentWeight += weight.weight;
                if (randomValue <= currentWeight)
                {
                    return weight.actionType;
                }
            }
            
            // フォールバック（通常は到達しない）
            return weights[weights.Count - 1].actionType;
        }
        
        /// <summary>
        /// 重み付けリストをデバッグ表示用文字列に変換
        /// </summary>
        /// <param name="weights">重み付けリスト</param>
        /// <returns>デバッグ文字列</returns>
        public static string GetWeightDebugString(System.Collections.Generic.List<AIActionWeight> weights)
        {
            if (weights == null || weights.Count == 0)
                return "No weights defined";
            
            string result = "Action Weights:\n";
            float total = GetTotalWeight(weights);
            
            foreach (var weight in weights)
            {
                float percentage = total > 0 ? (weight.weight / total) * 100f : 0f;
                result += $"  {weight.actionType}: {weight.weight:F2} ({percentage:F0}%)\n";
            }
            
            result += $"Total Weight: {total:F2}";
            return result;
        }
    }
}
