using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System;
using UnityEngine;

namespace BiggerBazaar
{
    class ModConfig
    {
        public static ConfigWrapper<int> lunarCoinWorth;
        public static ConfigWrapper<int> maxLunarExchanges;
        public static ConfigWrapper<int> chestCostType;
        public static ConfigWrapper<int> tier1Cost;
        public static ConfigWrapper<int> tier2Cost;
        public static ConfigWrapper<int> tier3Cost;
        public static ConfigWrapper<float> tier1Rarity;
        public static ConfigWrapper<float> tier2Rarity;
        public static ConfigWrapper<float> tier3Rarity;
        public static ConfigWrapper<int> maxChestPurchasesTier1;
        public static ConfigWrapper<int> maxChestPurchasesTier2;
        public static ConfigWrapper<int> maxChestPurchasesTier3;
        public static ConfigWrapper<int> maxPlayerPurchases;
        public static ConfigWrapper<int> maxPlayerPurchasesTier1;
        public static ConfigWrapper<int> maxPlayerPurchasesTier2;
        public static ConfigWrapper<int> maxPlayerPurchasesTier3;
        public static ConfigWrapper<bool> BroadcastShopSettings;
        public static ConfigWrapper<bool> ShareSuiteItemSharingEnabled;
        //public static ConfigWrapper<bool> ShareSuiteMoneySharingEnabled;
        public static bool infiniteLunarExchanges = false;
        public static bool isUsingShareSuite;
        private static BaseUnityPlugin ShareSuite = null;
        public static ConfigWrapper<bool> experimentalPriceScaling;
        public static ConfigWrapper<float> experimentalPriceScalingMinPercent;
        public static ConfigWrapper<float> experimentalPriceScalingMaxPercent;

        public static ConfigWrapper<int> tier1CostLunar;
        public static ConfigWrapper<int> tier2CostLunar;
        public static ConfigWrapper<int> tier3CostLunar;

