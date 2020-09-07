using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TexturePacker {

    public Texture2D[] sourceImages;
    public bool isTrim;
    public string inputPath;
    public string atlasPath;
    public string atlasJsonPath;
    public string atlasPlistPath;
    public Rect[] rects;
    public int maxTextureSize = 4096;

    private string[] guids;
    private string[] texturePaths;

    public void CreateAtlas()
    {
        guids = AssetDatabase.FindAssets("t: texture2D", new[] { inputPath });
        if (guids.Length > 0) {
            Texture2D temp = new Texture2D(maxTextureSize, maxTextureSize);
            temp = Tools.SaveTextureAsPNG(temp, atlasPath);
            sourceImages = new Texture2D[guids.Length];
            texturePaths = new string[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter texImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                if (texImporter != null)
                {
                    texImporter.GetPlatformTextureSettings("Andriod");
                    texImporter.GetPlatformTextureSettings("iPhone");
                    texImporter.textureType = TextureImporterType.Default;
                    texImporter.isReadable = true;
                    TextureImporterSettings texImporterSettings = new TextureImporterSettings();
                    TextureImporterPlatformSettings textureImporterPlatformSettings = new TextureImporterPlatformSettings();
                    textureImporterPlatformSettings.format = TextureImporterFormat.RGBA32;
                    texImporter.SetPlatformTextureSettings(textureImporterPlatformSettings);
                    texImporter.ReadTextureSettings(texImporterSettings);
                    texImporter.SetTextureSettings(texImporterSettings);
                    AssetDatabase.ImportAsset(texturePath);
                    AssetDatabase.Refresh();
                }
                
                texturePaths[i] = texturePath;
                sourceImages[i] = (Texture2D)AssetDatabase.LoadAssetAtPath(texImporter.assetPath, typeof(Texture2D));
                if (isTrim)
                {
                    if (Tools.HasAlpha(sourceImages[i]))
                    {
                        sourceImages[i] = Tools.Trim(sourceImages[i]);
                    }
                }
            }
            Texture2D textureAtlas = new Texture2D(maxTextureSize, maxTextureSize, TextureFormat.RGBA32, false);
            rects = textureAtlas.PackTextures(sourceImages, 2, maxTextureSize, false);

            Texture2D savedAtlas = new Texture2D(maxTextureSize, maxTextureSize);
            if (textureAtlas != null)
                savedAtlas = Tools.SaveTextureAsPNG(textureAtlas, atlasPath);

            Tools.AutoSlice(savedAtlas, sourceImages, rects, atlasJsonPath,atlasPlistPath);
            AssetDatabase.Refresh();

            for (int i = 0; i < texturePaths.Length; i++)
            {
                TextureImporter texImporter = (TextureImporter)AssetImporter.GetAtPath(texturePaths[i]);
                if (texImporter != null)
                {
                    texImporter.textureType = TextureImporterType.Sprite;
                    texImporter.isReadable = false;
                    AssetDatabase.ImportAsset(texturePaths[i]);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
    
}
