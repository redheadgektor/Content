using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Content;
using static ContentLoadingManager;

public class ContentLoadingManager
{
    static ContentLoadingManager instance = null;
    public static ContentLoadingManager Get()
    {
        if (instance == null)
        {
            instance = new ContentLoadingManager();
        }

        return instance;
    }


#if UNITY_EDITOR
    [MenuItem("Content/Tests/Detect Addons")]
    static void Test_DetectAddons() => Get().DetectAddons();
#endif

    public class DetectedAddonFolder
    {
        public Addon addon;
        public ChainAddonInfo chain;
    }

    public Dictionary<string, DetectedAddonFolder> DetectedAddons = new Dictionary<string, DetectedAddonFolder>();

    public int DetectAddons()
    {
        if (DetectedAddons.Count != 0)
        {
            return DetectedAddons.Count;
        }

        var dirs = Directory.GetDirectories(ContentFolder);

        foreach (var dir in dirs)
        {
            var dirName = new DirectoryInfo(dir).Name;

            var files = Directory.GetFiles(dir, "*.json*", SearchOption.TopDirectoryOnly);

            var has_addon_info = false;
            if (files.Length == 2)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    var filename = files[i];
                    if (filename.Contains(ContentFileName, StringComparison.OrdinalIgnoreCase) || filename.Contains(ChainFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        has_addon_info = true;
                    }
                }
            }

            if (has_addon_info)
            {
                var contentFile = $"{ContentFolder}/{dirName}/{ContentFileName}";
                var chainFile = $"{ContentFolder}/{dirName}/{ChainFileName}";

                DetectedAddonFolder detected_addon = new DetectedAddonFolder();

                try
                {
                    detected_addon.addon = JsonUtility.FromJson<Addon>(File.ReadAllText(contentFile));
                    detected_addon.chain = JsonUtility.FromJson<ChainAddonInfo>(File.ReadAllText(chainFile));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{nameof(Content)}] Failed {dirName} -> {ex.Message}");
                }
                finally
                {
                    if (detected_addon.addon.BundlesCount() > 0)
                    {
                        DetectedAddons.Add(detected_addon.addon.Name, detected_addon);
                        Debug.Log($"[{nameof(Content)}] Detected addon {dir} -> Bundles: {detected_addon.addon.BundlesCount()}");
                    }
                }
            }
        }

        return DetectedAddons.Count;
    }

    public enum MountStatus
    {
        NotMounted,
        NotFound,
        Loading,
        LoadingError,
        Mounted,
        AlreadyMounted
    }

    public interface IMountedBundle
    {
        public string GetName();
        public string GetPath();
        public MountStatus GetMountStatus();
        public float GetProgress();
        public AssetBundle GetAssetBundle();
    }

    public class MountedBundle : IMountedBundle
    {
        public string BundleName;
        public string BundlePath;

        public MountStatus Status;
        public float Progress;
        public AssetBundle AssetBundle;

        public MountedBundle(string BundleName, string BundlePath)
        {
            this.BundleName = BundleName;
            this.BundlePath = BundlePath;
            Status = MountStatus.NotMounted;
            Progress = 0;
            AssetBundle = null;
        }

        /* INTERFACE */
        string IMountedBundle.GetName()
        {
            return BundleName;
        }

        string IMountedBundle.GetPath()
        {
            return BundlePath;
        }

        MountStatus IMountedBundle.GetMountStatus()
        {
            return Status;
        }

        float IMountedBundle.GetProgress()
        {
            return Progress;
        }

        AssetBundle IMountedBundle.GetAssetBundle()
        {
            return AssetBundle;
        }
    }

    public interface IMountedAddon
    {
        public string GetName();
        public int Bundles();
        public bool GetBundle(int i, out IMountedBundle bundle);
        public float GetProgress();

        public int MountedBundles();
        public int MissingBundles();
    }

    public class MountedAddon : IMountedAddon
    {
        public string AddonName { get; private set; }
        public List<MountedBundle> Bundles { get; private set; }

        public float Progress;

        public int MountedBundles;
        public int MissingBundles;

        public MountedAddon(string addonName)
        {
            AddonName = addonName;
            Bundles = new List<MountedBundle>();
            Progress = 0;
            MountedBundles = 0;
            MissingBundles = 0;
        }


        /* INTERFACE */
        string IMountedAddon.GetName() => AddonName;

        int IMountedAddon.Bundles() => Bundles.Count;

        bool IMountedAddon.GetBundle(int i, out IMountedBundle bundle)
        {
            bundle = Bundles[i];
            return true;
        }

        float IMountedAddon.GetProgress()
        {
            return Progress;
        }

        int IMountedAddon.MountedBundles() => MountedBundles;
        int IMountedAddon.MissingBundles() => MissingBundles;
    }

    private List<IMountedAddon> Addons = new List<IMountedAddon>();

    public int AddonsCount() => Addons.Count;

    public bool GetAddon(int i, out IMountedAddon addon)
    {
        addon = Addons[i];
        return true;
    }

    public bool GetAddon(string addonName, out IMountedAddon addon)
    {
        addon = default;
        for(var i = 0; i < AddonsCount(); i++)
        {
            if(GetAddon(i, out var addon_result))
            {
                if (addon_result.GetName() == addonName)
                {
                    addon = addon_result;
                    return true;
                }
            }
        }

        return false;
    }

    /* Mounting */
    private async void InternalMountAddon(MountedAddon mountedAddon, Action<MountStatus> onResult)
    {
        if (GetAddon(mountedAddon.AddonName, out var exist_addon))
        {
            onResult?.Invoke(MountStatus.AlreadyMounted);
            return;
        }
        if (DetectedAddons.TryGetValue(mountedAddon.AddonName, out var detectedAddon))
        {
            Addons.Add(mountedAddon);
            for (int i = 0; i < detectedAddon.addon.BundlesCount(); i++)
            {
                if (detectedAddon.addon.GetBundle(i, out var bundle))
                {
                    var bundle_path = $"{ContentFolder}/{detectedAddon.addon.Name}/{bundle.name}";

                    var mountedBundle = new MountedBundle(bundle.name, bundle_path);
                    mountedAddon.Bundles.Add(mountedBundle);
                }
            }

            for(var i = 0; i < mountedAddon.Bundles.Count;i++)
            {
                var bundle = mountedAddon.Bundles[i];

                if (File.Exists(bundle.BundlePath))
                {
                    var bundleCreateRequest = AssetBundle.LoadFromFileAsync(bundle.BundlePath);

                    while (!bundleCreateRequest.isDone)
                    {
                        bundle.Status = MountStatus.Loading;
                        bundle.Progress = (bundleCreateRequest.progress / .9f) * 100f;
                        Debug.Log($"{bundle.BundleName} - {(bundleCreateRequest.progress / .9f) * 100f:f1}");
                        onResult?.Invoke(bundle.Status);
                        await Task.Yield();
                    }

                    if (bundleCreateRequest.assetBundle)
                    {
                        bundle.AssetBundle = bundleCreateRequest.assetBundle;
                        bundle.Status = MountStatus.Mounted;
                        bundle.Progress = 100f;
                        mountedAddon.MountedBundles++;
                        onResult?.Invoke(bundle.Status);
                    }
                    else
                    {
                        bundle.Status = MountStatus.LoadingError;
                        mountedAddon.MissingBundles++;
                        onResult?.Invoke(bundle.Status);
                    }
                }
                else
                {
                    bundle.Status = MountStatus.NotFound;
                    mountedAddon.MissingBundles++;
                    onResult?.Invoke(bundle.Status);
                }

                mountedAddon.Progress = i / (float)mountedAddon.Bundles.Count;
            }

            return;
        }

        onResult?.Invoke(MountStatus.NotFound);
    }

    public MountStatus MountAddon(string addonName, out IMountedAddon out_addon)
    {
        var addon = new MountedAddon(addonName);
        out_addon = addon;

        MountStatus result = MountStatus.NotMounted;
        InternalMountAddon(addon, delegate(MountStatus s) 
        {
            result = s;
        });

        return result;
    }

    public void UnmountAddon(string addonName, bool unloadAssets = false)
    {
        if (GetAddon(addonName, out var exist_addon))
        {
            for (var i = 0; i < exist_addon.Bundles(); i++)
            {
                if (exist_addon.GetBundle(i, out var bundle))
                {
                    if (bundle.GetAssetBundle())
                    {
                        bundle.GetAssetBundle().Unload(unloadAssets);
                    }
                }
            }

            Addons.Remove(exist_addon);
        }
    }

    public void UnmountAddon(IMountedAddon addon, bool unloadAssets = false)
    {
        UnmountAddon(addon.GetName(), unloadAssets);
    }
}
