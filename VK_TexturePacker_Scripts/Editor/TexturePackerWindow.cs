using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TexturePackerWindow : EditorWindow
{
    private static TexturePackerWindow window;
    private string atlasOutputPath = "Assets/zTextureAtlas/TextureAtlas.png";
    private Object sourceInputPath;
    private bool isTrim = false;
    private TexturePacker texturePacker;
    private static Texture2D windowTexture;
    private static string maxTextureSize = "4096";
    private string[] guids;

    [MenuItem("VK_Tools/VK Texture Packer")]
    private static void Init()
    {
        GetWindow();
    }

    private static void GetWindow()
    {
        window = (TexturePackerWindow)EditorWindow.GetWindow<TexturePackerWindow>();

        window.titleContent.text = "Texture Packer";
        window.minSize = new Vector2(500, 165);
        window.maxSize = new Vector2(500, 165);
        window.BeginWindows();
        window.wantsMouseMove = true;
        windowTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        windowTexture.SetPixel(0, 0, new Color(0.68f, 0.68f, 0.68f));
        windowTexture.Apply();

        window.Show();
    }
    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), windowTexture, ScaleMode.StretchToFill);
        EditorGUILayout.Space();
        GUILayout.Label("Drag Textures Folder", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        sourceInputPath = EditorGUILayout.ObjectField(sourceInputPath, typeof(Object), true);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        isTrim = EditorGUILayout.Toggle("Trim Textures?", isTrim);
        EditorGUILayout.Space();
        maxTextureSize = EditorGUILayout.TextField("Max Texture Atlas Size", maxTextureSize);
        EditorGUILayout.Space();
        atlasOutputPath = EditorGUILayout.TextField("Texture Atlas Output Path", atlasOutputPath);
        EditorGUILayout.Space();
        if (GUILayout.Button("Create Texture Atlas"))
        {
            if (sourceInputPath != null)
            {
                bool isFolder = Tools.ValidateFolder(sourceInputPath);
                if (isFolder)
                {
                    string[] words = atlasOutputPath.Split('/');
                    string convertPath = null;
                    for (int i = 0; i < words.Length - 1; i++)
                    {
                        convertPath = Tools.JoinString(convertPath, words[i]);
                    }
                    string p = AssetDatabase.GetAssetPath(sourceInputPath);
                    guids = AssetDatabase.FindAssets("t: texture2D", new[] { p });
                    if (guids.Length > 0)
                    {
                        if (!Directory.Exists(convertPath))
                            Tools.CreateFolder(atlasOutputPath);

                        texturePacker = new TexturePacker();
                        texturePacker.isTrim = isTrim;
                        texturePacker.atlasPath = atlasOutputPath;
                        texturePacker.maxTextureSize = int.Parse(maxTextureSize);
                        texturePacker.atlasJsonPath = Tools.RecreatePath(atlasOutputPath, "Json", ".json");
                        texturePacker.atlasPlistPath = Tools.RecreatePath(atlasOutputPath, "Plist", ".plist");
                        texturePacker.inputPath = p;
                        texturePacker.CreateAtlas();
                    }
                    else
                    {
                        sourceInputPath = null;
                        EditorUtility.DisplayDialog("ERROR", "No Texture Found in Folder", "OKAY", null);
                    }
                }
                else
                {
                    sourceInputPath = null;
                    EditorUtility.DisplayDialog("ERROR", "Please Select Textures Folder", "OKAY", null);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("ERROR", "Please Select Textures Folder", "OKAY", null);
            }
        }
    }
}