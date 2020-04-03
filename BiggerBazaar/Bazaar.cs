using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace BiggerBazaar
{
    class Bazaar : MonoBehaviour
    {
        List<BazaarItem> bazaarItems = new List<BazaarItem>();
        List<GameObject> displayItems = new List<GameObject>();
        List<Vector3> bazaarItemPositions = new List<Vector3>();
        List<Vector3> bazaarItemRotations = new List<Vector3>();
        List<BazaarPlayer> bazaarPlayers = new List<BazaarPlayer>();
        readonly Random r = new Random();
        readonly int bazaarItemAmount = 6;
        GameObject moneyLunarPod;
        Vector3 moneyPodPosition = new Vector3(-118.9f, -23.4f, -45.4f);
        float totalTierRarity = 0;
        public bool isUsingexperimentalScaling = false;
        int priceScaledLunarPodBaseCost;
        Dictionary<ItemTier, float> tierRatio = new Dictionary<ItemTier, float>();
        BarrelInteraction barrelInteraction;



        public Bazaar()
        {
            FillBazaarItemPositionsAndRotations();
            SetTotalTierRarity();
            tierRatio.Add(ItemTier.Tier1, 1f);
            tierRatio.Add(ItemTier.Tier2, (float)ModConfig.tier2Cost.Value / ModConfig.tier1Cost.Value);
            tierRatio.Add(ItemTier.Tier3, (float)ModConfig.tier3Cost.Value / ModConfig.tier1Cost.Value);
        }

        private void SetTotalTierRarity()
        {
            float total = 0;
            if (ModConfig.tier1Rarity.Value > 0)
                total += ModConfig.tier1Rarity.Value;
            if (ModConfig.tier2Rarity.Value > 0)
                total += ModConfig.tier2Rarity.Value;
            if (ModConfig.tier3Rarity.Value > 0)
                total += ModConfig.tier3Rarity.Value;

            totalTierRarity = total;
        }


        // bazaar building
        private void SpawnBazaarItemAt(Vector3 position, Vector3 rotation, ItemTier itemTier, int cost)
        {

            // chest players interact with
            SpawnCard chestCard = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscChest1");
            DirectorPlacementRule placementRule = new DirectorPlacementRule();
            placementRule.placementMode = DirectorPlacementRule.PlacementMode.Direct;
            GameObject chest = chestCard.DoSpawn(position, Quaternion.Euler(new Vector3(0f, 0f, 0f)), new DirectorSpawnRequest(chestCard, placementRule, Run.instance.runRNG)).spawnedInstance;
            chest.transform.eulerAngles = rotation;

            List<ItemIndex> drops = ItemDropAPI.GetDefaultDropList(itemTier);
            int rItemIndex = r.Next(drops.Count);
            ItemIndex rItem = drops[rItemIndex];
           
            GameObject itemGameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/GenericPickup"), position, Quaternion.identity);
            itemGameObject.GetComponent<GenericPickupController>().transform.Translate(-2f, 4f, -3f, Space.World);
            itemGameObject.GetComponent<GenericPickupController>().NetworkpickupIndex = PickupCatalog.FindPickupIndex(rItem);

            displayItems.Add(itemGameObject);
            NetworkServer.Spawn(itemGameObject);

            if(ModConfig.chestCostType.Value == 1)
            {
                chest.GetComponent<PurchaseInteraction>().costType = CostTypeIndex.LunarCoin;
                chest.GetComponent<PurchaseInteraction>().Networkcost = cost;
            }
            else if(cost == -1) {
                chest.GetComponent<PurchaseInteraction>().Networkcost = GetDifficultyScaledCostFromItemTier(ItemCatalog.GetItemDef(rItem).tier);
            } else
            {
                chest.GetComponent<PurchaseInteraction>().Networkcost = GetDifficultyScaledCost(cost);
            }
            


            BazaarItem bazaarItem = new BazaarItem();
            bazaarItem.chestBehavior = chest.GetComponent<ChestBehavior>();
            bazaarItem.genericPickupController = itemGameObject.GetComponent<GenericPickupController>();
            bazaarItem.pickupIndex = PickupCatalog.FindPickupIndex(rItem);
            bazaarItem.purchaseCount = 0;
            bazaarItem.maxPurchases = GetMaxPurchaseAmount(itemTier);

            bazaarItems.Add(bazaarItem);
            itemGameObject.GetComponent<GenericPickupController>().pickupIndex = PickupIndex.none;
        }

        private void SpawnMoneyLunarPod(Vector3 moneyPodPosition)
        {
            SpawnCard chestCard = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscLunarChest");
            DirectorPlacementRule placementRule = new DirectorPlacementRule();
            placementRule.placementMode = DirectorPlacementRule.PlacementMode.Direct;
            moneyLunarPod = chestCard.DoSpawn(moneyPodPosition, Quaternion.identity, new DirectorSpawnRequest(chestCard, placementRule, Run.instance.runRNG)).spawnedInstance;
            displayItems.Add(moneyLunarPod);
        }

        private void FillBazaarItemPositionsAndRotations()
        {
            bazaarItemPositions.Add(new Vector3(-99.1f, -26.0f, -54.6f));
            bazaarItemPositions.Add(new Vector3(-95.0f, -26.2f, -57.7f));
            bazaarItemPositions.Add(new Vector3(-90.9f, -26.0f, -60.7f));
            bazaarItemPositions.Add(new Vector3(-86.5f, -26.2f, -63.8f));
            bazaarItemPositions.Add(new Vector3(-82.3f, -25.0f, -67.0f));
            bazaarItemPositions.Add(new Vector3(-78.0f, -24.7f, -70.3f));

            bazaarItemRotations.Add(new Vector3(0.0f, 34.4f, 0.0f));
            bazaarItemRotations.Add(new Vector3(0.0f, 35.2f, 0.0f));
            bazaarItemRotations.Add(new Vector3(0.0f, 36.0f, 0.0f));
            bazaarItemRotations.Add(new Vector3(0.0f, 37.4f, 0.0f));
            bazaarItemRotations.Add(new Vector3(0.0f, 38.2f, 0.0f));
            bazaarItemRotations.Add(new Vector3(0.0f, 37.2f, 0.0f));
        }

        // select bazaar items
        private List<ItemTier> PickRandomWeightedBazaarItemTiers(int bazaarItemAmount)
        {

            if (totalTierRarity == 0)
                return PickRandomBazaarItemTiers(bazaarItemAmount);

            float t1 = ModConfig.tier1Rarity.Value / totalTierRarity;
            float t2 = ModConfig.tier2Rarity.Value / totalTierRarity;
            //float t3 = ModConfig.tier3Rarity.Value / totalTierRarity;

            List<ItemTier> itemTiers = new List<ItemTier>();

            for (int i = 0; i < bazaarItemAmount; i++)
            {
                var next = r.NextDouble();
                if (next <= t1)
                {
                    itemTiers.Add(ItemTier.Tier1);
                    continue;
                }
                else
                if (next <= t1 + t2)
                {
                    itemTiers.Add(ItemTier.Tier2);
                    continue;
                }
                else
                {
                    itemTiers.Add(ItemTier.Tier3);
                    continue;
                }
            }

            return itemTiers;
        }

        internal void ShareSuiteMoneyFix(Interactor activator, int money)
        {
            barrelInteraction.goldReward = money;
            barrelInteraction.expReward = 0;
            barrelInteraction.OnInteractionBegin(activator);
        }

        private List<ItemTier> PickRandomBazaarItemTiers(int bazaarItemAmount)
        {
            List<ItemTier> itemTiers = new List<ItemTier>();

            for (int i = 0; i < bazaarItemAmount; i++)
            {
                int randomTierNumber = r.Next(3);
                ItemTier itemTier = ItemTier.NoTier;
                switch (randomTierNumber)
                {
                    case 0:
                        itemTier = ItemTier.Tier1;
                        break;
                    case 1:
                        itemTier = ItemTier.Tier2;
                        break;
                    case 2:
                        itemTier = ItemTier.Tier3;
                        break;
                }
                itemTiers.Add(itemTier);
            }

            return itemTiers;
        }

        // scale item cost
        private int GetDifficultyScaledCostFromItemTier(ItemTier itemTier)
        {
            int baseCost = 0;
            switch (itemTier)
            {
                case ItemTier.Tier1:
                    baseCost = ModConfig.tier1Cost.Value;
                    break;
                case ItemTier.Tier2:
                    baseCost = ModConfig.tier2Cost.Value;
                    break;
                case ItemTier.Tier3:
                    baseCost = ModConfig.tier3Cost.Value;
                    break;
            }

            return (int)((double)baseCost * (double)Mathf.Pow(CurrentDifficultyCoefficient, 1.25f));
        }

        internal bool PlayerHasTierPurchasesLeft(ItemTier itemTier, BazaarPlayer bazaarPlayer)
        {
            if(itemTier == ItemTier.Tier1)
            {
                if (ModConfig.maxPlayerPurchasesTier1.Value == -1 || bazaarPlayer.tier1Purchases < ModConfig.maxPlayerPurchasesTier1.Value)
                    return true;
            } else if(itemTier == ItemTier.Tier2)
            {
                if (ModConfig.maxPlayerPurchasesTier2.Value == -1 || bazaarPlayer.tier2Purchases < ModConfig.maxPlayerPurchasesTier2.Value)
                    return true;
            } else if(itemTier == ItemTier.Tier3)
            {
                if (ModConfig.maxPlayerPurchasesTier3.Value == -1 || bazaarPlayer.tier3Purchases < ModConfig.maxPlayerPurchasesTier3.Value)
                    return true;
            }
            return false;
        }

        private int GetDifficultyScaledCost(int baseCost)
        {
            return (int)((double)baseCost * (double)Mathf.Pow(CurrentDifficultyCoefficient, 1.25f));
        }

        private uint GetDifficultyUnscaledCost(uint cost)
        {
            return (uint)((double)cost / (double)Mathf.Pow(CurrentDifficultyCoefficient, 1.25f));
        }

        //
        private int GetMaxPurchaseAmount(ItemTier itemTier)
        {
            switch (itemTier)
            {
                case ItemTier.Tier1:
                    return ModConfig.maxChestPurchasesTier1.Value;
                case ItemTier.Tier2:
                    return ModConfig.maxChestPurchasesTier2.Value;
                case ItemTier.Tier3:
                    return ModConfig.maxChestPurchasesTier3.Value;
            }

            return 0;
        }

        // Bazaar players
        private void CreateBazaarPlayers()
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                PlayerCharacterMasterController pcmc = PlayerCharacterMasterController.instances[i];
                BazaarPlayer pm = new BazaarPlayer(pcmc.networkUser, pcmc.master.money);
                bazaarPlayers.Add(pm);
                pcmc.master.money = 0;
            }
        }



        //
        public bool IsDisplayItem(GameObject gameObject)
        {
            if (displayItems.Contains(gameObject))
            {
                return true;
            }
            return false;
        }

        public void ResetBazaarPlayers()
        {
            bazaarPlayers.Clear();
            CreateBazaarPlayers();
        }

        public List<BazaarItem> GetBazaarItems()
        {
            return bazaarItems;
        }

        public int GetBazaarItemAmount()
        {
            return bazaarItemAmount;
        }

        public bool IsChestStillAvailable(BazaarItem bazaarItem)
        {
            if (bazaarItem.maxPurchases == -1 || bazaarItem.purchaseCount < bazaarItem.maxPurchases)
            {
                return true;
            }
            return false;
        }

        public bool IsMoneyLunarPodAvailable()
        {
            return moneyLunarPod != null;
        }

        public bool IsMoneyLunarPod(GameObject gameObject)
        {
            if(gameObject == moneyLunarPod.GetComponent<PurchaseInteraction>().gameObject)
            {
                return true;
            }
            return false;
        }

        public bool IsMoneyLunarPod(Vector3 PodPosition)
        {
            Vector3 offset = PodPosition - moneyPodPosition;
            float sqrLen = offset.sqrMagnitude;
            float maxDistance = 3f;
            if (sqrLen < maxDistance * maxDistance)
            {
                return true;
            }
            return false;
        }

        public bool PlayerHasPurchasesLeft(BazaarPlayer bazaarPlayer)
        {
            if(ModConfig.maxPlayerPurchases.Value > -1)
            {
                if(bazaarPlayer.chestPurchases >= ModConfig.maxPlayerPurchases.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public BazaarPlayer GetBazaarPlayer(NetworkUser networkUser)
        {
            for (int i = 0; i < bazaarPlayers.Count; i++)
            {
                if (bazaarPlayers[i].networkUser == networkUser)
                {
                    return bazaarPlayers[i];
                }
            }

            return null;
        }

        public int GetLunarCoinExchangeMoney()
        {
            if(!isUsingexperimentalScaling)
            {
                return GetDifficultyScaledCost(ModConfig.lunarCoinWorth.Value);
            } else
            {
                return GetDifficultyScaledCost(priceScaledLunarPodBaseCost);
            }
            
        }

        public List<BazaarPlayer> GetBazaarPlayers()
        {
            return bazaarPlayers;
        }

        private void ClearBazaarItems()
        {
            displayItems.Clear();
            bazaarItems.Clear();
        }

        public void StartBazaar(BiggerBazaar biggerBazaar)
        {
            isUsingexperimentalScaling = false;
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                for (int j = 0; j < bazaarPlayers.Count; j++)
                {
                    if (bazaarPlayers[j].networkUser == PlayerCharacterMasterController.instances[i].networkUser)
                    {
                        if(!ModConfig.IsShareSuiteMoneySharing())
                        {
                            PlayerCharacterMasterController.instances[i].master.money = bazaarPlayers[j].money;
                        } else
                        {
                            biggerBazaar.StartCoroutine(TriggerInteractorBarrelInteraction(PlayerCharacterMasterController.instances[i].master, (int)bazaarPlayers[j].money));
                            goto done;
                        }
                       
                        break;
                    }
                }
            }
            done:;
            ClearBazaarItems();
            List<ItemTier> bazaarItemTiers;
            bazaarItemTiers = PickRandomWeightedBazaarItemTiers(bazaarItemAmount);

            uint playerMoney = 0;
            for (int i = 0; i < bazaarPlayers.Count; i++)
            {
                playerMoney += bazaarPlayers[i].money;
            }

            if (ModConfig.chestCostType.Value == 1) {
                for (int i = 0; i < bazaarItemTiers.Count; i++)
                {
                    int chestLunarCost;
                    if(bazaarItemTiers[i] == ItemTier.Tier1)
                    {
                        chestLunarCost = ModConfig.tier1CostLunar.Value;
                    } else if(bazaarItemTiers[i] == ItemTier.Tier2)
                    {
                        chestLunarCost = ModConfig.tier2CostLunar.Value;
                    } else
                    {
                        chestLunarCost = ModConfig.tier3CostLunar.Value;
                    }
                    SpawnBazaarItemAt(bazaarItemPositions[i], bazaarItemRotations[i], bazaarItemTiers[i], chestLunarCost);
                }
            }
            // experimental price scaling
            else if (ModConfig.experimentalPriceScaling.Value && DoPlayersHaveTooMuchMoney(playerMoney, bazaarItemTiers))
            {
                isUsingexperimentalScaling = true;
                uint unscaledPlayerMoney = GetDifficultyUnscaledCost(playerMoney);
                float chestUnits = 0f;

                for (int i = 0; i < bazaarItemTiers.Count; i++)
                {
                    if (bazaarItemTiers[i] == ItemTier.Tier1)
                    {
                        chestUnits += 1f * ModConfig.maxChestPurchasesTier1.Value;
                    }
                    else if (bazaarItemTiers[i] == ItemTier.Tier2)
                    {
                        chestUnits += tierRatio[bazaarItemTiers[i]] * ModConfig.maxChestPurchasesTier2.Value;
                    }
                    else if (bazaarItemTiers[i] == ItemTier.Tier3)
                    {
                        chestUnits += tierRatio[bazaarItemTiers[i]] * ModConfig.maxChestPurchasesTier3.Value;
                    }
                }

                int tier1BaseCost = (int)(unscaledPlayerMoney / chestUnits);
                double randomMult = r.NextDouble() * (ModConfig.experimentalPriceScalingMaxPercent.Value - ModConfig.experimentalPriceScalingMinPercent.Value) + ModConfig.experimentalPriceScalingMinPercent.Value;
                for (int i = 0; i < bazaarItemTiers.Count; i++)
                {
                    tierRatio.TryGetValue(bazaarItemTiers[i], out float val);
                    int scaledCost = (int)(tier1BaseCost * ((1f / randomMult) * val));
                    //Debug.Log("Tier " + bazaarItemTiers[i] + ": " + scaledCost);
                    SpawnBazaarItemAt(bazaarItemPositions[i], bazaarItemRotations[i], bazaarItemTiers[i], scaledCost);
                }
                priceScaledLunarPodBaseCost = (int)(tier1BaseCost * ((1f / randomMult) * tierRatio[ItemTier.Tier2]));

            } else if(!ModConfig.experimentalPriceScaling.Value)
            // regular price
            {

                for (int i = 0; i < bazaarItemTiers.Count; i++)
                {
                    SpawnBazaarItemAt(bazaarItemPositions[i], bazaarItemRotations[i], bazaarItemTiers[i], -1);
                }

            } 
            
            if (ModConfig.maxLunarExchanges.Value != 0) 
                SpawnMoneyLunarPod(moneyPodPosition);

            if(ModConfig.isUsingShareSuite)
            {
                // if money sharing spawn a barrel interaction that handles giving money because of the ShareSuite money sharing issue
                if(ModConfig.IsShareSuiteMoneySharing())
                {
                    SpawnCard barrelCard = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscBarrel1");
                    Vector3 barrelPosition = new Vector3(200f, 200f, 200f);
                    DirectorPlacementRule placementRule = new DirectorPlacementRule();
                    placementRule.placementMode = DirectorPlacementRule.PlacementMode.Direct;
                    GameObject barrelGameObject = barrelCard.DoSpawn(barrelPosition, Quaternion.identity, new DirectorSpawnRequest(barrelCard, placementRule, Run.instance.runRNG)).spawnedInstance;
                    barrelInteraction = barrelGameObject.GetComponent<BarrelInteraction>();
                }
            }
        }

        IEnumerator TriggerInteractorBarrelInteraction(CharacterMaster master, int money)
        {
            yield return new WaitUntil(() => master.GetBody() != null);
            yield return new WaitUntil(() => master.GetBody().gameObject.GetComponentInChildren<Interactor>() != null);
            ShareSuiteMoneyFix(master.GetBody().gameObject.GetComponentInChildren<Interactor>(), money - Math.Abs((int)master.money));
        }

        public float CurrentDifficultyCoefficient
        {
            get; set;
        }

        public void CalcDifficultyCoefficient()
        {
            Run run = Run.instance;
            float num = run.GetRunStopwatch();
            DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
            float num2 = Mathf.Floor(num * 0.0166666675f);
            float num3 = (float)run.participatingPlayerCount * 0.3f;
            float num4 = 0.7f + num3;
            float num5 = 0.7f + num3;
            float num6 = Mathf.Pow((float)run.participatingPlayerCount, 0.2f);
            float num7 = 0.046f * difficultyDef.scalingValue * num6;
            float num8 = 0.046f * difficultyDef.scalingValue * num6;
            float num9 = Mathf.Pow(1.15f, (float)run.stageClearCount);
            //this.compensatedDifficultyCoefficient = (num5 + num8 * num2) * num9;
            CurrentDifficultyCoefficient = (num4 + num7 * num2) * num9;
        }

        public bool DoPlayersHaveTooMuchMoney(uint playersMoney, List<ItemTier> bazaarItemTiers)
        {
            uint allChestCost = 0;
            for (int i = 0; i < bazaarItemTiers.Count; i++)
            {
                if (bazaarItemTiers[i] == ItemTier.Tier1)
                {
                    allChestCost += (uint)(GetDifficultyScaledCostFromItemTier(ItemTier.Tier1) * ModConfig.maxChestPurchasesTier1.Value);
                }
                else if (bazaarItemTiers[i] == ItemTier.Tier2)
                {
                    allChestCost += (uint)(GetDifficultyScaledCostFromItemTier(ItemTier.Tier2) * ModConfig.maxChestPurchasesTier2.Value);
                }
                else if (bazaarItemTiers[i] == ItemTier.Tier3)
                {
                    allChestCost += (uint)(GetDifficultyScaledCostFromItemTier(ItemTier.Tier3) * ModConfig.maxChestPurchasesTier3.Value);
                }
            }

            if (playersMoney > (allChestCost*ModConfig.experimentalPriceScalingMaxPercent.Value))
                return true;
            return false;
        }
    }
}
