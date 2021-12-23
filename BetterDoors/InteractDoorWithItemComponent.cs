// -----------------------------------------------------------------------
// <copyright file="InteractDoorWithItemComponent.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Mistaken.API.Extensions;
using UnityEngine;

namespace Mistaken.BetterDoors
{
    internal class InteractDoorWithItemComponent : MonoBehaviour
    {
        private ItemPickupBase item;

        private void Awake()
        {
            this.item = this.GetComponent<ItemPickupBase>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            DateTime start = DateTime.UtcNow;
            if (!collision.collider.TryGetComponent<RegularDoorButton>(out RegularDoorButton regularDoorButton))
            {
                API.Diagnostics.MasterHandler.LogTime("Mistaken.BetterDoors.InteractDoorWithItemComponent", "OnCollisionEnter__1", start, DateTime.UtcNow);
                return;
            }

            DoorVariant doorVariant;
            if ((doorVariant = (DoorVariant)regularDoorButton.Target) is null)
            {
                API.Diagnostics.MasterHandler.LogTime("Mistaken.BetterDoors.InteractDoorWithItemComponent", "OnCollisionEnter__2", start, DateTime.UtcNow);
                return;
            }

            if (doorVariant.RequiredPermissions.RequiredPermissions != KeycardPermissions.None)
            {
                API.Diagnostics.MasterHandler.LogTime("Mistaken.BetterDoors.InteractDoorWithItemComponent", "OnCollisionEnter__3", start, DateTime.UtcNow);
                return;
            }

            var owner = this.item.PreviousOwner.Hub;

            if (doorVariant.AllowInteracting(owner, regularDoorButton.ColliderId))
            {
                var player = Player.Get(owner);
                player.SetSessionVariable(API.SessionVarType.THROWN_ITEM, this.item);
                var ev = new Exiled.Events.EventArgs.InteractingDoorEventArgs(player, doorVariant, doorVariant.ActiveLocks == 0);
                player.RemoveSessionVariable(API.SessionVarType.THROWN_ITEM);
                Exiled.Events.Handlers.Player.OnInteractingDoor(ev);
                if (ev.IsAllowed)
                    ev.Door.IsOpen = !ev.Door.IsOpen;
                else
                    ev.Door.Base.PermissionsDenied(owner, regularDoorButton.ColliderId);
            }

            API.Diagnostics.MasterHandler.LogTime("Mistaken.BetterDoors.InteractDoorWithItemComponent", "OnCollisionEnter__0", start, DateTime.UtcNow);
        }
    }
}
