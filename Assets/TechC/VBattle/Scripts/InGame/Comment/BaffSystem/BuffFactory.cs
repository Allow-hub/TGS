using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// バフを生成するファクトリクラス
    /// バフタイプに応じて、適切なバフを生成するためのメソッドを提供
    /// </summary>
    public class BuffFactory : MonoBehaviour
    {
        private static Dictionary<BuffType, System.Func<BuffBase>> buffDictionary;

        static BuffFactory()
        {
            /* 初期化 */
            buffDictionary = new Dictionary<BuffType, System.Func<BuffBase>>()
            {
                { BuffType.Speed, () => new SpeedBuff()},
                { BuffType.Power, () => new PowerBuff()},
                // { BuffType.Jump, () => new JumpBuff()},
                { BuffType.MapChange,() => new MapChangeBuff()}
            };
        }

        public static BuffBase CreateBuff(BuffType buffType)
        {
            /* Dictionaryにバフタイプが登録されていなければ、それに対応するバフを生成 */
            if (buffDictionary.ContainsKey(buffType))
            {
                return buffDictionary[buffType]();
            }
            else
            {
                Debug.LogError("不明なバフ");
                return null;
            }
        }

        /* 新しいバフを追加するためのメソッド */
        public static void AddBuffType(BuffType buffType, System.Func<BuffBase> buffFactoryMethod)
        {
            if (!buffDictionary.ContainsKey(buffType))
            {
                buffDictionary.Add(buffType, buffFactoryMethod);
            }
        }
    }
}

