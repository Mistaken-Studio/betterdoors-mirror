// -----------------------------------------------------------------------
// <copyright file="ButtonTargetScript.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using Footprinting;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using UnityEngine;

namespace Mistaken.BetterDoors
{
    internal class ButtonTargetScript : MonoBehaviour, IDestructible
    {
        public uint NetworkId => this.Door.netId;

        public Vector3 CenterOfMass => Vector3.zero;

        public bool Damage(float damage, IDamageDealer src, Footprint attackerFootprint, Vector3 exactHitPos)
        {
            Log.Debug($"Something");
            if (UnityEngine.Random.Range(0, 100) >= PluginHandler.Instance.Config.Chance) return false;

            Log.Debug($"Active Locks: {this.Door.ActiveLocks}");
            if (this.Door.ActiveLocks != 0) return false;

            this.Door.NetworkTargetState = !this.Door.NetworkTargetState;
            Log.Debug("Door toggled because of getting it's button shot");

            return true;
        }

        internal DoorVariant Door { get; set; }
    }
}
