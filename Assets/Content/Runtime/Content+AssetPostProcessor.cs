#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

class ContentPostProcessor : AssetPostprocessor
{
    static bool MoveAssets(string asset_guid, string oldPath, string newPath)
    {
        if (Content.Get().HasAsset(asset_guid, out var result_asset, Content.AssetSearchType.GUID))
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(newPath);

            var status = Content.Get().CheckAsset(asset, out var guid, out var path, out var blob);
            Debug.Log($"{status} {oldPath} -> {newPath}");
            if (status == Content.Status.OK || status == Content.Status.AlreadyContains)
            {
                result_asset.path = newPath;
                return true;
            }
            else
            {
                DeleteAssets(asset_guid);
            }
        }

        return false;
    }

    static bool DeleteAssets(string asset_guid)
    {
        if (
            Content
                .Get()
                .HasAsset(
                    asset_guid,
                    out var result_bundle,
                    out var result_asset,
                    Content.AssetSearchType.GUID
                )
        )
        {
            if (result_bundle.RemoveAsset(asset_guid))
            {
                return true;
            }
        }

        return false;
    }

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths
    )
    {
        var moved = 0;
        for (int i = 0; i < movedAssets.Length; i++)
        {
            if (
                MoveAssets(
                    AssetDatabase.AssetPathToGUID(movedAssets[i]),
                    movedFromAssetPaths[i],
                    movedAssets[i]
                )
            )
            {
                moved++;
            }
        }

        var deleted = 0;
        for (int i = 0; i < deletedAssets.Length; i++)
        {
            if (DeleteAssets(AssetDatabase.AssetPathToGUID(deletedAssets[i])))
            {
                deleted++;
            }
        }

        if (moved != 0 || deleted != 0) 
        { 
            Content.SaveToAsset();
        }
    }
}
#endif
