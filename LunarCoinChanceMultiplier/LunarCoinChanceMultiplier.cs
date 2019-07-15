using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;

namespace LunarCoinChanceMultiplier
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.LunarCoinChanceMultiplier", "LunarCoinChanceMultiplier", "1.0.0")]
    public class LunarCoinChanceMultiplier : BaseUnityPlugin
    {

        public static ConfigWrapper<float> lunarCoinChanceMultiplier;
        public void Awake()
        {
            initConfig();

            On.RoR2.PlayerCharacterMasterController.Awake += (orig, self) =>
            {
                orig(self);
                self.SetFieldValue<float>("lunarCoinChanceMultiplier", lunarCoinChanceMultiplier.Value);
            };
        }

        private void initConfig()
        {
            lunarCoinChanceMultiplier = Config.Wrap(
            "Config",
            "lunarCoinChanceMultiplier",
            "Set the chance of lunar coins dropping. (Chance naturally decreses every drop and resets on stage change).",
            0.5f);

        }
    }
}
