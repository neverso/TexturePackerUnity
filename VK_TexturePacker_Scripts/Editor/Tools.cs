using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class Tools  {

    private static Color32[] pixels;
    private static int width;
    private static int height;
    private static Rect trimmedRect;
    private static Vector4 border;
    public static TextureInfo[] texturesInfo;

    public static void AutoSlice(Texture2D atlas, Texture2D[] textures, Rect[] uvs, string atlasJsonPath, string atlasPlistPath)
    {
        string path = AssetDatabase.GetAssetPath(atlas);
        TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        TextureImporterSettings texImporterSettings = new TextureImporterSettings();

        texImporter.textureType = TextureImporterType.Sprite;
        texImporter.spriteImportMode = SpriteImportMode.Multiple;
        //  texImporterSettings.readable = true;
        SpriteMetaData[] spritesheetMeta = new SpriteMetaData[uvs.Length];
        texturesInfo = new TextureInfo[uvs.Length];
        
        for (int i = 0; i < uvs.Length; i++)
        {
            Rect currentRect = uvs[i];
            
            texturesInfo[i] = new TextureInfo();

            if (atlas.width == atlas.height)
            {
                
                currentRect.x *= atlas.width;
                currentRect.width *= atlas.width;
                currentRect.y *= atlas.height;
                currentRect.height *= atlas.height;
               
                if (texturesInfo[i] != null)
                {
                    texturesInfo[i].x = currentRect.x;
                    texturesInfo[i].y = currentRect.y;
                    texturesInfo[i].width = currentRect.width;
                    texturesInfo[i].height = currentRect.height;
                }
            }
            else
            {
                currentRect.x *= atlas.width * 2;
                currentRect.width *= atlas.width * 2;
                currentRect.y *= atlas.height * 2;
                currentRect.height *= atlas.height * 2;

                if (texturesInfo[i] != null)
                {
                    texturesInfo[i].x = currentRect.x;
                    texturesInfo[i].y = currentRect.y;
                    texturesInfo[i].width = currentRect.width;
                    texturesInfo[i].height = currentRect.height;
                }
            }
            SpriteMetaData currentMeta = new SpriteMetaData();
            currentMeta.rect = currentRect;
            currentMeta.name = textures[i].name;
            if (texturesInfo[i] != null)
                texturesInfo[i].name = currentMeta.name;

            currentMeta.alignment = (int)SpriteAlignment.Center;
            currentMeta.pivot = new Vector2(currentRect.width / 2, currentRect.height / 2);
            spritesheetMeta[i] = currentMeta;
        }

        //ToJson
        string textureInfoToJason = JsonHelper.ToJson<TextureInfo>(texturesInfo, true);
        File.WriteAllText(atlasJsonPath, textureInfoToJason);

        //ToPlist
        CreatePlist(texturesInfo, uvs, atlasPlistPath);

        texImporter.spritesheet = spritesheetMeta;
        texImporter.spritePixelsPerUnit = 1000f;
        texImporter.ReadTextureSettings(texImporterSettings);
        texImporter.SetTextureSettings(texImporterSettings);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

    }

    public static Texture2D Trim(Texture2D _sourceTex)
    {
        width = _sourceTex.width;
        height = _sourceTex.height;
        pixels = _sourceTex.GetPixels32();

        var xMin = 0;
        var xMax = width;
        var yMin = 0;
        var yMax = height;

        while (xMin < xMax) { for (var y = yMin; y < yMax; y++) { if (GetPixel(xMin, y).a > 0) goto Exit1; } xMin++; if (border.x > 0) border.x -= 1; }
    Exit1:
        while (xMax > xMin) { for (var y = yMin; y < yMax; y++) { if (GetPixel(xMax - 1, y).a > 0) goto Exit2; } xMax--; if (border.z > 0) border.z -= 1; }
    Exit2:
        while (yMin < yMax) { for (var x = xMin; x < xMax; x++) { if (GetPixel(x, yMin).a > 0) goto Exit3; } yMin++; if (border.y > 0) border.y -= 1; }
    Exit3:
        while (yMax > yMin) { for (var x = xMin; x < xMax; x++) { if (GetPixel(x, yMax - 1).a > 0) goto Exit4; } yMax--; if (border.w > 0) border.w -= 1; }
    Exit4:

        trimmedRect.xMin = xMin;
        trimmedRect.yMin = yMin;
        trimmedRect.xMax = xMax;
        trimmedRect.yMax = yMax;

        Color[] pix = _sourceTex.GetPixels((int)trimmedRect.x, (int)trimmedRect.y, (int)trimmedRect.width, (int)trimmedRect.height);
        Texture2D croppedTexture = new Texture2D((int)trimmedRect.width, (int)trimmedRect.height);
        croppedTexture.name = _sourceTex.name;
        croppedTexture.SetPixels(pix);
        croppedTexture.Apply();

        AssetDatabase.Refresh();
        return croppedTexture;
    }

    private static Color32 GetPixel(int x, int y)
    {
        return pixels[x + width * y];
    }

    public static Texture2D SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Texture2D t = (Texture2D)AssetDatabase.LoadAssetAtPath(_fullPath, typeof(Texture2D));
        AssetDatabase.Refresh();
        return t;
    }

    public static bool HasAlpha(Color[] aColors)
    {
        for (int i = 0; i < aColors.Length; i++)
            if (aColors[i].a < 1f)
                return true;
        return false;
    }

    public static bool HasAlpha(Texture2D aTex)
    {
        return HasAlpha(aTex.GetPixels());
    }

    public static void CreatePlist(TextureInfo[] texturesInfo, Rect[] rects, string plistOutPath)
    {
        var plist = new PlistDocument();
        plist.Create();
        PlistElementDict rootDict = plist.root;
        PlistElementArray plistArray = rootDict.CreateArray("texturesInfo");

        for (int i = 0; i < texturesInfo.Length; i++)
        {
            PlistElementDict dict = plistArray.AddDict();
            dict.SetString("name", texturesInfo[i].name);
            dict.SetInteger("x", (int)texturesInfo[i].x);
            dict.SetInteger("y", (int)texturesInfo[i].y);
            dict.SetInteger("width", (int)texturesInfo[i].width);
            dict.SetInteger("height", (int)texturesInfo[i].height);
        }
        File.WriteAllText(plistOutPath, plist.WriteToString());
        AssetDatabase.Refresh();
    }
    public static bool ValidateFolder(UnityEngine.Object path)
    {
        string filePath = AssetDatabase.GetAssetPath(path);
        FileAttributes attr = File.GetAttributes(filePath);
        return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
    }
    public static void CreateFolder(string _path)
    {
        string parentFolder = null;
        string[] words = _path.Split('/');
        for (int i = 0; i < words.Length; i++)
        {
            parentFolder = JoinString(parentFolder, words[i]);
            if ((i + 1) < words.Length - 1)
            {
                if (words[i + 1] != words[words.Length - 1])
                {
                    string guid = AssetDatabase.CreateFolder(parentFolder, words[i + 1]);
                }
            }
        }
    }
    public static string JoinString(string parent, string child)
    {
        if (parent != null)
        {
            parent = parent + "/" + child;
            return parent;
        }
        else
        {
            return child;
        }
    }
    public static string RecreatePath(string _atlasOutputPath, string _lastWord, string _exention)
    {
        string[] words = _atlasOutputPath.Split('/');
        string[] lastwords = words[words.Length - 1].Split('.');
        string parentFolder = null;
        for (int i = 0; i < words.Length - 1; i++)
        {
            parentFolder = JoinString(parentFolder, words[i]);
        }
        string outputPath = parentFolder + "/" + lastwords[0] + _lastWord + _exention;
        return outputPath;
    }

}


