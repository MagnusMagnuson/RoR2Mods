using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace BossRush
{
    class ModConfig
    {
        // config
        public static ConfigWrapper<float> Tier1Weight;
        public static ConfigWrapper<float> Tier2Weight;
        public static ConfigWrapper<float> Tier3Weight;
        public static ConfigWrapper<float> TierBossWeight;
        public static ConfigWrapper<float> TierLunarWeight;
        public static ConfigWrapper<float> EquipmentWeight;

        public static ConfigWrapper<int> GameMode;

        public static ConfigWrapper<int> MultiShopAmount;
        public static ConfigWrapper<bool> MultiShopAmountScaleWithPlayer;

        public static ConfigWrapper<int> MoneyGrantedPerTeleporterBossClear;
        public static ConfigWrapper<float> BossSpawnCostReduction;

        public static List<ItemTierShopConfig> tierWeights = new List<ItemTierShopConfig>();
        public static double tierTotal = 0;

        // item tiers
        public static ConfigWrapper<bool> Tier1Enabled;
        public static ConfigWrapper<bool> Tier2Enabled;
        public static ConfigWrapper<bool> Tier3Enabled;
        public static ConfigWrapper<bool> TierBossEnabled;
        public static ConfigWrapper<bool> TierLunarEnabled;
        public static ConfigWrapper<bool> EquipmentEnabled;

        // alternate mode
        public static ConfigWrapper<int> Tier1Price;
        public static ConfigWrapper<int> Tier2Price;
        public static ConfigWrapper<int> Tier3Price;
        public static ConfigWrapper<int> TierBossPrice;
        public static ConfigWrapper<int> TierLunarPrice;
        public static ConfigWrapper<int> EquipmentPrice;

        public static void InitConfig(ConfigFile config)
        {

            GameMode = config.Wrap(
            "1. General",
            "GameMode",
            "Set the desired game mode." +
            "\n 0 - Default mode: Spawn X amount of multi shops with random item tiers." +
            "\n 1 - Alternate mode: Spawn one multi shop for each enabled tier" +
            "\n(Default value: 0)",
            0);

            MoneyGrantedPerTeleporterBossClear = config.Wrap(
            "1. General",
            "MoneyGrantedPerTeleporterBossClear",
            "Set how much money, and thus items, each player obtains after every teleporter boss clear\n(Default value: 8)",
            8);

            BossSpawnCostReduction = config.Wrap(
            "1. General",
            "BossSpawnCostReduction",
            "Set how much less boss enemies (champions) cost the game to spawn. A higher value means that the game can afford to spawn more or stronger boss type enemies.\nSetting this to 2 e.g. should spawn two bosses on the first stage in single player, rather than just one. \n(Default value: 1.0 (float))",
            1f);


            // ITEM TIERS
            Tier1Enabled = config.Wrap(
            "2. Item Tiers",
            "Tier1",
            "Enable tier 1 items to appear in the multishop\n(Default value: true)",
            true);

            Tier2Enabled = config.Wrap(
            "2. Item Tiers",
            "Tier2",
            "Enable tier 2 items to appear in the multishop\n(Default value: true)",
            true);

            Tier3Enabled = config.Wrap(
            "2. Item Tiers",
            "Tier3",
            "Enable tier 3 items to appear in the multishop\n(Default value: true)",
            true);

            TierBossEnabled = config.Wrap(
            "2. Item Tiers",
            "TierBoss",
            "Enable boss tier items to appear in the multishop\n(Default value: false)",
            false);

            TierLunarEnabled = config.Wrap(
            "2. Item Tiers",
            "TierLunar",
            "Enable lunar tier items to appear in the multishop\n(Default value: false)",
            false);

            EquipmentEnabled = config.Wrap(
            "2. Item Tiers",
            "EquipmentEnabled",
            "Enable equipment items to appear in the multishop\n(Default value: true)",
            true);

            // Game Mode Config
            MultiShopAmount = config.Wrap(
            "3. DefaultMode Options",
            "MultiShopAmount",
            "Set the amount of multishops that spawn after the boss is defeated\n(Default value: 3)",
            3);

            MultiShopAmountScaleWithPlayer = config.Wrap(
            "3. DefaultMode Options",
            "MultiShopAmountScaleWithPlayer",
            "Overrides the amount specified at MultiShopAmount and instead scales the amount of multishops with amount of players (one shop per player)\n(Default value: false)",
            false);

            // TIER WEIGHTS
            Tier1Weight = config.Wrap(
            "3. DefaultMode Options",
            "Tier1Weights",
            "Set the weight for Tier 1 items to appear in the multishop\n(Default value: 0.8f)",
            0.8f);

            Tier2Weight = config.Wrap(
            "3. DefaultMode Options",
            "Tier2Weights",
            "Set the weight for Tier 2 items to appear in the multishop\n(Default value: 0.2f)",
            0.2f);

            Tier3Weight = config.Wrap(
            "3. DefaultMode Options",
            "Tier3Weights",
            "Set the weight for Tier 3 items to appear in the multishop\n(Default value: 0.1f)",
            0.1f);

            TierBossWeight = config.Wrap(
            "3. DefaultMode Options",
            "TierBossWeights",
            "Set the weight for Boss items to appear in the multishop\n(Default value: 0.1f)",
            0.1f);

            TierLunarWeight = config.Wrap(
            "3. DefaultMode Options",
            "TierLunarWeights",
            "Set the weight for Lunar items to appear in the multishop\n(Default value: 0.1f)",
            0.1f);

            EquipmentWeight = config.Wrap(
            "3. DefaultMode Options",
            "EquipmentWeight",
            "Set the weight for equipment items to appear in the multishop\n(Default value: 0.1f)",
            0.1f);


            Tier1Price = config.Wrap(
            "4. AlternateMode Options",
            "Tier1Price",
            "Set the price for tier 1 items\n(Default value: 1)",
            1);

            Tier2Price = config.Wrap(
            "4. AlternateMode Options",
            "Tier2Price",
            "Set the price for tier 2 items\n(Default value: 2)",
            2);

            Tier3Price = config.Wrap(
            "4. AlternateMode Options",
            "Tier3Price",
            "Set the price for tier 3 items\n(Default value: 4)",
            4);

            TierBossPrice = config.Wrap(
            "4. AlternateMode Options",
            "TierBossPrice",
            "Set the price for boss tier items\n(Default value: 7)",
            7);

            TierLunarPrice = config.Wrap(
            "4. AlternateMode Options",
            "TierLunarPrice",
            "Set the price for lunar tier items\n(Default value: 5)",
            5);

            EquipmentPrice = config.Wrap(
            "4. AlternateMode Options",
            "TierEquipmentPrice",
            "Set the price for equipment items\n(Default value: 3)",
            3);

            if (Tier1Enabled.Value && Tier1Weight.Value != 0f)
            {
                ItemTierShopConfig tierWeightConf = new ItemTierShopConfig
                {
                    itemTier = ItemTier.Tier1,
                    tierWeight = Tier1Weight.Value,
                    isEquipment = false,
                    price = Tier1Price.Value
                };
                tierWeights.Add(tierWeightConf);    
                tierTotal += tierWeightConf.tierWeight;
            }
            if (Tier2Enabled.Value && Tier2Weight.Value != 0f)
            {
                ItemTierShopConfig tierWeightConf = new ItemTierShopConfig
                {
                    itemTier = ItemTier.Tier2,
                    tierWeight = Tier2Weight.Value,
                    isEquipment = false,
                    price = Tier2Price.Value
                };
                tierWeights.Add(tierWeightConf);
                tierTotal += tierWeightConf.tierWeight;
            }
            if (Tier3Enabled.Value && Tier3Weight.Value != 0f)
            {
                ItemTierShopConfig tierWeightConf = new ItemTierShopConfig
                {
                    itemTier = ItemTier.Tier3,
                    tierWeight = Tier3Weight.Value,
                    isEquipment = false,
                    price = Tier3Price.Value
                };
                tierWeights.Add(tierWeightConf);
                tierTotal += tierWeightConf.tierWeight;
            }
            if (TierBossEnabled.Value && TierBossWeight.Value != 0f)
            {
                ItemTierShopConfig tierWeightConf = new ItemTierShopConfig
                {
                    itemTier = ItemTier.Boss,
                    tierWeight = TierBossWeight.Value,
                    isEquipment = false,
                    price = TierBossPrice.Value
                };
                tierWeights.Add(tierWeightConf);
                tierTotal += tierWeightConf.tierWeight;
            }
            if (TierLunarEnabled.Value && TierLunarWeight.Value != 0f)
            {
                ItemTierShopConfig tierWeightConf = new ItemTierShopConfig
                {
                    itemTier = ItemTier.Lunar,
                    tierWeight = TierLunarWeight.Value,
                    isEquipment = false,
                    price = TierLunarPrice.Value
                };
                tierWeights.Add(tierWeightConf);
                tierTotal += tierWeightConf.tierWeight;
            }
            if (EquipmentEnabled.Value && EquipmentWeight.Value != 0f)
            {
                ItemTierShopConfig tierWeightConf = new ItemTierShopConfig
                {
                    itemTier = ItemTier.NoTier,
                    tierWeight = EquipmentWeight.Value,
                    isEquipment = true,
                    price = EquipmentPrice.Value
                };
                tierWeights.Add(tierWeightConf);
                tierTotal += tierWeightConf.tierWeight;
            }
        }
    }

    public class ItemTierShopConfig
    {
        public ItemTier itemTier;
        public float tierWeight;
        public bool isEquipment;
        public int price;
    }
}
