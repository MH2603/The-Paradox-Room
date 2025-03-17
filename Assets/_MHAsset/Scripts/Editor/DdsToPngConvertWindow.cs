using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public class TextureConverterWindow : EditorWindow
{
    private string sourceFolderPath = "Assets";
    private string destinationFolderPath = "Assets";

    [MenuItem("Window/Texture Converter (DDS & TGA to PNG)")]
    public static void ShowWindow()
    {
        GetWindow<TextureConverterWindow>("Texture Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert DDS & TGA to PNG", EditorStyles.boldLabel);

        // Source folder path input field
        EditorGUILayout.BeginHorizontal();
        sourceFolderPath = EditorGUILayout.TextField("Source Folder Path", sourceFolderPath);
        if (GUILayout.Button("Choose Source Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select folder containing DDS/TGA", sourceFolderPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                sourceFolderPath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Destination folder path input field
        EditorGUILayout.BeginHorizontal();
        destinationFolderPath = EditorGUILayout.TextField("Destination Folder Path", destinationFolderPath);
        if (GUILayout.Button("Choose Destination Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select folder to save PNGs", destinationFolderPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                destinationFolderPath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Convert button
        if (GUILayout.Button("Convert"))
        {
            ConvertTexturesToPng();
        }
    }

    private void ConvertTexturesToPng()
    {
        // Check if source or destination paths are valid
        if (string.IsNullOrEmpty(sourceFolderPath) || string.IsNullOrEmpty(destinationFolderPath))
        {
            Debug.LogError("Source or destination folder path is empty.");
            return;
        }

        // Get all files in the source folder and subdirectories
        string[] textureFiles = Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories);
        foreach (string file in textureFiles)
        {
            string extension = Path.GetExtension(file).ToLower();
            if (extension != ".dds" && extension != ".tga") continue;

            string relativePath = file.Substring(sourceFolderPath.Length + 1);
            string destinationPath = Path.Combine(destinationFolderPath, Path.ChangeExtension(relativePath, ".png"));

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

            byte[] fileBytes = File.ReadAllBytes(file);
            Texture2D texture = null;

            if (extension == ".dds")
            {
                texture = LoadTextureDDS(fileBytes);
            }
            else if (extension == ".tga")
            {
                texture = LoadTextureTGA(fileBytes);
            }

            if (texture != null)
            {
                // Decompress and fix the texture (flip, correct colors), giả định texture gốc là sRGB
                Texture2D fixedTexture = DecompressAndFixTexture(texture, isSourceSRGB: true);

                // Lưu PNG
                byte[] pngBytes = fixedTexture.EncodeToPNG();
                File.WriteAllBytes(destinationPath, pngBytes);
                Debug.Log($"Converted {file} to {destinationPath}");
            }
            else
            {
                Debug.LogError($"Failed to load file: {file}");
            }
        }

        AssetDatabase.Refresh();
    }

    private Texture2D DecompressAndFixTexture(Texture2D source, bool isSourceSRGB = true)
    {
        // Kiểm tra Color Space của dự án
        bool isProjectLinear = PlayerSettings.colorSpace == ColorSpace.Linear;

        // Tạo RenderTexture để giải nén texture
        // Nếu texture gốc là sRGB, chúng ta cần đọc nó như sRGB để Unity tự động chuyển sang Linear
        RenderTextureReadWrite colorSpace = isSourceSRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.ARGB32,
            colorSpace);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        // Đọc dữ liệu từ RenderTexture
        // Nếu texture gốc là sRGB và dự án là Linear, Unity đã chuyển nó sang Linear
        Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false, true); // Linear texture
        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        // Tạo texture cuối cùng để lưu thành PNG
        // Đánh dấu texture là Linear (linear = true) để EncodeToPNG không áp dụng gamma correction
        Texture2D fixedTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
        Color[] pixels = readableTexture.GetPixels();
        Color[] fixedPixels = new Color[pixels.Length];

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                int sourceIndex = y * source.width + x;
                int flippedIndex = (source.height - 1 - y) * source.width + x; // Flip vertically
                Color color = pixels[sourceIndex];

                // Nếu texture gốc là sRGB và dự án là Linear, màu sắc đã được chuyển sang Linear.
                // Để bỏ qua gamma correction, chúng ta cần chuyển ngược về sRGB trước khi lưu.
                if (isSourceSRGB && isProjectLinear)
                {
                    color.r = Mathf.LinearToGammaSpace(color.r);
                    color.g = Mathf.LinearToGammaSpace(color.g);
                    color.b = Mathf.LinearToGammaSpace(color.b);
                }

                // Đảo BGR sang RGB
                fixedPixels[flippedIndex] = new Color(color.b, color.g, color.r, color.a);
            }
        }

        fixedTexture.SetPixels(fixedPixels);
        fixedTexture.Apply();

        return fixedTexture;
    }

    private Texture2D LoadTextureDDS(byte[] ddsBytes)
    {
        byte[] formatBytes = new byte[4];
        Array.Copy(ddsBytes, 84, formatBytes, 0, 4);
        string textureFormat = Encoding.UTF8.GetString(formatBytes);
        TextureFormat textureFormatEnum;

        switch (textureFormat)
        {
            case "DXT1":
                textureFormatEnum = TextureFormat.DXT1;
                break;
            case "DXT3":
                textureFormatEnum = TextureFormat.DXT5;
                Debug.LogWarning("Detected DXT3. Mapping to DXT5 for compatibility.");
                break;
            case "DXT5":
                textureFormatEnum = TextureFormat.DXT5;
                break;
            default:
                Debug.LogError($"Unsupported format: {textureFormat}. Defaulting to DXT5.");
                textureFormatEnum = TextureFormat.DXT5;
                break;
        }

        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
        {
            Debug.LogError("Invalid DDS file. Unable to read.");
            return Texture2D.whiteTexture;
        }

        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];

        int DDS_HEADER_SIZE = 128;
        byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        Texture2D texture = new Texture2D(width, height, textureFormatEnum, false);
        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();

        return texture;
    }

    private Texture2D LoadTextureTGA(byte[] tgaBytes)
    {
        byte idLength = tgaBytes[0];
        byte colorMapType = tgaBytes[1];
        byte imageType = tgaBytes[2];

        if (imageType != 2 && imageType != 10)
        {
            Debug.LogError("Only uncompressed RGB (type 2) or RLE-compressed RGB (type 10) TGA files are supported.");
            return Texture2D.whiteTexture;
        }

        int width = tgaBytes[13] * 256 + tgaBytes[12];
        int height = tgaBytes[15] * 256 + tgaBytes[14];
        byte pixelDepth = tgaBytes[16];

        if (pixelDepth != 24 && pixelDepth != 32)
        {
            Debug.LogError("Only 24-bit (RGB) or 32-bit (RGBA) TGA files are supported.");
            return Texture2D.whiteTexture;
        }

        int offset = 18 + idLength + (colorMapType != 0 ? tgaBytes[6] * 256 + tgaBytes[5] : 0);
        byte[] pixelData;

        if (imageType == 2)
        {
            pixelData = new byte[tgaBytes.Length - offset];
            Buffer.BlockCopy(tgaBytes, offset, pixelData, 0, pixelData.Length);
        }
        else
        {
            pixelData = DecodeRLETGA(tgaBytes, offset, width, height, pixelDepth);
            if (pixelData == null)
            {
                Debug.LogError("Failed to decode RLE-compressed TGA.");
                return Texture2D.whiteTexture;
            }
        }

        Texture2D texture = new Texture2D(width, height, pixelDepth == 32 ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
        texture.LoadRawTextureData(pixelData);
        texture.Apply();

        return texture;
    }

    private byte[] DecodeRLETGA(byte[] tgaBytes, int offset, int width, int height, byte pixelDepth)
    {
        int pixelSize = pixelDepth / 8;
        int totalPixels = width * height;
        byte[] pixelData = new byte[totalPixels * pixelSize];
        int pixelIndex = 0;
        int byteIndex = offset;

        while (pixelIndex < totalPixels && byteIndex < tgaBytes.Length)
        {
            byte header = tgaBytes[byteIndex++];
            int count = (header & 0x7F) + 1;

            if (pixelIndex + count > totalPixels)
            {
                Debug.LogError("RLE data exceeds image size.");
                return null;
            }

            if ((header & 0x80) == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Buffer.BlockCopy(tgaBytes, byteIndex, pixelData, pixelIndex * pixelSize, pixelSize);
                    byteIndex += pixelSize;
                    pixelIndex++;
                }
            }
            else
            {
                byte[] color = new byte[pixelSize];
                Buffer.BlockCopy(tgaBytes, byteIndex, color, 0, pixelSize);
                byteIndex += pixelSize;

                for (int i = 0; i < count; i++)
                {
                    Buffer.BlockCopy(color, 0, pixelData, pixelIndex * pixelSize, pixelSize);
                    pixelIndex++;
                }
            }
        }

        return pixelData;
    }
}