using BepInEx;
using BepInEx.Bootstrap;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BiggerBazaar
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.funkfrog_sipondo.sharesuite", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.MagnusMagnuson.BiggerBazaar", "BiggerBazaar", "1.10.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class BiggerBazaar : BaseUnityPlugin
    {

        bool isCurrentStageBazaar = false;

        private void Start()
        {
            foreach (var kvp in Chainloader.PluginInfos)
            {
                if (kvp.Key == "com.funkfrog_sipondo.sharesuite")
                {
                    ModConfig.SetShareSuiteReference(kvp.Value.Instance);
                    break;
                }
            }
        }

        //public void Update()
        //{
        //    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        //    {
        //        ShareSuite.MoneySharingHooks.AddMoneyExternal(100);
        //    }
        //}

        public void Awake()
        {
            ModConfig.InitConfig(Config);


            //if (Chainloader.PluginInfos.ContainsKey("com.funkfrog_sipondo.sharesuite"))
            //{

            //    ModConfig.SetShareSuiteReference(Chainloader.PluginInfos["com.funkfrog_sipondo.sharesuite"].Instance);
            //    Debug.Log(ModConfig.GetShareSuiteReference());
            //    AddMoney = ShareSuite.GetType().GetMethod("AddMoneyExternal", BindingFlags.Static | BindingFlags.Public


            //    AddMoney = ModConfig.GetShareSuiteReference().GetType().GetMethod("AddMoneyExternal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            //}

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
                        if (!ModConfig.isShareSuiteLoaded)
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
                    NetworkUser networkUser = Util.LookUpBodyNetworkUser(activator.gameObject);
                    BazaarPlayer bazaarPlayer = bazaar.GetBazaarPlayer(networkUser);
                    if (!self.CanBeAffordedByInteractor(activator))
                    {
                        return;
                    }
                    
                    if (bazaar.IsMoneyLunarPodAvailable())
                    {
                        if (bazaar.IsMoneyLunarPod(self.gameObject))
                        {
                            
                            if (bazaarPlayer.lunarExchanges < ModConfig.maxLunarExchanges.Value || ModConfig.infiniteLunarExchanges)
                            {
                                bazaarPlayer.lunarExchanges++;
                                int money = bazaar.GetLunarCoinExchangeMoney();
                                if(!ModConfig.IsShareSuiteMoneySharing())
                                {
                                    activator.GetComponent<CharacterBody>().master.money += ((uint)money);
                                } else
                                {
                                    //ShareSuite.MoneySharingHooks.AddMoneyExternal(money);
                                    bazaar.ShareSuiteMoneyFix(activator, money);
                                }
                                //activator.GetComponent<CharacterBody>().master.money += ((uint)money);

                                //activator.GetComponent<CharacterBody>().master.GiveMoney((uint)money);
                                networkUser.DeductLunarCoins((uint)self.cost);
                                var goldReward = (int)((double)ModConfig.lunarCoinWorth.Value * (double)bazaar.CurrentDifficultyCoefficient);
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
                    // New addition that made everything less nice. Added to check if player still has purchases left
                    int bazaarChestIndex = -1;
                    List<BazaarItem> bazaarItems = bazaar.GetBazaarItems();
                    PurchaseInteraction bazaarPI;
                    for (int i = 0; i < bazaarItems.Count; i++)
                    {
                        // Fix for SavedGames. SavedGames somehow breaks the BiggerBazaar chests and BiggerBazaar breaks everything else in return :)
                        if (bazaarItems[i].chestBehavior == null)
                            continue;
                        bazaarPI = bazaarItems[i].chestBehavior.GetComponent<PurchaseInteraction>();
                        if (bazaarPI.Equals(self))
                        {
                            if (!bazaar.PlayerHasPurchasesLeft(bazaarPlayer))
                            {
                                return;
                            }
                            ItemTier tier = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(bazaarItems[i].pickupIndex).itemIndex).tier;
                            if (!bazaar.PlayerHasTierPurchasesLeft(tier, bazaarPlayer)) {
                                return;
                            }
                            if(ModConfig.ShareSuite != null && ModConfig.ShareSuiteTotalPurchaseSharing.Value)
                            {
                                bazaar.GetBazaarPlayers().ForEach(x => x.chestPurchases++);
                            } else
                            {
                                bazaarPlayer.chestPurchases++;
                            }
                            
                            bazaarPlayer.IncreaseTierPurchase(tier);
                            bazaarChestIndex = i;
                            break;
                        }
                    }
                    // Special case for ShareSuite
                    if (ModConfig.isShareSuiteActive() && !ModConfig.ShareSuiteItemSharingEnabled.Value)
                    {
                        if(bazaarChestIndex != -1) 
                        {
                            CharacterMaster master = activator.GetComponent<CharacterBody>().master;
                            master.inventory.GiveItem(PickupCatalog.GetPickupDef(bazaarItems[bazaarChestIndex].pickupIndex).itemIndex);
                            if(ModConfig.chestCostType.Value == 1)
                            {
                                var netUser = Util.LookUpBodyNetworkUser(master.GetBody());
                                netUser.DeductLunarCoins((uint)self.cost);
                            }
                            else
                            {
                                if (!ModConfig.IsShareSuiteMoneySharing())
                                {
                                    master.money -= (uint)self.cost;
                                }
                                else
                                {
                                    //ShareSuite.MoneySharingHooks.AddMoneyExternal(-self.cost);
                                    bazaar.ShareSuiteMoneyFix(activator, -self.cost);
                                }
                            }

                            bazaarItems[bazaarChestIndex].purchaseCount++;
                            if (!bazaar.IsChestStillAvailable(bazaarItems[bazaarChestIndex]))
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
                            PurchaseInteraction.CreateItemTakenOrb(self.gameObject.transform.position, activator.GetComponent<CharacterBody>().gameObject, PickupCatalog.GetPickupDef(bazaarItems[bazaarChestIndex].pickupIndex).itemIndex);

                            return;
                        
                        }
                    }
                }
                orig(self, activator);

            };

            // Trying not to hurt the shopkeep
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
                                    writer.Write(self.Recycled);
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
                                if ((self.GetComponent<NetworkBehaviour>().GetFieldValue<uint>("m_SyncVarDirtyBits") & 2u) != 0u)
                                {
                                    if (!flag)
                                    {
                                        writer.WritePackedUInt32(self.GetComponent<NetworkBehaviour>().GetFieldValue<uint>("m_SyncVarDirtyBits"));
                                        flag = true;
                                    }
                                    writer.Write(self.Recycled);
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

            //On.RoR2.BazaarController.Start += (orig, self) =>
            On.RoR2.SceneDirector.Start += (orig, self) =>
            {
                
                if (NetworkServer.active)
                {
                    if (SceneManager.GetActiveScene().name.Contains("bazaar"))
                    {
                        Debug.LogWarning("Hello, log inspector. Hooking SceneDirector.Start in the Bazaar always throws an exception, no matter what you do. Luckily, it doesn't affect anything and it's unlikely that any of the mods mentioned here are the reason you are checking the log.");
                        var sacrificeArtifactDef = ArtifactCatalog.FindArtifactDef("Sacrifice");
                        bool isUsingSacrificeArtifact = false;

                        if (RunArtifactManager.instance.IsArtifactEnabled(sacrificeArtifactDef)) {
                            if(!ModConfig.sacrificeArtifactAllowChests.Value)
                            {
                                orig(self);
                            }
                            isUsingSacrificeArtifact = true;
                            RunArtifactManager.instance.SetArtifactEnabledServer(sacrificeArtifactDef, false);
                        }
                        
                        isCurrentStageBazaar = true;
                        // only the case if you start the run in the bazaar
                        if (Run.instance.stageClearCount == 0)
                        {
                            bazaar.ResetBazaarPlayers();
                            bazaar.CalcDifficultyCoefficient();
                        }
                        bazaar.StartBazaar(this);
                        if(isUsingSacrificeArtifact)
                        {
                            RunArtifactManager.instance.SetArtifactEnabledServer(sacrificeArtifactDef, true);
                        }
                        if (ModConfig.BroadcastShopSettings.Value)
                            StartCoroutine(BroadcastShopSettings());
                    }
                    else
                    {
                        isCurrentStageBazaar = false;
                        bazaar.CurrentDifficultyCoefficient = Run.instance.difficultyCoefficient;

                    }
                }
                orig(self);
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

        IEnumerator BroadcastShopSettings()
        {
            yield return new WaitForSeconds(2);

            string bazaarSettings = "";
            bool stockSetting = false;
            string stockString = "";
            if(ModConfig.maxChestPurchasesTier1.Value != -1)
            {
                stockSetting = true;
                stockString += "<color=#FFFFFF>Tier1: " + ModConfig.maxChestPurchasesTier1.Value + "</color>";
            }
            if (ModConfig.maxChestPurchasesTier2.Value != -1)
            {
                if (stockSetting) stockString += ", ";
                stockSetting = true;
                stockString += "<color=#08EB00>Tier2: " + ModConfig.maxChestPurchasesTier2.Value + "</color>";
            }
            if (ModConfig.maxChestPurchasesTier3.Value != -1)
            {
                if (stockSetting) stockString += ", ";
                stockSetting = true;
                stockString += "<color=#FF0000>Tier3: " + ModConfig.maxChestPurchasesTier3.Value + "</color>";
            }
            if (stockSetting)
            {
                bazaarSettings += "\nBazaar stock:: " + stockString + " item(s) per chest.";
            }

            bool totalSetting = false;
            if (ModConfig.maxPlayerPurchases.Value != -1)
            {
                totalSetting = true;
                if(ModConfig.ShareSuite != null && ModConfig.ShareSuiteTotalPurchaseSharing.Value)
                {
                    bazaarSettings += "\nYour party can buy a total of " + ModConfig.maxPlayerPurchases.Value + " items.";
                }
                else
                {
                    bazaarSettings += "\nYou can buy a total of " + ModConfig.maxPlayerPurchases.Value + " items.";
                }
                
            }
            bool tierSettings = false;
            string tierString = "";
            if (ModConfig.maxPlayerPurchasesTier1.Value > 0)
            {
                tierSettings = true;
                tierString += "<color=#FFFFFF>" + ModConfig.maxPlayerPurchasesTier1.Value + " Tier1</color>";
            }
            if (ModConfig.maxPlayerPurchasesTier2.Value > 0)
            {
                if (tierSettings) tierString += ", ";
                tierSettings = true;
                tierString += "<color=#08EB00>" + ModConfig.maxPlayerPurchasesTier2.Value + " Tier2</color>";
            }
            if (ModConfig.maxPlayerPurchasesTier3.Value > 0)
            {
                if (tierSettings) tierString += ", ";
                tierSettings = true;
                tierString += "<color=#FF0000>" + ModConfig.maxPlayerPurchasesTier3.Value + " Tier3</color>";
            }
            if (tierSettings)
            {
                tierString = "\nYou can only buy up to " + tierString + " items.";
                bazaarSettings += tierString;
            }
            bazaarSettings = "--Bazaar Restrictions--<color=#BCBCBC><size=16px>" + bazaarSettings + "</size></color>";

            if(totalSetting || tierSettings || stockSetting)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = bazaarSettings
                });
            }
            
        }

    }

}
