using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class HitStopManager : Singleton<HitStopManager>
    {
        [SerializeField] private float defaultHitStopDuration = 0.1f;
        [SerializeField] private float defaultTimeScale = 0.05f;
        // 標準のヒットストップを実行
        public void DoHitStop()
        {
            DoHitStop(defaultHitStopDuration, defaultTimeScale);
        }

        // パラメータ指定でヒットストップを実行
        public void DoHitStop(float duration, float timeScale)
        {
            StopAllCoroutines();
            StartCoroutine(HitStopCoroutine(duration, timeScale));
        }

        // 攻撃の強さに応じたヒットストップを実行
        public void DoHitStop(float damage, bool isStrongAttack)
        {
            // 攻撃の強さに応じてヒットストップのパラメータを調整
            float duration = isStrongAttack ?
                Mathf.Lerp(0.1f, 0.25f, damage / 20f) :
                Mathf.Lerp(0.05f, 0.15f, damage / 15f);

            float scale = isStrongAttack ?
                Mathf.Lerp(0.05f, 0.02f, damage / 20f) :
                Mathf.Lerp(0.1f, 0.05f, damage / 15f);

            DoHitStop(duration, scale);
        }

        private IEnumerator HitStopCoroutine(float duration, float timeScale)
        {
            // 元の時間スケールを保存
            float originalTimeScale = Time.timeScale;
            float originalFixedDeltaTime = Time.fixedDeltaTime;

            // 時間を遅くする
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = Time.fixedDeltaTime * timeScale;

            // 実時間で待機（タイムスケールの影響を受けない）
            yield return new WaitForSecondsRealtime(duration);

            // 元の時間に戻す
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }
    }
}