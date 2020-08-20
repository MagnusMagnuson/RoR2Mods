using BepInEx;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BazaarPrinter
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.BazaarPrinter", "BazaarPrinter", "0.2.3")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class BazaarPrinter : BaseUnityPlugin
    {
        System.Random r = new System.Random();
        Dictionary<int, PrinterInfo> printerPosAndRot = new Dictionary<int, PrinterInfo>();
        String[] duplicators = new String[] {"iscDuplicator", "iscDuplicatorLarge", "iscDuplicatorMilitary", "iscDuplicatorWild" };

        public void Awake()
        {
            ModConfig.InitConfig(Config);

            On.RoR2.BazaarController.Awake += BazaarController_Start;

            //On.RoR2.SceneDirector.Start += (orig, self) =>
            //{
            //    if (NetworkServer.active)
            //    {
            //        if (SceneManager.GetActiveScene().name.Contains("bazaar"))
            //        {
            //            SpawnPrinters();
            //        }
            //        orig(self);
            //    }
                
            //};
        }

        private void BazaarController_Start(On.RoR2.BazaarController.orig_Awake orig, BazaarController self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                SpawnPrinters();
            }
        }

        private void FillPrinterInfo()
        {
            List<int> ordered = new List<int> { 0, 1, 2, 3 };
            List<int> random = new List<int>();

            while(ordered.Count > 0)
            {
                int randomIndex = r.Next(ordered.Count);
                random.Add(ordered[randomIndex]);
                ordered.RemoveAt(randomIndex);
            }

            PrinterInfo printerInfo = new PrinterInfo();
            printerInfo.position = new Vector3(-133.3f, -25.7f, -17.9f);
            printerInfo.rotation = new Vector3(0, 72.6f, 0);
            printerPosAndRot.Add(random[0], printerInfo);

            printerInfo = new PrinterInfo();
            printerInfo.position = new Vector3(-71.3f, -24.7f, -29.2f);
            printerInfo.rotation = new Vector3(0, 291f, 0);
            printerPosAndRot.Add(random[1], printerInfo);

            printerInfo = new PrinterInfo();
            printerInfo.position = new Vector3(-143.8f, -24.7f, -23.6f);
            printerInfo.rotation = new Vector3(0, 61f, 0);
            printerPosAndRot.Add(random[2], printerInfo);

            printerInfo = new PrinterInfo();
            printerInfo.position = new Vector3(-110.7f, -26.7f, -46.4f);
            printerInfo.rotation = new Vector3(0, 32.2f, 0);
            printerPosAndRot.Add(random[3], printerInfo);
        }

        private void SpawnPrinters()
        {
            printerPosAndRot.Clear();
            FillPrinterInfo();

            for (int i = 0; i < ModConfig.printerCount.Value; i++)
            {
                string randomDuplicator = GetRandomDuplicator();
                SpawnCard printerCard = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/"+randomDuplicator);
                DirectorPlacementRule placementRule = new DirectorPlacementRule();
                placementRule.placementMode = DirectorPlacementRule.PlacementMode.Direct;
                GameObject printerOne = printerCard.DoSpawn(printerPosAndRot[i].position, Quaternion.identity, new DirectorSpawnRequest(printerCard, placementRule, Run.instance.runRNG)).spawnedInstance;
                printerOne.transform.eulerAngles = printerPosAndRot[i].rotation;
            }

        }

        private string GetRandomDuplicator()
        {
            double total = ModConfig.tier1Chance.Value + ModConfig.tier2Chance.Value + ModConfig.tier3Chance.Value + ModConfig.tierBossChance.Value;

            double d = r.NextDouble()*total;
            if(d <= ModConfig.tier1Chance.Value)
            {
                return duplicators[0];
            } else if(d <= ModConfig.tier1Chance.Value+ModConfig.tier2Chance.Value)
            {
                return duplicators[1];
            } else if(d <= ModConfig.tier1Chance.Value+ModConfig.tier2Chance.Value+ModConfig.tier3Chance.Value)
            {
                return duplicators[2];
            } else
            {
                return duplicators[3];
            }

        }
    }

    internal class PrinterInfo
    {
        public Vector3 position;
        public Vector3 rotation;
    }
}
