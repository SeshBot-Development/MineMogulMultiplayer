using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.Models;
using BepInEx.Logging;
using UnityEngine;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches machine processing coroutines to be host-authoritative.
    ///
    /// BlastFurnace.ProcessOre, CastingFurnace.ProcessOre, CrusherMachine.CrushOre,
    /// PolishingMachine.Update, RollingMill.PressIngot, RodExtruder.ExtrudeRod,
    /// PipeRoller.ProcessFirstPlate/CreatePipe, ThreadingLathe.ExtrudeRod,
    /// ShakerTable.Update, ClusterBreaker.CrushOre
    ///
    /// Host: runs original — output is replicated.
    /// Client: blocked — machines are visual-only, state comes from deltas.
    /// </summary>
    [HarmonyPatch]
    public static class MachineProcessingPatch
    {
        private static ManualLogSource _log;

        public static void Init(ManualLogSource log) => _log = log;

        // ── BlastFurnace ─────────────────────────────

        [HarmonyPatch(typeof(BlastFurnace), "EnqueueOrePiece")]
        [HarmonyPrefix]
        public static bool Prefix_BlastFurnace_Enqueue()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: suppress
        }

        [HarmonyPatch(typeof(BlastFurnace), "EnqueueOrePiece")]
        [HarmonyPostfix]
        public static void Postfix_BlastFurnace_Enqueue(BlastFurnace __instance)
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
        }

        [HarmonyPatch(typeof(BlastFurnace), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_BlastFurnace_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── CastingFurnace ──────────────────────────

        [HarmonyPatch(typeof(CastingFurnace), "EnqueueOrePiece")]
        [HarmonyPrefix]
        public static bool Prefix_CastingFurnace_Enqueue()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(CastingFurnace), "EnqueueOrePiece")]
        [HarmonyPostfix]
        public static void Postfix_CastingFurnace_Enqueue(CastingFurnace __instance)
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
        }

        [HarmonyPatch(typeof(CastingFurnace), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_CastingFurnace_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── CrusherMachine ──────────────────────────

        [HarmonyPatch(typeof(CrusherMachine), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_Crusher_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(CrusherMachine), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_Crusher_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(CrusherMachine), "OnTriggerEnter")]
        [HarmonyPostfix]
        public static void Postfix_Crusher_Trigger(CrusherMachine __instance)
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
        }

        // ── PolishingMachine ────────────────────────

        [HarmonyPatch(typeof(PolishingMachine), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_Polisher_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── RollingMill ─────────────────────────────

        [HarmonyPatch(typeof(RollingMill), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_RollingMill_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── ShakerTable ─────────────────────────────

        [HarmonyPatch(typeof(ShakerTable), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_ShakerTable_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── ClusterBreaker ──────────────────────────

        [HarmonyPatch(typeof(ClusterBreaker), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_ClusterBreaker_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── AutoMiner ───────────────────────────────

        [HarmonyPatch(typeof(AutoMiner), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_AutoMiner_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: host controls ore spawning
        }

        // ── SorterMachine ───────────────────────────

        [HarmonyPatch(typeof(SorterMachine), "FixedUpdate")]
        [HarmonyPrefix]
        public static bool Prefix_Sorter_FixedUpdate()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── SellerMachine ───────────────────────────

        [HarmonyPatch(typeof(SellerMachine), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_Seller_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: selling is host-authoritative
        }

        [HarmonyPatch(typeof(SellerMachine), "OnTriggerEnter")]
        [HarmonyPostfix]
        public static void Postfix_Seller_Trigger(SellerMachine __instance)
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
        }

        // ── RodExtruder ─────────────────────────────

        [HarmonyPatch(typeof(RodExtruder), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_RodExtruder_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── PipeRoller ──────────────────────────────

        [HarmonyPatch(typeof(PipeRoller), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_PipeRoller_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(PipeRoller), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_PipeRoller_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── ThreadingLathe ──────────────────────────

        [HarmonyPatch(typeof(ThreadingLathe), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_ThreadingLathe_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── PackagerMachine ─────────────────────────

        [HarmonyPatch(typeof(PackagerMachine), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_Packager_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── BulkSorter ──────────────────────────────

        [HarmonyPatch(typeof(BulkSorter), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_BulkSorter_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── RobotGrabberArm ─────────────────────────

        [HarmonyPatch(typeof(RobotGrabberArm), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_RobotGrabber_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(RobotGrabberArm), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_RobotGrabber_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── RollerSplitter ──────────────────────────

        [HarmonyPatch(typeof(RollerSplitter), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_RollerSplitter_Trigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── RoutingConveyor ─────────────────────────

        [HarmonyPatch(typeof(RoutingConveyor), "SetDirection")]
        [HarmonyPrefix]
        public static bool Prefix_RoutingConveyor_SetDirection()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── DetonatorExplosion ───────────────────────

        [HarmonyPatch(typeof(DetonatorExplosion), "Explode")]
        [HarmonyPrefix]
        public static bool Prefix_Detonator_Explode()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── BreakableCrate ──────────────────────────

        /// <summary>Set to true when processing a network crate event to allow re-entry.</summary>
        internal static bool CrateNetworkBypass;

        /// <summary>Position captured before TakeDamage runs (crate may be destroyed during).</summary>
        private static Vector3 _lastCratePosition;

        [HarmonyPatch(typeof(BreakableCrate), "TakeDamage")]
        [HarmonyPrefix]
        public static bool Prefix_BreakableCrate_TakeDamage(BreakableCrate __instance, float damage, Vector3 position)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (CrateNetworkBypass) return true;

            // Capture position before TakeDamage runs — crate may be destroyed during the call
            _lastCratePosition = __instance.transform.position;

            if (MultiplayerState.IsHost) return true;

            // Client: send RPC to host, suppress local execution
            try
            {
                SessionManager.Instance?.SendCrateDamageRPC(
                    new NetVector3(__instance.transform.position),
                    damage,
                    new NetVector3(position));
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"[MachineProcessingPatch] Crate RPC error: {ex}");
            }
            return false;
        }

        [HarmonyPatch(typeof(BreakableCrate), "TakeDamage")]
        [HarmonyPostfix]
        public static void Postfix_BreakableCrate_TakeDamage(BreakableCrate __instance, float damage, Vector3 position)
        {
            if (!MultiplayerState.IsOnline) return;
            if (CrateNetworkBypass) return;
            if (!MultiplayerState.IsHost) return;

            try
            {
                // Use position captured in Prefix — __instance may be destroyed by TakeDamage
                SessionManager.Instance?.BroadcastCrateDamage(
                    new NetVector3(_lastCratePosition),
                    damage,
                    new NetVector3(position));
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"[MachineProcessingPatch] Crate broadcast error: {ex}");
            }
        }

        // ── DepositBoxCrusher ────────────────────────

        [HarmonyPatch(typeof(DepositBoxCrusher), "CrushOre")]
        [HarmonyPrefix]
        public static bool Prefix_DepositBoxCrusher_CrushOre()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── RapidAutoMiner ───────────────────────────

        [HarmonyPatch(typeof(RapidAutoMiner), "AttachDrillBit")]
        [HarmonyPrefix]
        public static bool Prefix_RapidAutoMiner_AttachDrillBit()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(RapidAutoMiner), "EjectCurrentDrillBit")]
        [HarmonyPrefix]
        public static bool Prefix_RapidAutoMiner_EjectDrillBit()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(RapidAutoMiner), "BreakCurrentDrillBit")]
        [HarmonyPrefix]
        public static bool Prefix_RapidAutoMiner_BreakDrillBit()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── DamageableOrePiece ───────────────────────

        [HarmonyPatch(typeof(DamageableOrePiece), "TakeDamage")]
        [HarmonyPrefix]
        public static bool Prefix_DamageableOrePiece_TakeDamage()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── PackagerMachineInteractor ────────────────

        [HarmonyPatch(typeof(PackagerMachineInteractor), "Interact")]
        [HarmonyPrefix]
        public static bool Prefix_PackagerMachineInteractor_Interact()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        // ── OreSpawnerMacine ─────────────────────────

        [HarmonyPatch(typeof(OreSpawnerMacine), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_OreSpawnerMacine_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: suppress ore spawning
        }

        [HarmonyPatch(typeof(OreSpawnerMacine), "TrySpawnOre")]
        [HarmonyPrefix]
        public static bool Prefix_OreSpawnerMacine_TrySpawnOre()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(OreSpawnerMacine), "TrySpawnOre")]
        [HarmonyPostfix]
        public static void Postfix_OreSpawnerMacine_TrySpawnOre(OreSpawnerMacine __instance)
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            // Find parent BuildingObject to get instance ID for dirty tracking
            var bo = __instance.GetComponentInParent<BuildingObject>();
            if (bo != null)
                DirtyTracker.DirtyMachineInstanceIds.Add(bo.GetInstanceID());
        }
    }
}
