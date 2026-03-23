using HarmonyLib;
using MineMogulMultiplayer.Core;
using BepInEx.Logging;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Prevents clients from auto-saving or manually saving during multiplayer.
    /// Only the host is allowed to save. Prevents save file corruption.
    /// </summary>
    [HarmonyPatch]
    public static class SaveSystemPatch
    {
        private static ManualLogSource _log;

        public static void Init(ManualLogSource log) => _log = log;

        /// <summary>Block AutoSaveManager.Update on client — prevents auto-save every 5 minutes.</summary>
        [HarmonyPatch(typeof(AutoSaveManager), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_AutoSaveManager_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: only host auto-saves
        }

        /// <summary>Block SavingLoadingManager.SaveGame on client — prevents manual save corruption.</summary>
        [HarmonyPatch(typeof(SavingLoadingManager), "SaveGame")]
        [HarmonyPrefix]
        public static bool Prefix_SaveGame()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            _log?.LogWarning("[SaveSystemPatch] Blocked client save attempt during multiplayer");
            return false;
        }
    }
}
