using System;
using System.Collections.Generic;

public partial class Content
{
    [Serializable]
    public struct AddonInfo
    {
        public string name;
        public List<BundleInfo> bundles;
    }

    [Serializable]
    public struct BundleInfo
    {
        public string name;
        public uint crc;
        public string hash;
        public string[] dependencies;
    }

    [Serializable]
    public class AddonChain
    {
        public List<AddonInfo> Addons = new List<AddonInfo>();

        public bool HasAddon(string name, out AddonInfo result)
        {
            result = default;
            for (var i = 0; i < Addons.Count; i++)
            {
                var addon = Addons[i];

                if (addon.name == name)
                {
                    result = addon;
                    return true;
                }
            }
            return false;
        }

        public bool AddOrFindAddon(string name, out AddonInfo result)
        {
            if (HasAddon(name, out result))
            {
                return true;
            }
            else
            {
                result = new AddonInfo() { name = name, bundles = new List<BundleInfo>() };
                Addons.Add(result);
            }
            return true;
        }
    }
}
