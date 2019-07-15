using BepInEx;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using RoR2.Networking;

namespace ItemExchangeNotifier
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.ItemExchangeNotifier", "ItemExchangeNotifier", "1.3.0")]
    public class ItemExchangeNotifier : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
            {

                if (!self.CanBeAffordedByInteractor(activator))
                {
                    return;
                }
                switch (self.costType)
                {
                    case CostTypeIndex.WhiteItem:
                    case CostTypeIndex.GreenItem:
                    case CostTypeIndex.RedItem:
                        {
                            CharacterBody component = activator.GetComponent<CharacterBody>();
                            if (component)
                            {
                                Inventory inventory = component.inventory;
                                if (inventory)
                                {
                                    List<ItemCount> listBefore = new List<ItemCount>();
                                    List<ItemCount> listAfter = new List<ItemCount>();

                                    listBefore = getAllCurrentItems(inventory);

                                    orig(self, activator);

                                    listAfter = getAllCurrentItems(inventory);

                                    if (listBefore.Count > 0)
                                    {
                                        List<ItemCount> difference = new List<ItemCount>();
                                        foreach (ItemCount beforeItem in listBefore)
                                        {
                                            bool itemAppears = false;
                                            foreach (ItemCount afterItem in listAfter)
                                            {
                                                if (beforeItem.itemIndex == afterItem.itemIndex)
                                                {

                                                    itemAppears = true;
                                                    if (beforeItem.count > afterItem.count)
                                                    {
                                                        ItemCount differenceItem = new ItemCount(beforeItem.itemIndex, beforeItem.count - afterItem.count);
                                                        difference.Add(differenceItem);
                                                    }
                                                    break;
                                                }
                                            }
                                            if (!itemAppears)
                                            {
                                                ItemCount differenceItem = new ItemCount(beforeItem.itemIndex, beforeItem.count);
                                                difference.Add(differenceItem);
                                            }
                                        }
                                        string tradeInMessage = constructItemsLostString(difference);
                                        SendCustomMessage(activator, tradeInMessage);
                                    }
                                }
                                else
                                {
                                    orig(self, activator);
                                }
                            }
                            else
                            {
                                orig(self, activator);
                            }
                            break;
                        }
                    default:
                        {
                            orig(self, activator);
                            break;
                        }
                }
            };
        }

        private void SendCustomMessage(Interactor activator, string tradeInMessage)
        {
            NetworkConnection clientAuthorityOwner = activator.GetComponent<NetworkIdentity>().clientAuthorityOwner;
            Chat.SimpleChatMessage message = new Chat.SimpleChatMessage
            {
                baseToken = tradeInMessage,
            };

            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage((short)59);
            writer.Write(message.GetTypeIndex());
            writer.Write((MessageBase)message);
            writer.FinishMessage();
            clientAuthorityOwner?.SendWriter(writer, QosChannelIndex.chat.intVal);
        }

        private string constructItemsLostString(List<ItemCount> difference)
        {
            string result = "<color=#8296ae>You gave up ";
            for(int i = 0; i < difference.Count; i++)
            {
                ItemCount diffItem = difference[i];
                PickupIndex pickupIndex = new PickupIndex(diffItem.itemIndex);
                string itemName = Language.GetString(ItemCatalog.GetItemDef(diffItem.itemIndex).nameToken);
                string hexColor = "#" + ColorUtility.ToHtmlStringRGB(pickupIndex.GetPickupColor());

                result += "x" + diffItem.count + " " + "<color=" + hexColor + ">" + itemName + "</color>";
                if(i+1 < difference.Count)
                {
                    result += ", ";
                } else
                {
                    result += "</color>";
                }
            }

            return result;
        }

        private List<ItemCount> getAllCurrentItems(Inventory inv)
        {
            List<ItemCount> list = new List<ItemCount>();
            foreach (ItemIndex itemIndex in inv.itemAcquisitionOrder)
            {
                ItemCount itemCount = new ItemCount(itemIndex, inv.GetItemCount(itemIndex));
                list.Add(itemCount);
            }
            return list;
        }
    }

    class ItemCount
    {
        public ItemIndex itemIndex;
        public int count;

        public ItemCount(ItemIndex index, int count)
        {
            this.itemIndex = index;
            this.count = count;
        }

    }
}
