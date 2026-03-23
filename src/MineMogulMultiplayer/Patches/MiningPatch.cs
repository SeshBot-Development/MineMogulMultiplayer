using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.Models;
using BepInEx.Logging;
using UnityEngine;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches OreNode.TakeDamage.
    /// Mining damage is host-authoritative: clients send a "mine" RPC,
    /// the host processes damage, spawns ore, and broadcasts the damage to all clients.
    /// </summary>
    [HarmonyPatch]
    public static class MiningPatch
    {
        private static ManualLogSource _log;

        /// <summary>Set to true when processing a network event to allow re-entry past the patch guard.</summary>
        internal static bool NetworkBypass;

        /// <summary>Set to true while applying remote mining on a client — suppresses ore spawning.</summary>
        internal static bool SuppressOreSpawn;

        public static void Init(ManualLogSource log) => _log = log;

        [HarmonyPatch(typeof(OreNode), nameof(OreNode.TakeDamage))]
        [HarmonyPrefix]
        public static bool Prefix_TakeDamage(OreNode __instance, float damage, Vector3 position)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;

            if (MultiplayerState.IsHost)
            {
                // Let host TakeDamage run; postfix will broadcast to clients
                return true;
            }

            try
            {
                // Client: send mine RPC to host, suppress local execution
                SessionManager.Instance?.SendMineNodeRPC(
                    new NetVector3(__instance.transform.position),
                    damage,
                    new NetVector3(position));
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"[MiningPatch] Prefix_TakeDamage error: {ex}");
            }
            return false;
        }

        /// <summary>
        /// After TakeDamage runs on the host, broadcast the damage event to all clients
        /// so they see the node crack / break visually.
        /// </summary>
        [HarmonyPatch(typeof(OreNode), nameof(OreNode.TakeDamage))]
        [HarmonyPostfix]
        public static void Postfix_TakeDamage(OreNode __instance, float damage, Vector3 position)
        {
            if (!MultiplayerState.IsOnline) return;
            if (NetworkBypass) return;
            if (!MultiplayerState.IsHost) return;

            try
            {
                SessionManager.Instance?.BroadcastMineNode(
                    new NetVector3(__instance.transform.position),
                    damage,
                    new NetVector3(position));
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"[MiningPatch] Postfix_TakeDamage broadcast error: {ex}");
            }
        }
    }
}
