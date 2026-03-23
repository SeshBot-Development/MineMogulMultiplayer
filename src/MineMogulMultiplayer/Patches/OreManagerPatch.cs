using HarmonyLib;
using MineMogulMultiplayer.Core;
using BepInEx.Logging;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches OreManager.Update (round-robin ore cleanup).
    /// Only the host cleans up/validates ore pieces.
    /// </summary>
    [HarmonyPatch]
    public static class OreManagerPatch
    {
        private static ManualLogSource _log;

        public static void Init(ManualLogSource log) => _log = log;

        [HarmonyPatch(typeof(OreManager), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_OreManager_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: host decides what ore exists
        }
    }
}
