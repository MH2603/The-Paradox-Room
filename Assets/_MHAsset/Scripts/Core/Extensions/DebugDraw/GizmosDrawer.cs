using UnityEngine;

namespace MH
{
    public static class GizmosDrawer
    {
        public static void DrawSpheres(Vector3[] points, float sphereRadius = 0.1f)
        {
            for (int i=0; i < points.Length; i++)
            {
                Gizmos.DrawSphere(points[i], sphereRadius);
            }
       
        }
    }
}