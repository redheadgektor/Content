using System;
using System.IO;
using System.Text;
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

#if UNITY_EDITOR
    [MenuItem("Content/Load/JSON")]
#endif
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

    [MenuItem("Content/Report/Text")]
    static void TxtReport()
    {
        using (var sb = new StringWriter())
        {
            long total_sz = 0;
            long total_assets = 0;

            foreach (var addon in Get().Container.Addons)
            {
                for (var i = 0; i < addon.BundlesCount(); i++)
                {
                    if (addon.GetBundle(i, out var bundle))
                    {
                        total_sz += bundle.CalculateBundleSize();
                        total_assets += bundle.AssetsCount();
                    }
                }
            }

            var total_sz_s = string.Empty;

            if (total_sz < 1024)
            {
                total_sz_s = $"{total_sz} B";
            }

            if (total_sz >= 1024)
            {
                total_sz_s = $"{total_sz / 1024} KB";
            }

            if (total_sz >= 1024 * 1024)
            {
                total_sz_s = $"{total_sz / 1024f / 1024f:f2} MB";
            }

            if (total_sz >= 1024 * 1024 * 1024)
            {
                total_sz_s = $"{total_sz / 1024f / 1024f / 1024f:f2} GB";
            }

            sb.WriteLine($"//Report at {DateTime.Now}");
            sb.WriteLine(
                $"//Report contains data only about included assets. Their actual size and number may not match, because during build assets dependencies are automatically included in archives"
            );
            sb.WriteLine("//Use Notepad++ with C or C++ syntax for better perception");
            sb.WriteLine(
                $"Addons: {Get().AddonsCount()} | Size: {total_sz_s} | Assets: {total_assets}"
            );
            foreach (var addon in Get().Container.Addons)
            {
                sb.WriteLine($"Addon: [{addon.Name}] | Bundles: {addon.BundlesCount()}");
                sb.WriteLine("{");
                for (var i = 0; i < addon.BundlesCount(); i++)
                {
                    if (addon.GetBundle(i, out var bundle))
                    {
                        var sz = bundle.CalculateBundleSize();
                        var sz_s = string.Empty;

                        if (sz < 1024)
                        {
                            sz_s = $"{sz} B (Assets: {bundle.AssetsCount()})";
                        }

                        if (sz >= 1024)
                        {
                            sz_s = $"{sz / 1024} KB (Assets: {bundle.AssetsCount()})";
                        }

                        if (sz >= 1024 * 1024)
                        {
                            sz_s = $"{sz / 1024f / 1024f:f2} MB (Assets: {bundle.AssetsCount()})";
                        }

                        if (sz >= 1024 * 1024 * 1024)
                        {
                            sz_s =
                                $"{sz / 1024f / 1024f / 1024f:f2} GB (Assets: {bundle.AssetsCount()})";
                        }

                        sb.WriteLine($"\nBundle: [{bundle.name}] | Size: {sz_s}");
                        sb.WriteLine("{");
                        for (var j = 0; j < bundle.AssetsCount(); j++)
                        {
                            if (bundle.GetAsset(j, out var asset))
                            {
                                sb.WriteLine($" -  [{asset.base_type}:{asset.type}] {asset.name}");
                                sb.WriteLine($" -  [guid: {asset.guid}] -> {asset.path}");
                            }
                        }
                        sb.WriteLine("}");
                    }
                }
                sb.WriteLine("}");
            }

            var p = $"{ContentFolder}/report.log";
            File.WriteAllText(p, sb.ToString());
            Debug.Log($"[{nameof(Content)}] Report: {p}");
        }
    }

    [MenuItem("Content/Start Build")]
    static void StartBuild()
    {
        Get().BuildAll();
    }
#endif
}
