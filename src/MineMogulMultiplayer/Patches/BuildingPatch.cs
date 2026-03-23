using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.Models;
using BepInEx.Logging;
using UnityEngine;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches building placement (ToolBuilder.PrimaryFire) and removal (BuildingObject.TryTakeOrPack/Pack).
    /// Client: intercepts actions → sends RPC to host, skips local execution.
    /// Host: runs normally, broadcasts to clients.
    /// </summary>
    [HarmonyPatch]
    public static class BuildingPatch
    {
        private static ManualLogSource _log;

        /// <summary>Set to true when processing a network event to allow re-entry past the patch guard.</summary>
        internal static bool NetworkBypass;

        public static void Init(ManualLogSource log) => _log = log;

        // Host placement capture (prefix → postfix handoff)
        private static NetVector3? _pendingPlacePos;
        private static string _pendingPlaceType;

        // ── Placement ────────────────────────────────

        [HarmonyPatch(typeof(ToolBuilder), nameof(ToolBuilder.PrimaryFire))]
        [HarmonyPrefix]
        public static bool Prefix_PrimaryFire(ToolBuilder __instance)
        {
            _pendingPlacePos = null;
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;

            try
            {
                var selectedPrefab = Traverse.Create(__instance).Method("GetSelectedPrefab").GetValue<BuildingObject>();
                var buildMgr = Singleton<BuildingManager>.Instance;

                if (MultiplayerState.IsHost)
                {
                    // Capture ghost state before placement for postfix broadcast
                    if (selectedPrefab != null && buildMgr != null && buildMgr.GhostObjectTransform != null)
                    {
                        _pendingPlacePos = new NetVector3(buildMgr.GhostObjectTransform.position);
                        _pendingPlaceType = selectedPrefab.SavableObjectID.ToString();
                    }
                    return true;
                }

                // Client: send placement RPC to host, skip local instantiation
                if (selectedPrefab == null) return false;
                if (buildMgr == null || buildMgr.GhostObjectTransform == null) return false;

                SessionManager.Instance?.SendPlaceBuildingRPC(
                    selectedPrefab.SavableObjectID.ToString(),
                    new NetVector3(buildMgr.GhostObjectTransform.position),
                    new NetQuaternion(buildMgr.GhostObjectTransform.rotation));

                return false;
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"[BuildingPatch] Prefix_PrimaryFire error: {ex}");
                return true; // let the original method run on error
            }
        }

        [HarmonyPatch(typeof(ToolBuilder), nameof(ToolBuilder.PrimaryFire))]
        [HarmonyPostfix]
        public static void Postfix_PrimaryFire(ToolBuilder __instance)
        {
            if (!MultiplayerState.IsHost) return;
            if (NetworkBypass) return;
            if (_pendingPlacePos == null) return;

            try
            {
                var pos = _pendingPlacePos.Value;
                var type = _pendingPlaceType;
                _pendingPlacePos = null;

                // Find the building just placed at the ghost position
                bool found = false;
                foreach (var bo in Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
                {
                    if (bo.IsGhost) continue;
                    if (bo.SavableObjectID.ToString() != type) continue;
                    if (Vector3.Distance(bo.transform.position, pos.ToUnity()) > 0.15f) continue;

                    var state = new BuildingState
                    {
                        LocalInstanceId = bo.GetInstanceID(),
                        SavableObjectId = type,
                        Position = new NetVector3(bo.transform.position),
                        Rotation = new NetQuaternion(bo.transform.rotation)
                    };
                    if (bo is ICustomSaveDataProvider provider)
                        state.CustomSaveData = provider.GetCustomSaveData();

                    SessionManager.Instance?.BroadcastBuildingSpawned(state);
                    found = true;
                    break;
                }

                if (!found)
                    _log?.LogWarning($"[BuildingPatch] Could not find placed building '{type}' near {pos} -- clients won't see it");
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"[BuildingPatch] Postfix_PrimaryFire error: {ex}");
            }
        }

        // ── Removal ──────────────────────────────────

        [HarmonyPatch(typeof(BuildingObject), nameof(BuildingObject.TryTakeOrPack))]
        [HarmonyPrefix]
        public static bool Prefix_TryTakeOrPack(BuildingObject __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.RemovedBuildings.Add(new BuildingRemovalInfo
                {
                    Position = new NetVector3(__instance.transform.position),
                    SavableObjectId = __instance.SavableObjectID.ToString()
                });
                return true;
            }

            // Client: send remove RPC to host
            SessionManager.Instance?.SendRemoveBuildingRPC(
                new NetVector3(__instance.transform.position),
                __instance.SavableObjectID.ToString());
            return false;
        }

        [HarmonyPatch(typeof(BuildingObject), nameof(BuildingObject.Pack))]
        [HarmonyPrefix]
        public static bool Prefix_Pack(BuildingObject __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.RemovedBuildings.Add(new BuildingRemovalInfo
                {
                    Position = new NetVector3(__instance.transform.position),
                    SavableObjectId = __instance.SavableObjectID.ToString()
                });
                return true;
            }

            SessionManager.Instance?.SendRemoveBuildingRPC(
                new NetVector3(__instance.transform.position),
                __instance.SavableObjectID.ToString());
            return false;
        }
    }
}
