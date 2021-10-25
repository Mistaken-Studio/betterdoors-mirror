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
            DoorVariant doorVariant;
            if ((doorVariant = regularDoorButton.Target as DoorVariant) == null)
                return;

            if (!InventoryItemLoader.AvailableItems.TryGetValue(instance.Info.ItemId, out ItemBase item))
                return;

            if (doorVariant.AllowInteracting(null, regularDoorButton.ColliderId))
            {
                var ev = new InteractingDoorEventArgs(Player.Get(instance.PreviousOwner.Hub), doorVariant, doorVariant.ActiveLocks == 0 && doorVariant.RequiredPermissions.CheckPermissions(item, null));
                Exiled.Events.Handlers.Player.OnInteractingDoor(ev);

                if (ev.IsAllowed)
                    ev.Door.IsOpen = !ev.Door.IsOpen;
                else
                    ev.Door.Base.PermissionsDenied(null, regularDoorButton.ColliderId);
            }
        }
    }
}