        public static void InitConfig(ConfigFile config)
        {
            chestCostType = config.Wrap(
            "Config",
            "chestCostType",
            "Set the chests cost type to either gold (0) or lunar coins (1).",
            0);

            tier1Cost = config.Wrap(
            "Config",
            "tier1ChestCost",
            "Set the base cost for tier 1 items (white). This scales automatically with difficulty and player amount.\nA value of 25 means that these items will cost as much as a small chest in the prior stage (the one you opened the shop portal in) \n(Default value: 25)",
            25);
            //tier1Cost = tier1CostConf.Value;

            tier2Cost = config.Wrap(
            "Config",
            "tier2ChestCost",
            "Set the base cost for tier 2 items (green). This scales automatically with difficulty and player amount.\nA value of 50 means that these items will cost as much as a medium chest in the prior stage (the one you opened the shop portal in) \n(Default value: 50)",
            50);
            //tier2Cost = tier2CostConf.Value;

            tier3Cost = config.Wrap(
            "Config",
            "tier3ChestCost",
            "Set the base cost for tier 3 items (red). This scales automatically with difficulty and player amount.\nA value of 400 means that these items will cost as much as a legendary chest in the prior stage (the one you opened the shop portal in) \n(Default value: 400)",
            400);
            //tier3Cost = tier3CostConf.Value;

            tier1Rarity = config.Wrap(
            "Config",
            "tier1Rarity",
            "Set the rarity of tier 1 (white), tier 2 (green) and tier 3 (red) items. Higher value compared to the other tiers means that tier is more likely to appear (weighted random).\nSet a tier rarity to 0.0 if you don't want any items of that tier to appear.\n(Default values: 0.55, 0.3, 0.15)\n",
            0.55f);
            //tier1Rarity = tier1RarityConf.Value;

            tier2Rarity = config.Wrap(
            "Config",
            "tier2Rarity",
            "",
            0.3f);
            //tier2Rarity = tier2RarityConf.Value;

            tier3Rarity = config.Wrap(
            "Config",
            "tier3Rarity",
            "",
            0.15f);
            //tier3Rarity = tier3RarityConf.Value;

            maxChestPurchasesTier1 = config.Wrap(
            "Config",
            "maxChestPurchasesTier1",
            "How often you can buy an item before the chests runs out/becomes unavailable.\nSet -1 for infinite purchases.\n\nTier 1",
            3);


            maxChestPurchasesTier2 = config.Wrap(
            "Config",
            "maxChestPurchasesTier2",
            "Tier 2",
            2);

            maxChestPurchasesTier3 = config.Wrap(
            "Config",
            "maxChestPurchasesTier3",
            "Tier 3",
            1);

            maxPlayerPurchases = config.Wrap(
            "PlayerPurchaseLimits",
            "maxPlayerPurchases",
            "This sets how many total purchases a player is allowed to make per bazaar visit. Set this to -1 for unlimited.",
            3);

            maxPlayerPurchasesTier1 = config.Wrap(
            "PlayerPurchaseLimits",
            "maxPlayerPurchasesTier1",
            "This sets how many tier 1 (white) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.",
            -1);

            maxPlayerPurchasesTier2 = config.Wrap(
            "PlayerPurchaseLimits",
            "maxPlayerPurchasesTier2",
            "This sets how many tier 2 (green) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.",
            -1);

            maxPlayerPurchasesTier3 = config.Wrap(
            "PlayerPurchaseLimits",
            "maxPlayerPurchasesTier3",
            "This sets how many tier 3 (red) items a player can buy in total, per bazaar visit. Set this to -1 for unlimited.",
            -1);

            BroadcastShopSettings = config.Wrap(
            "PlayerPurchaseLimits",
            "BroadcastShopSettings",
            "Lets everyone know how many things they can purchase on bazaar entry.",
            true);

            lunarCoinWorth = config.Wrap(
            "Config",
            "lunarCoinWorth",
            "Conversion rate for Lunar Coins to Money. Conversion rate automatically scales with difficulty, meaning if you set this at the base price of a medium chest (50), you will always get enough money to buy a tier 2 item. (Reference: Small chest = 25, Medium Chest = 50, Legendary Chest = 400)\nIf you change the item tier prices, you might have to adjust this value.",
            50);


            maxLunarExchanges = config.Wrap(
            "Config",
            "maxLunarExchanges",
            "Sets how many Lunar Coin exchanges to money are allowed per player. A value of 0 will never allow anx exchange. A value of -1 allows infinite exchanges.",
            3);
            if (maxLunarExchanges.Value == -1)
                infiniteLunarExchanges = true;

            tier1CostLunar = config.Wrap(
            "LunarModePricing",
            "tier1ChestCostLunar",
            "Sets how many lunar coins a tier 1 (white) item costs",
            1);

            tier2CostLunar = config.Wrap(
            "LunarModePricing",
            "tier2ChestCostLunar",
            "Sets how many lunar coins a tier 2 (green) item costs",
            2);

            tier3CostLunar = config.Wrap(
            "LunarModePricing",
            "tier3ChestCostLunar",
            "Sets how many lunar coins a tier 3 (red) item costs",
            3);

            experimentalPriceScaling = config.Wrap(
            "PriceScaling",
            "experimentalPriceScaling",
            "Experimental scaling for chest prices. This prevents that the player(s) can buy all items, by recalculating chest prices if players money together exceeds a certain percentage of the shops worth. The adjustment is random within certain bounds (e.g. the players money together allows for theoretically 30% of the shops worth to be bought). The players' individual money remains unchanged.",
            false);

            experimentalPriceScalingMinPercent = config.Wrap(
            "PriceScaling",
            "experimentalPriceScalingMinPercent",
            "Lower random bound for shops inventory worth able to be bought. Numbers should be between 0 (for 0%) and 1 (for 100%). \nA value of 0.3 means that the most expensive the shop can become is that all of the players' money together is only enough to buy 30% of the shop's inventory.",
            0.3f);

            experimentalPriceScalingMaxPercent = config.Wrap(
            "PriceScaling",
            "experimentalPriceScalingMaxPercent",
            "Upper random bound for shops inventory worth able to be bought and also value at which a recalculation happens. Numbers should be between 0 (for 0%) and 1 (for 100%). \nA value of 0.7 means that the cheapest the shop can become is that all of the players' money together is enough to buy 70% of the shop's inventory.",
            0.7f);

            ShareSuiteItemSharingEnabled = config.Wrap(
            "ShareSuite",
            "ShareSuiteItemSharingEnabled",
            "This option is only relevant if you are using ShareSuite, otherwise ignore. \nSetting this to false, will disable item sharing for Bigger Bazaar items and put items directly into the buyers inventory, instead of having it pop out from the chest and drop on the floor and effectively sharing it with everyone on pickup.\nAutomatically turned on if money sharing is enabled in ShareSuite",
            false);

            isUsingShareSuite = IsShareSuiteLoaded();

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
            if(IsShareSuiteMoneySharing())
            {
                ModConfig.ShareSuiteItemSharingEnabled.Value = true;
            }
        }

        public static bool IsShareSuiteMoneySharing()
        {
            bool sharing = false;
            if (ShareSuite != null)
            {
                sharing = ShareSuite.GetFieldValue<ConfigEntry<bool>>("MoneyIsShared").Value;
                if(sharing)
                {
                    ModConfig.ShareSuiteItemSharingEnabled.Value = true;
                }
            }
            return sharing;
        }


    }
}
