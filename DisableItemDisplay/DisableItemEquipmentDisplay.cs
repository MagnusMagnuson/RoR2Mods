using System;
using BepInEx;
using BepInEx.Configuration;

namespace DisableItemDisplay
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.DisableItemEquipmentDisplay", "DisableItemEquipmentDisplay", "1.0.0")]
    public class DisableItemEquipmentDisplay : BaseUnityPlugin
    {
        public static ConfigWrapper<bool> DisableItemDisplay;
        public static ConfigWrapper<bool> DisableEquipmentDisplay;

        public void Awake()
        {
            InitConfig(Config);

            On.RoR2.CharacterModel.EnableItemDisplay += CharacterModel_EnableItemDisplay;

            On.RoR2.CharacterModel.SetEquipmentDisplay += CharacterModel_SetEquipmentDisplay;

        }

        private void InitConfig(ConfigFile config)
        {
            DisableItemDisplay = config.Wrap(
            "Config",
            "DisableItemDisplay",
            "Disables items from being displayed on your character body.\n(Default value: true)",
            true);

            DisableEquipmentDisplay = config.Wrap(
            "Config",
            "DisableEquipmentDisplay",
            "Disables equipment from being displayed on your character body (Exceptions are the gold gattling gun and milky chrysalis.\n(Default value: false)",
            false);
        }

        private void CharacterModel_EnableItemDisplay(On.RoR2.CharacterModel.orig_EnableItemDisplay orig, RoR2.CharacterModel self, RoR2.ItemIndex itemIndex)
        {
            if (!DisableItemDisplay.Value) orig(self, itemIndex);
            return;
        }

        private void CharacterModel_SetEquipmentDisplay(On.RoR2.CharacterModel.orig_SetEquipmentDisplay orig, RoR2.CharacterModel self, RoR2.EquipmentIndex newEquipmentIndex)
        {
            if(DisableEquipmentDisplay.Value)
            {
                if (newEquipmentIndex != RoR2.EquipmentIndex.GoldGat && newEquipmentIndex != RoR2.EquipmentIndex.Jetpack)
                {
                    newEquipmentIndex = RoR2.EquipmentIndex.None;
                }
            }
            orig(self, newEquipmentIndex);
        }
    }
}
