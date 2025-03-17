using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class SaveObjWindow : EditorWindow
{
    private GameObject selectedGameObject;
    private Mesh selectedMesh;
    private Texture2D selectedTexture;
    private string savePath = "Assets/ExportedMesh.obj";

    [MenuItem("Window/Save Obj")]
    public static void ShowWindow()
    {
        GetWindow<SaveObjWindow>("Save Obj");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save Mesh to .obj", EditorStyles.boldLabel);

        // Drop GameObject field
        selectedGameObject = (GameObject)EditorGUILayout.ObjectField("GameObject", selectedGameObject, typeof(GameObject), true);

        if (selectedGameObject != null)
        {
            MeshFilter meshFilter = selectedGameObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                selectedMesh = meshFilter.sharedMesh;
            }

            Renderer renderer = selectedGameObject.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                selectedTexture = renderer.sharedMaterial.mainTexture as Texture2D;
            }
        }

        // Display selected mesh and texture
        if (selectedMesh != null)
        {
            EditorGUILayout.ObjectField("Selected Mesh", selectedMesh, typeof(Mesh), false);
        }
        if (selectedTexture != null)
        {
            EditorGUILayout.ObjectField("Selected Texture", selectedTexture, typeof(Texture2D), false);
        }

        // Save Path
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        // Save button
        if (GUILayout.Button("Save to .obj"))
        {
            SaveMeshToObj();
        }
    }

    private void SaveMeshToObj()
    {
        if (selectedMesh == null)
        {
            Debug.LogError("No mesh selected.");
            return;
        }

        try
        {
            string objData = MeshToString(selectedMesh);
            if (string.IsNullOrEmpty(objData))
            {
                Debug.LogError("Failed to convert mesh to .obj format.");
                return;
            }

            using (StreamWriter sw = new StreamWriter(savePath))
            {
                sw.Write(objData);
            }

            if (selectedTexture != null)
            {
                string mtlPath = Path.ChangeExtension(savePath, ".mtl");
                SaveMaterialToMtl(mtlPath);
            }

            Debug.Log($"Mesh saved to {savePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save mesh to .obj file: {ex.Message}");
        }
    }

    private void SaveMaterialToMtl(string mtlPath)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(mtlPath))
            {
                sw.WriteLine("newmtl material_0");
                sw.WriteLine("Ka 1.000 1.000 1.000");
                sw.WriteLine("Kd 1.000 1.000 1.000");
                sw.WriteLine("Ks 0.000 0.000 0.000");
                sw.WriteLine("d 1.0");
                sw.WriteLine("illum 2");
                sw.WriteLine($"map_Kd {selectedTexture.name}.png");
            }

            string texturePath = Path.Combine(Path.GetDirectoryName(savePath), selectedTexture.name + ".png");
            File.WriteAllBytes(texturePath, selectedTexture.EncodeToPNG());

            Debug.Log($"Material saved to {mtlPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save material to .mtl file: {ex.Message}");
        }
    }

    private string MeshToString(Mesh mesh)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine($"mtllib {Path.GetFileNameWithoutExtension(savePath)}.mtl");
        sb.Append("g ").Append(mesh.name).Append("\n");
        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in mesh.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector2 v in mesh.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        sb.Append("\nusemtl material_0\n");
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            sb.Append("\n");
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
            }
        }
        return sb.ToString();
    }
}