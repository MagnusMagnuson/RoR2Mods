using System;
using System.Collections.Generic;
using System.Text;
using On.RoR2;
using RoR2;
using UnityEngine;

namespace BossRush
{
    class AlternateMode : IMode
    {
        public void CreateTerminals(On.RoR2.MultiShopController.orig_CreateTerminals orig, RoR2.MultiShopController self)
        {
            int index = MultiShop.instances.Count; // I don't super like this
            ItemTierShopConfig itemTierConfig = ModConfig.tierWeights[index];
            self.itemTier = itemTierConfig.itemTier;
            self.Networkcost = itemTierConfig.price;

            MultiShop.CreateTerminals(orig, self, itemTierConfig);
        }

        public void UpdateTerminals(RoR2.MultiShopController self)
        {
            MultiShop.RepopulateTerminals(self, MultiShop.RetrieveItemTierConfig(ModConfig.tierWeights, self.itemTier));
        }
    }
}
