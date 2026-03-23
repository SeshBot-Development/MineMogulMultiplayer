using System.Collections.Generic;
using System.IO;
using System.Linq;
using MineMogulMultiplayer.Models;

namespace MineMogulMultiplayer.Serialization
{
    /// <summary>
    /// Computes lightweight hashes of world state for desync detection.
    /// Host and clients compute independently; host broadcasts its hash periodically.
    /// Uses only deterministic data (counts, types, money) — NOT InstanceIDs or ore positions.
    /// </summary>
    public static class WorldHasher
    {
        public static long ComputeHash(WorldSnapshot snapshot)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(snapshot.World.Money);
                bw.Write(snapshot.World.ResearchTickets);

                WriteBuildings(bw, snapshot.Buildings);
                WriteOreSummary(bw, snapshot.OrePieces);
                WriteSortedStrings(bw, snapshot.World.CompletedResearchIds);
                WriteSortedStrings(bw, snapshot.World.CompletedQuestIds);
                // NOTE: ActiveQuests excluded from hash — quest progress tracking diverges
                // between host and client (e.g. trigger-based quests only fire on host),
                // causing constant false-positive desyncs.
                WriteShopPurchases(bw, snapshot.World.ShopPurchases);
                WriteContract(bw, snapshot.World.ActiveContract);
                WriteContracts(bw, snapshot.World.InactiveContracts);
                WriteConveyors(bw, snapshot.Conveyors);

