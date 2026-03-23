using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.Models;
using BepInEx.Logging;
using UnityEngine;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches building/machine interactions (AutoMiner toggle, ChuteHatch, ConveyorBlockerT2,
    /// EditableSign, DetonatorTrigger, DetonatorBuySign) to be host-authoritative.
    /// Client: blocks local execution, sends InteractBuilding RPC to host.
    /// Host: runs original, marks machine dirty for replication.
    /// </summary>
    [HarmonyPatch]
    public static class BuildingInteractionPatch
    {
        private static ManualLogSource _log;

        internal static bool NetworkBypass;

        public static void Init(ManualLogSource log) => _log = log;

        // ── AutoMiner Toggle ────────────────────────

        [HarmonyPatch(typeof(AutoMiner), "Interact")]
        [HarmonyPrefix]
        public static bool Prefix_AutoMiner_Interact(AutoMiner __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
                return true;
            }
            // Client: send RPC to host
            var session = SessionManager.Instance;
            if (session != null)
                session.SendInteractBuildingByPosRPC(
                    new NetVector3(__instance.transform.position), "toggleAutoMiner");
            return false;
        }

        // ── ChuteHatch ─────────────────────────────

        [HarmonyPatch(typeof(ChuteHatch), "SetDirection")]
        [HarmonyPrefix]
        public static bool Prefix_ChuteHatch_SetDirection(ChuteHatch __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
                return true;
            }
            return false; // RPC sent from Interact patch below
        }

        [HarmonyPatch(typeof(ChuteHatch), "Interact")]
        [HarmonyPrefix]
        public static bool Prefix_ChuteHatch_Interact(ChuteHatch __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost) return true;
            var session = SessionManager.Instance;
            if (session != null)
                session.SendInteractBuildingByPosRPC(
                    new NetVector3(__instance.transform.position), "toggleHatch");
            return false;
        }

        // ── ConveyorBlockerT2 ───────────────────────

        [HarmonyPatch(typeof(ConveyorBlockerT2), "SetClosed")]
        [HarmonyPrefix]
        public static bool Prefix_ConveyorBlockerT2_SetClosed(ConveyorBlockerT2 __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
                return true;
            }
            return false; // RPC sent from Interact patch below
        }

        [HarmonyPatch(typeof(ConveyorBlockerT2), "Interact")]
        [HarmonyPrefix]
        public static bool Prefix_ConveyorBlockerT2_Interact(ConveyorBlockerT2 __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost) return true;
            var session = SessionManager.Instance;
            if (session != null)
                session.SendInteractBuildingByPosRPC(
                    new NetVector3(__instance.transform.position), "toggleBlocker");
            return false;
        }

        // ── EditableSign ────────────────────────────

        [HarmonyPatch(typeof(EditableSign), "UpdateText")]
        [HarmonyPrefix]
        public static bool Prefix_EditableSign_UpdateText(EditableSign __instance, string input)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.DirtyMachineInstanceIds.Add(__instance.GetInstanceID());
                return true;
            }
            // Client: send text via RPC
            var session = SessionManager.Instance;
            if (session != null)
                session.SendInteractBuildingByPosRPC(
                    new NetVector3(__instance.transform.position), "editSign", input);
            return false;
        }

        // ── DetonatorTrigger ────────────────────────

        [HarmonyPatch(typeof(DetonatorTrigger), "Interact")]
        [HarmonyPrefix]
        public static bool Prefix_DetonatorTrigger_Interact(DetonatorTrigger __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost) return true;
            var session = SessionManager.Instance;
            if (session != null)
                session.SendInteractBuildingByPosRPC(
                    new NetVector3(__instance.transform.position), "triggerDetonator");
            return false;
        }

        // ── DetonatorBuySign ────────────────────────

        [HarmonyPatch(typeof(DetonatorBuySign), "Interact")]
        [HarmonyPrefix]
        public static bool Prefix_DetonatorBuySign_Interact(DetonatorBuySign __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost) return true;
            var session = SessionManager.Instance;
            if (session != null)
                session.SendInteractBuildingByPosRPC(
                    new NetVector3(__instance.transform.position), "buyDetonator");
            return false;
        }
    }
}
