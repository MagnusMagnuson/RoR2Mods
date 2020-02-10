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

        public BazaarPlayer(NetworkUser networkUser, uint money)
        {
            this.money = money;
            this.networkUser = networkUser;
            this.lunarExchanges = 0;
            this.chestPurchases = 0;
            this.tier1Purchases = 0;
            this.tier2Purchases = 0;
            this.tier3Purchases = 0;
        }

        internal void IncreaseTierPurchase(ItemTier itemTier)
        {
            switch (itemTier)
            {
                case ItemTier.Tier1:
                    this.tier1Purchases++;
                    break;
                case ItemTier.Tier2:
                    this.tier2Purchases++;
                    break;
                case ItemTier.Tier3:
                    this.tier3Purchases++;
                    break;
                default:
                    break;
            }
        }
    }
}
