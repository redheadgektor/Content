using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ContentTEST : MonoBehaviour
{
    string text;

    List<AssetBundle> loadedBundles = new List<AssetBundle>();

    void OnGUI()
    {
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Asset ");
        text = GUILayout.TextField(text);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Find"))
        {
            if (Content.Get().HasAsset(text, out var addon, out var bundle, out var asset))
            {
                if (Content.Get().GetChain().HasAddon(addon.Name, out var result_addon))
                {
                    if (result_addon.HasBundle(bundle.name, out var result_chain_bundle))
                    {
                        var main_bundle_path = Path.Combine(Content.ContentFolder, addon.Name, result_chain_bundle.name);
                        Debug.Log(
                            $"{main_bundle_path} | Deps: {result_chain_bundle.dependencies.Length}"
                        );

                        foreach(var dp in result_chain_bundle.dependencies)
                        {
                            var parts = dp.Split('@');

                            var dep_bundle_path = Path.Combine(Content.ContentFolder, parts[0], parts[1]);
                            Debug.Log($"{dep_bundle_path}");
                        }
                    }
                }
            }
        }
    }
}
