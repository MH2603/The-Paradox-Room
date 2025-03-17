
using System;
using System.Text;
using UnityEngine;

public class TestConventMethod : MonoBehaviour
{
    public string path;
    public MeshRenderer meshRenderer;
    
    public void LoadTexture()
    {
        byte[] ddsBytes = System.IO.File.ReadAllBytes(path);
        Texture2D texture = LoadTextureDDS(ddsBytes);
        meshRenderer.material.SetTexture("_BaseMap", texture);
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
}