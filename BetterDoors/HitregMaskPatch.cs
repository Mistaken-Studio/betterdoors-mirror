// -----------------------------------------------------------------------
// <copyright file="HitregMaskPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1118 // Parameter should not span multiple lines

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.MicroHID;
using UnityEngine;

namespace Mistaken.BetterDoors
{
    [HarmonyPatch(typeof(BuckshotHitreg), nameof(BuckshotHitreg.ShootPellet))]
    [HarmonyPatch(typeof(SingleBulletHitreg), nameof(SingleBulletHitreg.ServerPerformShot))]
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.ClientCalculateHit))]

    // [HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.Fire))]
    internal static class HitregMaskPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // var label = generator.DefineLabel();
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldsfld) + 2;

            // newInstructions[0].WithLabels(label);
            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldc_I4, 1 << 8),
                new CodeInstruction(OpCodes.Or),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }
    }
}
