using HarmonyLib;
using MineMogulMultiplayer.Core;
using BepInEx.Logging;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches QuestManager and ContractsManager to be host-authoritative.
    /// Quest completion and contract deposits only happen on the host.
    /// </summary>
    [HarmonyPatch]
    public static class QuestPatch
    {
        private static ManualLogSource _log;

        public static void Init(ManualLogSource log) => _log = log;

        /// <summary>Set to true when applying network state so patches don't block our own sync calls.</summary>
        internal static bool NetworkBypass;

        private static int _lastCompletedQuestCount = -1;

        [HarmonyPatch(typeof(QuestManager), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_QuestManager_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            if (NetworkBypass) return true;
            return false; // client: quest completion logic is host-driven
        }

        [HarmonyPatch(typeof(QuestManager), "Update")]
        [HarmonyPostfix]
        public static void Postfix_QuestManager_Update(QuestManager __instance)
        {
            if (!MultiplayerState.IsOnline) return;
            if (!MultiplayerState.IsHost) return;
            try
            {
                var completed = __instance.GetCompletedQuestIDs();
                int count = completed?.Count ?? 0;
                if (_lastCompletedQuestCount < 0) _lastCompletedQuestCount = count;
                if (count != _lastCompletedQuestCount)
                {
                    _lastCompletedQuestCount = count;
                    DirtyTracker.QuestDirty = true;
                }
            }
            catch { /* ignore if method unavailable */ }
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.DepositBox))]
        [HarmonyPrefix]
        public static bool Prefix_DepositBox()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.DepositBox))]
        [HarmonyPostfix]
        public static void Postfix_DepositBox()
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.ContractDirty = true;
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.ClaimReward))]
        [HarmonyPrefix]
        public static bool Prefix_ClaimReward()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.ClaimReward))]
        [HarmonyPostfix]
        public static void Postfix_ClaimReward()
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.ContractDirty = true;
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.SetContractActive))]
        [HarmonyPrefix]
        public static bool Prefix_SetContractActive()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            if (NetworkBypass) return true;
            return false;
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.SetContractActive))]
        [HarmonyPostfix]
        public static void Postfix_SetContractActive()
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.ContractDirty = true;
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.SetContractInactive))]
        [HarmonyPrefix]
        public static bool Prefix_SetContractInactive()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            if (NetworkBypass) return true;
            return false;
        }

        [HarmonyPatch(typeof(ContractsManager), nameof(ContractsManager.SetContractInactive))]
        [HarmonyPostfix]
        public static void Postfix_SetContractInactive()
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            DirtyTracker.ContractDirty = true;
        }

        // Block client contract sell trigger — prevents double deposit
        [HarmonyPatch(typeof(ContractSellTrigger), "OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool Prefix_ContractSellTrigger()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }
    }
}
