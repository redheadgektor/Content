using System;
using System.Collections.Generic;

public partial class Content
{
    [Serializable]
    public struct ChainAddonInfo
    {
        public string name;
        public List<ChainBundleInfo> bundles;

        public bool HasBundle(string name, out ChainBundleInfo result)
        {
            result = default;
            for (var i = 0; i < bundles.Count; i++)
            {
                var addon = bundles[i];

                if (addon.name == name)
                {
                    result = addon;
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public struct ChainBundleInfo
    {
        public string name;
        public uint crc;
        public string hash;
        public string[] dependencies;
    }

    [Serializable]
    public class AddonChain
    {
        public List<ChainAddonInfo> Addons = new List<ChainAddonInfo>();

        public bool HasAddon(string name, out ChainAddonInfo result)
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

        public bool AddOrFindAddon(string name, out ChainAddonInfo result)
        {
            if (HasAddon(name, out result))
            {
                return true;
            }
            else
            {
                result = new ChainAddonInfo() { name = name, bundles = new List<ChainBundleInfo>() };
                Addons.Add(result);
            }
            return true;
        }
    }
}
