using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using UnityEngine;

namespace BiggerBazaar
{
    class ModConfig
    {
        public static ConfigEntry<int> lunarCoinWorth;
        public static ConfigEntry<int> maxLunarExchanges;
        public static ConfigEntry<int> chestCostType;
        public static ConfigEntry<int> tier1Cost;
        public static ConfigEntry<int> tier2Cost;
        public static ConfigEntry<int> tier3Cost;
        public static ConfigEntry<float> tier1Rarity;
        public static ConfigEntry<float> tier2Rarity;
        public static ConfigEntry<float> tier3Rarity;
        //public static ConfigEntry<float> tierBossRarity;
        //public static ConfigEntry<float> tierLunarRarity;
        //public static ConfigEntry<float> tierEquipmentRarity;
        public static ConfigEntry<int> maxChestPurchasesTier1;
        public static ConfigEntry<int> maxChestPurchasesTier2;
        public static ConfigEntry<int> maxChestPurchasesTier3;
        public static ConfigEntry<int> maxPlayerPurchases;
        public static ConfigEntry<int> maxPlayerPurchasesTier1;
        public static ConfigEntry<int> maxPlayerPurchasesTier2;
        public static ConfigEntry<int> maxPlayerPurchasesTier3;
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

        public static ConfigEntry<int> tier1CostLunar;
        public static ConfigEntry<int> tier2CostLunar;
        public static ConfigEntry<int> tier3CostLunar;
        //public static ConfigEntry<int> tierBossCostLunar;
        //public static ConfigEntry<int> tierLunarCostLunar;
        //public static ConfigEntry<int> tierEquipmentCostLunar;

        public static BaseUnityPlugin ShareSuite;

