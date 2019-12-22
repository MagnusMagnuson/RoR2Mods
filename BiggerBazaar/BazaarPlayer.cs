using RoR2;

namespace BiggerBazaar
{
    class BazaarPlayer
    {
        public uint money;
        public NetworkUser networkUser;
        public int lunarExchanges;
        public int chestPurchases;

        public BazaarPlayer(NetworkUser networkUser, uint money)
        {
            this.money = money;
            this.networkUser = networkUser;
            this.lunarExchanges = 0;
            this.chestPurchases = 0;
        }
    }
}
