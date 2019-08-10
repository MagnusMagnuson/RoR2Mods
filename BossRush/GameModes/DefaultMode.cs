using System;
using System.Collections.Generic;
using System.Text;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BossRush
{
    class DefaultMode : IMode
    {
        System.Random random = new System.Random();

        public void CreateTerminals(On.RoR2.MultiShopController.orig_CreateTerminals orig, MultiShopController self)
        {
            // choose random item tier
            ItemTierShopConfig itemTierConfig = PickRandomItemTier();

            self.itemTier = itemTierConfig.itemTier;
            self.Networkcost = 1;
            MultiShop.CreateTerminals(orig, self, itemTierConfig);
            
        }

        public void UpdateTerminals(MultiShopController self)
        {
            // choose random item tier
            ItemTierShopConfig itemTierConfig = PickRandomItemTier();

            MultiShop.RepopulateTerminals(self, itemTierConfig);
        }

        private ItemTierShopConfig PickRandomItemTier()
        {
            double randomVal = random.NextDouble() * ModConfig.tierTotal;
            double currentVal = ModConfig.tierTotal;
            ItemTierShopConfig itemTierConfig = new ItemTierShopConfig();
            for (int i = ModConfig.tierWeights.Count - 1; i > 0; i--)
            {
                currentVal -= ModConfig.tierWeights[i].tierWeight;
                if (randomVal >= currentVal)
                {
                    itemTierConfig = ModConfig.tierWeights[i];
                    break;
                }
            }

            return itemTierConfig;
        }
    }
}
