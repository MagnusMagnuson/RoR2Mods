using BepInEx;
using System;
using RoR2;
using UnityEngine;
using R2API.Utils;
using System.Collections.Generic;
using System.Collections;
using MonoMod.Cil;
using RoR2.Navigation;
using Mono.Cecil.Cil;

namespace BossRush
{
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.ReinThings.ReinDirectorCardLib")]
    [BepInPlugin("com.MagnusMagnuson.BossRush", "BossRush", "0.5.0")]
    public class BossRush : BaseUnityPlugin
    {
        //bool isFirstBazaarVisit = true;
        //bool isFirstStage = true;

        //bool isAnimating = false;
        bool isTeleporterBossDead = false;

        IMode gameMode;

        //private ConvertPlayerMoneyToExperience experienceCollector;

        void Awake()
        {
            ModConfig.InitConfig(Config);

            if(ModConfig.GameMode.Value == 0)
            {
                gameMode = new DefaultMode();
            } else if(ModConfig.GameMode.Value == 1)
            {
                gameMode = new AlternateMode();
            }

            if (ModConfig.RandomBosses.Value)
            {
                RandomBoss.AddAllBossesToDirector();
            }

            On.RoR2.ClassicStageInfo.GenerateDirectorCardWeightedSelection += ClassicStageInfo_GenerateDirectorCardWeightedSelection;

            On.RoR2.Run.OnServerTeleporterPlaced += Run_OnServerTeleporterPlaced;

            //On.RoR2.SceneDirector.Start += SceneDirector_Start;

            //On.RoR2.Run.Start += Run_Start;

            On.RoR2.BossGroup.OnDefeatedServer += BossGroup_OnDefeatedServer;

            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;

            On.RoR2.MultiShopController.CreateTerminals += MultiShopController_CreateTerminals;
             
            On.RoR2.Run.BeginStage += Run_BeginStage;

            IL.RoR2.MultiShopController.Start += MultiShopController_Start;

            On.RoR2.DeathRewards.OnKilledServer += DeathRewards_OnKilledServer;

            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;

            IL.RoR2.SceneDirector.PlacePlayerSpawnsViaNodegraph += SceneDirector_PlacePlayerSpawnsViaNodegraph;

            //On.RoR2.ConvertPlayerMoneyToExperience.Start += ConvertPlayerMoneyToExperience_Start;
        }

        private void SceneDirector_PlacePlayerSpawnsViaNodegraph(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdcI4(3),
                x => x.MatchMul(),
                x => x.MatchLdcI4(4),
                x => x.MatchDiv()
            );
            c.Index += 1;
            c.RemoveRange(2);
            c.Index += 3;
            c.Emit(OpCodes.Ldc_I4_2);
            c.Emit(OpCodes.Mul);
            c.Emit(OpCodes.Ldc_I4_5);
            c.Emit(OpCodes.Div);

        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            self.InvokeMethod("RemoveAllExistingSpawnPoints");
            orig(self);
        }

        //private void ConvertPlayerMoneyToExperience_Start(On.RoR2.ConvertPlayerMoneyToExperience.orig_Start orig, ConvertPlayerMoneyToExperience self)
        //{
        //    self.burstCount = 1;
        //    //self.burstInterval = 0.05f;
        //    orig(self);
        //}

