using System;
using System.Collections.Generic;
using UnityEngine;

public partial class Content : ScriptableObject
{
    [Serializable]
    public class Addon
    {
        [SerializeField]
        string name;

        [SerializeField]
        public string description;

        [SerializeField]
        public string author;

        [SerializeField]
        List<Bundle> bundles;

        public Addon(string name, string description = "-", string author = "-")
        {
            this.name = name;
            this.bundles = new List<Bundle>();
            this.description = description;
            this.author = author;
        }

        public string Name
        {
            get { return name; }
            set
            {
                //fix name
                name = value;

                name = name.Replace('/', char.MinValue);
                name = name.Replace('\\', char.MinValue);
                name = name.Replace('.', char.MinValue);
                name = name.Replace('@', char.MinValue);
            }
        }

        public bool GetBundle(int index, out Bundle bundle)
        {
            bundle = default;

            if (index < 0 || index > bundles.Count)
            {
                return false;
            }

            bundle = bundles[index];

            return true;
        }

        public int BundlesCount() => bundles.Count;

        public bool HasBundle(string name, out Bundle result)
        {
            result = default;
            for (var i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];

                if (bundle.name == name)
                {
                    result = bundle;
                    return true;
                }
            }
            return false;
        }

        public bool AddBundle(Bundle bundle)
        {
            if (!HasBundle(bundle.name, out var result))
            {
                bundles.Add(bundle);
                return true;
            }
            return false;
        }

        public bool AddBundle(string name, out Bundle result)
        {
            if (!HasBundle(name, out result))
            {
                result = new Bundle(name);
                bundles.Add(result);
                return true;
            }
            return false;
        }

        public bool AddOrFindBundle(string name, out Bundle result)
        {
            if (HasBundle(name, out result))
            {
                return true;
            }
            else
            {
                result = new Bundle(name);
                bundles.Add(result);
            }
            return true;
        }

        public bool RemoveBundle(string name)
        {
            if (HasBundle(name, out var result))
            {
#if UNITY_EDITOR
                Get().CheckCompressionTypes();
#endif
                bundles.Remove(result);
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public class AddonsContainer
    {
        [SerializeField]
        public List<Addon> Addons = new List<Addon>();
    }

    [HideInInspector]
    [SerializeField]
    public AddonsContainer Container;

    public bool GetAddon(int index, out Addon addon)
    {
        addon = default;

        if (index < 0 || index > Container.Addons.Count)
        {
            return false;
        }

        addon = Container.Addons[index];

        return true;
    }

    public int AddonsCount() => Container.Addons.Count;

    public bool HasAddon(string name, out Addon result)
    {
        result = default;
        for (var i = 0; i < Container.Addons.Count; i++)
        {
            var addon = Container.Addons[i];

            if (addon.Name == name)
            {
                result = addon;
                return true;
            }
        }
        return false;
    }

    public bool AddAddon(string name, out Addon result)
    {
        if (!HasAddon(name, out result))
        {
            result = new Addon(name);
            Container.Addons.Add(result);
            return true;
        }
        return false;
    }

    public bool AddAddon(Addon addon)
    {
        if (!HasAddon(addon.Name, out var result))
        {
            return true;
        }
        return false;
    }

    public bool RemoveAddon(string name)
    {
        if (HasAddon(name, out var result))
        {
#if UNITY_EDITOR
            Get().CheckCompressionTypes();
#endif
            Container.Addons.Remove(result);
            return true;
        }

        return false;
    }
}
