using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Mono;
using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.Patches;
using MineMogulMultiplayer.UI;
using MineMogulMultiplayer.Updater;
using Steamworks;
using UnityEngine;

namespace MineMogulMultiplayer
{
    [BepInPlugin(Core.PluginInfo.GUID, Core.PluginInfo.Name, Core.PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        // ── Config entries ───────────────────────────

        private ConfigEntry<string> _cfgPlayerName;
        private ConfigEntry<int> _cfgTickRate;

        // ── Runtime state ────────────────────────────

        private Harmony _harmony;
        private SessionManager _session;

        public MultiplayerPanel MultiplayerPanel { get; private set; }
        public EventLog EventLog { get; private set; }

        private float _tickInterval;
        private float _tickTimer;

        private void Awake()
        {
            Instance = this;

            // Keep the plugin alive across scene changes
            DontDestroyOnLoad(gameObject);

            // Auto-update check (background thread, won't block startup)
            var modDir = System.IO.Path.GetDirectoryName(Info.Location);
            AutoUpdater.CheckInBackground(modDir, Core.PluginInfo.Version, Logger);

            // Config
            _cfgPlayerName = Config.Bind("Network", "PlayerName", "Player", "Display name in multiplayer.");
            _cfgTickRate = Config.Bind("Network", "TickRate", 20, "State updates per second (host only).");

            _tickInterval = 1f / _cfgTickRate.Value;

            // Session manager (also initializes Steamworks)
            _session = new SessionManager(Logger);

            // UI — Event log (always visible during gameplay)
            EventLog = gameObject.AddComponent<EventLog>();

            // UI — Canvas-based multiplayer panel (F9 / menu buttons)
            MultiplayerPanel = gameObject.AddComponent<MultiplayerPanel>();
            MultiplayerPanel.Init(Logger, _session, _cfgPlayerName);
            Logger.LogInfo("Multiplayer panel attached");

            // Harmony patches (wrapped so a bad patch can't kill the plugin)
            bool patchesOk = false;
            try
            {
                _harmony = new Harmony(Core.PluginInfo.GUID);
                _harmony.PatchAll();
                Logger.LogInfo("Harmony patches applied");
                patchesOk = true;
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Harmony patch failed: {ex}");
            }

            if (!patchesOk)
            {
                Logger.LogError("Multiplayer patches failed — multiplayer will not work. Check the log above.");
            }

            // Init patch statics
            EconomyPatch.Init(Logger);
            MachineProcessingPatch.Init(Logger);
            ConveyorPatch.Init(Logger);
            BuildingPatch.Init(Logger);
            ResearchPatch.Init(Logger);
            QuestPatch.Init(Logger);
            MiningPatch.Init(Logger);
            OreManagerPatch.Init(Logger);
            BuildingInteractionPatch.Init(Logger);
            SaveSystemPatch.Init(Logger);
            RemotePlayerManager.Init(Logger);

            Logger.LogInfo($"{Core.PluginInfo.Name} v{Core.PluginInfo.Version} loaded");
        }

        private float _steamRetryTimer;
        private int _steamRetryCount;
        private const float SteamRetryInterval = 3f;
        private const int MaxSteamRetries = 10;

        private void Update()
        {
            // Retry Steam init if it wasn't ready at startup (game may init Steam after us)
            if (!_session.SteamReady)
            {
                if (_steamRetryCount >= MaxSteamRetries) return; // give up after max retries
                _steamRetryTimer += Time.deltaTime;
                if (_steamRetryTimer >= SteamRetryInterval)
                {
                    _steamRetryTimer = 0f;
                    _steamRetryCount++;
                    _session.RetryInitSteam();
                    if (!_session.SteamReady && _steamRetryCount >= MaxSteamRetries)
                        Logger.LogError("[Steam] Failed to connect after all retries. Restart the game with Steam running.");
                }
                return;
            }

            // Always pump Steam callbacks so async lobby operations can complete
            try { SteamClient.RunCallbacks(); }
            catch { /* handled in Tick */ }

            // Only do full tick when in lobby, online, or relay socket is open (pending host)
            if (!MultiplayerState.IsOnline && !_session.IsPendingHost && !_session.IsInLobby) return;

            // Smooth interpolation for remote player visuals (every frame, only during gameplay)
            if (MultiplayerState.IsOnline)
                RemotePlayerManager.Interpolate();

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _tickInterval)
            {
                _tickTimer -= _tickInterval;
                _session.Tick();
            }
        }

        private void OnDestroy()
        {
            _session?.Dispose();
            _harmony?.UnpatchSelf();
        }
    }
}
