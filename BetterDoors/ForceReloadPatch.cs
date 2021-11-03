// -----------------------------------------------------------------------
// <copyright file="ForceReloadPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using UnityEngine;

namespace Mistaken.BetterDoors
{
    [HarmonyPatch(typeof(InventoryItemLoader), nameof(InventoryItemLoader.ForceReload))]
    internal class ForceReloadPatch
    {
        private static bool Prefix()
        {
            try
            {
                InventoryItemLoader._loadedItems = new Dictionary<ItemType, ItemBase>();
                ItemBase[] array = Resources.LoadAll<ItemBase>(InventoryItemLoader.ItemsDirectoryName);
                Array.Sort<ItemBase>(array, (x, y) =>
                {
                    int itemTypeId = (int)x.ItemTypeId;
                    return itemTypeId.CompareTo((int)y.ItemTypeId);
                });

                foreach (ItemBase itemBase in array)
                {
                    InventoryItemLoader._loadedItems[itemBase.ItemTypeId] = itemBase;
                    if (itemBase is KeycardItem)
                        continue;
                    itemBase.PickupDropModel.gameObject.AddComponent<InteractDoorWithItemComponent>();
                }

                InventoryItemLoader._loaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while loading items from the resources folder: " + ex.Message);
                InventoryItemLoader._loaded = false;
            }

            return false;
        }
    }
}
