using BepInEx;
using R2API.Utils;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace StartInBazaar
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.StartInBazaar", "StartInBazaar", "0.1.1")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class StartInBazaar : BaseUnityPlugin 
    {
        bool isFirstBazaarVisit = true;
        bool isFirstStage = true;

        public void Awake()
        {
            On.RoR2.Run.Start += (orig, self) =>
            {
                orig(self);
                SceneField sc = new SceneField("bazaar");
                NetworkManager.singleton.ServerChangeScene(sc);
                isFirstBazaarVisit = true;
                isFirstStage = true;
            };

            On.RoR2.BazaarController.Start += (orig, self) =>
            //On.RoR2.SceneDirector.Start += (orig, self) =>
            {
                if (NetworkServer.active)
                {
                    if(Run.instance.stageClearCount == 0)
                    {
                        if(isFirstBazaarVisit && SceneManager.GetActiveScene().name.Contains("bazaar"))
                        {
                            GiveMoneyToPlayers(-(int)Run.instance.ruleBook.startingMoney);
                            isFirstBazaarVisit = false;
                        } else if(isFirstStage)
                        {
                            GiveMoneyToPlayers((int)Run.instance.ruleBook.startingMoney);
                            isFirstStage = false;
                        }
                        
                    }
                }
                orig(self);
            };
        }

        public void GiveMoneyToPlayers(int money)
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                PlayerCharacterMasterController.instances[i].master.GiveMoney((uint)money);
            };
        }
    }
}
