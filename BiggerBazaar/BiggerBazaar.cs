using BepInEx;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BiggerBazaar
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.BiggerBazaar", "BiggerBazaar", "1.6.2")]
    public class BiggerBazaar : BaseUnityPlugin
    {

        bool isCurrentStageBazaar = false;

        public void Awake()
        {

            ModConfig.initConfig(Config);
            Bazaar bazaar = new Bazaar();
            

            On.RoR2.GenericPickupController.AttemptGrant += (orig, self, body) =>
            {
                if (NetworkServer.active)
                {
                    if (isCurrentStageBazaar)
                    {
                        if (!bazaar.IsDisplayItem(self.gameObject))
                        {
                            orig(self, body);
                        }
                    } else
                    {
                        orig(self, body);
                    }

                }
            };

            On.RoR2.SceneExitController.Begin += (orig, self) =>
            {
                if(NetworkServer.active)
                {
                    if (self.destinationScene)
                    {
                        if (self.destinationScene.baseSceneName.Contains("bazaar") && !SceneInfo.instance.sceneDef.baseSceneName.Contains("bazaar"))
                        {
                            bazaar.ResetBazaarPlayers();
                        }
                    }
                }
                orig(self);
            };

            On.RoR2.PickupDisplay.SetPickupIndex += (orig, self, newPickupIndex, newHidden) =>
            {
                if (NetworkServer.active)
                {
                    if (isCurrentStageBazaar)
                    {
                        List<BazaarItem> bazaarItems = bazaar.GetBazaarItems();
                        for (int i = 0; i < bazaarItems.Count; i++)
                        {
                            if (bazaarItems[i].genericPickupController.pickupDisplay == self)
                            {
                                orig(self, bazaarItems[i].pickupIndex, newHidden);
                                return;
                            }
                        }
                    }
                }

                orig(self, newPickupIndex, newHidden);
            };


            On.RoR2.ChestBehavior.Open += (orig, self) =>
            {
                if (NetworkServer.active)
                {
                    if (isCurrentStageBazaar)
                    {
                        bool isCreatingDroplet = false;
                        if (!ModConfig.isUsingShareSuite)
                        {
                            isCreatingDroplet = true;
                        }
                        else
                        {
                            if (ModConfig.ShareSuiteItemSharingEnabled.Value)
                            {
                                isCreatingDroplet = true;
                            }
                        }
                        if (isCreatingDroplet)
                        {
                            List<BazaarItem> bazaarItems = bazaar.GetBazaarItems();
                            for (int i = 0; i < bazaar.GetBazaarItemAmount(); i++)
                            {
                                if (bazaarItems[i].chestBehavior.Equals(self))
                                {
                                    PickupDropletController.CreatePickupDroplet(bazaarItems[i].pickupIndex, self.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + self.transform.forward * 2f);
                                    bazaarItems[i].purchaseCount++;
                                    if (bazaar.IsChestStillAvailable(bazaarItems[i]))
                                    {
                                        self.GetComponent<PurchaseInteraction>().SetAvailable(true);
                                    }

                                    return;
                                }
                            }
                        }

                    };
                }
                orig(self);
            };

            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
            {
                if (isCurrentStageBazaar)
                {
                    if (!self.CanBeAffordedByInteractor(activator))
                    {
                        return;
                    }
                    if (bazaar.IsMoneyLunarPodAvailable())
                    {
                        if (bazaar.IsMoneyLunarPod(self.gameObject))
                        {
                            NetworkUser networkUser = Util.LookUpBodyNetworkUser(activator.gameObject);
                            BazaarPlayer bazaarPlayer = bazaar.GetBazaarPlayer(networkUser);
                            if (bazaarPlayer.lunarExchanges < ModConfig.maxLunarExchanges.Value || ModConfig.infiniteLunarExchanges)
                            {
                                bazaarPlayer.lunarExchanges++;
                                int money = bazaar.GetLunarCoinExchangeMoney();
                                activator.GetComponent<CharacterBody>().master.money += ((uint)money);
                                networkUser.DeductLunarCoins((uint)self.cost);
                                var goldReward = (int)((double)ModConfig.lunarCoinWorth.Value * (double)bazaar.currentDifficultyCoefficient);
                                GameObject coinEmitterResource = Resources.Load<GameObject>("Prefabs/Effects/CoinEmitter");
                                EffectManager.SpawnEffect(coinEmitterResource, new EffectData()
                                {
                                    origin = self.transform.position,
                                    genericFloat = (float)goldReward
                                }, true);
                                EffectManager.SpawnEffect(coinEmitterResource, new EffectData()
                                {
                                    origin = self.transform.position,
                                    genericFloat = (float)goldReward
                                }, true);
                                Util.PlaySound("Play_UI_coin", self.gameObject);
                            }

                            return;
                        }
                    }
                    if (ModConfig.isUsingShareSuite && !ModConfig.ShareSuiteItemSharingEnabled.Value)
                    {
                        List<BazaarItem> bazaarItems = bazaar.GetBazaarItems();
                        for (int i = 0; i < bazaarItems.Count; i++)
                        {
                            PurchaseInteraction bazaarPI = bazaarItems[i].chestBehavior.GetComponent<PurchaseInteraction>();
                            if (bazaarPI.Equals(self))
                            {
                                CharacterMaster master = activator.GetComponent<CharacterBody>().master;
                                master.inventory.GiveItem(bazaarItems[i].pickupIndex.itemIndex);
                                master.GiveMoney((uint)-self.cost);
                                bazaarItems[i].purchaseCount++;
                                if (!bazaar.IsChestStillAvailable(bazaarItems[i]))
                                {
                                    self.GetComponent<PurchaseInteraction>().SetAvailable(false);
                                }
                                Vector3 effectPos = self.transform.position;
                                effectPos.y -= 1;
                                EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData()
                                {
                                    origin = effectPos,
                                    rotation = Quaternion.identity,
                                    scale = 0.01f,
                                    color = (Color32)Color.yellow
                                }, true);
                                PurchaseInteraction.CreateItemTakenOrb(self.gameObject.transform.position, activator.GetComponent<CharacterBody>().gameObject, bazaarItems[i].pickupIndex.itemIndex);

                                return;
                            }
                        }
                    }
                }
                orig(self, activator);

            };

            On.RoR2.FireworkLauncher.FireMissile += (orig, self) =>
            {
                if(NetworkServer.active)
                {
                    if (isCurrentStageBazaar)
                    {
                        if (bazaar.IsMoneyLunarPod(self.gameObject.transform.position))
                        {
                            self.remaining = 0;
                            return;
                        }
                        
                    }
                    orig(self);
                }
                
            };

            On.RoR2.GenericPickupController.OnSerialize += (orig, self, writer, forceAll) =>
            {
                if (NetworkServer.active)
                {
                    if (isCurrentStageBazaar)
                    {
                        List<BazaarItem> bazaarItems = bazaar.GetBazaarItems();
                        for (int i = 0; i < bazaarItems.Count; i++)
                        {
                            if (bazaarItems[i].genericPickupController == self)
                            {
                                if (forceAll)
                                {
                                    GeneratedNetworkCode._WritePickupIndex_None(writer, bazaarItems[i].pickupIndex);
                                    return true;
                                }
                                bool flag = false;
                                if ((self.GetComponent<NetworkBehaviour>().GetFieldValue<uint>("m_SyncVarDirtyBits") & 1u) != 0u)
                                {
                                    if (!flag)
                                    {
                                        writer.WritePackedUInt32(self.GetComponent<NetworkBehaviour>().GetFieldValue<uint>("m_SyncVarDirtyBits"));
                                        flag = true;
                                    }
                                    GeneratedNetworkCode._WritePickupIndex_None(writer, bazaarItems[i].pickupIndex);
                                }
                                if (!flag)
                                {
                                    writer.WritePackedUInt32(self.GetComponent<NetworkBehaviour>().GetFieldValue<uint>("m_SyncVarDirtyBits"));
                                }
                                return flag;
                            }
                        }
                    }
                }
                return orig(self, writer, forceAll);
            };

            On.RoR2.SceneDirector.Start += (orig, self) =>
            {
                if (NetworkServer.active)
                {
                    if (SceneManager.GetActiveScene().name.Contains("bazaar"))
                    {
                        isCurrentStageBazaar = true;
                        // only the case if you start the run in the bazaar
                        if (Run.instance.stageClearCount == 0)
                        {
                            bazaar.ResetBazaarPlayers();
                            bazaar.CalcDifficultyCoefficient();
                        }
                        bazaar.StartBazaar();
                    }
                    else
                    {
                        isCurrentStageBazaar = false;
                        orig(self);
                        bazaar.currentDifficultyCoefficient = Run.instance.difficultyCoefficient;

                    }
                }
            };

            On.RoR2.Run.AdvanceStage += (orig, self, nextSceneDef) =>
            {
                if (!SceneExitController.isRunning)
                {
                    // advancing stage through cheats!
                    if (nextSceneDef.baseSceneName == "bazaar")
                    {
                        bazaar.ResetBazaarPlayers();
                    }
                }
                orig(self, nextSceneDef);
            };

        }

    }

}
