using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class Content : ScriptableObject
{
    [Serializable]
    public class Bundle
    {
        public string name;

        [SerializeField]
        private List<Asset> assets;

        public Bundle(string name)
        {
            this.name = name;
            this.assets = new List<Asset>();
        }

        public bool GetAsset(int index, out Asset asset)
        {
            asset = default;

            if (index < 0 || index >= assets.Count)
            {
                return false;
            }

            asset = assets[index];

            return true;
        }

        public int AssetsCount() => assets.Count;

        public void CopyAssets(List<Asset> dest) 
        { 
            dest.AddRange(assets);
        }

        public bool HasAsset(
            string value,
            out Asset result,
            AssetSearchType searchType = AssetSearchType.Name
        )
        {
            result = default;
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];

                switch (searchType)
                {
                    case AssetSearchType.Name:
                        if (asset.name == value)
                        {
                            result = asset;
                            return true;
                        }
                        break;

                    case AssetSearchType.GUID:
                        if (asset.guid == value)
                        {
                            result = asset;
                            return true;
                        }
                        break;

                    case AssetSearchType.Path:
                        if (asset.path == value)
                        {
                            result = asset;
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        public Status AddAsset(Object asset)
        {
            var status = Get().CheckAsset(asset, out var guid, out var path, out var mainType);

            if (status == Status.OK)
            {
                return AddAsset(
                    Path.GetFileNameWithoutExtension(path),
                    path,
                    guid,
                    mainType.Name,
                    mainType.BaseType.Name,
                    out var result
                );
            }

            return status;
        }

        public Status AddAsset(Asset asset)
        {
            return AddAsset(
                asset.name,
                asset.path,
                asset.guid,
                asset.type,
                asset.base_type,
                out var blob
            );
        }

        public Status AddAsset(
            string name,
            string path,
            string guid,
            string type,
            string base_type,
            out Asset result
        )
        {
            if (!HasAsset(guid, out result, AssetSearchType.GUID))
            {
                result = new Asset(name, path, guid, type, base_type);

                if (!result.IsScene() && Scenes() > 0)
                {
                    return Status.BundleHaveScenes;
                }
                else if (Scenes() == 0 && result.IsScene() && assets.Count > 0)
                {
                    return Status.BundleHaveAssets;
                }

                assets.Add(result);
                return Status.OK;
            }

            return Status.AlreadyContains;
        }

        public bool RemoveAsset(string value, AssetSearchType searchType = AssetSearchType.GUID)
        {
            if (HasAsset(value, out var result, searchType))
            {
                assets.Remove(result);
                return true;
            }

            return false;
        }

        public int Scenes()
        {
            var scenes = 0;
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].IsScene())
                {
                    scenes++;
                }
            }

            return scenes;
        }

        public long CalculateBundleSize()
        {
            long size = 0;
            try
            {
                for (var i = 0; i < AssetsCount(); i++)
                {
                    if (GetAsset(i, out var asset))
                    {
                        size += new FileInfo(asset.path).Length;
                    }
                }
            }
            catch { }

            return size;
        }
    }
}
