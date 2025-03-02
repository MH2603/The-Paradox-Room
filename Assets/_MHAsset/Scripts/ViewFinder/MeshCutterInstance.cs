using System;
using UnityEngine;
using System.Collections.Generic;
using MH;

public class MeshCutterInstance : MonoBehaviour
{
    // Reference to the plane used for cutting
    public Transform cuttingPlane;
    // Reference to the target object to be cut
    [SerializeField] private GameObject targetObject;

    private Mesh _positiveMesh;
    private Mesh _negativeMesh;
    
    private GameObject _positiveObject, _negativeObject;

    // Unity's Start method, called before the first frame update
    void Start()
    {
        // Initialization code can be added here if needed
    }

    // Method to perform the cut operation
    public void Cut()
    {
        
        // Create a plane using the cuttingPlane's up direction and position
        Plane plane = new Plane(cuttingPlane.up, cuttingPlane.transform.position);
            
        // Call the static Cut method to perform the cut
        MeshCutter.Cut(targetObject, plane, out _positiveObject, out _negativeObject);
        
        _positiveMesh = _positiveObject.GetComponent<MeshFilter>().mesh;
        _negativeMesh = _negativeObject.GetComponent<MeshFilter>().mesh;
    }

    private void OnDrawGizmos()
    {
        if (_positiveMesh) GizmosDrawer.DrawSpheres(MeshCutter.ConvertVerticesToWorldSpace(_positiveMesh, _positiveObject.transform), 0.1f);
        if (_negativeMesh) GizmosDrawer.DrawSpheres( MeshCutter.ConvertVerticesToWorldSpace(_negativeMesh, _negativeObject.transform), 0.1f);
    }
}

public static class MeshCutter
{
    // Static method to cut the target object using the specified plane
    public static void Cut(GameObject target, Plane plane, out GameObject positiveObject, out GameObject negativeObject)
    {
        // Get the MeshFilter component from the target object
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found on target object.");
            positiveObject = null;
            negativeObject = null;
            return;
        }

        List<Vector3> intersectionPoints;
        // Generate the positive part of the mesh
        Mesh positiveMesh = GenerateMesh(meshFilter.mesh, target.transform, plane, true, out intersectionPoints);
        // Generate the negative part of the mesh
        Mesh negativeMesh = GenerateMesh(meshFilter.mesh, target.transform, plane, false, out intersectionPoints);

