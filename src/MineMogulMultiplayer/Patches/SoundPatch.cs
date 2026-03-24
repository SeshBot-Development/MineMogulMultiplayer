using System.Collections.Generic;
using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.Models;
using BepInEx.Logging;
using UnityEngine;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Patches SoundManager.PlaySoundAtLocation to broadcast positional sound events
    /// across multiplayer clients, so players hear each other's tool/action sounds.
    /// </summary>
    [HarmonyPatch]
    public static class SoundPatch
    {
        private static ManualLogSource _log;

        /// <summary>When true, suppresses re-broadcast of sounds received from network.</summary>
        internal static bool SuppressBroadcast;

        /// <summary>Cache of SoundDefinition objects by name for reverse lookup.</summary>
        private static Dictionary<string, SoundDefinition> _soundCache;
        private static bool _cacheBuilt;

        // Rate-limit: max sounds per second to avoid flooding
        private static float _lastBroadcastTime;
        private static int _broadcastsThisSecond;
        private const int MaxBroadcastsPerSecond = 15;

        public static void Init(ManualLogSource log) => _log = log;

        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySoundAtLocation),
            new[] { typeof(SoundDefinition), typeof(Vector3), typeof(float), typeof(float), typeof(bool), typeof(bool) })]
        [HarmonyPostfix]
        public static void Postfix_PlaySoundAtLocation(SoundDefinition definition, Vector3 position,
            bool dontPlayIfTooFarFromPlayer, bool isUISound)
        {
            if (!MultiplayerState.IsOnline) return;
            if (SuppressBroadcast) return;
            if (definition == null) return;
            if (isUISound) return; // Don't broadcast UI sounds

            // Rate limit
            float now = Time.unscaledTime;
            if (now - _lastBroadcastTime > 1f)
            {
                _lastBroadcastTime = now;
                _broadcastsThisSecond = 0;
            }
            if (_broadcastsThisSecond >= MaxBroadcastsPerSecond) return;
            _broadcastsThisSecond++;

            var soundName = definition.name;
            if (string.IsNullOrEmpty(soundName)) return;

            // Don't broadcast very common ambient/physics sounds to reduce spam
            if (soundName.Contains("Footstep") || soundName.Contains("footstep")) return;

            SessionManager.Instance?.BroadcastSoundEvent(soundName, position);
        }

        /// <summary>Build/rebuild the SoundDefinition cache from all loaded assets.</summary>
        public static void EnsureCache()
        {
            if (_cacheBuilt && _soundCache != null) return;
            _soundCache = new Dictionary<string, SoundDefinition>();
            var allDefs = Resources.FindObjectsOfTypeAll<SoundDefinition>();
            foreach (var def in allDefs)
            {
                if (def == null || string.IsNullOrEmpty(def.name)) continue;
                _soundCache[def.name] = def;
            }
            _cacheBuilt = true;
        }

        /// <summary>Play a sound by name at a world position (called on receive from network).</summary>
        public static void PlayRemoteSound(string soundName, Vector3 position)
        {
            EnsureCache();
            if (_soundCache == null) return;
            if (!_soundCache.TryGetValue(soundName, out var def) || def == null) return;

            var sm = Singleton<SoundManager>.Instance;
            if (sm == null) return;

            SuppressBroadcast = true;
            try
            {
                sm.PlaySoundAtLocation(def, position);
            }
            finally
            {
                SuppressBroadcast = false;
            }
        }

        /// <summary>Reset cache when scene changes.</summary>
        public static void InvalidateCache()
        {
            _cacheBuilt = false;
            _soundCache = null;
        }
    }
}
