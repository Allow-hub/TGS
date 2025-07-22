using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TechC
{
    [InitializeOnLoad]
    public static class HitboxSceneDrawer
    {
        private struct HitboxInfo
        {
            public Vector3 position;
            public float radius;
            public Color color;
            public float expireTime;
        }

        private static List<HitboxInfo> hitboxes = new();

        static HitboxSceneDrawer()
        {
            // SceneView の描画イベントに登録（Editor開始時に呼ばれる）
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public static void ShowHitbox(Vector3 position, float radius, Color color, float duration = 0.1f)
        {
            hitboxes.Add(new HitboxInfo
            {
                position = position,
                radius = radius,
                color = color,
                expireTime = Time.realtimeSinceStartup + duration
            });

            SceneView.RepaintAll();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            float now = Time.realtimeSinceStartup;
            hitboxes.RemoveAll(h => h.expireTime < now);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            foreach (var hitbox in hitboxes)
            {
                Handles.color = hitbox.color;
                Handles.SphereHandleCap(0, hitbox.position, Quaternion.identity, hitbox.radius * 2f, EventType.Repaint);
            }
        }
    }
}
