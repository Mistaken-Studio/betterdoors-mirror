// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using Interactables.Interobjects.DoorUtils;
using Mistaken.Updater.Config;

namespace Mistaken.BetterDoors
{
    /// <inheritdoc/>
    internal class Config : IAutoUpdatableConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug should be displayed.
        /// </summary>
        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        public Dictionary<DoorType, bool> GrenadeResistantDoors { get; set; } = new Dictionary<DoorType, bool>()
        {
            { DoorType.HID, true },
            { DoorType.Scp106Primary, true },
            { DoorType.Scp106Secondary, true },
            { DoorType.Scp106Bottom, true },
        };

        public Dictionary<DoorType, bool> SCP096ResistantDoors { get; set; } = new Dictionary<DoorType, bool>()
        {
            { DoorType.CheckpointEntrance, false },
            { DoorType.CheckpointLczA, false },
            { DoorType.CheckpointLczB, false },
        };

        public Dictionary<DoorType, float> CustomDoorHealth { get; set; } = new Dictionary<DoorType, float>()
        {
            { DoorType.CheckpointEntrance, 2000f },
            { DoorType.CheckpointLczA, 1000f },
            { DoorType.CheckpointLczB, 1000f },
        };

        public Dictionary<DoorType, float> CheckpointDoors { get; set; } = new Dictionary<DoorType, float>()
        {
            { DoorType.GateA, 15f },
            { DoorType.GateB, 15f },
        };

        public Dictionary<DoorType, DoorType> AirlockDoors { get; set; } = new Dictionary<DoorType, DoorType>
        {
            { DoorType.EscapePrimary, DoorType.EscapeSecondary },
        };

        /// <inheritdoc/>
        [Description("Auto Update Settings")]
        public System.Collections.Generic.Dictionary<string, string> AutoUpdateConfig { get; set; }
    }
}
