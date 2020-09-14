using RoR2;
using System.Collections.Generic;

namespace BiggerBazaar
{
    public class TierContainer
    {
        public float totalRarity;
        public List<TemporaryTierUnit> tierUnits;

        public TierContainer()
        {
            tierUnits = new List<TemporaryTierUnit>();

            AddTierUnit(PickupTier.Tier1, Run.instance.availableTier1DropList.Count == 0 ? true : false);
            AddTierUnit(PickupTier.Tier2, Run.instance.availableTier2DropList.Count == 0 ? true : false);
            AddTierUnit(PickupTier.Tier3, Run.instance.availableTier3DropList.Count == 0 ? true : false);
            AddTierUnit(PickupTier.Boss, Run.instance.availableBossDropList.Count == 0 ? true : false);
            AddTierUnit(PickupTier.Lunar, Run.instance.availableLunarDropList.Count == 0 ? true : false);
            AddTierUnit(PickupTier.Equipment, Run.instance.availableEquipmentDropList.Count == 0 ? true : false);
            AddTierUnit(PickupTier.LunarEquipment, Run.instance.availableLunarEquipmentDropList.Count == 0 ? true : false);

            //tierUnits.ForEach(x => { Debug.LogWarning(x.pickupTier + " " + x.rarity); });

            CalculateTotalRarity();
        }

        private void CalculateTotalRarity()
        {
            totalRarity = 0;
            tierUnits.ForEach(x => { totalRarity += x.rarity; });
        }

        private void AddTierUnit(PickupTier pickupTier, bool removeTier)
        {
            ModConfig.TierUnitConfig tUC = ModConfig.GetTierUnitConfig(pickupTier);
            tierUnits.Add(new TemporaryTierUnit()
            {
                pickupTier = pickupTier,
                cost = tUC.cost,
                costLunar = tUC.costLunar,
                rarity = removeTier ? 0 : tUC.rarity,
                maxChestPurchases = tUC.maxChestPurchases
            });
        }
    }

    public struct TemporaryTierUnit
    {
        public PickupTier pickupTier;
        public int cost;
        public int costLunar;
        public float rarity;
        public int maxChestPurchases;
    }
}
