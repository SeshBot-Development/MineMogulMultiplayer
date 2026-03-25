using HarmonyLib;
using MineMogulMultiplayer.Core;
using UnityEngine;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches tool drop and pickup to sync across multiplayer.
    /// Drop: broadcasts the dropped tool's world position so both players can see it.
    /// Pickup: sends RPC + destroys remote world copies.
    /// </summary>
    [HarmonyPatch]
    internal static class ToolPatch
    {
        /// <summary>When set, skip sending RPC (host is handling the drop internally).</summary>
        public static bool NetworkBypass;

        /// <summary>After DropItem completes, send position/velocity to other player.</summary>
        [HarmonyPatch(typeof(BaseHeldTool), "DropItem")]
        [HarmonyPostfix]
        public static void Postfix_BaseHeldTool_DropItem(BaseHeldTool __instance)
        {
            if (!MultiplayerState.IsOnline) return;
            if (NetworkBypass) return;

            var session = SessionManager.Instance;
            if (session == null) return;

            string toolName = __instance.SavableObjectID.ToString();
            if (string.IsNullOrEmpty(toolName) || toolName == "ToolBuilder") return;

            // Capture post-drop transform (DropItem positions tool in front of camera)
            var rb = __instance.GetComponentInChildren<Rigidbody>();
            var pos = rb != null ? rb.position : __instance.transform.position;
            var rot = rb != null ? rb.rotation : __instance.transform.rotation;
            var vel = rb != null ? rb.linearVelocity : Vector3.zero;

            if (MultiplayerState.IsClient)
            {
                session.SendToolDropRPC(toolName, pos, rot, vel);
            }
            else if (MultiplayerState.IsHost)
            {
                session.BroadcastToolWorldDrop(toolName, pos, rot, vel);
                session.ScheduleInventoryBroadcast();
            }
        }

        [HarmonyPatch(typeof(BaseHeldTool), "TryAddToInventory")]
        [HarmonyPrefix]
        public static void Prefix_BaseHeldTool_TryAddToInventory(BaseHeldTool __instance)
        {
            if (!MultiplayerState.IsOnline) return;
            if (NetworkBypass) return;

            var session = SessionManager.Instance;
            if (session == null) return;

            string toolName = __instance.SavableObjectID.ToString();
            if (string.IsNullOrEmpty(toolName) || toolName == "ToolBuilder") return;

            if (MultiplayerState.IsClient)
            {
                // Client: tell host to add this tool to shared inventory
                session.SendToolPickupRPC(toolName);
            }
            else if (MultiplayerState.IsHost)
            {
                session.BroadcastToolWorldPickup(toolName);
                session.ScheduleInventoryBroadcast();
            }
        }
    }
}
