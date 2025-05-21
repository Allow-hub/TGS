using System.Collections;
using UnityEngine;

namespace TechC
{
    public class HitStopManager : Singleton<HitStopManager>
    {
        [SerializeField] private float defaultHitStopDuration = 0.1f;
        [SerializeField] private float defaultTimeScale = 0.05f;

        private Coroutine currentHitStopCoroutine;
        private float remainingTime = 0f;

        public void DoHitStop()
        {
            DoHitStop(defaultHitStopDuration, defaultTimeScale);
        }

        public void DoHitStop(float duration, float timeScale)
        {
            // 残り時間が今より長ければ延長しない（より強いヒットストップを優先）
            if (remainingTime < duration)
            {
                remainingTime = duration;

                if (currentHitStopCoroutine == null)
                {
                    currentHitStopCoroutine = StartCoroutine(HitStopCoroutine(timeScale));
                }
            }
        }

        private IEnumerator HitStopCoroutine(float timeScale)
        {
            float originalTimeScale = Time.timeScale;
            float originalFixedDeltaTime = Time.fixedDeltaTime;

            Time.timeScale = timeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * timeScale;

            // 残り時間がゼロになるまでリアルタイムでカウントダウン
            while (remainingTime > 0f)
            {
                yield return null;
                remainingTime -= Time.unscaledDeltaTime;
            }

            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            currentHitStopCoroutine = null;
        }
    }
}
