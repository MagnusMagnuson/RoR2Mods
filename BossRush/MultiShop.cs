using System.Collections.Generic;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BossRush
{
    public class MultiShop
    {
        public static List<MultiShop> instances = new List<MultiShop>();
        public static List<MultiShop> firstStageInstances = new List<MultiShop>();

        public static float movementSpeed = 2.4f;
        public static float terminalOffset;

        public MultiShopController multiShopController;
        public List<PurchaseInteraction> terminalPurchaseInteractions;
        public Vector3 endPosition;
        public ItemTierShopConfig itemTierConfig;

        

        public static void CreateTerminals(On.RoR2.MultiShopController.orig_CreateTerminals orig, MultiShopController self, ItemTierShopConfig itemTierConfig)
        {
            // Boss Items and Equipment can not be part of multi shops by default, so needs a special case
            List<PickupIndex> otherItemsList = new List<PickupIndex>();
            if (itemTierConfig.itemTier == ItemTier.Boss)
            {
                var itemIndexList = R2API.ItemDropAPI.GetDefaultDropList(ItemTier.Boss);
                foreach (var itemIndex in itemIndexList)
                {
                    otherItemsList.Add(new PickupIndex(itemIndex));
                }
            }
            else if (itemTierConfig.itemTier == ItemTier.NoTier && itemTierConfig.isEquipment)
            {
                var itemIndexList = R2API.ItemDropAPI.GetDefaultEquipmentDropList();
                foreach (var itemIndex in itemIndexList)
                {
                    otherItemsList.Add(new PickupIndex(itemIndex));
                }
            }
            if (otherItemsList.Count > 0)
            {
                self.SetFieldValue("terminalGameObjects", new GameObject[self.terminalPositions.Length]);
                for (int i = 0; i < self.terminalPositions.Length; i++)
                {
                    PickupIndex newPickupIndex = PickupIndex.none;
                    Xoroshiro128Plus treasureRng = Run.instance.treasureRng;
                    newPickupIndex = treasureRng.NextElementUniform<PickupIndex>(otherItemsList);
                    bool newHidden = Run.instance.treasureRng.nextNormalizedFloat < 0.2f;
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(self.terminalPrefab, self.terminalPositions[i].position, self.terminalPositions[i].rotation);
                    self.GetFieldValue<GameObject[]>("terminalGameObjects")[i] = gameObject;
                    gameObject.GetComponent<ShopTerminalBehavior>().SetPickupIndex(newPickupIndex, newHidden);
                    NetworkServer.Spawn(gameObject);

                }
                return;
            }

            orig(self);
        }

        public static void RepopulateTerminals(MultiShopController multiShopController, ItemTierShopConfig itemTierConfig)
        {
            GameObject[] terminalGameObjects = multiShopController.GetFieldValue<GameObject[]>("terminalGameObjects");
            for (int i = 0; i < multiShopController.terminalPositions.Length; i++)
            {
                List<PickupIndex> otherItemsList = new List<PickupIndex>();
                PickupIndex newPickupIndex = PickupIndex.none;
                Xoroshiro128Plus treasureRng = Run.instance.treasureRng;
                switch (itemTierConfig.itemTier)
                {
                    case ItemTier.Tier1:
                        newPickupIndex = treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
                        break;
                    case ItemTier.Tier2:
                        newPickupIndex = treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
                        break;
                    case ItemTier.Tier3:
                        newPickupIndex = treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
                        break;
                    case ItemTier.Lunar:
                        newPickupIndex = treasureRng.NextElementUniform<PickupIndex>(Run.instance.availableLunarDropList);
                        break;
                    case ItemTier.Boss:
                        otherItemsList = new List<PickupIndex>();
                        var bossItemIndexList = R2API.ItemDropAPI.GetDefaultDropList(ItemTier.Boss);
                        foreach (var itemIndex in bossItemIndexList)
                        {
                            otherItemsList.Add(new PickupIndex(itemIndex));
                        }
                        newPickupIndex = treasureRng.NextElementUniform<PickupIndex>(otherItemsList);
                        break;
                    case ItemTier.NoTier:
                        if (itemTierConfig.isEquipment)
                        {
                            otherItemsList = new List<PickupIndex>();
                            var equipmentIndexList = R2API.ItemDropAPI.GetDefaultEquipmentDropList();
                            foreach (var itemIndex in equipmentIndexList)
                            {
                                otherItemsList.Add(new PickupIndex(itemIndex));
                            }
                            newPickupIndex = treasureRng.NextElementUniform<PickupIndex>(otherItemsList);
                            //self.itemTier = ItemTier.Tier1;
                        }
                        break;
                }
                bool newHidden = Run.instance.treasureRng.nextNormalizedFloat < 0.2f;
                terminalGameObjects[i].GetComponent<ShopTerminalBehavior>().SetPickupIndex(newPickupIndex, newHidden);
            }
        }

        public static MultiShop GetMultiShopFromController(MultiShopController multiShopController)
        {
            foreach(MultiShop multiShop in instances)
            {
                if(multiShop.multiShopController == multiShopController)
                {
                    return multiShop;
                }
            }

            return null;
        }

        public static ItemTierShopConfig RetrieveItemTierConfig(List<ItemTierShopConfig> tierWeights, ItemTier itemTier)
        {
            foreach(ItemTierShopConfig itemTierConfig in tierWeights)
            {
                if(itemTierConfig.itemTier == itemTier)
                {
                    return itemTierConfig;
                }
            }

            return null;
        }
    }
}