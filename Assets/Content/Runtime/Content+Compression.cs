#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System;

public partial class Content
{
    [Serializable]
    public struct BundleCompressionType
    {
        public string addon;
        public string name;
        public CompressionType type;
    }

    [HideInInspector]
    [SerializeField]
    List<BundleCompressionType> BundleCompressionTypes = new List<BundleCompressionType>();

    public void CheckCompressionTypes()
    {
        //remove compression types
        for (var i = 0; i < BundleCompressionTypes.Count; i++)
        {
            if (!HasAddon(BundleCompressionTypes[i].addon, out var result_addon))
            {
                if (!result_addon.HasBundle(BundleCompressionTypes[i].name, out var result_bundle))
                {
                    BundleCompressionTypes.RemoveAt(i);
                    return;
                }

                BundleCompressionTypes.RemoveAt(i);
            }
        }
    }

    public CompressionType GetCompressionType(string addon, string name)
    {
        for (var i = 0; i < BundleCompressionTypes.Count; i++)
        {
            var ct = BundleCompressionTypes[i];
            if (ct.addon == addon && ct.name == name)
            {
                return ct.type;
            }
        }

        return CompressionType.None;
    }

    public CompressionType SetCompressionType(
        string addon,
        string name,
        CompressionType type = CompressionType.None
    )
    {
        for (var i = 0; i < BundleCompressionTypes.Count; i++)
        {
            var ct = BundleCompressionTypes[i];
            if (ct.addon == addon && ct.name == name)
            {
                BundleCompressionTypes[i] = new BundleCompressionType()
                {
                    addon = addon,
                    name = name,
                    type = type
                };
                return BundleCompressionTypes[i].type;
            }
        }

        BundleCompressionTypes.Add(
            new BundleCompressionType()
            {
                addon = addon,
                name = name,
                type = type
            }
        );

        return type;
    }
}
#endif
