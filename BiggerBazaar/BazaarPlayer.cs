using RoR2;

namespace BiggerBazaar
{
    class BazaarPlayer
    {
        public uint money;
        public NetworkUser networkUser;
        public int lunarExchanges;

        public BazaarPlayer(NetworkUser networkUser, uint money)
        {
            this.money = money;
            this.networkUser = networkUser;
            this.lunarExchanges = 0;
        }
    }
}
