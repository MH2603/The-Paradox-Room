using System.Collections.Generic;
using UnityEngine;

public class MergeMeshExam : MonoBehaviour
{
    public A3DLITMESH[] A3DLitMesh;
    public Material material;
    
    public void MergeMesh()
    {
        A3DLITMESH[] a3dLitMeshs;
        A3DLitMeshExtensions.MergeMeshWithSameTexture(A3DLitMesh, out a3dLitMeshs);

        // List<Mesh> mergedMeshList = new List<Mesh>();
        // foreach (var customMesh in mergedCustomMeshs)
        // {
        //     mergedMeshList.Add(CustomMeshExtensions.ToMesh(customMesh));
        // }

        foreach (var a3dLitMesh in a3dLitMeshs)
        {
            var gameObejct = A3DLitMeshExtensions.BuildGameObject(a3dLitMesh, material);
            gameObejct.transform.SetParent(transform);
            
            Vector3 pos = Vector3.zero;
            pos.x += Random.Range(-2f, 2f);
            pos.y += Random.Range(-2f, 2f);
            
            gameObejct.transform.localPosition = pos;
        }
    }
}

