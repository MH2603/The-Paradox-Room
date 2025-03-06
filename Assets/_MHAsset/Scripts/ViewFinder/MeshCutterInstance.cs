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
    private List<Vector3> _intersections = new();

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
        MeshCutter.Cut(targetObject, plane, out _positiveObject, out _negativeObject,_intersections);
        
        // _positiveMesh = _positiveObject.GetComponent<MeshFilter>().mesh;
        // _negativeMesh = _negativeObject.GetComponent<MeshFilter>().mesh;
        //
        // Vector2[] uvArray = targetObject.GetComponent<MeshFilter>().mesh.uv;
        // Vector3[] vertices = targetObject.GetComponent<MeshFilter>().mesh.vertices;
        // var content = " Target UVs: ";
        // foreach (var uv in uvArray)
        // {
        //    content += uv + "| ";
        // }
        //
        // content += "\n Vertices: ";
        // foreach (var vertex in vertices)
        // {
        //     content += vertex + " | ";
        // }
        // Debug.Log(content);
    }

    private void OnDrawGizmosSelected()
    {
        // if (_positiveMesh) GizmosDrawer.DrawSpheres(MeshCutter.ConvertVerticesToWorldSpace(_positiveMesh, _positiveObject.transform), 0.1f);
        // if (_negativeMesh) GizmosDrawer.DrawSpheres( MeshCutter.ConvertVerticesToWorldSpace(_negativeMesh, _negativeObject.transform), 0.1f);

        for (int i=0; i < _intersections.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_intersections[i], 0.05f);
            
        }
   
    }
}

public static class MeshCutter
{
    // Static method to cut the target object using the specified plane
    public static void Cut(GameObject target, Plane plane, out GameObject positiveObject, out GameObject negativeObject, List<Vector3> intersections)
    {
        // Get the MeshFilter component from the target object
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        var skinnedMeshRenderer = target.GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = null;
        if (meshFilter != null) mesh = meshFilter.mesh;
        if (skinnedMeshRenderer != null)  mesh = skinnedMeshRenderer.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError(" Bug : Not found mesh container !");
            positiveObject = null;
            negativeObject = null;
            // intersections = null;
            return;
        }

        List<Vector3> intersectionPoints;
        // Generate the positive part of the mesh
        Mesh positiveMesh = GenerateMesh(mesh, target.transform, plane, true, out intersectionPoints);
        // Generate the negative part of the mesh
        Mesh negativeMesh = GenerateMesh(mesh, target.transform, plane, false, out intersectionPoints);

        // Create new GameObjects for the positive and negative parts
        positiveObject = CreateMeshObject(target, positiveMesh, target.name + "_PositivePart");
        negativeObject = CreateMeshObject(target, negativeMesh, target.name + "_NegativePart");

