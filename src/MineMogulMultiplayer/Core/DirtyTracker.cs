using System.Collections.Generic;
using MineMogulMultiplayer.Models;

namespace MineMogulMultiplayer.Core
{
    /// <summary>
    /// Tracks which game objects have changed since the last delta broadcast.
    /// Host-only. Reset each tick after broadcasting.
    /// </summary>
    public static class DirtyTracker
    {
        public static bool MoneyDirty;
        public static bool ResearchDirty;
        public static bool QuestDirty;
        public static bool ContractDirty;
        public static bool ShopPurchaseDirty;
        public static bool ActiveQuestProgressDirty;
        public static readonly HashSet<int> DirtyMachineInstanceIds = new HashSet<int>();
        public static readonly List<BuildingRemovalInfo> RemovedBuildings = new List<BuildingRemovalInfo>();
        public static readonly HashSet<int> DirtyBeltIds = new HashSet<int>();

        // Ore tracking: host records spawned/removed ore each tick
        public static readonly List<OrePieceState> SpawnedOrePieces = new List<OrePieceState>();
        public static readonly List<int> RemovedOrePieceIds = new List<int>();

        // Set of ore InstanceIDs known from last tick (for diff detection)
        public static readonly HashSet<int> KnownOreIds = new HashSet<int>();

        public static void Reset()
        {
            MoneyDirty = false;
            ResearchDirty = false;
            QuestDirty = false;
            ContractDirty = false;
            ShopPurchaseDirty = false;
            ActiveQuestProgressDirty = false;
            DirtyMachineInstanceIds.Clear();
            RemovedBuildings.Clear();
            DirtyBeltIds.Clear();
            SpawnedOrePieces.Clear();
            RemovedOrePieceIds.Clear();
        }
    }
}
