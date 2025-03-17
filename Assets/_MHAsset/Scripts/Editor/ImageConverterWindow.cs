using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class ImageConverterWindow : EditorWindow
{
    private string originFolderPath = ""; // Đường dẫn thư mục gốc
    private string saveFolderPath = "";   // Đường dẫn thư mục lưu

    // Thêm mục menu để mở cửa sổ
    [MenuItem("Window/Image Converter")]
    public static void ShowWindow()
    {
        GetWindow<ImageConverterWindow>("Image Converter");
    }

    // Giao diện của Editor Window
    private void OnGUI()
    {
        GUILayout.Label("Image Converter", EditorStyles.boldLabel);

        // Nút chọn thư mục gốc
        if (GUILayout.Button("Choose Origin Folder"))
        {
            originFolderPath = EditorUtility.OpenFolderPanel("Select Origin Folder", "", "");
        }
        if (!string.IsNullOrEmpty(originFolderPath))
        {
            GUILayout.Label("Origin Folder: " + originFolderPath);
        }

        // Nút chọn thư mục lưu
        if (GUILayout.Button("Choose Save Folder"))
        {
            saveFolderPath = EditorUtility.OpenFolderPanel("Select Save Folder", "", "");
        }
        if (!string.IsNullOrEmpty(saveFolderPath))
        {
            GUILayout.Label("Save Folder: " + saveFolderPath);
        }

        // Nút chuyển đổi
        if (GUILayout.Button("Convert"))
        {
            if (string.IsNullOrEmpty(originFolderPath) || string.IsNullOrEmpty(saveFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Vui lòng chọn cả thư mục gốc và thư mục lưu.", "OK");
                return;
            }
            ConvertImages();
        }
    }

    // Hàm xử lý chuyển đổi ảnh
    private void ConvertImages()
    {
        // Lấy tất cả file DDS và TGA trong thư mục gốc và các thư mục con
        string[] files = Directory.GetFiles(originFolderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => file.ToLower().EndsWith(".dds") || file.ToLower().EndsWith(".tga")).ToArray();

        foreach (string file in files)
        {
            // Tính toán đường dẫn tương đối so với thư mục gốc
            string relativePath = file.Substring(originFolderPath.Length + 1);
            string saveFilePath = Path.Combine(saveFolderPath, relativePath);
            string saveDirectory = Path.GetDirectoryName(saveFilePath);

            // Tạo thư mục lưu nếu chưa tồn tại
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            // Đổi phần mở rộng sang PNG
            string pngFilePath = Path.ChangeExtension(saveFilePath, ".png");

            // Đọc file ảnh và chuyển đổi thành Texture2D
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData = File.ReadAllBytes(file);
            texture.LoadImage(fileData);

            // Lưu dưới dạng PNG
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(pngFilePath, pngData);

            Debug.Log("Đã chuyển đổi: " + file + " sang " + pngFilePath);
        }

        EditorUtility.DisplayDialog("Thành công", "Quá trình chuyển đổi đã hoàn tất.", "OK");
    }
}