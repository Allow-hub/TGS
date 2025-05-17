using System;
using Cysharp.Threading.Tasks;

namespace TechC
{
    public static class DelayUtility
    {
        /// <summary>
        /// 数秒おいてメソッドを発火
        /// </summary>
        /// <param name="delaySeconds">秒数</param>
        /// <param name="callback">発火させたいメソッド</param>
        /// <returns></returns>
        public static async UniTask RunAfterDelay(float delaySeconds, Action callback)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
            callback?.Invoke();
        }

        /// <summary>
        /// async/await対応の関数を渡したい場合
        /// </summary>
        /// <param name="delaySeconds"></param>
        /// <param name="asyncCallback"></param>
        /// <returns></returns>
        public static async UniTask RunAfterDelay(float delaySeconds, Func<UniTask> asyncCallback)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
            if (asyncCallback != null)
            {
                await asyncCallback();
            }
        }
    }
}