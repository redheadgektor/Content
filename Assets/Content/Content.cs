using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class Content : ScriptableObject
{
    public const string FolderName = "Content";
    public const string FileName = "content.json";
    public const string ChainFileName = "chain.json";

    public static string ContentFolder
    {
        get { return Path.Combine(Environment.CurrentDirectory, FolderName); }
    }
    public static string ContentFile
    {
        get { return Path.Combine(Environment.CurrentDirectory, FolderName, FileName); }
    }

    static Content Instance;

    public static Content Get()
    {
#if UNITY_EDITOR
        LoadOrCreate();
        if (!Instance.UseAssetDatabase && EditorApplication.isPlaying)
        {
            LoadJSON();
        }
#endif
#if !UNITY_EDITOR
        Instance = CreateInstance<Content>();
        LoadJSON();
#endif
        return Instance;
    }

    static void LoadJSON()
    {
        if (File.Exists(ContentFile))
        {
            try
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(ContentFile), Get().Container);
                Debug.Log($"[{nameof(Content)}] Parsed {Instance.Container.Addons.Count} addons");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[{nameof(Content)}] Error while parsing {ContentFile} -> {ex.Message}"
                );
            }
        }
    }

    public static async void SaveToJson(bool pretty = true)
    {
        if (!Directory.Exists(ContentFolder))
        {
            Directory.CreateDirectory(ContentFolder);
        }
        await File.WriteAllTextAsync(ContentFile, JsonUtility.ToJson(Get().Container, pretty));
    }

#if UNITY_EDITOR
    public const string EditorFolder = "Assets/";
    public static string EditorContentFile { get; private set; } =
        Path.Combine(EditorFolder, "Content.asset");

    public bool UseAssetDatabase = false;

    [MenuItem("Content/LoadOrCreate")]
    static void Editor_LoadOrCreate()
    {
        LoadOrCreate();
    }

    private static Content LoadOrCreate()
    {
        if (!Instance)
        {
            Instance = AssetDatabase.LoadAssetAtPath<Content>(EditorContentFile);

            if (!Instance)
            {
                Instance = CreateInstance<Content>();

                if (!Directory.Exists(EditorContentFile))
                {
                    Directory.CreateDirectory(EditorFolder);
                }

                AssetDatabase.CreateAsset(Instance, EditorContentFile);
                AssetDatabase.SaveAssetIfDirty(Instance);
            }
        }

        return Instance;
    }

    [MenuItem("Content/Save (Json)/Non-Pretty")]
    static void SaveToJson0() => SaveToJson(false);

    [MenuItem("Content/Save (Json)/Pretty")]
    static void SaveToJson1() => SaveToJson(true);

    public static void SaveToAsset()
    {
        EditorUtility.SetDirty(Get());
        AssetDatabase.SaveAssetIfDirty(Get());
    }

    [MenuItem("Content/Start Build")]
    static void StartBuild() 
    {
        Get().Build();
    }
#endif
}
