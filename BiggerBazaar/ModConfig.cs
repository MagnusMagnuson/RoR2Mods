using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BiggerBazaar
{
    class ModConfig
    {
        public static ConfigEntry<int> lunarCoinWorth;
        public static ConfigEntry<int> maxLunarExchanges;
        public static ConfigEntry<int> chestCostType;
        public static ConfigEntry<float> tier1Cost;
        public static ConfigEntry<float> tier2Cost;
        public static ConfigEntry<float> tier3Cost;
        public static ConfigEntry<float> tierBossCost;
        public static ConfigEntry<float> tierLunarCost;
        public static ConfigEntry<float> tierEquipmentCost;
        public static ConfigEntry<float> tierLunarEquipmentCost;
        public static ConfigEntry<float> tier1Rarity;
        public static ConfigEntry<float> tier2Rarity;
        public static ConfigEntry<float> tier3Rarity;
        public static ConfigEntry<float> tierBossRarity;
        public static ConfigEntry<float> tierLunarRarity;
        public static ConfigEntry<float> tierEquipmentRarity;
        public static ConfigEntry<float> tierLunarEquipmentRarity;
        public static ConfigEntry<int> maxChestPurchasesTier1;
        public static ConfigEntry<int> maxChestPurchasesTier2;
        public static ConfigEntry<int> maxChestPurchasesTier3;
        public static ConfigEntry<int> maxChestPurchasesTierBoss;
        public static ConfigEntry<int> maxChestPurchasesTierLunar;
        public static ConfigEntry<int> maxChestPurchasesTierEquipment;
        public static ConfigEntry<int> maxChestPurchasesTierLunarEquipment;
        public static ConfigEntry<int> maxPlayerPurchases;
        public static ConfigEntry<int> maxPlayerPurchasesTier1;
        public static ConfigEntry<int> maxPlayerPurchasesTier2;
        public static ConfigEntry<int> maxPlayerPurchasesTier3;
        public static ConfigEntry<int> maxPlayerPurchasesTierBoss;
        public static ConfigEntry<int> maxPlayerPurchasesTierLunar;
        public static ConfigEntry<int> maxPlayerPurchasesTierEquipment;
        public static ConfigEntry<int> maxPlayerPurchasesTierLunarEquipment;
        public static ConfigEntry<bool> BroadcastShopSettings;
        public static ConfigEntry<bool> ShareSuiteItemSharingEnabled;
        public static ConfigEntry<bool> ShareSuiteTotalPurchaseSharing;
        public static ConfigEntry<bool> sacrificeArtifactAllowChests;
        //public static ConfigEntry<bool> ShareSuiteMoneySharingEnabled;
        public static bool infiniteLunarExchanges = false;
        public static bool isShareSuiteLoaded;
        //private static BaseUnityPlugin ShareSuite = null;
        //public static ConfigEntry<bool> experimentalPriceScaling;
        //public static ConfigEntry<float> experimentalPriceScalingMinPercent;
        //public static ConfigEntry<float> experimentalPriceScalingMaxPercent;

        //original Bazaar
        public static ConfigEntry<bool> modifyOriginalBazaar;
        public static ConfigEntry<int> seerLunarCost;
        public static ConfigEntry<int> lunarBudLunarCost;

        public static ConfigEntry<int> tier1CostLunar;
        public static ConfigEntry<int> tier2CostLunar;
        public static ConfigEntry<int> tier3CostLunar;
        public static ConfigEntry<int> tierBossCostLunar;
        public static ConfigEntry<int> tierLunarCostLunar;
        public static ConfigEntry<int> tierEquipmentCostLunar;
        public static ConfigEntry<int> tierLunarEquipmentCostLunar;

        //other
        public static ConfigEntry<bool> disableTransferMoney;
        public static ConfigEntry<bool> nextLevelChestPriceScaling;

        public static ConfigEntry<int> configNumber;

        public static BaseUnityPlugin ShareSuite;
        public static Dictionary<PickupTier, TierUnitConfig> tierConfigs = new Dictionary<PickupTier, TierUnitConfig>();

        private static readonly int CurrentVersionNumber = 2;


        public static void InitConfig(ConfigFile config)
        {
            chestCostType = config.Bind(
            "0. Config",
            "chestCostType",
            0,
            new ConfigDescription("Set the chests cost type to either gold (0) or lunar coins (1).")
            );

            BroadcastShopSettings = config.Bind(
            "0. Config",
            "BroadcastShopSettings",
            true,
            new ConfigDescription("Lets everyone know how many things they can purchase on bazaar entry.")
            );

            lunarCoinWorth = config.Bind(
            "0. Config",
            "lunarCoinWorth",
            50,
            new ConfigDescription("Conversion rate for Lunar Coins to Money. Conversion rate automatically scales with difficulty, meaning if you set this at the base price of a medium chest (50), you will always get enough money to buy a tier 2 item. (Reference: Small chest = 25, Medium Chest = 50, Legendary Chest = 400)\nIf you change the item tier prices, you might have to adjust this value.")
            );

            maxLunarExchanges = config.Bind(
            "0. Config",
            "maxLunarExchanges",
            3,
            new ConfigDescription("Sets how many Lunar Coin exchanges for money are allowed per player. A value of 0 will never allow an exchange. A value of -1 allows infinite exchanges.")
            );
            if (maxLunarExchanges.Value == -1)
                infiniteLunarExchanges = true;



            tier1Cost = config.Bind(
            "1. TierCost",
            "tier1ChestCostMulti",
            1f,
            new ConfigDescription("Set the base cost multiplier for tier 1 items (white). This scales automatically with difficulty and player amount.\nA value of 1 means that these items will cost as much as a small chest in the prior stage (the one you opened the shop portal in) \n(Default value: 1)")
            );
            //tier1Cost = tier1CostConf.Value;

            tier2Cost = config.Bind(
            "1. TierCost",
            "tier2ChestCostMulti",
            2f,
            new ConfigDescription("Set the base cost multiplier for tier 2 items (green). This scales automatically with difficulty and player amount.\nA value of 2 means that these items will cost as much as a medium chest in the prior stage (the one you opened the shop portal in) \n(Default value: 2)")
            );
            //tier2Cost = tier2CostConf.Value;

            tier3Cost = config.Bind(
            "1. TierCost",
            "tier3ChestCostMulti",
            16f,
            new ConfigDescription("Set the base cost multiplier for tier 3 items (red). This scales automatically with difficulty and player amount.\nA value of 16 means that these items will cost as much as a legendary chest in the prior stage (the one you opened the shop portal in) \n(Default value: 16)")
            );

            tierBossCost = config.Bind(
            "1. TierCost",
            "tierBossCostMulti",
            24f,
            new ConfigDescription("Set the base cost multiplier for boss tier items (yellow). This scales automatically with difficulty and player amount. (Default value: 24, untested)")
            );

            tierLunarCost = config.Bind(
            "1. TierCost",
            "tierLunarCostMulti",
            16f,
            new ConfigDescription("Set the base cost multiplier for lunar tier items (blue). This scales automatically with difficulty and player amount. (Default value: 16, untested)")
            );

            tierEquipmentCost = config.Bind(
            "1. TierCost",
            "tierEquipmentCostMulti",
            8f,
            new ConfigDescription("Set the base cost multiplier for equipment tier items (orange). This scales automatically with difficulty and player amount. (Default value: 8, untested)")
            );

            tierLunarEquipmentCost = config.Bind(
            "1. TierCost",
            "tierLunarEquipmentCostMulti",
            16f,
            new ConfigDescription("Set the base cost multiplier for lunar equipment tier items (blue). This scales automatically with difficulty and player amount. (Default value: 16, untested)")
            );

            tier1Rarity = config.Bind(
            "2. TierRarity",
            "tier1Rarity",
            0.55f,
            new ConfigDescription("Set the rarity of items/equipment for each tier. Higher value compared to the other tiers means that tier is more likely to appear (weighted random).\nDoes not need to add up to 1. Values are relative.\nSet a tier rarity to 0 if you don't want any items of that tier to appear.\n(Default values: 0.55, 0.3, 0.15, 0, 0, 0, 0)\n")
            );
            //tier1Rarity = tier1RarityConf.Value;

            tier2Rarity = config.Bind(
            "2. TierRarity",
            "tier2Rarity",
            0.3f,
            new ConfigDescription("")
            );
            //tier2Rarity = tier2RarityConf.Value;

            tier3Rarity = config.Bind(
            "2. TierRarity",
            "tier3Rarity",
            0.15f,
            new ConfigDescription("")
            );
            //tier3Rarity = tier3RarityConf.Value;

            tierBossRarity = config.Bind(
            "2. TierRarity",
            "tierBossRarity",
            0f,
            new ConfigDescription("")
            );

            tierLunarRarity = config.Bind(
            "2. TierRarity",
            "tierLunarRarity",
            0f,
            new ConfigDescription("")
            );

            tierEquipmentRarity = config.Bind(
            "2. TierRarity",
            "tierEquipmentRarity",
            0f,
            new ConfigDescription("")
            );

            tierLunarEquipmentRarity = config.Bind(
            "2. TierRarity",
            "tierLunarEquipmentRarity",
            0f,
            new ConfigDescription("")
            );




            maxChestPurchasesTier1 = config.Bind(
            "3. ChestPurchaseLimits",
            "maxChestPurchasesTier1",
            3,
            new ConfigDescription("How often you can buy an item before a chest of this tier runs out/becomes unavailable.\nSet -1 for infinite purchases.\n\nTier 1")
            );

            maxChestPurchasesTier2 = config.Bind(
            "3. ChestPurchaseLimits",
            "maxChestPurchasesTier2",
            2,
            new ConfigDescription("Tier 2")
            );

            maxChestPurchasesTier3 = config.Bind(
            "3. ChestPurchaseLimits",
            "maxChestPurchasesTier3",
            1,
            new ConfigDescription("Tier 3")
            );

            maxChestPurchasesTierBoss = config.Bind(
            "3. ChestPurchaseLimits",
            "maxChestPurchasesTierBoss",
            1,
            new ConfigDescription("Tier Boss")
            );

            maxChestPurchasesTierLunar = config.Bind(
            "3. ChestPurchaseLimits",
            "maxChestPurchasesTierLunar",
            1,
            new ConfigDescription("Tier Lunar")
            );

            maxChestPurchasesTierEquipment = config.Bind(
            "3. ChestPurchaseLimits",
            "maxChestPurchasesTierEquipment",
            1,
            new ConfigDescription("Tier Equipment")
            );

            maxChestPurchasesTierLunarEquipment = config.Bind(
            "3. ChestPurchaseLimits",
            "maxChestPurchasesTierLunarEquipment",
            1,
            new ConfigDescription("Tier Lunar Equipment")
            );




            maxPlayerPurchases = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchases",
            3,
            new ConfigDescription("This sets how many total purchases a player is allowed to make per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTier1 = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchasesTier1",
            -1,
            new ConfigDescription("This sets how many tier 1 (white) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTier2 = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchasesTier2",
            -1,
            new ConfigDescription("This sets how many tier 2 (green) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTier3 = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchasesTier3",
            -1,
            new ConfigDescription("This sets how many tier 3 (red) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTierBoss = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchasesTierBoss",
            -1,
            new ConfigDescription("This sets how many boss tier (yellow) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTierLunar = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchasesTierLunar",
            -1,
            new ConfigDescription("This sets how many lunar tier (blue) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTierEquipment = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchasesTierEquipment",
            -1,
            new ConfigDescription("This sets how many equipment (orange) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTierLunarEquipment = config.Bind(
            "4. PlayerPurchaseLimits",
            "maxPlayerPurchasesTierLunarEquipment",
            -1,
            new ConfigDescription("This sets how many lunar equipment (blue) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );






            tier1CostLunar = config.Bind(
            "5. LunarModePricing",
            "tier1ChestCostLunar",
            1,
            new ConfigDescription("Sets how many lunar coins a tier 1 (white) item costs")
            );

            tier2CostLunar = config.Bind(
            "5. LunarModePricing",
            "tier2ChestCostLunar",
            2,
            new ConfigDescription("Sets how many lunar coins a tier 2 (green) item costs")
            );

            tier3CostLunar = config.Bind(
            "5. LunarModePricing",
            "tier3ChestCostLunar",
            3,
            new ConfigDescription("Sets how many lunar coins a tier 3 (red) item costs")
            );

            tierBossCostLunar = config.Bind(
            "5. LunarModePricing",
            "tierBossCostLunar",
            5,
            new ConfigDescription("Sets how many lunar coins a boss (yellow) item costs")
            );

            tierLunarCostLunar = config.Bind(
            "5. LunarModePricing",
            "tierLunarCostLunar",
            3,
            new ConfigDescription("Sets how many lunar coins a lunar (blue) item costs")
            );

            tierEquipmentCostLunar = config.Bind(
            "5. LunarModePricing",
            "tierEquipmentCostLunar",
            3,
            new ConfigDescription("Sets how many lunar coins equipment (orange) costs")
            );

            tierLunarEquipmentCostLunar = config.Bind(
            "5. LunarModePricing",
            "tierLunarEquipmentCostLunar",
            3,
            new ConfigDescription("Sets how many lunar coins lunar equipment (blue) costs")
            );

            //experimentalPriceScaling = config.Bind(
            //"PriceScaling",
            //"experimentalPriceScaling",
            //false,
            //new ConfigDescription("Experimental scaling for chest prices. This prevents that the player(s) can buy all items, by recalculating chest prices if players money together exceeds a certain percentage of the shops worth. The adjustment is random within certain bounds (e.g. the players money together allows for theoretically 30% of the shops worth to be bought). The players' individual money remains unchanged.")
            //);

            //experimentalPriceScalingMinPercent = config.Bind(
            //"PriceScaling",
            //"experimentalPriceScalingMinPercent",
            //0.3f,
            //new ConfigDescription("Lower random bound for shops inventory worth able to be bought. Numbers should be between 0 (for 0%) and 1 (for 100%). \nA value of 0.3 means that the most expensive the shop can become is that all of the players' money together is only enough to buy 30% of the shop's inventory.")
            //);

            //experimentalPriceScalingMaxPercent = config.Bind(
            //"PriceScaling",
            //"experimentalPriceScalingMaxPercent",
            //0.7f,
            //new ConfigDescription("Upper random bound for shops inventory worth able to be bought and also value at which a recalculation happens. Numbers should be between 0 (for 0%) and 1 (for 100%). \nA value of 0.7 means that the cheapest the shop can become is that all of the players' money together is enough to buy 70% of the shop's inventory.")
            //);

            ShareSuiteItemSharingEnabled = config.Bind(
            "6. ShareSuite",
            "ShareSuiteItemSharingEnabled",
            false,
            new ConfigDescription("This option is only relevant if you are using ShareSuite, otherwise ignore. \nSetting this to false, will disable item sharing for Bigger Bazaar items and put items directly into the buyers inventory, instead of having it pop out from the chest and drop on the floor and effectively sharing it with everyone on pickup.\nNo longer automatically turned on if money sharing is enabled in ShareSuite")
            );

            ShareSuiteTotalPurchaseSharing = config.Bind(
            "6. ShareSuite",
            "ShareSuiteTotalPurchaseSharing",
            true,
            new ConfigDescription("This option is only relevant if you are using ShareSuite, otherwise ignore. \nSets the total amount of available purchases (maxPlayerPurchases in this config) to count for the whole party, rather than individual players.\nThis makes sense if you're sharing money and items.")
            );

            isShareSuiteLoaded = IsShareSuiteLoaded();

            sacrificeArtifactAllowChests = config.Bind(
            "7. Sacrifice Artifact",
            "sacrificeArtifactAllowChests",
            true,
            new ConfigDescription("Prevents the Sacrifice Artifact from removing the chests in the bazaar.")
            );

            modifyOriginalBazaar = config.Bind(
            "8. Original Bazaar Modifications",
            "modifyOriginalBazaar",
            false,
            new ConfigDescription("If enabled, you can change the lunar cost of seer stations and lunar pods.")
            );

            seerLunarCost = config.Bind(
            "8. Original Bazaar Modifications",
            "seerLunarCost",
            3,
            new ConfigDescription("Set the lunar cost for seer stations.")
            );

            lunarBudLunarCost = config.Bind(
            "8. Original Bazaar Modifications",
            "lunarBudLunarCost",
            2,
            new ConfigDescription("Set the lunar cost for lunar buds.")
            );

            disableTransferMoney = config.Bind(
            "9. Other",
            "disableTransferMoney",
            false,
            new ConfigDescription("If set to true, no money will be transfered to the bazaar.")
            );

            nextLevelChestPriceScaling = config.Bind(
            "9. Other",
            "nextLevelChestPriceScaling",
            false,
            new ConfigDescription("If set to true, chest prices will be calculated based on how much things cost in the next stage, rather than the previous.")
            );

            configNumber = config.Bind(
            "z_config version",
            "config version",
            0,
            new ConfigDescription("No need to touch this")
            );

            //experimentalPriceScaling.Value = false;
            CreateTierConfigs();


            if(configNumber.Value == 1)
            {
                UpdateTierCostToMulti(config);
            }
            if (configNumber.Value < ModConfig.CurrentVersionNumber)
            {
                DeleteOldEntries(config);
            }
        }

        
        private static void UpdateTierCostToMulti(ConfigFile config)
        {
            System.Reflection.PropertyInfo OrphanedEntriesProperty = typeof(ConfigFile).GetProperty("OrphanedEntries", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)OrphanedEntriesProperty.GetValue(config);
            List<KeyValuePair<ConfigDefinition, string>> entriesToKeep = new List<KeyValuePair<ConfigDefinition, string>>();
            foreach (var oe in orphanedEntries)
            {
                if (oe.Key.Section == "1. TierCost")
                {
                    switch (oe.Key.Key)
                    {
                        case "tier1ChestCost":
                            tier1Cost.Value = int.Parse(oe.Value) / 25;
                            break;
                        case "tier2ChestCost":
                            tier2Cost.Value = int.Parse(oe.Value) / 25;
                            break;
                        case "tier3ChestCost":
                            tier3Cost.Value = int.Parse(oe.Value) / 25;
                            break;
                        case "tierBossCost":
                            tierBossCost.Value = int.Parse(oe.Value) / 25;
                            break;
                        case "tierLunarCost":
                            tierLunarCost.Value = int.Parse(oe.Value) / 25;
                            break;
                        case "tierEquipmentCost":
                            tierEquipmentCost.Value = int.Parse(oe.Value) / 25;
                            break;
                        case "tierLunarEquipmentCost":
                            tierLunarEquipmentCost.Value = int.Parse(oe.Value) / 25;
                            break;
                    }
                    config.Save();
                }
            }
        }

        private static void DeleteOldEntries(ConfigFile config)
        {
            System.Reflection.PropertyInfo OrphanedEntriesProperty = typeof(ConfigFile).GetProperty("OrphanedEntries", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)OrphanedEntriesProperty.GetValue(config);
            List<KeyValuePair<ConfigDefinition, string>> entriesToKeep = new List<KeyValuePair<ConfigDefinition, string>>();
            foreach (var oe in orphanedEntries)
            {
                if(oe.Key.Section == "Config" || oe.Key.Section == "PlayerPurchaseLimits")
                {
                    entriesToKeep.Add(oe);
                }
            }
            orphanedEntries.Clear();
            foreach(var entry in entriesToKeep)
            {
                orphanedEntries.Add(new ConfigDefinition("z_backup_config (delete if you dont need it)", entry.Key.Key), entry.Value);
            }
            configNumber.Value = ModConfig.CurrentVersionNumber;
            config.Save(); 
        }


        internal static bool isShareSuiteActive()
        {
            if(isShareSuiteLoaded)
            {
                return ShareSuite.GetFieldValue<ConfigEntry<bool>>("ModIsEnabled").Value;
            }
            return false;
        }

        private static bool IsShareSuiteLoaded()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("ShareSuite"))
                {
                    return true;
                }
            }

            return false;
        }

        internal static void SetShareSuiteReference(BaseUnityPlugin ShareSuiteInstance)
        {
            ShareSuite = ShareSuiteInstance;
            IsShareSuiteMoneySharing();
            //if (IsShareSuiteMoneySharing()) // there seems to be some redundancy here ...
            //{
            //    ModConfig.ShareSuiteItemSharingEnabled.Value = true;
            //}
        }

        public static bool IsShareSuiteMoneySharing()
        {
            bool sharing = false;
            if (ShareSuite != null)
            {
                if (!isShareSuiteActive())
                    return false;

                sharing = ShareSuite.GetFieldValue<ConfigEntry<bool>>("MoneyIsShared").Value;
                //if (sharing)
                //{
                //    ModConfig.ShareSuiteItemSharingEnabled.Value = true;
                //}
            }
            return sharing;

        }

        private static void CreateTierConfigs()
        {
            //tier1
            TierUnitConfig tier1 = new TierUnitConfig()
            {
                cost = tier1Cost.Value,
                costLunar = tier1CostLunar.Value,
                rarity = tier1Rarity.Value,
                maxChestPurchases = maxChestPurchasesTier1.Value
            };
            tierConfigs.Add(PickupTier.Tier1, tier1);
            //tier2
            TierUnitConfig tier2 = new TierUnitConfig()
            {
                cost = tier2Cost.Value,
                costLunar = tier2CostLunar.Value,
                rarity = tier2Rarity.Value,
                maxChestPurchases = maxChestPurchasesTier2.Value
            };
            tierConfigs.Add(PickupTier.Tier2, tier2);
            //tier3
            TierUnitConfig tier3 = new TierUnitConfig()
            {
                cost = tier3Cost.Value,
                costLunar = tier3CostLunar.Value,
                rarity = tier3Rarity.Value,
                maxChestPurchases = maxChestPurchasesTier3.Value
            };
            tierConfigs.Add(PickupTier.Tier3, tier3);
            //tierBoss
            TierUnitConfig tierBoss = new TierUnitConfig()
            {
                cost = tierBossCost.Value,
                costLunar = tierBossCostLunar.Value,
                rarity = tierBossRarity.Value,
                maxChestPurchases = maxChestPurchasesTierBoss.Value
            };
            tierConfigs.Add(PickupTier.Boss, tierBoss);
            //tierLunar
            TierUnitConfig tierLunar = new TierUnitConfig()
            {
                cost = tierLunarCost.Value,
                costLunar = tierLunarCostLunar.Value,
                rarity = tierLunarRarity.Value,
                maxChestPurchases = maxChestPurchasesTierLunar.Value
            };
            tierConfigs.Add(PickupTier.Lunar, tierLunar);
            //tierEquipment
            TierUnitConfig tierEquipment = new TierUnitConfig()
            {
                cost = tierEquipmentCost.Value,
                costLunar = tierEquipmentCostLunar.Value,
                rarity = tierEquipmentRarity.Value,
                maxChestPurchases = maxChestPurchasesTierEquipment.Value
            };
            tierConfigs.Add(PickupTier.Equipment, tierEquipment);
            //tierLunarEquipment
            TierUnitConfig tierLunarEquipment = new TierUnitConfig()
            {
                cost = tierLunarEquipmentCost.Value,
                costLunar = tierLunarEquipmentCostLunar.Value,
                rarity = tierLunarEquipmentRarity.Value,
                maxChestPurchases = maxChestPurchasesTierLunarEquipment.Value
            };
            tierConfigs.Add(PickupTier.LunarEquipment, tierLunarEquipment);
        }

        public static TierUnitConfig GetTierUnitConfig(PickupTier pickupTier)
        {
            return tierConfigs[pickupTier];
        }

        public struct TierUnitConfig
        {
            public float rarity;
            public float cost;
            public int costLunar;
            public int maxChestPurchases;
        }


    }
}
