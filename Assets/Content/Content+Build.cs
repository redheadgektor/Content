#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEditor.Build;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using BuildCompression = UnityEngine.BuildCompression;

public partial class Content : ScriptableObject, IPostprocessBuildWithReport
{
    class ContentBuildParameters : BundleBuildParameters
    {
        public Dictionary<string, CompressionType> PerBundleCompression { get; set; }

        public ContentBuildParameters(
            BuildTarget target,
            BuildTargetGroup group,
            string outputFolder
        )
            : base(target, group, outputFolder)
        {
            PerBundleCompression = new Dictionary<string, CompressionType>();
        }

        public override BuildCompression GetCompressionForIdentifier(string identifier)
        {
            if (PerBundleCompression.TryGetValue(identifier, out var value))
            {
                switch (value)
                {
                    case CompressionType.Lz4:
                        return BuildCompression.LZ4;

                    case CompressionType.Lzma:
                        return BuildCompression.LZMA;
                }
            }

            return BundleCompression;
        }
    }

    int IOrderedCallback.callbackOrder => 0;

    void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report) { }

    void Build()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog(
                "ContentDatabase - Building",
                "Unable to start archiving! There are unsaved scenes!",
                "Close"
            );
            return;
        }
        AssetDatabase.SaveAssets();

        var chain = new AddonChain();

        var build_params = new ContentBuildParameters(
            EditorUserBuildSettings.activeBuildTarget,
            BuildTargetGroup.Standalone,
            ContentFolder
        );
        build_params.BundleCompression = BuildCompression.Uncompressed;

        var total_addons_builded = 0;

        var total_bundles = 0;
        var total_bundles_builded = 0;

        var addons = new Dictionary<string, List<AssetBundleBuild>>();

        for (var i = 0; i < AddonsCount(); i++)
        {
            if (GetAddon(i, out var addon))
            {
                var addon_folder = Path.Combine(ContentFolder, addon.Name);
                var addon_bundles = new List<AssetBundleBuild>();

                var bundles_builded = 0;
                total_bundles += addon.BundlesCount();
                for (var j = 0; j < addon.BundlesCount(); j++)
                {
                    if (addon.GetBundle(j, out var bundle))
                    {
                        //ignore if no assets
                        if (bundle.AssetsCount() == 0)
                        {
                            Debug.LogWarning(
                                $"[{nameof(Content)}] Bundle {addon.Name}/{bundle.name} ignored! (No Assets!)"
                            );
                            continue;
                        }

                        var addon_bundle = $"{addon.Name}/{bundle.name}";
                        build_params.PerBundleCompression.Add(
                            addon_bundle,
                            GetCompressionType(addon.Name, bundle.name)
                        );
                        var abb = new AssetBundleBuild();
                        abb.assetBundleName = addon_bundle;
                        abb.assetNames = new string[bundle.AssetsCount()];
                        abb.addressableNames = new string[bundle.AssetsCount()];

                        for (var k = 0; k < bundle.AssetsCount(); k++)
                        {
                            if (bundle.GetAsset(k, out var asset))
                            {
                                abb.assetNames[k] = asset.path;
                                abb.addressableNames[k] = asset.name;
                            }
                        }

                        addon_bundles.Add(abb);
                        bundles_builded++;
                        total_bundles_builded++;
                    }
                }

                if (bundles_builded > 0)
                {
                    addons.TryAdd(addon.Name, addon_bundles);
                    total_addons_builded++;
                }
            }
        }

        var bundles = new List<AssetBundleBuild>();

        foreach (var k in addons)
        {
            bundles.AddRange(k.Value);
        }

        var buildContent = new BundleBuildContent(bundles);
        ReturnCode exitCode = ContentPipeline.BuildAssetBundles(
            build_params,
            buildContent,
            out var results
        );

        if (exitCode == ReturnCode.Success)
        {
            foreach (var report in results.BundleInfos)
            {
                var addon_name = report.Key.Split('/')[0];
                var bundle_name = report.Key.Substring(addon_name.Length);

                if (Get().HasAddon(addon_name, out var addon))
                {
                    if (chain.AddOrFindAddon(addon_name, out var chain_addon))
                    {
                        var bundle_info = new BundleInfo
                        {
                            name = bundle_name.Substring(1),
                            crc = report.Value.Crc,
                            hash = report.Value.Hash.ToString(),
                            dependencies = report.Value.Dependencies
                        };

                        //mark addons@bundle
                        for (var i = 0; i < bundle_info.dependencies.Length; i++)
                        {
                            var parts = bundle_info.dependencies[i].Split('/');
                            var temp_a_name = parts[0];
                            var temp_b_name = parts[1];

                            bundle_info.dependencies[i] = $"{temp_a_name}@{temp_b_name}";

                            if (parts.Length >= 3)
                            {
                                for (var k = 2; k < parts.Length; k++)
                                {
                                    bundle_info.dependencies[i] += $"/{parts[k]}";
                                }
                            }
                        }

                        chain_addon.bundles.Add(bundle_info);
                    }
                }
            }

            File.WriteAllText($"{ContentFolder}/{ChainFileName}", JsonUtility.ToJson(chain, true));
            SaveToJson(true);

            Debug.Log(
                $"[{nameof(Content)}] Build finished! [Addons: {total_addons_builded}/{AddonsCount()}] [Bundles: {total_bundles_builded}/{total_bundles}]"
            );
        }
        else
        {
            Debug.LogError($"[{nameof(Content)}] Build error -> {exitCode}");
        }
    }
}
#endif
