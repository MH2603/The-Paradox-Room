using UnityEditor;
using UnityEngine;
using UnityEditor.Formats.Fbx.Exporter;
using System.IO;

public class SaveFbxWindow : EditorWindow
{
    private GameObject selectedGameObject;
    private string saveFolderPath = "Assets";

    [MenuItem("Window/Save Fbx")]
    public static void ShowWindow()
    {
        GetWindow<SaveFbxWindow>("Save Fbx");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save GameObject to .fbx", EditorStyles.boldLabel);

        // Drop GameObject field
        selectedGameObject = (GameObject)EditorGUILayout.ObjectField("GameObject", selectedGameObject, typeof(GameObject), true);

        // Save Folder Path
        EditorGUILayout.BeginHorizontal();
        saveFolderPath = EditorGUILayout.TextField("Save Folder Path", saveFolderPath);
        if (GUILayout.Button("Choose Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", saveFolderPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                saveFolderPath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Save button
        if (GUILayout.Button("Save to .fbx"))
        {
            SaveGameObjectToFbx();
        }
    }

    private void SaveGameObjectToFbx()
    {
        if (selectedGameObject == null)
        {
            Debug.LogError("No GameObject selected.");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Save .fbx file", saveFolderPath, selectedGameObject.name + ".fbx", "fbx");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // Đảm bảo texture được lưu và gán lại vào material trước khi xuất
        PrepareTexturesForExport(selectedGameObject);

        // Cấu hình export options để nhúng texture
        var exportOptions = new ExportModelOptions();
        exportOptions.ExportFormat = ExportFormat.Binary; // Định dạng nhị phân hỗ trợ nhúng texture
        exportOptions.EmbedTextures = true; // Bật nhúng texture vào FBX

        // Xuất GameObject thành FBX
        ModelExporter.ExportObject(path, selectedGameObject, exportOptions);
        Debug.Log($"GameObject saved to {path}");

        // Refresh AssetDatabase để Unity nhận diện các texture mới
        AssetDatabase.Refresh();
    }

    private void PrepareTexturesForExport(GameObject gameObject)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            Debug.LogWarning("GameObject has no renderer or material.");
            return;
        }

        Material material = renderer.sharedMaterial;
        Texture2D customTexture = material.mainTexture as Texture2D;

        if (customTexture == null)
        {
            Debug.LogWarning("No main texture found on the material.");
            return;
        }

        // Đảm bảo texture có thể đọc được (readable)
        if (!customTexture.isReadable)
        {
            Debug.LogWarning("Texture is not readable. Attempting to make it readable...");
            customTexture = MakeTextureReadable(customTexture);
            if (customTexture == null)
            {
                Debug.LogError("Failed to make texture readable. Cannot embed texture into FBX.");
                return;
            }
            // Gán lại customTexture đã readable vào material
            material.mainTexture = customTexture;
        }

        // Lưu texture thành file .png trong thư mục dự án
        string textureFileName = string.IsNullOrEmpty(customTexture.name) ? "CustomTexture" : customTexture.name;
        string texturePath = Path.Combine(saveFolderPath, textureFileName + ".png");

        // Ghi đè file nếu đã tồn tại để đảm bảo texture mới nhất được sử dụng
        if (File.Exists(texturePath))
        {
            File.Delete(texturePath);
        }
        File.WriteAllBytes(texturePath, customTexture.EncodeToPNG());
        Debug.Log($"Texture saved to {texturePath}");

        // Refresh AssetDatabase để Unity nhận diện file texture mới
        AssetDatabase.Refresh();

        // Tải lại texture từ file và gán vào material
        string relativeTexturePath = FileUtil.GetProjectRelativePath(texturePath);
        Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(saveFolderPath + "/" + textureFileName + ".png");
        if (importedTexture != null)
        {
            material.mainTexture = importedTexture; // Gán texture từ file vào material
            Debug.Log($"Material updated with texture at {relativeTexturePath}");
        }
        else
        {
            Debug.LogError("Failed to load texture asset after saving. Using original custom texture.");
            // Nếu không tải được texture từ file, giữ nguyên customTexture
            material.mainTexture = customTexture;
        }
    }

    private Texture2D MakeTextureReadable(Texture2D originalTexture)
    {
        // Tạo một bản sao texture có thể đọc được
        RenderTexture renderTex = RenderTexture.GetTemporary(
            originalTexture.width,
            originalTexture.height,
            0,
            RenderTextureFormat.ARGB32
        );

        Graphics.Blit(originalTexture, renderTex);
        RenderTexture.active = renderTex;

        Texture2D readableTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.ARGB32, false);
        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableTexture;
    }
}