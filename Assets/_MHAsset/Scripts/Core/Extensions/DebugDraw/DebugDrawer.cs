using UnityEngine;

namespace MH
{
    public static class DebugDrawer
    {
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = true)
        {
            Debug.DrawLine(start, end, color, duration, depthTest);
        }

        public static void DrawRay(Vector3 start, Vector3 direction, Color color, float duration = 0, bool depthTest = true)
        {
            Debug.DrawRay(start, direction, color, duration, depthTest);
        }

        public static void DrawCircle(Vector3 position, float radius, Color color, float duration = 0.1f)
        {
            int segments = 20;
            float angle = 0f;
            for (int i = 0; i < segments; i++)
            {
                Vector3 start = position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
                angle += 2 * Mathf.PI / segments;
                Vector3 end = position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
                Debug.DrawLine(start, end, color, duration);
            }
        }

        public static void DrawCube(Vector3 position, Vector3 size, Color color, float duration = 0)
        {
            Vector3 halfSize = size / 2;
            Vector3[] points = new Vector3[8]
            {
                position + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
                position + new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
                position + new Vector3(halfSize.x, -halfSize.y, halfSize.z),
                position + new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
                position + new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
                position + new Vector3(halfSize.x, halfSize.y, -halfSize.z),
                position + new Vector3(halfSize.x, halfSize.y, halfSize.z),
                position + new Vector3(-halfSize.x, halfSize.y, halfSize.z)
            };

            Debug.DrawLine(points[0], points[1], color, duration);
            Debug.DrawLine(points[1], points[2], color, duration);
            Debug.DrawLine(points[2], points[3], color, duration);
            Debug.DrawLine(points[3], points[0], color, duration);
            Debug.DrawLine(points[4], points[5], color, duration);
            Debug.DrawLine(points[5], points[6], color, duration);
            Debug.DrawLine(points[6], points[7], color, duration);
            Debug.DrawLine(points[7], points[4], color, duration);
            Debug.DrawLine(points[0], points[4], color, duration);
            Debug.DrawLine(points[1], points[5], color, duration);
            Debug.DrawLine(points[2], points[6], color, duration);
            Debug.DrawLine(points[3], points[7], color, duration);
        }
    }
}