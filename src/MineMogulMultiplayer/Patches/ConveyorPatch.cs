using HarmonyLib;
using MineMogulMultiplayer.Core;
using BepInEx.Logging;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches ConveyorBelt physics and speed changes.
    /// Host: runs original physics, tracks speed changes.
    /// Client: physics suppressed — belt state comes from host.
    /// </summary>
    [HarmonyPatch]
    public static class ConveyorPatch
    {
        private static ManualLogSource _log;

        public static void Init(ManualLogSource log) => _log = log;

        // Conveyor belt physics run on both host and client so ores
        // visually move along belts. Speed/disabled state is synced from host.

        /// <summary>Set to true when applying network state so patches don't block our own sync calls.</summary>
        internal static bool NetworkBypass;

        [HarmonyPatch(typeof(ConveyorBelt), nameof(ConveyorBelt.ChangeSpeed))]
        [HarmonyPrefix]
        public static bool Prefix_ChangeSpeed()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: speed comes from host delta
        }

        [HarmonyPatch(typeof(ConveyorBelt), nameof(ConveyorBelt.ChangeSpeed))]
        [HarmonyPostfix]
        public static void Postfix_ChangeSpeed(ConveyorBelt __instance)
        {
            if (!MultiplayerState.IsOnline) return;
            if (MultiplayerState.IsHost)
                DirtyTracker.DirtyBeltIds.Add(__instance.GetInstanceID());
        }

        // Track previous Disabled state per ConveyorBlocker so we only mark dirty on actual changes
        private static readonly System.Collections.Generic.Dictionary<int, bool> _blockerPrevDisabled
            = new System.Collections.Generic.Dictionary<int, bool>();

        /// <summary>Block ConveyorBlocker.Update on client — it directly sets Conveyor.Disabled
        /// based on local hinge physics, which would fight with the host-synced disabled state.
        /// On host, the postfix marks the conveyor dirty only when the Disabled value actually changes.</summary>
        [HarmonyPatch(typeof(ConveyorBlocker), "Update")]
        [HarmonyPrefix]
        public static bool Prefix_ConveyorBlocker_Update()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;
            return false; // client: disabled state comes from host
        }

        [HarmonyPatch(typeof(ConveyorBlocker), "Update")]
        [HarmonyPostfix]
        public static void Postfix_ConveyorBlocker_Update(ConveyorBlocker __instance)
        {
            if (!MultiplayerState.IsOnline || !MultiplayerState.IsHost) return;
            if (__instance.Conveyor == null) return;

            int id = __instance.GetInstanceID();
            bool current = __instance.Conveyor.Disabled;
            _blockerPrevDisabled.TryGetValue(id, out bool prev);
            if (current != prev)
            {
                _blockerPrevDisabled[id] = current;
                DirtyTracker.DirtyBeltIds.Add(__instance.Conveyor.GetInstanceID());
            }
        }
    }
}
