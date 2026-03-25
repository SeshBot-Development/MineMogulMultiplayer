using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.Models;
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
                session.SendToolPickupRPC(toolName);
            }
            else if (MultiplayerState.IsHost)
            {
                session.BroadcastToolWorldPickup(toolName);
            }
        }

        // ── ToolDebugSpawnTool (ore spawner tool) ────────────────

        /// <summary>Client: suppress local SpawnObject and send RPC to host instead.</summary>
        [HarmonyPatch(typeof(ToolDebugSpawnTool), "SpawnObject")]
        [HarmonyPrefix]
        public static bool Prefix_ToolDebugSpawnTool_SpawnObject(ToolDebugSpawnTool __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true; // host spawns normally; DetectOreChanges picks it up

            var session = SessionManager.Instance;
            if (session == null) return true;

            // Replicate the same raycast logic as the original method
            var cam = __instance.Owner?.GetComponentInChildren<Camera>();
            if (cam == null) return false;

            RaycastHit hitInfo;
            Vector3 spawnPos;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, __instance.SpawnRange, __instance.HitLayers))
                spawnPos = hitInfo.point - cam.transform.forward * 0.25f;
            else
                spawnPos = cam.transform.position + cam.transform.forward * __instance.SpawnRange;

            session.SendSpawnOreRPC(
                (int)__instance.SelectedResourceType,
                (int)__instance.SelectedPieceType,
                __instance.SelectedIsPolished,
                spawnPos, Quaternion.identity,
                Vector3.zero, Vector3.zero);

            return false; // suppress local spawn
        }

        /// <summary>Client: suppress local LaunchObject and send RPC to host instead.</summary>
        [HarmonyPatch(typeof(ToolDebugSpawnTool), "LaunchObject")]
        [HarmonyPrefix]
        public static bool Prefix_ToolDebugSpawnTool_LaunchObject(ToolDebugSpawnTool __instance)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;

            var session = SessionManager.Instance;
            if (session == null) return true;

            var cam = __instance.Owner?.GetComponentInChildren<Camera>();
            if (cam == null) return false;

            Vector3 position = cam.transform.position + cam.transform.forward * 1f;
            Vector3 forward = Quaternion.Euler(
                Random.Range(-__instance.AngleSpread, __instance.AngleSpread),
                Random.Range(-__instance.AngleSpread, __instance.AngleSpread), 0f) * cam.transform.forward;
            Quaternion rotation = Quaternion.LookRotation(forward);

            Vector3 force = forward.normalized * __instance.LaunchForce;
            Vector3 torque = new Vector3(
                Random.Range(-__instance.SpinForce, __instance.SpinForce),
                Random.Range(-__instance.SpinForce, __instance.SpinForce),
                Random.Range(-__instance.SpinForce, __instance.SpinForce));

            session.SendSpawnOreRPC(
                (int)__instance.SelectedResourceType,
                (int)__instance.SelectedPieceType,
                __instance.SelectedIsPolished,
                position, rotation,
                force, torque);

            return false;
        }
    }
}
