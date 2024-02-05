using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class Content : ScriptableObject
{
    public static string StatusToString(Status status)
    {
        switch (status)
        {
            case Status.OK:
                return "OK";

            case Status.Failed:
                return "Failed";

            case Status.NotSupported:
                return "Not Supported";

            case Status.AlreadyContains:
                return "Already contains";

            case Status.HasHideFlags:
                return "Has hide flags";

            case Status.IsAssetFromScene:
                return "Asset from Scene";

            case Status.IsResourceAsset:
                return "Resource asset";

            case Status.IsEditorAsset:
                return "Editor asset";

            case Status.MissingOrNullAsset:
                return "Missing/Invalid asset";

            case Status.BundleHaveScenes:
                return "Bundle have scenes";

            case Status.BundleHaveAssets:
                return "Bundle have assets";

            case Status.Unknown:
            default:
                return "Unknown";
        }
    }

    public int PutInDependenciesOfAsset(string asset_guid, Addon addon)
    {
#if UNITY_EDITOR
        var assets = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(asset_guid));

        var mainAssetName = Path.Combine(AssetDatabase.GUIDToAssetPath(asset_guid));

        foreach (var path in assets)
        {
            if (addon.AddOrFindBundle($"{mainAssetName}_Shared", out var bundle))
            {
                bundle.AddAsset(AssetDatabase.LoadMainAssetAtPath(path));
            }
        }
#endif
        return 0;
    }

    public int PutInDependenciesOfBundle(string bundle_name, Addon addon)
    {
#if UNITY_EDITOR
        if (addon.HasBundle(bundle_name, out var bundle))
        {
            void _find_and_add(string asset_guid)
            {
                var assets = AssetDatabase.GetDependencies(
                    AssetDatabase.GUIDToAssetPath(asset_guid)
                );

                var mainAssetName = Path.Combine(AssetDatabase.GUIDToAssetPath(asset_guid));

                foreach (var path in assets)
                {
                    if (addon.AddOrFindBundle($"{bundle.name}_Shared", out var new_bundle))
                    {
                        new_bundle.AddAsset(AssetDatabase.LoadMainAssetAtPath(path));
                    }
                }
            }

            for (var i = 0; i < bundle.AssetsCount(); i++)
            {
                if (bundle.GetAsset(i, out var asset)) 
                { 
                    _find_and_add(asset.guid);
                }
            }
        }
#endif
        return 0;
    }

    public Status CheckAsset(Object asset, out string guid, out string path, out Type mainType)
    {
        guid = string.Empty;
        path = string.Empty;
        mainType = null;

#if UNITY_EDITOR
        if (!asset)
        {
            return Status.MissingOrNullAsset;
        }

        if (
            asset.hideFlags.HasFlag(HideFlags.DontSave)
            || asset.hideFlags.HasFlag(HideFlags.HideAndDontSave)
            || asset.hideFlags.HasFlag(HideFlags.DontSaveInBuild)
            || asset.hideFlags.HasFlag(HideFlags.DontSaveInEditor)
        )
        {
            return Status.HasHideFlags;
        }

        if (asset is GameObject)
        {
            if ((asset as GameObject).scene.IsValid())
            {
                return Status.IsAssetFromScene;
            }
        }

        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out guid, out long localId))
        {
            if (HasAsset(guid, out var result_asset, AssetSearchType.GUID))
            {
                return Status.AlreadyContains;
            }
            path = AssetDatabase.GetAssetPath(asset);
            mainType = AssetDatabase.GetMainAssetTypeAtPath(path);

            if (!IsSupportedType(mainType))
            {
                return Status.NotSupported;
            }

            if (IsResource(path))
            {
                return Status.IsResourceAsset;
            }

            if (IsEditor(path))
            {
                return Status.IsEditorAsset;
            }

            return Status.OK;
        }
#endif

        return Status.Unknown;
    }
}
