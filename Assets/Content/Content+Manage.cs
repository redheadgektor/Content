using UnityEngine;

public partial class Content : ScriptableObject
{
    /* Bundles */
    public bool HasBundle(string name, out Bundle bundle)
    {
        return HasBundle(name, out var addon, out bundle);
    }

    public bool HasBundle(string name, out Addon addon, out Bundle bundle)
    {
        addon = default;
        bundle = default;

        for (var i = 0; i < AddonsCount(); i++)
        {
            if (GetAddon(i, out var addon_result))
            {
                for (var j = 0; j < addon_result.BundlesCount(); j++)
                {
                    if (addon_result.GetBundle(j, out var bundle_result))
                    {
                        if (bundle_result.name == name)
                        {
                            bundle = bundle_result;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool HasAsset(
        string value,
        out Asset asset,
        AssetSearchType searchType = AssetSearchType.Name
    )
    {
        return HasAsset(value, out var addon, out var bundle, out asset, searchType);
    }

    public bool HasAsset(
        string value,
        out Bundle bundle,
        out Asset asset,
        AssetSearchType searchType = AssetSearchType.Name
    )
    {
        return HasAsset(value, out var addon, out bundle, out asset, searchType);
    }

    public bool HasAsset(
        string value,
        out Addon addon,
        out Bundle bundle,
        out Asset asset,
        AssetSearchType searchType = AssetSearchType.Name
    )
    {
        addon = default;
        bundle = default;
        asset = default;

        for (var i = 0; i < AddonsCount(); i++)
        {
            if (GetAddon(i, out var addon_result))
            {
                for (var j = 0; j < addon_result.BundlesCount(); j++)
                {
                    if (addon_result.GetBundle(j, out var bundle_result))
                    {
                        for (var k = 0; k < bundle_result.AssetsCount(); k++)
                        {
                            if (bundle_result.GetAsset(k, out var asset_result))
                            {
                                switch (searchType)
                                {
                                    case AssetSearchType.Name:
                                        if (asset_result.name == value)
                                        {
                                            addon = addon_result;
                                            bundle = bundle_result;
                                            asset = asset_result;
                                            return true;
                                        }
                                        break;

                                    case AssetSearchType.GUID:
                                        if (asset_result.guid == value)
                                        {
                                            addon = addon_result;
                                            bundle = bundle_result;
                                            asset = asset_result;
                                            return true;
                                        }
                                        break;

                                    case AssetSearchType.Path:
                                        if (asset_result.path == value)
                                        {
                                            addon = addon_result;
                                            bundle = bundle_result;
                                            asset = asset_result;
                                            return true;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public Status MoveAsset(Asset target_asset, Bundle target_bundle)
    {
        if (!target_asset.IsScene() && target_bundle.Scenes() > 0)
        {
            return Status.BundleHaveScenes;
        }
        if (target_bundle.Scenes() == 0 && target_asset.IsScene() && target_bundle.AssetsCount() > 0)
        {
            return Status.BundleHaveAssets;
        }
        if (HasAsset(target_asset.guid, out var result_bundle, out var asset, AssetSearchType.GUID))
        {
            if (result_bundle.RemoveAsset(target_asset.guid, AssetSearchType.GUID))
            {
                return target_bundle.AddAsset(target_asset);
            }
        }

        return Status.Failed;
    }

    public bool RemoveAsset(string value, AssetSearchType searchType = AssetSearchType.GUID)
    {
        if (HasAsset(value, out var result_bundle, out var asset, searchType))
        {
            return result_bundle.RemoveAsset(value, searchType);
        }

        return false;
    }

    public bool MoveBundle(Bundle target_bundle, Addon target_addon)
    {
        if (HasBundle(target_bundle.name, out var result_addon, out var result_bundle))
        {
            if (result_addon.RemoveBundle(target_bundle.name))
            {
                if (target_addon.AddBundle(target_bundle))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
