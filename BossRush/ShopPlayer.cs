using RoR2;
using System.Collections.Generic;

namespace BossRush
{
    class ShopPlayer
    {
        public static List<ShopPlayer> instances = new List<ShopPlayer>();

        //public int madePurchases;
        public NetworkUser networkUser;
        public uint currentMoney;

        public ShopPlayer(NetworkUser networkUser)
        {
            //madePurchases = 0;
            this.networkUser = networkUser;
        }
    }
}