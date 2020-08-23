using BepInEx;
using BepInEx.Bootstrap;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModSyncForcer
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.ModSyncForcer", "ModSyncForcer", "0.1.0")]
    public class ModSyncForcer : BaseUnityPlugin
    {
        private void Start()
        {
            Debug.Log("ModSyncForcer: Adding all plugins to networkModList");
            NetworkModCompatibilityHelper.networkModList = new string[0];
            foreach (var info in Chainloader.PluginInfos)
            {
                if (info.Key.Equals("com.bepis.r2api"))
                    continue;
                string mod = info.Key + ";" + info.Value.Metadata.Version;
                NetworkModCompatibilityHelper.networkModList = NetworkModCompatibilityHelper.networkModList.Append(mod);
                Debug.Log(mod);
            }
        }
    }
}
 