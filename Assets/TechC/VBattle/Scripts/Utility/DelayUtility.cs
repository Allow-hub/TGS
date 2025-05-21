using System;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TechC
{
    public static class DelayUtility
    {
        // ================================
        // UniTask版
        // ================================

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

        // ================================
        // コルーチン版
        // ================================

        /// <summary>
        /// 数秒おいてメソッドを発火（コルーチン版）
        /// </summary>
        /// <param name="delaySeconds">秒数</param>
        /// <param name="callback">発火させたいメソッド</param>
        /// <returns></returns>
        public static IEnumerator RunAfterDelayCoroutine(float delaySeconds, Action callback)
        {
            yield return new WaitForSeconds(delaySeconds);
            callback?.Invoke();
        }

        /// <summary>
        /// コルーチンを渡したい場合
        /// </summary>
        /// <param name="delaySeconds">秒数</param>
        /// <param name="coroutineCallback">発火させたいコルーチン</param>
        /// <returns></returns>
        public static IEnumerator RunAfterDelayCoroutine(float delaySeconds, Func<IEnumerator> coroutineCallback)
        {
            yield return new WaitForSeconds(delaySeconds);
            if (coroutineCallback != null)
            {
                yield return coroutineCallback();
            }
        }

        /// <summary>
        /// MonoBehaviourのインスタンスを使用してコルーチンを開始するヘルパーメソッド
        /// </summary>
        /// <param name="monoBehaviour">コルーチンを開始するMonoBehaviour</param>
        /// <param name="delaySeconds">秒数</param>
        /// <param name="callback">発火させたいメソッド</param>
        /// <returns>開始されたコルーチン</returns>
        public static Coroutine StartDelayedAction(MonoBehaviour monoBehaviour, float delaySeconds, Action callback)
        {
            return monoBehaviour.StartCoroutine(RunAfterDelayCoroutine(delaySeconds, callback));
        }

        /// <summary>
        /// MonoBehaviourのインスタンスを使用してコルーチンを開始するヘルパーメソッド（コルーチン版）
        /// </summary>
        /// <param name="monoBehaviour">コルーチンを開始するMonoBehaviour</param>
        /// <param name="delaySeconds">秒数</param>
        /// <param name="coroutineCallback">発火させたいコルーチン</param>
        /// <returns>開始されたコルーチン</returns>
        public static Coroutine StartDelayedCoroutine(MonoBehaviour monoBehaviour, float delaySeconds, Func<IEnumerator> coroutineCallback)
        {
            return monoBehaviour.StartCoroutine(RunAfterDelayCoroutine(delaySeconds, coroutineCallback));
        }
        /// <summary>
        /// 一定間隔で処理を繰り返す（コルーチン版）
        /// </summary>
        /// <param name="duration">繰り返す合計時間（秒）</param>
        /// <param name="interval">実行間隔（秒）</param>
        /// <param name="callback">繰り返し実行する処理</param>
        public static IEnumerator RunRepeatedly(float duration, float interval, Action callback)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                callback?.Invoke();
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }

        /// <summary>
        /// MonoBehaviour を使って一定間隔で処理を繰り返すコルーチンを開始する
        /// </summary>
        /// <param name="monoBehaviour">コルーチンを開始する対象</param>
        /// <param name="duration">繰り返す合計時間（秒）</param>
        /// <param name="interval">実行間隔（秒）</param>
        /// <param name="callback">繰り返し実行する処理</param>
        /// <returns>開始されたコルーチン</returns>
        public static Coroutine StartRepeatedAction(MonoBehaviour monoBehaviour, float duration, float interval, Action callback)
        {
            return monoBehaviour.StartCoroutine(RunRepeatedly(duration, interval, callback));
        }

    }
}