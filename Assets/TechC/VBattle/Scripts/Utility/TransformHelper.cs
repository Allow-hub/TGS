using UnityEngine;

namespace TechC
{
    public static class TransformHelper
    {
        public static void ResetLocalTransform(this Transform t)
        {
            if (t == null) return;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static void ResetWorldTransform(this Transform t)
        {
            if (t == null) return;
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static void DestroyChildren(this Transform t)
        {
            if (t == null) return;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(t.GetChild(i).gameObject);
            }
        }

        public static void SetUniformScale(this Transform t, float scale)
        {
            if (t == null) return;
            t.localScale = new Vector3(scale, scale, scale);
        }

        public static Vector3 GetPositionXZ(this Transform t)
        {
            if (t == null) return Vector3.zero;
            var pos = t.position;
            return new Vector3(pos.x, 0f, pos.z);
        }

        public static void LookAtLockedAxis(this Transform transform, Vector3 targetPosition, Vector3 lockAxis)
        {
            Vector3 direction = targetPosition - transform.position;
            direction = Vector3.ProjectOnPlane(direction, lockAxis.normalized);
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