        public static void InitConfig(ConfigFile config)
        {
            chestCostType = config.Bind(
            "Config",
            "chestCostType",
            0,
            new ConfigDescription("Set the chests cost type to either gold (0) or lunar coins (1).")
            );

            tier1Cost = config.Bind(
            "Config",
            "tier1ChestCost",
            25,
            new ConfigDescription("Set the base cost for tier 1 items (white). This scales automatically with difficulty and player amount.\nA value of 25 means that these items will cost as much as a small chest in the prior stage (the one you opened the shop portal in) \n(Default value: 25)")
            );
            //tier1Cost = tier1CostConf.Value;

            tier2Cost = config.Bind(
            "Config",
            "tier2ChestCost",
            50,
            new ConfigDescription("Set the base cost for tier 2 items (green). This scales automatically with difficulty and player amount.\nA value of 50 means that these items will cost as much as a medium chest in the prior stage (the one you opened the shop portal in) \n(Default value: 50)")
            );
            //tier2Cost = tier2CostConf.Value;

            tier3Cost = config.Bind(
            "Config",
            "tier3ChestCost",
            400,
            new ConfigDescription("Set the base cost for tier 3 items (red). This scales automatically with difficulty and player amount.\nA value of 400 means that these items will cost as much as a legendary chest in the prior stage (the one you opened the shop portal in) \n(Default value: 400)")
            );
            //tier3Cost = tier3CostConf.Value;

            tier1Rarity = config.Bind(
            "Config",
            "tier1Rarity",
            0.55f,
            new ConfigDescription("Set the rarity of tier 1 (white), tier 2 (green) and tier 3 (red) items. Higher value compared to the other tiers means that tier is more likely to appear (weighted random).\nSet a tier rarity to 0.0 if you don't want any items of that tier to appear.\n(Default values: 0.55, 0.3, 0.15)\n")
            );
            //tier1Rarity = tier1RarityConf.Value;

            tier2Rarity = config.Bind(
            "Config",
            "tier2Rarity",
            0.3f,
            new ConfigDescription("")
            );
            //tier2Rarity = tier2RarityConf.Value;

            tier3Rarity = config.Bind(
            "Config",
            "tier3Rarity",
            0.15f,
            new ConfigDescription("")
            );
            //tier3Rarity = tier3RarityConf.Value;

            //tierBossRarity = config.Bind(
            //"Config",
            //"tierBossRarity",
            //0f,
            //new ConfigDescription("")
            //);

            //tierLunarRarity = config.Bind(
            //"Config",
            //"tierLunarRarity",
            //0f,
            //new ConfigDescription("")
            //);

            //tierEquipmentRarity = config.Bind(
            //"Config",
            //"tierEquipmentRarity",
            //0f,
            //new ConfigDescription("")
            //);

            maxChestPurchasesTier1 = config.Bind(
            "Config",
            "maxChestPurchasesTier1",
            3,
            new ConfigDescription("How often you can buy an item before the chests runs out/becomes unavailable.\nSet -1 for infinite purchases.\n\nTier 1")
            );


            maxChestPurchasesTier2 = config.Bind(
            "Config",
            "maxChestPurchasesTier2",
            2,
            new ConfigDescription("Tier 2")
            );

            maxChestPurchasesTier3 = config.Bind(
            "Config",
            "maxChestPurchasesTier3",
            1,
            new ConfigDescription("Tier 3")
            );

            maxPlayerPurchases = config.Bind(
            "PlayerPurchaseLimits",
            "maxPlayerPurchases",
            3,
            new ConfigDescription("This sets how many total purchases a player is allowed to make per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTier1 = config.Bind(
            "PlayerPurchaseLimits",
            "maxPlayerPurchasesTier1",
            -1,
            new ConfigDescription("This sets how many tier 1 (white) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTier2 = config.Bind(
            "PlayerPurchaseLimits",
            "maxPlayerPurchasesTier2",
            -1,
            new ConfigDescription("This sets how many tier 2 (green) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            maxPlayerPurchasesTier3 = config.Bind(
            "PlayerPurchaseLimits",
            "maxPlayerPurchasesTier3",
            -1,
            new ConfigDescription("This sets how many tier 3 (red) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.")
            );

            BroadcastShopSettings = config.Bind(
            "PlayerPurchaseLimits",
            "BroadcastShopSettings",
            true,
            new ConfigDescription("Lets everyone know how many things they can purchase on bazaar entry.")
            );

            lunarCoinWorth = config.Bind(
            "Config",
            "lunarCoinWorth",
            50,
            new ConfigDescription("Conversion rate for Lunar Coins to Money. Conversion rate automatically scales with difficulty, meaning if you set this at the base price of a medium chest (50), you will always get enough money to buy a tier 2 item. (Reference: Small chest = 25, Medium Chest = 50, Legendary Chest = 400)\nIf you change the item tier prices, you might have to adjust this value.")
            );


            maxLunarExchanges = config.Bind(
            "Config",
            "maxLunarExchanges",
            3,
            new ConfigDescription("Sets how many Lunar Coin exchanges to money are allowed per player. A value of 0 will never allow anx exchange. A value of -1 allows infinite exchanges.")
            );
            if (maxLunarExchanges.Value == -1)
                infiniteLunarExchanges = true;

            tier1CostLunar = config.Bind(
            "LunarModePricing",
            "tier1ChestCostLunar",
            1,
            new ConfigDescription("Sets how many lunar coins a tier 1 (white) item costs")
            );

            tier2CostLunar = config.Bind(
            "LunarModePricing",
            "tier2ChestCostLunar",
            2,
            new ConfigDescription("Sets how many lunar coins a tier 2 (green) item costs")
            );

            tier3CostLunar = config.Bind(
            "LunarModePricing",
            "tier3ChestCostLunar",
            3,
            new ConfigDescription("Sets how many lunar coins a tier 3 (red) item costs")
            );

            //tierBossCostLunar = config.Bind(
            //"LunarModePricing",
            //"tierBossCostLunar",
            //5,
            //new ConfigDescription("Sets how many lunar coins a boss (yellow) item costs")
            //);

            //tierLunarCostLunar = config.Bind(
            //"LunarModePricing",
            //"tierLunarCostLunar",
            //3,
            //new ConfigDescription("Sets how many lunar coins a lunar (blue) item costs")
            //);

            //tierEquipmentCostLunar = config.Bind(
            //"LunarModePricing",
            //"tierEquipmentCostLunar",
            //3,
            //new ConfigDescription("Sets how many lunar coins equipment (orange) costs")
            //);

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
            "ShareSuite",
            "ShareSuiteItemSharingEnabled",
            false,
            new ConfigDescription("This option is only relevant if you are using ShareSuite, otherwise ignore. \nSetting this to false, will disable item sharing for Bigger Bazaar items and put items directly into the buyers inventory, instead of having it pop out from the chest and drop on the floor and effectively sharing it with everyone on pickup.\nAutomatically turned on if money sharing is enabled in ShareSuite")
            );

            ShareSuiteTotalPurchaseSharing = config.Bind(
            "ShareSuite",
            "ShareSuiteTotalPurchaseSharing",
            true,
            new ConfigDescription("This option is only relevant if you are using ShareSuite, otherwise ignore. \nSets the total amount of available purchases (maxPlayerPurchases in this config) to count for the whole party, rather than individual players.\nThis makes sense if you're sharing money and items.")
            );

            isShareSuiteLoaded = IsShareSuiteLoaded();

            sacrificeArtifactAllowChests = config.Bind(
            "Sacrifice Artifact",
            "sacrificeArtifactAllowChests",
            true,
            new ConfigDescription("Prevents the Sacrifice Artifact from removing the chests in the bazaar.")
            );

            //experimentalPriceScaling.Value = false;
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
            if (IsShareSuiteMoneySharing()) // there seems to be some redundancy here ...
            {
                ModConfig.ShareSuiteItemSharingEnabled.Value = true;
            }
        }


        public static bool IsShareSuiteMoneySharing()
        {
            bool sharing = false;
            if (ShareSuite != null)
            {
                if (!isShareSuiteActive())
                    return false;

                sharing = ShareSuite.GetFieldValue<ConfigEntry<bool>>("MoneyIsShared").Value;
                if (sharing)
                {
                    ModConfig.ShareSuiteItemSharingEnabled.Value = true;
                }
            }
            return sharing;

        }


    }
}