        intersections.AddRange(intersectionPoints); 
    }

    // Method to generate a new mesh based on the plane and whether it's the positive or negative part
    public static Mesh GenerateMesh(Mesh mesh, Transform target, Plane plane, bool isPositive,
        out List<Vector3> intersectionPoints)
    {
        intersectionPoints = new List<Vector3>();
        var intersectionUVs = new List<Vector2>();
        
        // datas from origin mesh
        List<Vector3> originVertices = new List<Vector3>(mesh.vertices); // take all vertices of origin mesh
        List<int> originTriangles = new List<int>(mesh.triangles); // take all triangles of origin mesh
        Vector2[] originUVs = mesh.uv; // take all UVs of origin mesh
        
        List<Vector3> newVertices = new List<Vector3>(); // init a new list of vertices
        List<int> newTriangles = new List<int>(); // init a new list of triangles
        List<Vector2> newUVs = new();
        
        // Iterate through each triangle in the mesh
        for (int i = 0; i < originTriangles.Count; i += 3)
        {
            
            Vector3 v0 = originVertices[originTriangles[i]];
            v0 = target.TransformPoint(v0);
            Vector3 v1 = originVertices[originTriangles[i + 1]];
            v1 = target.TransformPoint(v1);
            Vector3 v2 = originVertices[originTriangles[i + 2]];
            v2 = target.TransformPoint(v2);
            
            // Debug.Log(
            //     $"Set triangle {i} with v[0,1,2] world-positions: {v0} , {v1} , {v2} and plane position: {plane.distance}");
            
            List<Vector3> insideVertices = new List<Vector3>();
            List<Vector3> outsideVertices = new List<Vector3>();
            List<Vector2> insideUVs = new List<Vector2>();
            List<Vector2> outsideUVs = new List<Vector2>();

            // Classify each vertex of the triangle as inside or outside the plane
            ClassifyVertex(v0, originUVs[originTriangles[i]], plane, insideVertices, outsideVertices, insideUVs, outsideUVs, isPositive);
            ClassifyVertex(v1, originUVs[originTriangles[i+1]], plane, insideVertices, outsideVertices, insideUVs, outsideUVs, isPositive);
            ClassifyVertex(v2, originUVs[originTriangles[i+2]], plane, insideVertices, outsideVertices, insideUVs, outsideUVs, isPositive);

            // calculate normal vector of triangle
            // use to define normal of new triangle
            var normal = CalculateNormal(v0, v1, v2);
            // DebugDrawer.DrawRay(v0, normal, Color.red, 5f);
            
            List<Vector2> totalUVs = new List<Vector2>();
            totalUVs.AddRange(insideUVs);
            totalUVs.AddRange(outsideUVs);
            
            // Handle different cases based on the number of inside and outside vertices
            if (insideVertices.Count == 3)
            {
                newTriangles.AddRange(new int[] { newVertices.Count, newVertices.Count + 1, newVertices.Count + 2 });
                newVertices.Add(v0);
                newVertices.Add(v1);
                newVertices.Add(v2);
                newUVs.AddRange(totalUVs);
            }
            else if (insideVertices.Count == 2 && outsideVertices.Count == 1)
            {
                SplitTriangle_Case01(insideVertices[0], insideVertices[1], outsideVertices[0], normal, totalUVs.ToArray(), plane, newVertices,
                    newTriangles, newUVs,intersectionPoints, intersectionUVs);
            }
            else if (insideVertices.Count == 1 && outsideVertices.Count == 2)
            {
                // SplitTriangle_Case01(insideVertices[0], outsideVertices[0], outsideVertices[1], plane, newVertices,
                //     newTriangles, currentIntersections);
                SplitTriangle_Case02(insideVertices[0], outsideVertices[0], outsideVertices[1], normal, totalUVs.ToArray(), plane, newVertices,
                    newTriangles, newUVs, intersectionPoints, intersectionUVs);

            }
            
        }

        //
        Vector3 intersectionNormal = isPositive ? plane.normal.normalized * -1 : plane.normal.normalized ;
        SplitTriangleFromIntersections(intersectionPoints, intersectionUVs, intersectionNormal,
                                        newVertices, newUVs, newTriangles);

        // Create a new mesh with the generated vertices and triangles
        Mesh newMesh = new Mesh();
        newMesh.vertices = ConvertToObjectSpace(target, newVertices.ToArray()) ;
        newMesh.triangles = newTriangles.ToArray();
        newMesh.uv = newUVs.ToArray();
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        return newMesh;
    }

    

    // Method to classify a vertex as inside or outside the plane
    private static void ClassifyVertex(Vector3 vertex, Vector2 uv, Plane plane, List<Vector3> inside, List<Vector3> outside, List<Vector2> insideUVs, List<Vector2> outsideUVs,
        bool positive)
    {
        if (positive == (plane.GetDistanceToPoint(vertex) >= 0))
        {
            inside.Add(vertex);
            insideUVs.Add(uv);
        }
        else
        {
            outside.Add(vertex);
            outsideUVs.Add(uv);
        }
            
            
    }

    // Method to split a triangle by the plane with case [ 2 inside point and 1 outside points ]
    private static void SplitTriangle_Case01(Vector3 insideA, Vector3 insideB, Vector3 outside,Vector3 normal, Vector2[] uvArray, Plane plane,
        List<Vector3> newVertices, List<int> newTriangles, List<Vector2> newUVs,List<Vector3> intersections, List<Vector2> intersectionUVs)
    {
        // find 2 intersection points and calculate 2 intersection UVs
        Vector3 intersection1 = LinePlaneIntersection(insideA, outside, plane);
        Vector2 intersectionUV1 = CalculateIntersectionUV(insideA, outside, uvArray[0], uvArray[2], intersection1);
        Vector3 intersection2 = LinePlaneIntersection(insideB, outside, plane);
        Vector2 intersectionUV2 = CalculateIntersectionUV(insideB, outside, uvArray[1], uvArray[2], intersection2);

        intersections.Add(intersection1);
        intersections.Add(intersection2);
        intersectionUVs.Add(intersectionUV1);
        intersectionUVs.Add(intersectionUV2);

        // from information of new vertices and UVs, we can build 2 new triangles
        BuildTriangle(insideA, insideB, intersection1, uvArray[0], uvArray[1], intersectionUV1, normal,
                    newVertices, newTriangles, newUVs);
        
        BuildTriangle(insideB, intersection1, intersection2, uvArray[1], intersectionUV1, intersectionUV2, normal,
                    newVertices, newTriangles, newUVs);
    }
    
    // Method to split a triangle by the plane with case [ 1 inside point and 2 outside points ]
    private static void SplitTriangle_Case02(Vector3 inside, Vector3 outsideA, Vector3 outsideB, Vector3 normal, Vector2[] uvArray, Plane plane,
        List<Vector3> newVertices, List<int> newTriangles, List<Vector2> newUVs,List<Vector3> intersections, List<Vector2> intersectionUVs)
    {
        // find 2 intersection points and calculate 2 intersection UVs
        Vector3 intersection1 = LinePlaneIntersection(outsideA, inside, plane);
        Vector2 intersectionUV1 = CalculateIntersectionUV(outsideA, inside, uvArray[1], uvArray[0], intersection1);
        Vector3 intersection2 = LinePlaneIntersection(outsideB, inside, plane);
        Vector2 intersectionUV2 = CalculateIntersectionUV(outsideB, inside, uvArray[2], uvArray[0], intersection2);

        intersections.Add(intersection1);
        intersections.Add(intersection2);
        intersectionUVs.Add(intersectionUV1);
        intersectionUVs.Add(intersectionUV2);
        
        // from information of new vertices and UVs, we can build 1 new triangles
        BuildTriangle(intersection1, intersection2, inside, intersectionUV1, intersectionUV2, uvArray[0], normal, 
                    newVertices, newTriangles, newUVs);
        
    }
    
    private static void SplitTriangleFromIntersections(List<Vector3> intersections, List<Vector2> intersectionUVs, Vector3 normal,
        List<Vector3> newVertices,  List<Vector2> newUVs, List<int> newTriangles)
    {
        // check if intersections and intersectionUVs not have the same size
        if (intersections.Count != intersectionUVs.Count)
        {
            Debug.LogError($"Intersections and intersectionUVs have different size: {intersections.Count} and {intersectionUVs.Count}");
            return;
        }
        
        // remove duplicate intersections which have the same position
        List<int> removeIndexs = new();
        intersections = ListExtensions.RemoveDuplicates(intersections, removeIndexs);
        ListExtensions.RemoveItemsByIndices(intersectionUVs, removeIndexs);
        
        if(intersections.Count < 3)  return;
        
        // find center point
        Vector3 centerPoint = GetCenterPoint(intersections);
        Vector2 centerUV = GetCenterPoint(intersectionUVs);
        
        // sort intersections follow clockwise
        intersections = SortPointsFollowClockWise(intersections, centerPoint);
        
        
        // use for loop to build new triangles with a vertex is center point and 2 other vertices are intersections
        // DebugDrawer.DrawCircle(centerPoint, 0.05f, Color.red, 5f);
        for (int i = 0; i < intersections.Count; i += 1)
        {
            // BuildTriangle(centerPoint, intersections[i], intersections[(i + 1) % intersections.Count], 
            //     centerUV, intersectionUVs[i], intersectionUVs[(i + 1) % intersections.Count], normal, 
            //     newVertices, newTriangles, newUVs);
            BuildTriangle(centerPoint, intersections[i], intersections[(i + 1) % intersections.Count], 
                Vector2.zero, Vector2.zero, Vector2.zero, normal, 
                newVertices, newTriangles, newUVs);
            
            // DebugDrawer.DrawCircle(intersections[i], 0.05f, Color.yellow, 5f);
            // DebugDrawer.DrawCircle(intersections[(i + 1) % intersections.Count], 0.05f, Color.yellow, 5f);
        }
        
    }

    private static void BuildTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector3 originNormal, 
                                    List<Vector3> newVertices, List<int> newTriangles, List<Vector2> newUVs)
    {
        int baseIndex = newVertices.Count;

        // Add vertices
        newVertices.Add(v0);
        newVertices.Add(v1);
        newVertices.Add(v2);

        // Add UVs
        newUVs.Add(uv0);
        newUVs.Add(uv1);
        newUVs.Add(uv2);

        // Calculate the normal of the new triangle
        Vector3 newNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

        // Determine the winding order based on the dot product of the normals
        if (Vector3.Dot(originNormal, newNormal) > 0)
        {
            // Same direction, add normally
            newTriangles.Add(baseIndex);
            newTriangles.Add(baseIndex + 1);
            newTriangles.Add(baseIndex + 2);
        }
        else
        {
            // Opposite direction, add in reverse order
            newTriangles.Add(baseIndex);
            newTriangles.Add(baseIndex + 2);
            newTriangles.Add(baseIndex + 1);
        }
    }

    private static List<Vector3> SortPointsFollowClockWise(List<Vector3> points, Vector3 centerPoint)
    {
        points.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.z - centerPoint.z, a.x - centerPoint.x);
            float angleB = Mathf.Atan2(b.z - centerPoint.z, b.x - centerPoint.x);
            return angleA.CompareTo(angleB);
        });
        return points;
    }

    private static Vector3 GetCenterPoint(List<Vector3> points)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 point in points)
        {
            center += point;
        }
        center /= points.Count;

        return center;
    }
    private static Vector2 GetCenterPoint(List<Vector2> points)
    {
        Vector2 center = Vector2.zero;
        foreach (Vector2 point in points)
        {
            center += point;
        }
        center /= points.Count;

        return center;
    }
    

    public static Vector3 CalculateNormal(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        return Vector3.Cross(edge1, edge2).normalized;
    }
    
    // NOTE: Intersection is must in line AB
    public static Vector2 CalculateIntersectionUV(Vector3 A, Vector3 B, Vector2 uvA, Vector2 uvB, Vector3 intersection)
    {
        // Calculate the interpolation factor (t) for the intersection point
        float t = Vector3.Distance(A, intersection) / Vector3.Distance(A, B);

        // Interpolate the UV coordinates using the factor t
        Vector2 intersectionUV = Vector2.Lerp(uvA, uvB, t);

        return intersectionUV;
    }

    // Method to find the intersection point of a line segment and the plane
    private static Vector3 LinePlaneIntersection(Vector3 start, Vector3 end, Plane plane)
    {
        Ray ray = new Ray(start, (end - start).normalized);
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    // Method to create a new GameObject with the specified mesh
    public static GameObject CreateMeshObject(GameObject original, Mesh mesh, string name)
    {

        GameObject newObject = new GameObject(name);
        newObject.transform.position = original.transform.position;
        newObject.transform.rotation = original.transform.rotation;
        newObject.transform.localScale = original.transform.lossyScale;
        newObject.AddComponent<MeshFilter>().mesh = mesh;
        Material material = null;
        if(original.GetComponent<MeshRenderer>()) material = original.GetComponent<MeshRenderer>().material;
        if (original.GetComponent<SkinnedMeshRenderer>()) material = original.GetComponent<SkinnedMeshRenderer>().material;
            
        newObject.AddComponent<MeshRenderer>().material = material;
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