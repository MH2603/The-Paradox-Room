using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[Serializable]
// Represents a 3D vector with x, y, and z coordinates
public struct A3DVECTOR3
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
// Represents vertex data of a mesh
public struct A3DLMVERTEX 
{
    public A3DVECTOR3 pos;    // Position of the vertex
    public A3DVECTOR3 normal; // Normal vector of the vertex
    public uint       diffuse; // Diffuse color of the vertex

    public float u; // U coordinate for texture mapping
    public float v; // V coordinate for texture mapping
}

[Serializable]
// Represents a mesh similar to Unity's Mesh
public struct A3DLITMESH
{
    public string      m_TextureLink;   // Name or path of the texture linked to this mesh
    public A3DLMVERTEX[] m_pVerts;   // Array of vertices in this mesh
    public ushort[]      m_pIndices; // Array of indices for the mesh triangles

    public A3DVECTOR3[] m_pNormals; // Array of normals for lighting calculations
}

public static class A3DLitMeshExtensions
{
    /// <summary>
    /// Merges meshes with the same texture into a single mesh.
    /// </summary>
    /// <param name="a3dLitMeshs">Array of meshes to merge.</param>
    /// <param name="mergedMeshs">Output array of merged meshes.</param>
    /// <returns>True if the merge was successful, otherwise false.</returns>
    public static bool MergeMeshWithSameTexture(A3DLITMESH[] a3dLitMeshs, out A3DLITMESH[] mergedMeshs)
    {
        var textureToMeshMap = new Dictionary<string, List<A3DLITMESH>>();

        // Group meshes by texture
        foreach (var mesh in a3dLitMeshs)
        {
            if (!textureToMeshMap.ContainsKey(mesh.m_TextureLink))
            {
                textureToMeshMap[mesh.m_TextureLink] = new List<A3DLITMESH>();
            }
            textureToMeshMap[mesh.m_TextureLink].Add(mesh);
        }

        var mergedMeshList = new List<A3DLITMESH>();

        // Merge meshes with the same texture
        foreach (var kvp in textureToMeshMap)
        {
            var texture = kvp.Key;
            var meshList = kvp.Value;

            var mergedMesh = new A3DLITMESH
            {
                m_TextureLink = texture,
                m_pVerts = meshList.SelectMany(m => m.m_pVerts).ToArray(),
                m_pIndices = meshList.SelectMany(m => m.m_pIndices).ToArray(),
                m_pNormals = meshList.SelectMany(m => m.m_pNormals).ToArray()
            };

            mergedMeshList.Add(mergedMesh);
        }

        mergedMeshs = mergedMeshList.ToArray();
        return true;
    }

    /// <summary>
    /// Converts a custom mesh to a Unity Mesh.
    /// </summary>
    /// <param name="customMesh">The custom mesh to convert.</param>
    /// <returns>The converted Unity Mesh.</returns>
    public static Mesh ToMesh(A3DLITMESH customMesh)
    {
        var mesh = new Mesh();
        mesh.vertices = customMesh.m_pVerts.Select(v => new Vector3(v.pos.x, v.pos.y, v.pos.z)).ToArray();
        mesh.normals = customMesh.m_pNormals.Select(n => new Vector3(n.x, n.y, n.z)).ToArray();
        mesh.triangles = customMesh.m_pIndices.Select(i => (int)i).ToArray();
        mesh.uv = customMesh.m_pVerts.Select(v => new Vector2(v.u, v.v)).ToArray();
        return mesh;
    }
    
    /// <summary>
    /// Loads a texture from a file.
    /// </summary>
    /// <param name="filePath">The path to the texture file.</param>
    /// <param name="size">The size of the texture.</param>
    /// <returns>The loaded Texture2D object, or null if loading failed.</returns>
    public static Texture2D LoadTextureFromFile(string filePath, Vector2Int size)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found at {filePath}");
            return null;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
        if (ImageConversion.LoadImage(texture, fileData))
        {
            return texture;
        }
        else
        {
            Debug.LogError("Failed to load texture from file data");
            return null;
        }
    }
    
    /// <summary>
    /// Builds a GameObject from a custom mesh and material.
    /// </summary>
    /// <param name="a3Dlitmesh">The custom mesh to use.</param>
    /// <param name="material">The material to apply to the mesh.</param>
    /// <returns>The created GameObject.</returns>
    public static GameObject BuildGameObject(A3DLITMESH a3Dlitmesh, Material material)
    {
        Mesh mesh = ToMesh(a3Dlitmesh);
        
        // Create a new GameObject
        GameObject gameObject = new GameObject("MeshGameObject");

        // Add a MeshFilter component and set the mesh
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Add a MeshRenderer component
        var renderer =  gameObject.AddComponent<MeshRenderer>();
        renderer.material = material;

        Vector2Int size = new Vector2Int(256, 256);
        renderer.material.SetTexture("_BaseMap", LoadTextureFromFile(a3Dlitmesh.m_TextureLink, size));

        return gameObject;
    }
}