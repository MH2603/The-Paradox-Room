using UnityEngine;
using System.Collections.Generic;

public class PlaneMeshIntersection : MonoBehaviour
{
    public MeshFilter meshFilter;    // Mesh cần check
    public Transform planeTransform; // Tấm Plane
    public float sphereRadius = 0.1f; // Kích thước Sphere Gizmo

    private void OnDrawGizmos()
    {
        if (meshFilter == null || planeTransform == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) return;

        Plane plane = new Plane(planeTransform.up, planeTransform.position);
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        List<Vector3> allIntersections = new List<Vector3>();

        // Duyệt qua từng tam giác
        for (int i = 0; i < triangles.Length; i += 3)
        {
            List<Vector3> intersections = new List<Vector3>();

            // Kiểm tra từng cạnh
            CheckEdgeIntersection(vertices[triangles[i]], vertices[triangles[i + 1]], plane, intersections);
            CheckEdgeIntersection(vertices[triangles[i + 1]], vertices[triangles[i + 2]], plane, intersections);
            CheckEdgeIntersection(vertices[triangles[i + 2]], vertices[triangles[i]], plane, intersections);

            allIntersections.AddRange(intersections);
        }

        // Vẽ tất cả các giao điểm
        foreach (Vector3 point in allIntersections)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, sphereRadius);
        }

        // Vẽ trọng tâm trung bình cộng của tất cả giao điểm
        if (allIntersections.Count > 0)
        {
            Vector3 centroid = CalculateCentroid(allIntersections);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(centroid, sphereRadius);

            // Vẽ các đường nối từ G đến các giao điểm
            Gizmos.color = Color.green;
            foreach (Vector3 point in allIntersections)
            {
                Gizmos.DrawLine(centroid, point);
            }
        }
    }

    private void CheckEdgeIntersection(Vector3 vertexA, Vector3 vertexB, Plane plane, List<Vector3> intersections)
    {
        Vector3 worldA = meshFilter.transform.TransformPoint(vertexA);
        Vector3 worldB = meshFilter.transform.TransformPoint(vertexB);
        Vector3 direction = worldB - worldA;
        Ray ray = new Ray(worldA, direction);

        float distance;
        if (plane.Raycast(ray, out distance))
        {
            if (distance >= 0 && distance <= direction.magnitude)
            {
                Vector3 intersection = ray.GetPoint(distance);
                intersections.Add(intersection);
            }
        }
    }

    private Vector3 CalculateCentroid(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in points)
        {
            sum += point;
        }
        return sum / points.Count;
    }
}
