using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// Transformの記録、引き出しを行うクラス
    /// </summary>
    public class TransformRecorder : MonoBehaviour
    {
        public List<TransformData> records = new List<TransformData>();
        private Transform target;

        public float recordInterval = 1.0f; // 何秒ごとに記録
        public float keepDuration = 5.0f;   // 最大保持時間（秒）
        public int maxRecords = 100;        // 最大記録数

        private float timer = 0f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        private void Awake()
        {
            target = transform;
        }
        private void Update()
        {
            if (target == null) return;

            timer += Time.deltaTime;
            if (timer >= recordInterval)
            {
                RecordTransform();
                TrimOldRecords(); // 古い・多すぎる記録を削除
                timer = 0f;
            }
        }

        void RecordTransform()
        {
            records.Add(new TransformData(target));
        }

        void TrimOldRecords()
        {
            float cutoffTime = Time.time - keepDuration;

            // 古すぎる記録を削除（時間ベース）
            while (records.Count > 0 && records[0].timestamp < cutoffTime)
            {
                records.RemoveAt(0);
            }

            // 多すぎる記録を削除（件数ベース）
            while (records.Count > maxRecords)
            {
                records.RemoveAt(0);
            }
        }
        public void StartReplayFromSecondsAgo(float secondsAgo, Transform t)
        {
            if (records.Count == 0) return;
            StopAllCoroutines();
            StartCoroutine(ReplayCoroutine(secondsAgo, t));
        }

        private IEnumerator ReplayCoroutine(float secondsAgo, Transform t)
        {
            float startTime = Time.time - secondsAgo;
            List<TransformData> replayData = records.FindAll(d => d.timestamp >= startTime);

            if (replayData.Count < 2)
                yield break;

            for (int i = 0; i < replayData.Count - 1; i++)
            {
                TransformData from = replayData[i];
                TransformData to = replayData[i + 1];
                float duration = to.timestamp - from.timestamp;
                float timer = 0f;

                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float tLerp = Mathf.Clamp01(timer / duration);

                    // 線形補間でTransform再現
                    Vector3 pos = Vector3.Lerp(from.position, to.position, tLerp);
                    Quaternion rot = Quaternion.Slerp(from.rotation, to.rotation, tLerp);
                    Vector3 scale = Vector3.Lerp(from.scale, to.scale, tLerp);

                    t.position = pos;
                    t.rotation = rot;
                    t.localScale = scale;

                    yield return null;
                }
            }
        }

    }
}
