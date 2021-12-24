// -----------------------------------------------------------------------
// <copyright file="KeycardProcessCollisionPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1118 // Parameter should not span multiple lines

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using Mistaken.API.Extensions;
using NorthwoodLib.Pools;
using UnityEngine;

namespace Mistaken.BetterDoors
{
    [HarmonyPatch(typeof(KeycardPickup), nameof(KeycardPickup.ProcessCollision), new Type[] { typeof(Collision) })]
    internal static class KeycardProcessCollisionPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ldloc_0);
            newInstructions.InsertRange(
                index,
                new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(KeycardProcessCollisionPatch), nameof(KeycardProcessCollisionPatch.Run))),
                    new CodeInstruction(OpCodes.Ret),
                });

            index += 4;

            for (int z = 0; z < index; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }

        private static void Run(KeycardPickup instance, RegularDoorButton regularDoorButton)
        {
            DateTime start = DateTime.UtcNow;
            DoorVariant doorVariant;
            if ((doorVariant = regularDoorButton.Target as DoorVariant) == null)
            {
                API.Diagnostics.MasterHandler.LogTime("Mistaken.BetterDoors.KeycardProcessCollisionPatch", "OnCollisionEnter", start, DateTime.UtcNow);
                return;
            }

            if (!InventoryItemLoader.AvailableItems.TryGetValue(instance.Info.ItemId, out ItemBase item))
            {
                API.Diagnostics.MasterHandler.LogTime("Mistaken.BetterDoors.KeycardProcessCollisionPatch", "OnCollisionEnter", start, DateTime.UtcNow);
                return;
            }

            if (doorVariant.AllowInteracting(null, regularDoorButton.ColliderId))
            {
                var player = Player.Get(instance.PreviousOwner.Hub);
                var ev = new InteractingDoorEventArgs(player, doorVariant, doorVariant.ActiveLocks == 0 && doorVariant.RequiredPermissions.CheckPermissions(item, null));
                player.SetSessionVariable(API.SessionVarType.THROWN_ITEM, instance);
                Exiled.Events.Handlers.Player.OnInteractingDoor(ev);
                MEC.Timing.CallDelayed(1, () => player.RemoveSessionVariable(API.SessionVarType.THROWN_ITEM));

                if (ev.IsAllowed)
                    ev.Door.IsOpen = !ev.Door.IsOpen;
                else
                    ev.Door.Base.PermissionsDenied(null, regularDoorButton.ColliderId);
            }

            API.Diagnostics.MasterHandler.LogTime("Mistaken.BetterDoors.KeycardProcessCollisionPatch", "OnCollisionEnter", start, DateTime.UtcNow);
        }
    }
}
