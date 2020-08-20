using RoR2;
using BepInEx;
using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;
using BepInEx.Configuration;
using UnityEngine.Networking;
using RoR2.Hologram;
using System;
using System.Linq;
using R2API.Utils;
using RoR2.Networking;

namespace ShrineOfDio
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.ShrineOfDio", "ShrineOfDio", "1.3.4")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ShrineOfDio : BaseUnityPlugin
    {

        public static ConfigEntry<bool> UseBalancedMode;
        public static ConfigEntry<int> ResurrectionCost;
        public static ConfigEntry<bool> AllowDuringTeleporterCharge;

        public const int UNINITIALIZED = -2;
        public const int BALANCED_MODE = -1;

        public int clientCost = UNINITIALIZED;
        public bool isBalancedMode = false;


    public void Awake()
        {
            InitConfig();

            //On.RoR2.PurchaseInteraction.OnTeleporterBeginCharging += PurchaseInteraction_OnTeleporterBeginCharging;

            On.RoR2.OutsideInteractableLocker.LockPurchasable += OutsideInteractableLocker_LockPurchasable;

            On.RoR2.SceneDirector.PlaceTeleporter += (orig, self) =>
            //On.RoR2.SceneDirector.PopulateScene += (orig, self) =>
            {
                orig(self);
                if (!RoR2Application.isInSinglePlayer)
                {
                    SpawnShrineOfDio(self);
                }
                
            };

            On.RoR2.ShrineHealingBehavior.FixedUpdate += (orig, self) =>
            {
                orig(self);
                if(!NetworkServer.active)
                {
                    if(clientCost == UNINITIALIZED)
                    {
                        int piCost = self.GetFieldValue<PurchaseInteraction>("purchaseInteraction").cost;
                        if(piCost != clientCost)
                        {
                            clientCost = piCost;
                            if (clientCost == BALANCED_MODE)
                            {
                                isBalancedMode = true;
                            }
                            else
                            {
                                isBalancedMode = false;
                            }
                            Type[] arr = ((IEnumerable<System.Type>)typeof(ChestRevealer).Assembly.GetTypes()).Where<System.Type>((Func<System.Type, bool>)(t => typeof(IInteractable).IsAssignableFrom(t))).ToArray<System.Type>();
                            for (int i = 0; i < arr.Length; i++)
                            {
                                foreach (UnityEngine.MonoBehaviour instances in InstanceTracker.FindInstancesEnumerable(arr[i]))
                                {
                                    if (((IInteractable)instances).ShouldShowOnScanner())
                                    {
                                        string item = ((IInteractable)instances).ToString().ToLower();
                                        if (item.Contains("shrinehealing"))
                                        {
                                            UpdateShrineDisplay(instances.GetComponentInParent<ShrineHealingBehavior>());
                                        }
                                    };
                                }
                            }

                        }
                    }
                }
            };

            On.RoR2.ShrineHealingBehavior.Awake += (orig, self) =>
            {
                orig(self);
                if (!RoR2Application.isInSinglePlayer)
                {
                    PurchaseInteraction pi = self.GetFieldValue<PurchaseInteraction>("purchaseInteraction");
                    pi.contextToken = "Offer to the Shrine of Dio";
                    pi.displayNameToken = "Shrine of Dio";
                    self.costMultiplierPerPurchase = 1f;
                    

                    if (NetworkServer.active)
                    {
                        isBalancedMode = UseBalancedMode.Value;
                        if (UseBalancedMode.Value)
                        {
                            pi.costType = CostTypeIndex.None;
                            pi.cost = BALANCED_MODE;
                            pi.GetComponent<HologramProjector>().displayDistance = 0f;
                            self.GetComponent<HologramProjector>().displayDistance = 0f;
                        }
                        else
                        {
                            pi.costType = CostTypeIndex.Money;
                            pi.cost = ResurrectionCost.Value;
                        }
                    }
                    else
                    {
                        clientCost = UNINITIALIZED;
                    }



                }

            };

            On.RoR2.ShrineHealingBehavior.AddShrineStack += (orig, self, interactor) =>
            {
                if (!RoR2Application.isInSinglePlayer)
                {
                    if (NetworkServer.active)
                    {
                        PlayerCharacterMasterController deadCharacter = GetRandomDeadCharacter();
                        deadCharacter.master.Respawn(deadCharacter.master.GetFieldValue<Vector3>("deathFootPosition"), deadCharacter.master.transform.rotation, true);

                        string resurrectionMessage = $"<color=#beeca1>{interactor.GetComponent<CharacterBody>().GetUserName()}</color> resurrected <color=#beeca1>{deadCharacter.networkUser.userName}</color>";
                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                        {
                            baseToken = resurrectionMessage
                        });

                        GameObject spawnEffect = Resources.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
                        EffectManager.SpawnEffect(spawnEffect, new EffectData
                        {
                            origin = deadCharacter.master.GetBody().footPosition,
                            rotation = deadCharacter.master.gameObject.transform.rotation
                        }, true);
                        self.SetFieldValue("waitingForRefresh", true);
                        self.SetFieldValue("refreshTimer", 2f);
                        EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData()
                        {
                            origin = self.transform.position,
                            rotation = Quaternion.identity,
                            scale = 1f,
                            color = (Color32)Color.green
                        }, true);
                        // dio
                        CharacterBody cb = interactor.GetComponent<CharacterBody>();
                        if (UseBalancedMode.Value)
                        {
                            PurchaseInteraction pi = self.GetComponent<PurchaseInteraction>();
                            PurchaseInteraction.CreateItemTakenOrb(cb.corePosition, pi.gameObject, ItemIndex.ExtraLife);
                            cb.inventory.RemoveItem(ItemIndex.ExtraLife, 1);
                            cb.inventory.GiveItem(ItemIndex.ExtraLifeConsumed, 1);
                        }
                    }
                } else
                {
                    orig(self, interactor);
                }
                
            };

            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += (orig, self, interactor) =>
            {
                if (!RoR2Application.isInSinglePlayer)
                {
                    if (self.displayNameToken.Contains("Shrine of Dio") || self.displayNameToken.Contains("SHRINE_HEALING"))
                    {
                        if (isBalancedMode)
                        {
                            if (interactor.GetComponent<CharacterBody>().inventory.GetItemCount(ItemIndex.ExtraLife) > 0)
                            {
                                if (IsAnyoneDead())
                                {
                                    return true;
                                }
                            }
                            return false;
                        }
                        else
                        {
                            if (IsAnyoneDead())
                            {
                                return orig(self, interactor);
                            }
                            return false;
                        }

                    }
                    return orig(self, interactor);
                }
                return orig(self, interactor);

            };

        }


        private void OutsideInteractableLocker_LockPurchasable(On.RoR2.OutsideInteractableLocker.orig_LockPurchasable orig, OutsideInteractableLocker self, PurchaseInteraction purchaseInteraction)
        {
            if(AllowDuringTeleporterCharge.Value)
            {
                if (purchaseInteraction.displayNameToken.Equals("Shrine of Dio") || purchaseInteraction.displayNameToken.Contains("SHRINE_HEALING"))
                {
                    return;
                }
            }
            orig(self, purchaseInteraction);
            
        }


        private void InitConfig()
        {
            UseBalancedMode = Config.Bind(
            "Config",
            "UseBalancedMode",
            false,
            new ConfigDescription("Setting this to true will only allow you to resurrect other players for one of your Dio's Best Friend. Turning this off will allow you to instead use gold.")
            );

            ResurrectionCost = Config.Bind(
            "Config",
            "ResurrectionCost",
            300,
            new ConfigDescription("[Only active if you set UseBalancedMode to false] Cost for a resurrection. Scales with difficulty but doesn't increase each usage. Regular Chest cost is 25, Golden/Legendary Chest is 400. Default is 300.")
            );

            AllowDuringTeleporterCharge = Config.Bind(
            "Config",
            "AllowDuringTeleporterCharge",
            false,
            new ConfigDescription("Allows the Shrine of Dio to be used while the teleporter charges/prevents lock.")
            );
        }

        private void UpdateShrineDisplay(ShrineHealingBehavior self)
        {
            PurchaseInteraction pi = self.GetFieldValue<PurchaseInteraction>("purchaseInteraction");
            if (clientCost == BALANCED_MODE)
            {
                pi.costType = CostTypeIndex.None;
                pi.cost = BALANCED_MODE;
                pi.GetComponent<HologramProjector>().displayDistance = 0f;
                self.GetComponent<HologramProjector>().displayDistance = 0f;

            }
            else
            {
                pi.costType = CostTypeIndex.Money;
                pi.cost = clientCost;
            }
        }

        private int GetDifficultyScaledCost(int baseCost)
        {
            return (int)((double) baseCost * (double)Mathf.Pow(Run.instance.difficultyCoefficient, 1.25f)); // 1.25f
        }

        private PlayerCharacterMasterController GetRandomDeadCharacter()
        {
            List<PlayerCharacterMasterController> deadCharacterList = new List<PlayerCharacterMasterController>();
            foreach (PlayerCharacterMasterController enumerator in PlayerCharacterMasterController.instances)
            {
                if (enumerator.isConnected)
                {
                    if (enumerator.master.IsDeadAndOutOfLivesServer())
                    {
                        deadCharacterList.Add(enumerator);
                    }
                }
            }
            Random random = new Random();
            int index = random.Next(deadCharacterList.Count);

            return deadCharacterList[index];
        }

        private bool IsAnyoneDead()
        {
            foreach (PlayerCharacterMasterController enumerator in PlayerCharacterMasterController.instances)
            {
                //if (!enumerator.master.GetBody().healthComponent.alive)
                if (enumerator.isConnected)
                {
                    if (!enumerator.master.GetBody() || !enumerator.master.GetBody().healthComponent.alive)
                    {
                        //if(enumerator.master.networkIdentity.connectionToClient.isConnected)
                        return true;
                    }
                }
            }
            return false;
        }

        public void SpawnShrineOfDio(SceneDirector self)
        {
            Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(self.GetFieldValue<Xoroshiro128Plus>("rng").nextUlong);
            if (SceneInfo.instance.countsAsStage)
            {
                SpawnCard card = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscShrineHealing");
                GameObject gameObject3 = DirectorCore.instance.TrySpawnObject( new DirectorSpawnRequest(card, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, xoroshiro128Plus));

                if(!UseBalancedMode.Value)
                {
                    gameObject3.GetComponent<PurchaseInteraction>().Networkcost = GetDifficultyScaledCost(ResurrectionCost.Value);
                } 
            }
        }

    }
}

