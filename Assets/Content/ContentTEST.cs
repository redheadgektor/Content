using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Content;

public class ContentTEST : MonoBehaviour
{

   void Start() 
   {
        DontDestroyOnLoad(this.gameObject);
   }

    Rect detectedAddons = new Rect(0,0,450,300);
    Rect mountedAddons = new Rect(0, 0, 450, 300);

    private void OnGUI()
    {
        foreach(var bundle in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (GUILayout.Button($"Unload {bundle.name} (Assets: {bundle.GetAllAssetNames().Length}) (Scenes: {bundle.GetAllScenePaths().Length})"))
            {
                bundle.Unload(true);
            }
        }
        if (GUILayout.Button("Detect Addons"))
        {
            int addons = ContentLoadingManager.Get().DetectAddons();
            Debug.Log($"[Test] Detected "+addons+" addons");
        }

        detectedAddons = GUI.Window(0, detectedAddons, detectedAddonsWin, "Detected Addons");
        mountedAddons = GUI.Window(1, mountedAddons, mountedAddonsWin, "Mounted Addons");
    }

    private void detectedAddonsWin(int id)
    {
        foreach(var addon in ContentLoadingManager.Get().DetectedAddons)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box($"{addon.Key} by {addon.Value.addon.author} -> {addon.Value.addon.BundlesCount()}");
            if (GUILayout.Button("Mount"))
            {
                Debug.Log($"{addon.Key} -> {ContentLoadingManager.Get().MountAddon(addon.Key, out var result)}");
            }
            if (GUILayout.Button("Unmount (unloadAssets)"))
            {
                ContentLoadingManager.Get().UnmountAddon(addon.Key, true);
            }
            if (GUILayout.Button("Unmount"))
            {
                ContentLoadingManager.Get().UnmountAddon(addon.Key);
            }
            GUILayout.EndHorizontal();
        }
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    Vector2 scroll;

    private void mountedAddonsWin(int id)
    {
        scroll = GUILayout.BeginScrollView(scroll);
        for(var i = 0; i < ContentLoadingManager.Get().AddonsCount(); i++)
        {
            if (ContentLoadingManager.Get().GetAddon(i, out var addon))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Box(addon.GetName());
                GUILayout.BeginVertical();
                GUILayout.Box("Bundles: " + addon.Bundles());
                for (var a = 0; a < addon.Bundles(); a++)
                {
                    if (addon.GetBundle(a, out var bundle))
                    {
                        GUILayout.Box($"{bundle.GetName()} {(bundle.GetMountStatus() == ContentLoadingManager.MountStatus.Loading ? $"{bundle.GetProgress()}%" : $"{bundle.GetMountStatus()}")} {(bundle.GetAssetBundle() ? bundle.GetAssetBundle().isStreamedSceneAssetBundle ? "StreamedScene" : string.Empty : string.Empty)}");

                        if (bundle.GetAssetBundle() && bundle.GetAssetBundle().isStreamedSceneAssetBundle)
                        {
                            foreach(var scene in bundle.GetAssetBundle().GetAllScenePaths())
                            {
                                if(GUILayout.Button($"Load {scene}"))
                                {
                                    SceneManager.LoadScene(scene);
                                }
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndScrollView();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
}
