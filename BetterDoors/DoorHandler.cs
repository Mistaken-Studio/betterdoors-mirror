// -----------------------------------------------------------------------
// <copyright file="DoorHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mistaken.API.Diagnostics;
using UnityEngine;

namespace Mistaken.BetterDoors
{
    /// <inheritdoc/>
    internal class DoorHandler : Module
    {
        public static readonly Dictionary<Door, float> CheckpointDoors = new Dictionary<Door, float>();
        public static readonly Dictionary<Door, Door> AirlockDoors = new Dictionary<Door, Door>();

        /// <inheritdoc cref="Module.Module(Exiled.API.Interfaces.IPlugin{Exiled.API.Interfaces.IConfig})"/>
        public DoorHandler(PluginHandler p)
            : base(p)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Door";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Player.InteractingDoor += this.Player_InteractingDoor;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Player_InteractingDoor;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
        }

        private void Server_RoundStarted()
        {
            foreach (var item in PluginHandler.Instance.Config.CustomDoorHealth)
            {
                var door = Map.Doors.First(x => x.Type == item.Key);

                this.Log.Debug($"Setting custom health for {door.Type}, heath: {item.Value}", PluginHandler.Instance.Config.VerbouseOutput);

                if (door.Base is BreakableDoor damageableDoor)
                {
                    damageableDoor._maxHealth = item.Value;
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    this.Log.Debug($"Done setting door", PluginHandler.Instance.Config.VerbouseOutput);
                }

                if (door.Base is CheckpointDoor checkpoint)
                {
                    this.Log.Debug($"Checkpont", PluginHandler.Instance.Config.VerbouseOutput);
                    foreach (var subDoor in checkpoint._subDoors)
                    {
                        this.Log.Debug($"Setting sub doors", PluginHandler.Instance.Config.VerbouseOutput);
                        if (!(subDoor is BreakableDoor subDamageableDoor))
                            continue;
                        subDamageableDoor._maxHealth = item.Value;
                        subDamageableDoor._remainingHealth = subDamageableDoor._maxHealth;
                        this.Log.Debug($"Done setting sub doors", PluginHandler.Instance.Config.VerbouseOutput);
                    }
                }
            }

            this.RunCoroutine(this.DoRoundLoop(), "RoundLoop");
        }

        private IEnumerator<float> DoRoundLoop()
        {
            yield return Timing.WaitForSeconds(1);
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(1);
                foreach (var door in CheckpointDoors)
                {
                    if (door.Key.IsLocked)
                        continue;
                    if (!door.Key.IsOpen)
                        continue;
                    door.Key.IsOpen = false;
                }
            }
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            var type = ev.Door.Type;

            if (AirlockDoors.TryGetValue(ev.Door, out Door pair))
                pair.IsOpen = ev.Door.IsOpen;
            else if (CheckpointDoors.TryGetValue(ev.Door, out float time))
            {
                if (ev.Door.IsOpen)
                    return;
                ev.Door.Base.ServerChangeLock(DoorLockReason.SpecialDoorFeature, true);
                this.CallDelayed(
                    time,
                    () =>
                    {
                        ev.Door.Base.ServerChangeLock(DoorLockReason.SpecialDoorFeature, false);
                    },
                    "Closing Gate");
            }
        }

        private void Server_WaitingForPlayers()
        {
            CheckpointDoors.Clear();
            AirlockDoors.Clear();

            foreach (var doorType in PluginHandler.Instance.Config.CheckpointDoors)
                CheckpointDoors.Add(Map.Doors.First(x => x.Type == doorType.Key), doorType.Value);
            foreach (var doorType in PluginHandler.Instance.Config.AirlockDoors)
            {
                var d1 = Map.Doors.First(x => x.Type == doorType.Key);
                var d2 = Map.Doors.First(x => x.Type == doorType.Value);
                AirlockDoors.Add(d1, d2);
                AirlockDoors.Add(d2, d1);
            }

            foreach (var item in PluginHandler.Instance.Config.GrenadeResistantDoors)
            {
                var door = Map.Doors.First(x => x.Type == item.Key);

                if (item.Value)
                    door.IgnoredDamageTypes |= DoorDamageType.Grenade;
                else
                    door.IgnoredDamageTypes &= ~DoorDamageType.Grenade;

                if (door.Base is CheckpointDoor checkpoint)
                {
                    foreach (var subDoor in checkpoint._subDoors)
                    {
                        if (!(subDoor is BreakableDoor damageableDoor))
                            continue;
                        if (item.Value)
                            damageableDoor._ignoredDamageSources |= DoorDamageType.Grenade;
                        else
                            damageableDoor._ignoredDamageSources &= ~DoorDamageType.Grenade;
                    }
                }
            }

            foreach (var item in PluginHandler.Instance.Config.SCP096ResistantDoors)
            {
                var door = Map.Doors.First(x => x.Type == item.Key);

                if (item.Value)
                    door.IgnoredDamageTypes |= DoorDamageType.Scp096;
                else
                    door.IgnoredDamageTypes &= ~DoorDamageType.Scp096;

                if (door.Base is CheckpointDoor checkpoint)
                {
                    foreach (var subDoor in checkpoint._subDoors)
                    {
                        if (!(subDoor is BreakableDoor damageableDoor))
                            continue;
                        if (item.Value)
                            damageableDoor._ignoredDamageSources |= DoorDamageType.Scp096;
                        else
                            damageableDoor._ignoredDamageSources &= ~DoorDamageType.Scp096;
                    }
                }
            }
        }
    }
}
