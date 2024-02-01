using UnityEngine;

public partial class Content
{
    public enum Status : int
    {
        OK,
        Failed,
        NotSupported,
        AlreadyContains,
        HasHideFlags,
        IsAssetFromScene,
        IsResourceAsset,
        IsEditorAsset,
        MissingOrNullAsset,
        BundleHaveScenes,
        BundleHaveAssets,
        Unknown = int.MaxValue
    }

    public enum AssetSearchType
    {
        Name,
        GUID,
        Path
    }
}