        private void DeathRewards_OnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport)
        {

            if (isTeleporterBossDead) SaveCurrentPlayerMoney();
            orig(self, damageReport);
            GiveExpToPlayers();
            //experienceCollector = base.gameObject.AddComponent<ConvertPlayerMoneyToExperience>(); //doesn't work because any money added within a certain amount of time just evaporates
            if (damageReport.victimMaster.isBoss)
            {
                GiveMoneyToPlayers(ModConfig.MoneyGrantedPerTeleporterBossClear.Value);
            } else if (isTeleporterBossDead) 
            {
                RestoreCurrentPlayerMoney();
            }
        }

        //void Update()
        //{
        //    if(isAnimating)
        //    {
        //        foreach(MultiShop shop in MultiShop.instances)
        //        {
        //            AnimateShop(shop);
        //        }
        //    }
        //}


        // Sadly can't get the animation to work on client with Multi Shops for some reason. Chests work, but even their movement is very stuttery on the client ¯\_(ツ)_/¯ I don't know anything about unity
        //private void AnimateShop(MultiShop shop)
        //{
        //    if (shop.multiShopController.gameObject.transform.position == shop.endPosition)
        //    {
        //        isAnimating = false;
        //    }
        //    else
        //    {
        //        shop.multiShopController.gameObject.transform.position = Vector3.MoveTowards(shop.multiShopController.gameObject.transform.position, shop.endPosition, MultiShop.movementSpeed * Time.deltaTime);
        //        foreach (PurchaseInteraction terminal in shop.terminalPurchaseInteractions)
        //        {
        //            terminal.gameObject.transform.position = Vector3.MoveTowards(terminal.gameObject.transform.position, new Vector3(terminal.gameObject.transform.position.x, shop.endPosition.y + MultiShop.terminalOffset, terminal.gameObject.transform.position.z), MultiShop.movementSpeed * Time.deltaTime);
        //        }
        //    }
        //}

        private void MultiShopController_Start(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<MultiShopController>("baseCost")
            );
            c.Index -= 2;
            c.RemoveRange(6);
        }

        private void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            orig(self);
            ShopPlayer.instances.Clear();
            MultiShop.instances.Clear();
            isTeleporterBossDead = false;
            foreach (NetworkUser networkUser in NetworkUser.readOnlyInstancesList)
            {
                ShopPlayer.instances.Add(new ShopPlayer(networkUser));
            }
        }

        private void MultiShopController_CreateTerminals(On.RoR2.MultiShopController.orig_CreateTerminals orig, MultiShopController self)
        {
            gameMode.CreateTerminals(orig, self);
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (SceneInfo.instance.countsAsStage)
            {
                if (!self.CanBeAffordedByInteractor(activator))
                {
                    return;
                }
                MultiShopController usedShop = GetMultiShopController(self);
                if (usedShop != null)
                {
                    self.GetComponent<ShopTerminalBehavior>().DropPickup();
                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    characterBody.master.GiveMoney((uint)-usedShop.Networkcost);
                    gameMode.UpdateTerminals(usedShop);
                    return;
                }
            }
            orig(self, activator);
        }

        private void BossGroup_OnDefeatedServer(On.RoR2.BossGroup.orig_OnDefeatedServer orig, BossGroup self)
        {
            orig(self);
            TeleporterInteraction.instance.remainingChargeTimer = 0f;
            if(BossGroup.GetTotalBossCount() == 0) isTeleporterBossDead = true;
            StartCoroutine(Coroutine_SpawnTripleShops());
        }

        //private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        //{
        //    orig(self);
        //    SceneField sc = new SceneField("bazaar");
        //    NetworkManager.singleton.ServerChangeScene(sc);
        //    isFirstBazaarVisit = true;
        //    isFirstStage = true;
        //}

        //private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        //{
        //    if (NetworkServer.active)
        //    {
        //        if (Run.instance.stageClearCount == 0)
        //        {
        //            if (isFirstBazaarVisit && SceneManager.GetActiveScene().name.Contains("bazaar"))
        //            {
        //                GiveMoneyToPlayers(-(int)Run.instance.ruleBook.startingMoney);
        //                isFirstBazaarVisit = false;
        //            }
        //            else if (isFirstStage)
        //            {
        //                GiveMoneyToPlayers((int)Run.instance.ruleBook.startingMoney);
        //                isFirstStage = false;
        //            }
        //        }
        //    }
        //    orig(self);
        //}

        private WeightedSelection<DirectorCard> ClassicStageInfo_GenerateDirectorCardWeightedSelection(On.RoR2.ClassicStageInfo.orig_GenerateDirectorCardWeightedSelection orig, ClassicStageInfo self, DirectorCardCategorySelection categorySelection)
        {
            WeightedSelection<DirectorCard> weightedSelection = new WeightedSelection<DirectorCard>(8);
            foreach (DirectorCardCategorySelection.Category category in categorySelection.categories)
            {
                float num = categorySelection.SumAllWeightsInCategory(category);
                if (category.name.Contains("Champion"))
                {
                    foreach (DirectorCard directorCard in category.cards)
                    {
                        directorCard.cost = Mathf.RoundToInt(directorCard.cost * (1 / ModConfig.BossSpawnCostReductionMultiplier.Value));
                        weightedSelection.AddChoice(directorCard, (float)directorCard.selectionWeight / num * category.selectionWeight);    
                    }

                }

            }
            return weightedSelection;
        }

        private void Run_OnServerTeleporterPlaced(On.RoR2.Run.orig_OnServerTeleporterPlaced orig, Run self, SceneDirector sceneDirector, GameObject teleporter)
        {
            orig(self, sceneDirector, teleporter);
            TeleporterInteraction teleporterInteraction = teleporter.GetComponent<TeleporterInteraction>();
            teleporterInteraction.NetworkactivationStateInternal = 1; // 1 == idleToChargingState
        }

        private IEnumerator Coroutine_SpawnTripleShops()
        {
            int shopCount = 0;
            if (ModConfig.GameMode.Value == 0)
            {
                shopCount = ModConfig.MultiShopAmount.Value;
            } else if(ModConfig.GameMode.Value == 1)
            {
                shopCount = ModConfig.tierWeights.Count;
            }
            int tripleShopAmount = 0;
            switch (ModConfig.GameMode.Value)
            {
                case 0:
                    tripleShopAmount = ModConfig.MultiShopAmount.Value;
                    break;
                case 1:
                    tripleShopAmount = ModConfig.tierWeights.Count;
                    break;

            }
            Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
            for (int i = 0; i < tripleShopAmount; i++)
            {
                yield return new WaitForSeconds(2f / shopCount);
                SpawnShop(xoroshiro128Plus);
                
            }
            //isAnimating = true;
        }

        public void GiveMoneyToPlayers(int money)
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                PlayerCharacterMasterController.instances[i].master.GiveMoney((uint)money);
                //PlayerCharacterMasterController.instances[i].master.money = ((uint)money);
            };
        }

        private void GiveExpToPlayers()
        {
            var instances = PlayerCharacterMasterController.instances;
            for (int i = 0; i < instances.Count; i++)
            {
                GameObject gameObject = instances[i].gameObject;
                CharacterMaster component = gameObject.GetComponent<CharacterMaster>();
                uint num = component.money;
                component.money -= num;
                GameObject bodyObject = component.GetBodyObject();
                ulong num2 = (ulong)(num / 2f / (float)instances.Count);
                if (bodyObject)
                {
                    ExperienceManager.instance.AwardExperience(base.transform.position, bodyObject.GetComponent<CharacterBody>(), num2);
                }
            }
        }

        private void RestoreCurrentPlayerMoney()
        {
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                foreach (ShopPlayer shopPlayer in ShopPlayer.instances)
                {
                    if (player.networkUser == shopPlayer.networkUser)
                    {
                        player.master.money = shopPlayer.currentMoney;
                        break;
                    }
                }
            }
        }

        private void SaveCurrentPlayerMoney()
        {
            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                foreach (ShopPlayer shopPlayer in ShopPlayer.instances)
                {
                    if (player.networkUser == shopPlayer.networkUser)
                    {
                        shopPlayer.currentMoney = player.master.money;
                        break;
                    }
                }
            }
        }

        public void SpawnShop(Xoroshiro128Plus xoroshiro128Plus)
        {
            SpawnCard card = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscTripleShop");
            GameObject multiShopGameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(card, new DirectorPlacementRule
            {
                maxDistance = 30f,
                minDistance = 10f,
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                position = TeleporterInteraction.instance.transform.position
            }, xoroshiro128Plus));

            float startPosOffsetY = 0; // -7.2f;
            Vector3 endPosition = multiShopGameObject.transform.position;
            multiShopGameObject.transform.position = new Vector3(multiShopGameObject.transform.position.x, multiShopGameObject.transform.position.y + startPosOffsetY, multiShopGameObject.transform.position.z);

            MultiShopController multiShopController = multiShopGameObject.GetComponent<MultiShopController>();
            List<PurchaseInteraction> purchaseInteractionList = new List<PurchaseInteraction>();
            GameObject[] terminalGameObjects = multiShopController.GetFieldValue<GameObject[]>("terminalGameObjects");
            for (int k = 0; k < terminalGameObjects.Length; k++)
            {
                purchaseInteractionList.Add(terminalGameObjects[k].GetComponent<PurchaseInteraction>());
                terminalGameObjects[k].gameObject.transform.position = new Vector3(terminalGameObjects[k].gameObject.transform.position.x, terminalGameObjects[k].gameObject.transform.position.y + startPosOffsetY, terminalGameObjects[k].gameObject.transform.position.z);
            }

            //EffectManager.instance.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/BeetleQueenBurrow"), new EffectData()
            EffectManager.instance.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/TeleportOutBoom"), new EffectData()
            {
                origin = endPosition,
                rotation = Quaternion.identity
            }, true);

            MultiShop.terminalOffset = Math.Abs(terminalGameObjects[0].gameObject.transform.position.y - multiShopGameObject.transform.position.y);

            MultiShop multiShop = new MultiShop
            {
                multiShopController = multiShopController,
                terminalPurchaseInteractions = purchaseInteractionList,
                endPosition = endPosition
            };
            MultiShop.instances.Add(multiShop);
            
        }

        private MultiShopController GetMultiShopController(PurchaseInteraction purchaseInteraction)
        {
            foreach (MultiShop shop in MultiShop.instances)
            {
                foreach (PurchaseInteraction shopPI in shop.terminalPurchaseInteractions)
                {
                    if (shopPI == purchaseInteraction)
                    {
                        return shop.multiShopController;
                    }
                }
            }
            return null;
        }
    }
}