                bw.Flush();
                return Fnv1a64(ms.ToArray());
            }
        }

        private static void WriteSortedStrings(BinaryWriter bw, string[] ids)
        {
            if (ids == null) { bw.Write(0); return; }
            var sorted = ids.OrderBy(s => s).ToArray();
            bw.Write(sorted.Length);
            foreach (var s in sorted)
                bw.Write(s ?? "");
        }

        private static void WriteBuildings(BinaryWriter bw, List<BuildingState> buildings)
        {
            if (buildings == null) { bw.Write(0); return; }
            // Sort deterministically by type+position (not InstanceId which differs per process)
            var sorted = buildings.OrderBy(b => b.SavableObjectId ?? "")
                                  .ThenBy(b => b.Position.X)
                                  .ThenBy(b => b.Position.Y)
                                  .ThenBy(b => b.Position.Z)
                                  .ToList();
            bw.Write(sorted.Count);
            foreach (var b in sorted)
            {
                bw.Write(b.SavableObjectId ?? "");
                bw.Write((int)(b.Position.X * 10));
                bw.Write((int)(b.Position.Y * 10));
                bw.Write((int)(b.Position.Z * 10));
            }
        }

        private static void WriteActiveQuests(BinaryWriter bw, ActiveQuestData[] quests)
        {
            if (quests == null) { bw.Write(0); return; }
            var sorted = quests.OrderBy(q => q.QuestId ?? "").ToArray();
            bw.Write(sorted.Length);
            foreach (var q in sorted)
            {
                bw.Write(q.QuestId ?? "");
                if (q.ResourceProgress != null)
                    foreach (var rp in q.ResourceProgress.OrderBy(r => r.ResourceType ?? "").ThenBy(r => r.PieceType ?? ""))
                        bw.Write(rp.CurrentAmount);
                if (q.TriggeredProgress != null)
                    foreach (var tp in q.TriggeredProgress.OrderBy(t => t.Type ?? ""))
                        bw.Write(tp.CurrentAmount);
            }
        }

        private static void WriteShopPurchases(BinaryWriter bw, ShopPurchaseData[] purchases)
        {
            if (purchases == null) { bw.Write(0); return; }
            var sorted = purchases.OrderBy(p => p.SavableObjectId ?? "").ToArray();
            bw.Write(sorted.Length);
            foreach (var p in sorted)
            {
                bw.Write(p.SavableObjectId ?? "");
                bw.Write(p.Amount);
            }
        }

        private static void WriteContract(BinaryWriter bw, ContractData contract)
        {
            if (contract == null) { bw.Write((byte)0); return; }
            bw.Write((byte)1);
            bw.Write(contract.Name ?? "");
            bw.Write(contract.RewardMoney);
            if (contract.Progress != null)
                foreach (var rp in contract.Progress.OrderBy(r => r.ResourceType ?? "").ThenBy(r => r.PieceType ?? ""))
                    bw.Write(rp.CurrentAmount);
        }

        private static void WriteContracts(BinaryWriter bw, ContractData[] contracts)
        {
            if (contracts == null) { bw.Write(0); return; }
            var sorted = contracts.OrderBy(c => c.Name ?? "").ToArray();
            bw.Write(sorted.Length);
            foreach (var c in sorted)
                WriteContract(bw, c);
        }

        /// <summary>Hash ore by type counts rather than individual positions (ores move constantly).</summary>
        private static void WriteOreSummary(BinaryWriter bw, List<OrePieceState> orePieces)
        {
            if (orePieces == null) { bw.Write(0); return; }
            bw.Write(orePieces.Count);
            // Count by resource type for deterministic comparison
            var counts = new Dictionary<byte, int>();
            foreach (var o in orePieces)
            {
                byte key = (byte)o.ResourceType;
                counts.TryGetValue(key, out int c);
                counts[key] = c + 1;
            }
            foreach (var kv in counts.OrderBy(k => k.Key))
            {
                bw.Write(kv.Key);
                bw.Write(kv.Value);
            }
        }

        /// <summary>Compute separate hashes per component for diagnosing which part diverged.</summary>
        public static string DiagnoseComponents(WorldSnapshot snapshot)
        {
            long HashPart(System.Action<BinaryWriter> write)
            {
                using (var ms = new MemoryStream())
                using (var bw = new BinaryWriter(ms))
                {
                    write(bw);
                    bw.Flush();
                    return Fnv1a64(ms.ToArray());
                }
            }

            var money = HashPart(bw => bw.Write(snapshot.World.Money));
            var research = HashPart(bw => bw.Write(snapshot.World.ResearchTickets));
            var buildings = HashPart(bw => WriteBuildings(bw, snapshot.Buildings));
            var ores = HashPart(bw => WriteOreSummary(bw, snapshot.OrePieces));
            var completedResearch = HashPart(bw => WriteSortedStrings(bw, snapshot.World.CompletedResearchIds));
            var completedQuests = HashPart(bw => WriteSortedStrings(bw, snapshot.World.CompletedQuestIds));
            var activeQuests = HashPart(bw => WriteActiveQuests(bw, snapshot.World.ActiveQuests));
            var shop = HashPart(bw => WriteShopPurchases(bw, snapshot.World.ShopPurchases));
            var contracts = HashPart(bw =>
            {
                WriteContract(bw, snapshot.World.ActiveContract);
                WriteContracts(bw, snapshot.World.InactiveContracts);
            });
            var conveyors = HashPart(bw => WriteConveyors(bw, snapshot.Conveyors));

            int oreCount = snapshot.OrePieces?.Count ?? 0;
            int bldCount = snapshot.Buildings?.Count ?? 0;
            int cvCount = snapshot.Conveyors?.Count ?? 0;

            return $"Money={money} Res={research} Bld({bldCount})={buildings} Ore({oreCount})={ores} CRes={completedResearch} CQuest={completedQuests} AQuest={activeQuests} Shop={shop} Contracts={contracts} Conv({cvCount})={conveyors}";
        }

        private static void WriteConveyors(BinaryWriter bw, List<ConveyorState> conveyors)
        {
            if (conveyors == null) { bw.Write(0); return; }
            var sorted = conveyors.OrderBy(c => c.Position.X)
                                   .ThenBy(c => c.Position.Y)
                                   .ThenBy(c => c.Position.Z)
                                   .ToList();
            bw.Write(sorted.Count);
            foreach (var c in sorted)
            {
                bw.Write((int)(c.Position.X * 10));
                bw.Write((int)(c.Position.Y * 10));
                bw.Write((int)(c.Position.Z * 10));
                bw.Write(c.Speed);
                bw.Write(c.Disabled);
            }
        }

        private static long Fnv1a64(byte[] data)
        {
            const long fnvOffset = unchecked((long)0xcbf29ce484222325);
            const long fnvPrime = unchecked((long)0x100000001b3);
            long hash = fnvOffset;
            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= fnvPrime;
            }
            return hash;
        }
    }
}
