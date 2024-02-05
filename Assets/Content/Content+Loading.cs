using System;
using System.IO;
using UnityEngine;

public partial class Content
{

    /* Read from file of bundle load chain */
    private AddonChain Chain;

    public AddonChain GetChain()
    {
        if (Chain == null)
        {
            Chain = new AddonChain();
        }

        try
        {
            JsonUtility.FromJsonOverwrite(File.ReadAllText($"{ContentFolder}/{ChainFileName}"), Chain);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{nameof(Content)}] GetChain failed! {ex.Message}");
        }

        return Chain;
    }
}
