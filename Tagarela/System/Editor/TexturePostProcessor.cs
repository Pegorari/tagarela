using UnityEngine;
using UnityEditor;

public class TexturePostProcessor : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        if (assetPath.Contains("Tagarela/System/Editor/Images"))
        {
            TextureImporter importer = assetImporter as TextureImporter;
            importer.textureType = TextureImporterType.GUI;
        }

    }
}