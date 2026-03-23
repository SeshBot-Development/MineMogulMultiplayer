using HarmonyLib;
using MineMogulMultiplayer.Core;
using BepInEx.Logging;
using UnityEngine;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches EconomyManager.AddMoney and EconomyManager.SetMoney.
    /// Host: runs normally, change is replicated via delta.
    /// Client: blocked — money comes from host deltas only.
    /// </summary>
    [HarmonyPatch]
    public static class EconomyPatch
    {
        private static ManualLogSource _log;

        /// <summary>Set to true when applying network state so patches don't block our own sync calls.</summary>
        internal static bool NetworkBypass;

        // Pre-purchase money snapshot for calculating cost
        private static float _preCartMoney;

        public static void Init(ManualLogSource log) => _log = log;

        [HarmonyPatch(typeof(EconomyManager), nameof(EconomyManager.AddMoney))]
        [HarmonyPrefix]
        public static bool Prefix_AddMoney(float amount)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.MoneyDirty = true;
                return true;
            }
            // Client: suppress local money changes
            return false;
        }

        [HarmonyPatch(typeof(EconomyManager), nameof(EconomyManager.SetMoney))]
        [HarmonyPrefix]
        public static bool Prefix_SetMoney(float amount)
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost)
            {
                DirtyTracker.MoneyDirty = true;
                return true;
            }
            return false;
        }

        // Allow clients to use the shop computer — each player has their own inventory
        [HarmonyPatch(typeof(ComputerShopUI), "PurchaseCart")]
        [HarmonyPrefix]
        public static bool Prefix_PurchaseCart()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (MultiplayerState.IsHost) return true;

            // Client: snapshot money before purchase so we can calculate the cost
            _preCartMoney = 0f;
            try
            {
                var eco = Singleton<EconomyManager>.Instance;
                if (eco != null) _preCartMoney = eco.Money;
            }
            catch { }

            NetworkBypass = true;
            return true;
        }

        [HarmonyPatch(typeof(ComputerShopUI), "PurchaseCart")]
        [HarmonyPostfix]
        public static void Postfix_PurchaseCart()
        {
            if (!MultiplayerState.IsOnline) return;
            NetworkBypass = false;

            if (MultiplayerState.IsHost)
            {
                DirtyTracker.ShopPurchaseDirty = true;
                return;
            }

            // Client: calculate how much money was spent and tell the host to deduct it
            try
            {
                var eco = Singleton<EconomyManager>.Instance;
                float cost = (eco != null) ? (_preCartMoney - eco.Money) : 0f;

                if (cost > 0)
                {
                    var session = SessionManager.Instance;
                    session?.NotifyHostOfPurchase(System.Array.Empty<string>(), cost);
                    _log?.LogInfo($"[EconomyPatch] Client spent {cost} at the shop");
                }
            }
            catch (System.Exception ex)
            {
                _log?.LogWarning($"[EconomyPatch] Error tracking purchase: {ex.GetType().Name}");
            }
        }

        // Block client TNT purchase — money is host-authoritative
        // (Client RPC is sent by BuildingInteractionPatch on Interact)
        [HarmonyPatch(typeof(DetonatorBuySign), "TryBuySign")]
        [HarmonyPrefix]
        public static bool Prefix_TryBuySign()
        {
            if (!MultiplayerState.IsOnline) return true;
            if (NetworkBypass) return true;
            if (MultiplayerState.IsHost) return true;
            return false;
        }
    }
}
