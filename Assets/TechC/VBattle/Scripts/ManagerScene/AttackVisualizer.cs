using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class AttackVisualizer : Singleton<AttackVisualizer>
    {

        [System.Serializable]
        public class HitboxInfo
        {
            public Vector3 position;
            public float radius;
            public Color color;
            public float duration;
            public float startTime;

            public bool IsExpired => Time.time > startTime + duration;
        }

        private List<HitboxInfo> activeHitboxes = new List<HitboxInfo>();

   

        public  void DrawHitbox(Vector3 position, float radius, float duration = 0.5f, Color? color = null)
        {
            I.activeHitboxes.Add(new HitboxInfo
            {
                position = position,
                radius = radius,
                color = color ?? new Color(1f, 0f, 0f, 0.5f),
                duration = duration,
                startTime = Time.time
            });
        }

        private void Update()
        {
            // 期限切れのヒットボックスを削除
            activeHitboxes.RemoveAll(h => h.IsExpired);
        }

        private void OnDrawGizmos()
        {
            // エディタ上でも実行中でも描画される
            foreach (var hitbox in activeHitboxes)
            {
                Gizmos.color = hitbox.color;
                Gizmos.DrawSphere(hitbox.position, hitbox.radius);
            }
        }
    }
}