using System.Collections.Generic;
using MessagePack;

namespace MineMogulMultiplayer.Models
{
    // ──────────────────────────────────────────────
    //  Full-world snapshot (sent when a client joins)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class WorldSnapshot
    {
        [Key(0)] public WorldState World;
        [Key(1)] public List<PlayerState> Players;
        [Key(2)] public List<BuildingState> Buildings;
        [Key(3)] public List<OrePieceState> OrePieces;
        [Key(4)] public List<ConveyorState> Conveyors;

        /// <summary>Monotonically increasing tick number. Clients use this for ordering.</summary>
        [Key(5)] public long Tick;

        /// <summary>All breakable crate positions (synced on join so clients see correct crate layout).</summary>
        [Key(6)] public List<CrateState> Crates;

        /// <summary>State of all detonator explosions (doors/barriers blown open).</summary>
        [Key(7)] public List<DetonatorState> Detonators;
    }

    // ──────────────────────────────────────────────
    //  Delta update (sent every tick from host)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class WorldDelta
    {
        [Key(0)] public long Tick;

        /// <summary>Buildings whose custom save data changed.</summary>
        [Key(1)] public List<BuildingState> ChangedBuildings;

        /// <summary>Buildings removed since last delta (identified by position+type).</summary>
        [Key(2)] public List<BuildingRemovalInfo> RemovedBuildings;

        /// <summary>Ore pieces that were spawned or changed.</summary>
        [Key(3)] public List<OrePieceState> ChangedOrePieces;

        /// <summary>InstanceIDs of ore pieces that were destroyed/sold.</summary>
        [Key(4)] public List<int> RemovedOrePieceIds;

        /// <summary>All connected player positions (sent every tick for smooth movement).</summary>
        [Key(5)] public List<PlayerState> PlayerUpdates;

        /// <summary>Economy/research/quest state changes.</summary>
        [Key(6)] public WorldState World;

        /// <summary>Conveyor belt states that changed.</summary>
        [Key(7)] public List<ConveyorState> ChangedConveyors;

        /// <summary>Detonators whose state changed (purchased or exploded).</summary>
        [Key(8)] public List<DetonatorState> ChangedDetonators;
    }

    // ──────────────────────────────────────────────
    //  Desync detection
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class WorldHash
    {
        [Key(0)] public long Tick;
        /// <summary>Hash of the entire world state. Clients compute locally and compare.</summary>
        [Key(1)] public long Hash;
    }
}
