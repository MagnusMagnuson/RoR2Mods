using BepInEx.Configuration;
using System;

namespace BazaarPrinter
{
    class ModConfig
    {
        public static ConfigEntry<int> printerCount;
        public static ConfigEntry<float> tier1Chance;
        public static ConfigEntry<float> tier2Chance;
        public static ConfigEntry<float> tier3Chance;
        public static ConfigEntry<float> tierBossChance;

        public static void InitConfig(ConfigFile config)
        {

            printerCount = config.Bind(
            "Config",
            "printerCount",
            1,
            new ConfigDescription("Set how many 3D Printers should spawn in the bazaar. Maximum is 4")
            );
            printerCount.Value = Math.Abs(printerCount.Value);
            if (printerCount.Value > 4)
                printerCount.Value = 4;


            tier1Chance = config.Bind(
            "Config",
            "tier1Chance",
            0.7f,
            new ConfigDescription("Set how likely it is for a bazaar 3D Printer to be tier 1")
            );
            tier1Chance.Value = Math.Abs(tier1Chance.Value);

            tier2Chance = config.Bind(
            "Config",
            "tier2Chance",
            0.2f,
            new ConfigDescription("Set how likely it is for a bazaar 3D Printer to be tier 2")
            );
            tier2Chance.Value = Math.Abs(tier2Chance.Value);

            tier3Chance = config.Bind(
            "Config",
            "tier3Chance",
            0.05f,
            new ConfigDescription("Set how likely it is for a bazaar 3D Printer to be tier 3")
            );
            tier3Chance.Value = Math.Abs(tier3Chance.Value);

            tierBossChance = config.Bind(
            "Config",
            "tierBossChance",
            0.05f,
            new ConfigDescription("Set how likely it is for a bazaar 3D Printer to be boss tier")
            );
            tierBossChance.Value = Math.Abs(tierBossChance.Value);
        }
    }
}