        // Create new GameObjects for the positive and negative parts
        positiveObject = CreateMeshObject(target, positiveMesh, "PositivePart");
        negativeObject = CreateMeshObject(target, negativeMesh, "NegativePart");
    }

    // Method to generate a new mesh based on the plane and whether it's the positive or negative part
    private static Mesh GenerateMesh(Mesh mesh, Transform target, Plane plane, bool isPositive,
        out List<Vector3> intersectionPoints)
    {
        intersectionPoints = new List<Vector3>();

        List<Vector3> vertices = new List<Vector3>(mesh.vertices); // take all vertices of origin mesh

        List<int> triangles = new List<int>(mesh.triangles); // take all triangles of origin mesh
        List<Vector3> newVertices = new List<Vector3>(); // init a new list of vertices
        List<int> newTriangles = new List<int>(); // init a new list of triangles

        // Iterate through each triangle in the mesh
        for (int i = 0; i < triangles.Count; i += 3)
        {
            
            Vector3 v0 = vertices[triangles[i]];
            v0 = target.TransformPoint(v0);
            Vector3 v1 = vertices[triangles[i + 1]];
            v1 = target.TransformPoint(v1);
            Vector3 v2 = vertices[triangles[i + 2]];
            v2 = target.TransformPoint(v2);
            
            Debug.Log(
                $"Set triangle {i} with v[0,1,2] world-positions: {v0} , {v1} , {v2} and plane position: {plane.distance}");

            List<Vector3> currentIntersections = new List<Vector3>();
            List<Vector3> insideVertices = new List<Vector3>();
            List<Vector3> outsideVertices = new List<Vector3>();

            // Classify each vertex of the triangle as inside or outside the plane
            ClassifyVertex(v0, plane, insideVertices, outsideVertices, isPositive);
            ClassifyVertex(v1, plane, insideVertices, outsideVertices, isPositive);
            ClassifyVertex(v2, plane, insideVertices, outsideVertices, isPositive);

            // Handle different cases based on the number of inside and outside vertices
            if (insideVertices.Count == 3)
            {
                newTriangles.AddRange(new int[] { newVertices.Count, newVertices.Count + 1, newVertices.Count + 2 });
                newVertices.Add(v0);
                newVertices.Add(v1);
                newVertices.Add(v2);
            }
            else if (insideVertices.Count == 2 && outsideVertices.Count == 1)
            {
                SplitTriangle(insideVertices[0], insideVertices[1], outsideVertices[0], plane, newVertices,
                    newTriangles, currentIntersections);
            }
            else if (insideVertices.Count == 1 && outsideVertices.Count == 2)
            {
                // SplitTriangle(insideVertices[0], outsideVertices[0], outsideVertices[1], plane, newVertices,
                //     newTriangles, currentIntersections);
                SplitTriangle_Ver2(insideVertices[0], outsideVertices[0], outsideVertices[1], plane, newVertices,
                    newTriangles, currentIntersections);

            }

            intersectionPoints.AddRange(currentIntersections);
        }

        if (isPositive)
        {
            var content = "";
            content = "--  Positive Mesh \n";
            
            content += " Vertices: ";
            foreach (var pos in newVertices)
            {
                content += $"{pos} | ";
            }
            
            content += "\n Triangles: ";
            foreach (var index in newTriangles)
            {
                content += index + " | ";
            }
            Debug.Log(content);
        }

        // Create a new mesh with the generated vertices and triangles
        Mesh newMesh = new Mesh();
        newMesh.vertices = ConvertToObjectSpace(target, newVertices.ToArray()) ;
        newMesh.triangles = newTriangles.ToArray();
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        return newMesh;
    }

    // Method to classify a vertex as inside or outside the plane
    private static void ClassifyVertex(Vector3 vertex, Plane plane, List<Vector3> inside, List<Vector3> outside,
        bool positive)
    {
        if (positive == (plane.GetDistanceToPoint(vertex) >= 0))
            inside.Add(vertex);
        else
            outside.Add(vertex);
    }

    // Method to split a triangle by the plane
    private static void SplitTriangle(Vector3 insideA, Vector3 insideB, Vector3 outside, Plane plane,
        List<Vector3> newVertices, List<int> newTriangles, List<Vector3> intersections)
    {
        Vector3 intersection1 = LinePlaneIntersection(insideA, outside, plane);
        Vector3 intersection2 = LinePlaneIntersection(insideB, outside, plane);

        intersections.Add(intersection1);
        intersections.Add(intersection2);

        int baseIndex = newVertices.Count;
        newVertices.Add(insideA);
        newVertices.Add(insideB);
        newVertices.Add(intersection1);
        newTriangles.AddRange(new int[] { baseIndex, baseIndex + 1, baseIndex + 2 });

        newVertices.Add(insideB);
        newVertices.Add(intersection2);
        newVertices.Add(intersection1);
        newTriangles.AddRange(new int[] { baseIndex + 3, baseIndex + 4, baseIndex + 5 });
    }
    
    private static void SplitTriangle_Ver2(Vector3 inside, Vector3 outsideA, Vector3 outsideB, Plane plane,
        List<Vector3> newVertices, List<int> newTriangles, List<Vector3> intersections)
    {
        Vector3 intersection1 = LinePlaneIntersection(outsideA, inside, plane);
        Vector3 intersection2 = LinePlaneIntersection(outsideB, inside, plane);

        intersections.Add(intersection1);
        intersections.Add(intersection2);

        int baseIndex = newVertices.Count;
        newVertices.Add(intersection1);
        newVertices.Add(intersection2);
        newVertices.Add(inside);
        newTriangles.AddRange(new int[] { baseIndex, baseIndex + 1, baseIndex + 2 });
        
    }

    // Method to find the intersection point of a line segment and the plane
    private static Vector3 LinePlaneIntersection(Vector3 start, Vector3 end, Plane plane)
    {
        Ray ray = new Ray(start, (end - start).normalized);
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    // Method to create a new GameObject with the specified mesh
    private static GameObject CreateMeshObject(GameObject original, Mesh mesh, string name)
    {

        GameObject newObject = new GameObject(name);
        newObject.transform.position = original.transform.position;
        newObject.transform.rotation = original.transform.rotation;
        newObject.transform.localScale = original.transform.localScale;
        newObject.AddComponent<MeshFilter>().mesh = mesh;
        newObject.AddComponent<MeshRenderer>().material = original.GetComponent<MeshRenderer>().material;
        return newObject;
    }

    public static Vector3[] ConvertVerticesToWorldSpace(Mesh mesh, Transform transform)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transform.TransformPoint(vertices[i]);
        }

        return vertices;
    }

    private static Vector3[] ConvertToObjectSpace(Transform target, Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = target.InverseTransformPoint(positions[i]);
        }

        return positions;
    }
}