using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace BiggerBazaar
{
    class Bazaar
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


        public Bazaar()
        {
            FillBazaarItemPositionsAndRotations();
            SetTotalTierRarity();
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
        private void SpawnBazaarItemAt(Vector3 position, Vector3 rotation, ItemTier itemTier)
        {

            // chest players interact with
            SpawnCard chestCard = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscChest1");
            GameObject chest = chestCard.DoSpawn(position, Quaternion.Euler(new Vector3(0f, 0f, 0f)), null);
            // TODO I don't know how fix the random wonky rotations when spawning a chest. This works but is obviously very hacky
            chest.transform.eulerAngles = rotation;

            List<ItemIndex> drops = ItemDropAPI.GetDefaultDropList(itemTier);
            int rItemIndex = r.Next(drops.Count);
            ItemIndex rItem = drops[rItemIndex];
           
            GameObject itemGameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/GenericPickup"), position, Quaternion.identity);
            itemGameObject.GetComponent<GenericPickupController>().transform.Translate(-2f, 4f, -3f, Space.World);
            itemGameObject.GetComponent<GenericPickupController>().NetworkpickupIndex = new PickupIndex(rItem);

            displayItems.Add(itemGameObject);
            NetworkServer.Spawn(itemGameObject);

            chest.GetComponent<PurchaseInteraction>().Networkcost = GetDifficultyScaledCostFromItemTier(ItemCatalog.GetItemDef(rItem).tier);


            BazaarItem bazaarItem = new BazaarItem();
            bazaarItem.chestBehavior = chest.GetComponent<ChestBehavior>();
            bazaarItem.genericPickupController = itemGameObject.GetComponent<GenericPickupController>();
            bazaarItem.pickupIndex = new PickupIndex(rItem);
            bazaarItem.purchaseCount = 0;
            bazaarItem.maxPurchases = GetMaxPurchaseAmount(itemTier);

            bazaarItems.Add(bazaarItem);
            itemGameObject.GetComponent<GenericPickupController>().pickupIndex = PickupIndex.none;
        }
        private void SpawnMoneyLunarPod(Vector3 moneyPodPosition)
        {
            SpawnCard chestCard = Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscLunarChest");
            moneyLunarPod = chestCard.DoSpawn(moneyPodPosition, Quaternion.identity, null);
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

            return (int)((double)baseCost * (double)Mathf.Pow(currentDifficultyCoefficient, 1.25f));
        }

        private int GetDifficultyScaledCost(int baseCost)
        {
            return (int)((double)baseCost * (double)Mathf.Pow(currentDifficultyCoefficient, 1.25f));
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
            else
            {
                return false;
            }
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
            return GetDifficultyScaledCost(ModConfig.lunarCoinWorth.Value);
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

        public void StartBazaar()
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                for (int j = 0; j < bazaarPlayers.Count; j++)
                {
                    if (bazaarPlayers[j].networkUser == PlayerCharacterMasterController.instances[i].networkUser)
                    {
                        PlayerCharacterMasterController.instances[i].master.money = bazaarPlayers[j].money;
                        break;
                    }
                }
            }
            ClearBazaarItems();
            List<ItemTier> bazaarItemTiers;
            bazaarItemTiers = PickRandomWeightedBazaarItemTiers(bazaarItemAmount);

            for (int i = 0; i < bazaarItemTiers.Count; i++)
            {
                SpawnBazaarItemAt(bazaarItemPositions[i], bazaarItemRotations[i], bazaarItemTiers[i]);
            }
            if (ModConfig.maxLunarExchanges.Value != 0)
                SpawnMoneyLunarPod(moneyPodPosition);
        }

        public float currentDifficultyCoefficient
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
            currentDifficultyCoefficient = (num4 + num7 * num2) * num9;
        }
    }
}
