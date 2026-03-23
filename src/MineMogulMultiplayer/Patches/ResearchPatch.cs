using HarmonyLib;
using MineMogulMultiplayer.Core;
using BepInEx.Logging;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches ResearchManager to be host-authoritative.
    /// Clients cannot research locally — they send an RPC.
    /// </summary>
    [HarmonyPatch]
    public static class ResearchPatch
    {
        private static ManualLogSource _log;

        /// <summary>Set to true when applying network state so patches don't block our own sync calls.</summary>
        internal static bool NetworkBypass;

        public static void Init(ManualLogSource log) => _log = log;

        [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ResearchItem))]
        [HarmonyPrefix]
        public static bool Prefix_ResearchItem(ResearchItemDefinition researchItem)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.ResearchDirty = true;
                return true;
            }
            // Client: send research RPC to host with the item's SavableObjectID
            if (researchItem != null)
                SessionManager.Instance?.SendResearchRPC(researchItem.GetSavableObjectID().ToString());
            return false;
        }

        [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.AddResearchTickets))]
        [HarmonyPrefix]
        public static bool Prefix_AddTickets()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }
    }
}
