using System;
using RoR2;

namespace BiggerBazaar
{
    class BazaarPlayer
    {
        public uint money;
        public NetworkUser networkUser;
        public int lunarExchanges;
        public int chestPurchases;
        public int tier1Purchases;
        public int tier2Purchases;
        public int tier3Purchases;
        public int tierBossPurchases;
        public int tierLunarPurchases;
        public int tierEquipmentPurchases;
        public int tierLunarEquipmentPurchases;

        public BazaarPlayer(NetworkUser networkUser, uint money)
        {
            this.money = money;
            this.networkUser = networkUser;
            this.lunarExchanges = 0;
            this.chestPurchases = 0;
            this.tier1Purchases = 0;
            this.tier2Purchases = 0;
            this.tier3Purchases = 0;
            this.tierBossPurchases = 0;
            this.tierLunarPurchases = 0;
            this.tierEquipmentPurchases = 0;
            this.tierLunarEquipmentPurchases = 0;
        }

        internal void IncreaseTierPurchase(PickupTier pickupTier)
        {
            switch (pickupTier)
            {
                case PickupTier.Tier1:
                    this.tier1Purchases++;
                    break;
                case PickupTier.Tier2:
                    this.tier2Purchases++;
                    break;
                case PickupTier.Tier3:
                    this.tier3Purchases++;
                    break;
                case PickupTier.Boss:
                    this.tierBossPurchases++;
                    break;
                case PickupTier.Lunar:
                    this.tierLunarPurchases++;
                    break;
                case PickupTier.Equipment:
                    this.tierEquipmentPurchases++;
                    break;
                case PickupTier.LunarEquipment:
                    this.tierLunarEquipmentPurchases++;
                    break;
                default:
                    break;
            }
        }
    }
}
