using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using MineMogulMultiplayer.Models;
using MineMogulMultiplayer.Networking;
using MineMogulMultiplayer.Patches;
using MineMogulMultiplayer.Serialization;
using MineMogulMultiplayer.UI;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MineMogulMultiplayer.Core
{
    /// <summary>
    /// Central session manager. Orchestrates the host/client lifecycle,
    /// wires up Steam P2P networking events, and drives the tick loop.
    /// </summary>
    public class SessionManager : IDisposable
    {
        private readonly ManualLogSource _log;
        private SteamP2P _net;

        // Authoritative player list (host only)
        private readonly List<PlayerState> _players = new List<PlayerState>();
        private long _tick;

        // Cached PlayerController to avoid expensive FindFirstObjectByType every tick
        private PlayerController _cachedPlayerController;

        private const int HashCheckInterval = 1200; // ~60s at 20 tps
        private const int ConsecutiveDesyncThreshold = 5;
        private int _consecutiveDesyncCount;

        public bool SteamReady { get; private set; }

        /// <summary>Global instance so patches can dispatch RPCs without holding a reference.</summary>
        public static SessionManager Instance { get; private set; }

        /// <summary>True while the host scene is still loading (P2P not yet started).</summary>
        public bool IsPendingHost => _pendingHost;

        // Pending host: we opened the relay socket but are waiting for a scene to load
        private bool _pendingHost;
        private string _pendingPlayerName;

        // Client: waiting for gameplay scene to load before processing snapshots
        private bool _clientLoadingScene;

        // Client: scene is loaded, waiting for host to signal "ingame" before connecting P2P
        private bool _clientSceneReady;

        // Client connection timeout
        private float _clientConnectTime;
        private bool _clientWaitingForConnect;
        private const float ConnectTimeoutSeconds = 15f;

        // Track connected player names for event log
        private readonly Dictionary<uint, string> _clientNames = new Dictionary<uint, string>();

        // ── Ore network ID tracking (host only) ──
        private int _nextOreNetId = 1;
        private readonly Dictionary<int, int> _hostOreInstanceToNetId = new Dictionary<int, int>(); // hostInstanceId → netId

        // ── Client ore tracking ──
        private readonly Dictionary<int, OrePiece> _clientOreByNetId = new Dictionary<int, OrePiece>();

        // Client: true until the first FullSnapshot is received (ignore deltas/ores until then)
        private bool _clientWaitingForSnapshot;

        // Client: handshake/sync retry mechanism
        private bool _handshakeAckReceived;
        private float _lastSyncRequestTime;
        private int _syncRetryCount;
        private const float SyncRetryInterval = 3f;
        private const int MaxSyncRetries = 10;

        // Host: clients accepted but who haven't received FullSnapshot yet (edge case: scene briefly not ready)
        private readonly HashSet<uint> _pendingSnapshotClients = new HashSet<uint>();

        // ── Pooled per-tick collections (avoid GC pressure) ──
        private readonly List<OrePieceState> _poolSpawnedOres = new List<OrePieceState>();
        private readonly List<int> _poolRemovedOreIds = new List<int>();
        private readonly List<BuildingRemovalInfo> _poolRemovedBuildings = new List<BuildingRemovalInfo>();
        private readonly HashSet<int> _poolDirtyMachines = new HashSet<int>();
        private readonly HashSet<int> _poolDirtyBelts = new HashSet<int>();
        private readonly HashSet<int> _poolDirtyDetonators = new HashSet<int>();
        private readonly HashSet<int> _oreCurrentIds = new HashSet<int>(); // reused in DetectOreChanges

        // ── Ore position sync (host tracks last-sent positions, sends updates for moved ores) ──
        private readonly Dictionary<int, Vector3> _lastSentOrePositions = new Dictionary<int, Vector3>(); // netId → pos
        private const float OrePositionMoveThreshold = 0.05f; // send if moved more than 5cm
        private const int OrePositionSyncInterval = 2; // every 2 ticks (~10 times/sec at 20 tps)
        private readonly List<OrePositionUpdate> _poolOrePositionUpdates = new List<OrePositionUpdate>();

        // ── BreakableCrate network ID tracking (host only) ──
        private int _nextCrateNetId = 1;
        private readonly Dictionary<int, int> _hostCrateInstanceToNetId = new Dictionary<int, int>(); // hostInstanceId → netId
        // ── Client crate tracking ──
        private readonly Dictionary<int, BreakableCrate> _clientCrateByNetId = new Dictionary<int, BreakableCrate>();
        // ── Crate position sync ──
        private readonly Dictionary<int, Vector3> _lastSentCratePositions = new Dictionary<int, Vector3>();
        private readonly List<CratePositionUpdate> _poolCratePositionUpdates = new List<CratePositionUpdate>();

        // ── Smooth interpolation targets for ore/crate positions on client ──
        private readonly Dictionary<int, Vector3> _oreTargetPositions = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Quaternion> _oreTargetRotations = new Dictionary<int, Quaternion>();
        private readonly Dictionary<int, Vector3> _crateTargetPositions = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Quaternion> _crateTargetRotations = new Dictionary<int, Quaternion>();
        private const float ItemInterpolationSpeed = 18f; // higher = snappier tracking
        // Host-side guard against transient empty-tool packets that cause remote flicker.
        private readonly Dictionary<int, string> _hostLastToolByPlayerId = new Dictionary<int, string>();
        private readonly Dictionary<int, int> _hostEmptyToolTicksByPlayerId = new Dictionary<int, int>();
        private const int HostToolEmptyDebounce = 24;
        // Cached crate lookup for ApplyClientCratePositions (cleared each tick)
        private Dictionary<int, BreakableCrate> _cachedCrateLookup;
        // Cached building lookup for BuildDirtyBuildings (cleared each tick)
        private Dictionary<int, BuildingObject> _cachedBuildingLookup;

        // ── Batched building reconciliation (avoid spawning 1000+ buildings in one frame) ──
        private Queue<BuildingState> _pendingBuildingSpawns;
        private const int MaxBuildingSpawnsPerTick = 30;

        // ── Remotely-held ore kinematic management ──
        private readonly Dictionary<int, float> _remotelyHeldOres = new Dictionary<int, float>(); // netId → lastUpdateTime
        private readonly Dictionary<int, float> _remotelyHeldCrates = new Dictionary<int, float>(); // netId → lastUpdateTime
        private const float RemoteHoldTimeout = 0.8f; // seconds before ore returns to physics

        // ── Debug bot (local testing without a second player) ──
        private bool _debugBotActive;
        private float _debugBotAngle;
        private const int DebugBotPlayerId = 999;
        private static readonly string[] DebugBotTools = new[]
        {
            "PickaxeBasic", "HammerBasic", "JackHammer", "ToolBuilder", "HardHat",
            "MiningHelmet", "MagnetTool", "WrenchTool", "IngotMold", "GearMold",
            "DoubleIngotMold", "Lantern", "ResourceScannerTool",
            "RapidAutoMinerStandardDrillBit", "RapidAutoMinerTurboDrillBit",
            "RapidAutoMinerHardenedDrillBit", null // null = empty-handed
        };
        /// <summary>When true, bot mirrors local player's equipment instead of cycling.</summary>
        private bool _debugBotMirrorMode = true;
        /// <summary>When true, the bot stands still in front of the player instead of circling.</summary>
        private bool _debugBotStatic = true;
        /// <summary>True when the bot was spawned in solo mode (no real session). We manage our own player list.</summary>
        private bool _debugBotSoloMode;
        /// <summary>Bot's own sticky tool name — never expires, only replaced by a valid tool.</summary>
        private string _debugBotLastTool;
        /// <summary>True once the audit has run for this boot (avoid spamming logs).</summary>
        private bool _debugBotAuditDone;

        // ── Player inventory persistence (host tracks per-player tools by SteamID) ──
        private readonly Dictionary<ulong, string[]> _savedPlayerInventories = new Dictionary<ulong, string[]>();
        private string _hostSaveFilePath; // set when LaunchGame is called, used for companion file
        // Map clientId → SteamId for inventory tracking
        private readonly Dictionary<uint, ulong> _clientSteamIds = new Dictionary<uint, ulong>();

        // ── Steam Lobby (for invites & friend joining) ──
        private Steamworks.Data.Lobby? _lobby;
        public const int MaxPlayers = 4;

        // ── Lobby-first flow ──
        // Phase 1: Lobby only (main menu). No P2P yet.
        // Phase 2: Host launches game → P2P starts, clients connect.
        public enum LobbyPhase { None, InLobby, Launching, InGame }
        public LobbyPhase Phase { get; private set; } = LobbyPhase.None;

        /// <summary>Names of players currently in the Steam lobby (updated from lobby member list).</summary>
        public List<string> LobbyPlayerNames { get; private set; } = new List<string>();

        /// <summary>Fires when the lobby player list changes.</summary>
        public event Action OnLobbyPlayersChanged;

        /// <summary>Fires when the lobby host launches the game (client side).</summary>
        public event Action OnLobbyLaunched;

        // Client: host Steam ID to connect P2P to after scene loads
        private ulong _lobbyHostId;
        // Client: save info received from lobby data for scene loading
        private string _launchSceneName;

        /// <summary>True when in lobby or in game.</summary>
        public bool IsInLobby => Phase != LobbyPhase.None;


        public SessionManager(ManualLogSource log)
        {
            _log = log;
            Instance = this;
            PreloadSteamNative();
            InitSteam();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint GetModuleFileName(IntPtr hModule, System.Text.StringBuilder lpFilename, uint nSize);

        /// <summary>
        /// Ensure steam_api64.dll is available for P/Invoke by Facepunch.Steamworks.
        /// Mono's DllImport resolver ONLY checks: app base dir, assembly dir, and PATH.
        /// It ignores LoadLibrary/SetDllDirectory. So we must ensure a physical copy of
        /// the DLL exists in one of those locations. As a last resort, we extract a copy
        /// from the embedded resource baked into this assembly.
        /// </summary>
        private void PreloadSteamNative()
        {
            const string DllName = "steam_api64.dll";
            try
            {
                var gameDir = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
                var modDir = System.IO.Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
                _log.LogInfo($"[Steam] Game dir: '{gameDir}'  Mod dir: '{modDir}'");

                // ── 0. Add mod dir + game dir to PATH so Mono can resolve from both ──
                try
                {
                    var path = System.Environment.GetEnvironmentVariable("PATH") ?? "";
                    bool changed = false;
                    if (!string.IsNullOrEmpty(modDir) && !path.Contains(modDir))
                    { path = modDir + ";" + path; changed = true; }
                    if (!string.IsNullOrEmpty(gameDir) && !path.Contains(gameDir))
                    { path = gameDir + ";" + path; changed = true; }
                    if (changed)
                    {
                        System.Environment.SetEnvironmentVariable("PATH", path);
                        _log.LogInfo("[Steam] Updated PATH for Mono P/Invoke resolution");
                    }
                }
                catch (Exception ex) { _log.LogWarning($"[Steam] PATH update failed: {ex.Message}"); }

                // ── 1. Already loaded by the game process? ──
                var hModule = GetModuleHandle("steam_api64");
                if (hModule != IntPtr.Zero)
                {
                    _log.LogInfo("[Steam] steam_api64.dll already loaded in process");
                    EnsureDllInGameRoot(hModule, gameDir, modDir);
                    return;
                }

                SetDllDirectory(gameDir);

                // ── 2. Quick check: game root ──
                var gameRootDll = System.IO.Path.Combine(gameDir, DllName);
                if (TryLoadNative(gameRootDll)) return;

                // ── 3. Quick check: mod directory (bundled with zip) ──
                if (!string.IsNullOrEmpty(modDir))
                {
                    var modDll = System.IO.Path.Combine(modDir, DllName);
                    if (TryLoadNative(modDll))
                    {
                        CopyDllTo(modDll, gameRootDll);
                        return;
                    }
                }

                // ── 4. Extract from embedded resource (guaranteed present in our assembly) ──
                if (ExtractEmbeddedSteamDll(gameRootDll))
                {
                    if (TryLoadNative(gameRootDll)) return;
                }

                // ── 5. Search loaded process modules ──
                try
                {
                    foreach (System.Diagnostics.ProcessModule mod in
                             System.Diagnostics.Process.GetCurrentProcess().Modules)
                    {
                        try
                        {
                            if (mod.ModuleName != null &&
                                mod.ModuleName.Equals(DllName, System.StringComparison.OrdinalIgnoreCase))
                            {
                                _log.LogInfo($"[Steam] Found via process modules: '{mod.FileName}'");
                                if (TryLoadNative(mod.FileName))
                                {
                                    CopyDllTo(mod.FileName, gameRootDll);
                                    return;
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex) { _log.LogWarning($"[Steam] Process module scan: {ex.Message}"); }

                // ── 6. Search ALL games in every Steam library folder ──
                if (TryFindFromAnySteamGame(gameRootDll)) return;

                // ── 7. Recursive search under game directory ──
                try
                {
                    foreach (var found in System.IO.Directory.GetFiles(
                                 gameDir, DllName, System.IO.SearchOption.AllDirectories))
                    {
                        if (TryLoadNative(found)) { CopyDllTo(found, gameRootDll); return; }
                    }
                }
                catch (Exception ex) { _log.LogWarning($"[Steam] Recursive game dir search: {ex.Message}"); }

                _log.LogError("[Steam] Could not obtain steam_api64.dll from any source — multiplayer will NOT work");
            }
            catch (Exception ex)
            {
                _log.LogError($"[Steam] PreloadSteamNative error: {ex}");
            }
        }

        /// <summary>
        /// Extract the embedded steam_api64.dll resource from our assembly to disk.
        /// Tries the target path first (game root), then falls back to mod dir.
        /// Both locations are on PATH, so Mono P/Invoke will find it either way.
        /// </summary>
        private bool ExtractEmbeddedSteamDll(string targetPath)
        {
            // If it already exists at the target, we're good
            if (System.IO.File.Exists(targetPath))
            {
                _log.LogInfo($"[Steam] DLL already exists: '{targetPath}'");
                return true;
            }

            // Also check mod dir — might already be there from zip extraction
            var modDir = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(modDir))
            {
                var modDll = System.IO.Path.Combine(modDir, "steam_api64.dll");
                if (System.IO.File.Exists(modDll))
                {
                    _log.LogInfo($"[Steam] DLL already in mod dir: '{modDll}'");
                    return true;
                }
            }

            // Read the embedded resource once
            byte[] dllBytes;
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream("steam_api64.dll"))
                {
                    if (stream == null)
                    {
                        _log.LogWarning("[Steam] Embedded steam_api64.dll resource not found in assembly");
                        return false;
                    }
                    dllBytes = new byte[stream.Length];
                    stream.Read(dllBytes, 0, dllBytes.Length);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[Steam] Failed to read embedded resource: {ex.Message}");
                return false;
            }

            _log.LogInfo($"[Steam] Embedded DLL read: {dllBytes.Length} bytes. Attempting extraction...");

            // Try writing to game root (e.g. C:\Program Files\...\MineMogul\)
            if (TryWriteBytes(targetPath, dllBytes))
                return true;

            // Fallback: write to mod dir (BepInEx\plugins\MineMogulMultiplayer\)
            // This is on PATH so Mono will find it
            if (!string.IsNullOrEmpty(modDir))
            {
                var altPath = System.IO.Path.Combine(modDir, "steam_api64.dll");
                if (TryWriteBytes(altPath, dllBytes))
                    return true;
            }

            _log.LogError("[Steam] Could not write steam_api64.dll to any location — check file permissions");
            return false;
        }

        /// <summary>Write bytes to a file, returning true on success. Never throws.</summary>
        private bool TryWriteBytes(string path, byte[] data)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(path, data);
                _log.LogInfo($"[Steam] Extracted DLL → '{path}'");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[Steam] Cannot write to '{path}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// When the DLL is already loaded, ensure a physical copy exists in game root
        /// so Mono P/Invoke can find it for Facepunch.Steamworks.
        /// </summary>
        private void EnsureDllInGameRoot(IntPtr hModule, string gameDir, string modDir)
        {
            var targetPath = System.IO.Path.Combine(gameDir, "steam_api64.dll");
            if (System.IO.File.Exists(targetPath)) return;

            // Try to get the loaded path
            var sb = new System.Text.StringBuilder(512);
            if (GetModuleFileName(hModule, sb, (uint)sb.Capacity) > 0)
            {
                var sourcePath = sb.ToString();
                _log.LogInfo($"[Steam] Loaded from: '{sourcePath}'");
                CopyDllTo(sourcePath, targetPath);
                return;
            }

            // Fallback: extract from embedded resource
            ExtractEmbeddedSteamDll(targetPath);
        }

        /// <summary>Copy DLL file, logging success/failure. Silent if source == target.</summary>
        private void CopyDllTo(string sourcePath, string targetPath)
        {
            try
            {
                if (System.IO.File.Exists(targetPath)) return;
                var srcFull = System.IO.Path.GetFullPath(sourcePath);
                var tgtFull = System.IO.Path.GetFullPath(targetPath);
                if (string.Equals(srcFull, tgtFull, System.StringComparison.OrdinalIgnoreCase)) return;
                System.IO.File.Copy(sourcePath, targetPath, false);
                _log.LogInfo($"[Steam] Copied DLL → '{targetPath}'");
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[Steam] Copy failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse Steam's libraryfolders.vdf and search ALL installed games in every library
        /// for a steam_api64.dll we can borrow. Any Steam game ships one.
        /// </summary>
        private bool TryFindFromAnySteamGame(string gameRootDll)
        {
            try
            {
                var steamPath = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                if (string.IsNullOrEmpty(steamPath)) return false;

                var vdfPath = System.IO.Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (!System.IO.File.Exists(vdfPath)) return false;

                var libPaths = new System.Collections.Generic.List<string>();
                foreach (var line in System.IO.File.ReadAllLines(vdfPath))
                {
                    var trimmed = line.Trim();
                    if (!trimmed.StartsWith("\"path\"")) continue;
                    var parts = trimmed.Split('"');
                    if (parts.Length < 4) continue;
                    libPaths.Add(parts[3].Replace("\\\\", "\\"));
                }

                foreach (var libPath in libPaths)
                {
                    var commonDir = System.IO.Path.Combine(libPath, "steamapps", "common");
                    if (!System.IO.Directory.Exists(commonDir)) continue;

                    try
                    {
                        foreach (var gameFolder in System.IO.Directory.GetDirectories(commonDir))
                        {
                            var candidate = System.IO.Path.Combine(gameFolder, "steam_api64.dll");
                            if (!System.IO.File.Exists(candidate)) continue;
                            _log.LogInfo($"[Steam] Found DLL in sibling game: '{candidate}'");
                            if (TryLoadNative(candidate))
                            {
                                CopyDllTo(candidate, gameRootDll);
                                return true;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[Steam] Steam library scan: {ex.Message}");
            }
            return false;
        }

        private bool TryLoadNative(string path)
        {
            if (!System.IO.File.Exists(path)) return false;
            var handle = LoadLibrary(path);
            if (handle != IntPtr.Zero)
            {
                _log.LogInfo($"[Steam] Loaded steam_api64.dll from '{path}'");
                return true;
            }
            _log.LogWarning($"[Steam] LoadLibrary failed for '{path}' (error {Marshal.GetLastWin32Error()})");
            return false;
        }

        private void InitSteam()
        {
            try
            {
                // The game already initializes Steamworks — try to use the existing session first
                if (SteamClient.IsValid)
                {
                    SteamReady = true;
                    _log.LogInfo($"[Steam] Already initialized by game. User: {SteamClient.Name} ({SteamClient.SteamId})");
                }
                else
                {
                    // Try standard init; if that fails, try with asyncCallbacks
                    try
                    {
                        SteamClient.Init(3846120, false);
                    }
                    catch
                    {
                        SteamClient.Init(3846120, true);
                    }
                    SteamReady = true;
                    _log.LogInfo($"[Steam] Initialized. User: {SteamClient.Name} ({SteamClient.SteamId})");
                }

                // Listen for lobby join requests from Steam overlay / friend invites
                SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
                SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
                SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
                SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
                SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
            }
            catch (Exception ex)
            {
                _log.LogError($"[Steam] Failed to init: {ex.Message}");
                SteamReady = false;
            }

            // Warm up Steam Relay network access early (separate try/catch so Steam stays usable)
            try
            {
                SteamNetworkingUtils.InitRelayNetworkAccess();
                _log.LogInfo("[Steam] Relay network access initialized");
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[Steam] InitRelayNetworkAccess failed: {ex.Message}");
            }
        }

        /// <summary>Retry Steam init — called from Plugin.Update if Steam wasn't ready at startup.</summary>
        public void RetryInitSteam()
        {
            if (SteamReady) return;
            // Re-run native DLL search — the game may have loaded it since our last attempt
            PreloadSteamNative();
            InitSteam();
        }

        // ══════════════════════════════════════════════
        //  LOBBY-FIRST FLOW
        // ══════════════════════════════════════════════

        /// <summary>Host creates a Steam lobby from the main menu. No P2P yet.</summary>
        public async void CreateLobby(string playerName)
        {
            if (!SteamReady) { _log.LogError("[Session] Steam not ready"); LogEvent("Steam not connected!"); return; }
            if (Phase != LobbyPhase.None) { _log.LogWarning("[Session] Already in a lobby"); return; }

            _pendingPlayerName = playerName;
            try
            {
                var result = await SteamMatchmaking.CreateLobbyAsync(MaxPlayers);
                if (!result.HasValue) { _log.LogError("[Session] Failed to create lobby"); return; }

                _lobby = result.Value;
                _lobby.Value.SetFriendsOnly();
                _lobby.Value.SetData("mod", PluginInfo.GUID);
                _lobby.Value.SetData("version", PluginInfo.Version);
                _lobby.Value.SetData("host_steamid", SteamClient.SteamId.ToString());
                _lobby.Value.SetData("state", "waiting");
                Phase = LobbyPhase.InLobby;
                _log.LogInfo($"[Session] Lobby created: {_lobby.Value.Id}");
                RefreshLobbyPlayers();
            }
            catch (Exception ex)
            {
                _log.LogError($"[Session] Lobby creation error: {ex.Message}");
            }
        }

        /// <summary>Client joins an existing lobby by host Steam ID or lobby.</summary>
        public async void JoinLobbyByHostId(ulong hostSteamId, string playerName, Action<string> statusCallback = null)
        {
            if (!SteamReady)
            {
                _log.LogError("[Session] Steam not ready");
                statusCallback?.Invoke("<color=#CC4444>Steam not connected.</color>");
                return;
            }
            if (Phase != LobbyPhase.None)
            {
                _log.LogWarning("[Session] Already in a lobby");
                statusCallback?.Invoke("<color=#CC4444>Already in a lobby.</color>");
                return;
            }

            _pendingPlayerName = playerName;
            _lobbyHostId = hostSteamId;

            _log.LogInfo($"[Session] Looking for lobby hosted by {hostSteamId}...");
            statusCallback?.Invoke("Searching for lobby...");

            try
            {
                var list = await SteamMatchmaking.LobbyList
                    .FilterDistanceWorldwide()
                    .WithKeyValue("mod", PluginInfo.GUID)
                    .WithKeyValue("host_steamid", hostSteamId.ToString())
                    .RequestAsync();

                if (list != null && list.Length > 0)
                {
                    var lobby = list[0];
                    var joinResult = await lobby.Join();
                    if (joinResult == RoomEnter.Success)
                    {
                        _lobby = lobby;
                        Phase = LobbyPhase.InLobby;
                        _log.LogInfo($"[Session] Joined lobby {lobby.Id}");
                        statusCallback?.Invoke($"<color=#66CC66>Joined lobby!</color>");
                        RefreshLobbyPlayers();

                        var state = lobby.GetData("state");
                        if (state == "launching" || state == "ingame")
                        {
                            HandleLobbyLaunch();
                        }
                    }
                    else
                    {
                        _log.LogError($"[Session] Failed to join lobby: {joinResult}");
                        statusCallback?.Invoke($"<color=#CC4444>Failed to join: {joinResult}</color>");
                    }
                }
                else
                {
                    _log.LogWarning("[Session] No lobby found — try using Steam invite instead");
                    statusCallback?.Invoke("<color=#CC4444>No lobby found. Ask the host to send a Steam invite.</color>");
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[Session] JoinLobbyByHostId error: {ex.Message}");
                statusCallback?.Invoke($"<color=#CC4444>Error: {ex.Message}</color>");
            }
        }

        /// <summary>Join a specific lobby (used by Steam invite callback).</summary>
        public async void JoinLobbyDirect(Steamworks.Data.Lobby lobby, string playerName)
        {
            if (Phase != LobbyPhase.None) { _log.LogWarning("[Session] Already in a lobby"); return; }

            _pendingPlayerName = playerName;
            try
            {
                var joinResult = await lobby.Join();
                if (joinResult == RoomEnter.Success)
                {
                    _lobby = lobby;
                    _lobbyHostId = 0;
                    var hostStr = lobby.GetData("host_steamid");
                    if (ulong.TryParse(hostStr, out ulong hid))
                        _lobbyHostId = hid;
                    Phase = LobbyPhase.InLobby;
                    _log.LogInfo($"[Session] Joined lobby {lobby.Id} (host: {_lobbyHostId})");
                    RefreshLobbyPlayers();

                    // Check if host already launched
                    var state = lobby.GetData("state");
                    if (state == "launching" || state == "ingame")
                    {
                        HandleLobbyLaunch();
                    }
                }
                else
                {
                    _log.LogError($"[Session] Failed to join lobby: {joinResult}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[Session] JoinLobbyDirect error: {ex.Message}");
            }
        }

        /// <summary>Host launches the game: starts P2P, sets lobby data, loads save.</summary>
        public void LaunchGame(string fullFilePath, string sceneName, string playerName)
        {
            if (!SteamReady || !_lobby.HasValue) { _log.LogError("[Session] Cannot launch — no lobby"); return; }

            _pendingPlayerName = playerName;
            Phase = LobbyPhase.Launching;

            // Set lobby data so clients know to connect
            _lobby.Value.SetData("state", "launching");
            _lobby.Value.SetData("scene_name", sceneName);

            // P2P host will start AFTER scene loads (in OnSceneLoaded) to avoid
            // race conditions where clients connect before the host is ready.
            _pendingHost = true;
            _hostSaveFilePath = fullFilePath;

            // Load saved multiplayer data (player inventories) from companion file
            LoadMultiplayerCompanionData();

            // Load the save
            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm != null)
            {
                slm.LoadSceneThenLoadSave(fullFilePath, sceneName);
                _log.LogInfo($"[Session] Host launching: loading '{fullFilePath}' scene '{sceneName}'");
            }
            else
            {
                _log.LogError("[Session] SavingLoadingManager not available");
                Phase = LobbyPhase.InLobby;
                _pendingHost = false;
                _net.Stop(); _net = null;
            }
        }

        /// <summary>Launch a new game in multiplayer mode. Creates the save file on first auto-save.</summary>
        public void LaunchNewGame(string saveName, string sceneName, string playerName)
        {
            if (!SteamReady || !_lobby.HasValue) { _log.LogError("[Session] Cannot launch — no lobby"); return; }

            _pendingPlayerName = playerName;
            Phase = LobbyPhase.Launching;

            _lobby.Value.SetData("state", "launching");
            _lobby.Value.SetData("scene_name", sceneName);

            _pendingHost = true;
            _hostSaveFilePath = null; // New game — no save file yet

            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm != null)
            {
                slm.LoadSceneAndStartNewSaveFile(saveName, sceneName);
                _log.LogInfo($"[Session] Host launching new game: save='{saveName}' scene='{sceneName}'");
            }
            else
            {
                _log.LogError("[Session] SavingLoadingManager not available");
                Phase = LobbyPhase.InLobby;
                _pendingHost = false;
                _net.Stop(); _net = null;
            }
        }

        /// <summary>Called on client when lobby data says "launching".</summary>
        private void HandleLobbyLaunch()
        {
            if (Phase == LobbyPhase.InGame || Phase == LobbyPhase.Launching) return;
            if (!_lobby.HasValue) return;

            var sceneName = _lobby.Value.GetData("scene_name");
            var hostStr = _lobby.Value.GetData("host_steamid");
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(hostStr)) return;
            if (!ulong.TryParse(hostStr, out ulong hostId)) return;

            _lobbyHostId = hostId;
            _launchSceneName = sceneName;
            Phase = LobbyPhase.Launching;
            _log.LogInfo($"[Session] Host launched game — loading scene '{sceneName}', will connect P2P to {hostId}");
            LogEvent("Host launched the game — loading...");
            OnLobbyLaunched?.Invoke();

            // Load the gameplay scene
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>Update the lobby player name list from Steam lobby members.</summary>
        public void RefreshLobbyPlayers()
        {
            LobbyPlayerNames.Clear();
            if (!_lobby.HasValue) return;
            foreach (var member in _lobby.Value.Members)
            {
                LobbyPlayerNames.Add(member.Name ?? member.Id.ToString());
            }
            _log.LogInfo($"[Session] Lobby players: {string.Join(", ", LobbyPlayerNames)}");
            OnLobbyPlayersChanged?.Invoke();
        }

        /// <summary>Leave the current lobby and reset state.</summary>
        public void LeaveLobbyAndReset()
        {
            LeaveLobby();
            if (_net != null) { _net.Stop(); _net = null; }
            Phase = LobbyPhase.None;
            _pendingHost = false;
            _clientLoadingScene = false;
            _clientSceneReady = false;
            _clientWaitingForConnect = false;
            _clientWaitingForSnapshot = false;
            _handshakeAckReceived = false;
            _syncRetryCount = 0;
            _consecutiveDesyncCount = 0;
            MultiplayerState.CurrentRole = MultiplayerState.Role.Offline;
            _players.Clear();
            _clientNames.Clear();
            LobbyPlayerNames.Clear();
            RemotePlayerManager.Clear();
            _log.LogInfo("[Session] Left lobby and reset");
            OnLobbyPlayersChanged?.Invoke();
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Invalidate cached PlayerController — it will be re-found next tick
            _cachedPlayerController = null;

            // Destroy stale remote player visuals from previous scene
            RemotePlayerManager.Clear();

            // Invalidate sound cache for new scene
            Patches.SoundPatch.InvalidateCache();

            if (_pendingHost)
            {
                // Scene finished loading — start P2P and finalize host startup
                if (_net != null) { _net.Stop(); _net = null; }
                _net = new SteamP2P();
                _net.StartHost(_log);
                _net.OnClientConnected += HandleClientConnected;
                _net.OnClientDisconnected += HandleClientDisconnected;
                _net.OnMessageReceived += HandleHostMessage;

                _pendingHost = false;
                MultiplayerState.CurrentRole = MultiplayerState.Role.Host;
                MultiplayerState.LocalPlayerId = 0;
                Phase = LobbyPhase.InGame;

                // Mark lobby as in-game so clients know the host is ready for P2P
                if (_lobby.HasValue)
                    _lobby.Value.SetData("state", "ingame");

                var eco = Singleton<EconomyManager>.Instance;
                _players.Clear();
                _players.Add(new PlayerState
                {
                    PlayerId = 0,
                    DisplayName = _pendingPlayerName,
                    Money = eco != null ? eco.Money : 0
                });

                _log.LogInfo($"[Session] Scene '{scene.name}' loaded. Host is now active.");
                LogEvent($"Server started. Waiting for players...");
                InitializeKnownOres();
                return;
            }

            // Client: scene loaded after lobby launch → wait for host to be ready
            if (Phase == LobbyPhase.Launching && _lobbyHostId != 0)
            {
                _clientSceneReady = true;
                _log.LogInfo($"[Session] Client scene '{scene.name}' loaded, waiting for host to be ready...");
                LogEvent("Scene loaded. Waiting for host...");

                // If the host is already in-game, connect immediately
                if (_lobby.HasValue && _lobby.Value.GetData("state") == "ingame")
                {
                    ConnectP2PToHost(_pendingPlayerName ?? SteamClient.Name ?? "Player");
                }
                return;
            }

            if (_clientLoadingScene && MultiplayerState.IsClient)
            {
                _clientLoadingScene = false;
                _log.LogInfo($"[Session] Client scene '{scene.name}' loaded. Requesting sync...");
                LogEvent("Scene loaded. Syncing with host...");
                _net?.SendToHost(MessageType.ResyncRequest, _tick);
            }
        }

        /// <summary>Client establishes P2P connection to host after scene is loaded.</summary>
        private void ConnectP2PToHost(string playerName)
        {
            _clientSceneReady = false;
            if (_net != null) { _net.Stop(); _net = null; }

            try { SteamNetworkingUtils.InitRelayNetworkAccess(); }
            catch (Exception ex) { _log.LogWarning($"[Session] InitRelayNetworkAccess: {ex.Message}"); }

            _net = new SteamP2P();
            _net.StartClient(_log, _lobbyHostId);
            MultiplayerState.CurrentRole = MultiplayerState.Role.Client;
            Phase = LobbyPhase.InGame;
            _clientWaitingForSnapshot = true;

            _net.OnConnectedToHost += () =>
            {
                _clientWaitingForConnect = false;
                _handshakeAckReceived = false;
                _lastSyncRequestTime = UnityEngine.Time.unscaledTime;
                _syncRetryCount = 0;
                _net.SendToHost(MessageType.Handshake, new HandshakeMessage
                {
                    PlayerName = playerName,
                    ModVersion = PluginInfo.Version,
                    SteamId = SteamClient.SteamId
                });
                _log.LogInfo("[Session] P2P connected to host, sent handshake");
                LogEvent("Connected to host!");
            };
            _net.OnDisconnectedFromHost += () =>
            {
                _log.LogWarning("[Session] Disconnected from host");
                LogEvent("Disconnected from host.");
                Stop();
            };
            _net.OnMessageReceived += HandleClientMessage;

            _log.LogInfo($"[Session] P2P connecting to {_lobbyHostId}...");
            _clientWaitingForConnect = true;
            _clientConnectTime = UnityEngine.Time.unscaledTime;
        }

        // ── Tick (called from Plugin.Update) ─────────

        public void Tick()
        {
            // Note: SteamClient.RunCallbacks() is pumped by Plugin.Update() every frame.
            // Do NOT call it again here to avoid double-pumping.

            // Lobby-only phase: no P2P, just keep Steam callbacks flowing
            if (Phase == LobbyPhase.InLobby)
            {
                // Client: poll lobby data to detect host launch
                if (_lobby.HasValue && _lobbyHostId != 0)
                {
                    var state = _lobby.Value.GetData("state");
                    if (state == "launching" || state == "ingame")
                    {
                        HandleLobbyLaunch();
                    }
                }
                return;
            }

            // Launching phase: waiting for scene to load
            if (Phase == LobbyPhase.Launching)
            {
                _net?.Poll();

                // Client: scene loaded, poll lobby data until host signals "ingame"
                if (_clientSceneReady && _net == null && _lobby.HasValue)
                {
                    var state = _lobby.Value.GetData("state");
                    if (state == "ingame")
                    {
                        _log.LogInfo("[Session] Host is ready — connecting P2P...");
                        ConnectP2PToHost(_pendingPlayerName ?? SteamClient.Name ?? "Player");
                    }
                }
                return;
            }

            // Don't tick game state while waiting for scene load
            if (_pendingHost)
            {
                _net?.Poll();
                return;
            }

            // Client connection timeout
            if (_clientWaitingForConnect)
            {
                _net?.Poll();
                if (UnityEngine.Time.unscaledTime - _clientConnectTime > ConnectTimeoutSeconds)
                {
                    _log.LogWarning("[Session] Connection timed out");
                    LogEvent("Connection timed out.");
                    _clientWaitingForConnect = false;
                    Stop();
                }
                return;
            }

            if (MultiplayerState.IsHost)
                TickHost();
            else if (MultiplayerState.IsClient)
                TickClient();
        }

        private bool IsGameSceneReady()
        {
            // Check that the core singletons exist — they don't during scene transitions
            return Singleton<EconomyManager>.Instance != null;
        }

        private void TickHost()
        {
            _net?.Poll();

            if (!IsGameSceneReady()) return;

            // Send deferred snapshots to clients who were accepted while scene was briefly unavailable
            if (_pendingSnapshotClients.Count > 0)
            {
                var snapshot = BuildSnapshot();
                foreach (var cid in _pendingSnapshotClients)
                {
                    _log.LogInfo($"[Session] Sending deferred snapshot to client {cid}");
                    _net.SendToClient(cid, MessageType.FullSnapshot, snapshot);
                }
                _pendingSnapshotClients.Clear();
            }

            _tick++;

            // Invalidate per-tick caches
            _cachedCrateLookup = null;
            _cachedBuildingLookup = null;

            UpdateHostPlayerState();

            // Detect item drops/throws and send velocity to clients
            if (_cachedPlayerController != null)
                DetectAndSendItemDrop(_cachedPlayerController);

            // Detect ore changes by diffing AllOrePieces against known set
            DetectOreChanges();

            // Periodically refresh all machine states for clients (~every 6 seconds)
            if (_tick % 120 == 0)
            {
                foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
                {
                    if (bo.IsGhost) continue;
                    if (bo is ICustomSaveDataProvider)
                        DirtyTracker.DirtyMachineInstanceIds.Add(bo.GetInstanceID());
                }
            }

            // Snapshot dirty state into pooled collections and reset immediately
            _poolSpawnedOres.Clear();
            _poolSpawnedOres.AddRange(DirtyTracker.SpawnedOrePieces);
            _poolRemovedOreIds.Clear();
            _poolRemovedOreIds.AddRange(DirtyTracker.RemovedOrePieceIds);
            _poolRemovedBuildings.Clear();
            _poolRemovedBuildings.AddRange(DirtyTracker.RemovedBuildings);
            _poolDirtyMachines.Clear();
            foreach (var id in DirtyTracker.DirtyMachineInstanceIds) _poolDirtyMachines.Add(id);
            _poolDirtyBelts.Clear();
            foreach (var id in DirtyTracker.DirtyBeltIds) _poolDirtyBelts.Add(id);
            _poolDirtyDetonators.Clear();
            foreach (var id in DirtyTracker.DirtyDetonatorIds) _poolDirtyDetonators.Add(id);
            bool tickMoneyDirty = DirtyTracker.MoneyDirty;
            bool tickResearchDirty = DirtyTracker.ResearchDirty;
            bool tickQuestDirty = DirtyTracker.QuestDirty;
            bool tickContractDirty = DirtyTracker.ContractDirty;
            bool tickShopPurchaseDirty = DirtyTracker.ShopPurchaseDirty;
            bool tickActiveQuestProgressDirty = DirtyTracker.ActiveQuestProgressDirty;
            DirtyTracker.Reset();

            // Batch ore spawn/remove events into single messages to reduce network overhead
            if (_poolSpawnedOres.Count > 0)
                _net.SendToAll(MessageType.OreSpawnedBatch, _poolSpawnedOres);
            if (_poolRemovedOreIds.Count > 0)
                _net.SendToAll(MessageType.OreRemovedBatch, _poolRemovedOreIds);

            // Periodically send position updates for ores that have moved significantly
            if (_tick % OrePositionSyncInterval == 0)
            {
                SendOrePositionUpdates();
                SendCratePositionUpdates();
            }

            // Release ores that haven't been remotely updated recently
            ReleaseStaleRemoteOres();

            // Update debug bot position if active
            if (_debugBotActive)
                UpdateDebugBot();

            // Host also needs to see remote player visuals
            RemotePlayerManager.UpdatePlayers(_players, MultiplayerState.LocalPlayerId);

            var delta = BuildDelta(_poolDirtyMachines, _poolDirtyBelts, _poolDirtyDetonators, _poolRemovedBuildings, tickMoneyDirty, tickResearchDirty, tickQuestDirty, tickContractDirty, tickShopPurchaseDirty, tickActiveQuestProgressDirty);
            if (delta != null)
            {
                // Always send reliably — unreliable delivery over Steam relay was dropping
                // position-only packets too aggressively, causing players to never update.
                _net.SendToAll(MessageType.DeltaUpdate, delta);
            }

            // Inventory sync disabled — each player has their own independent inventory

            // Broadcast inventory if a tool was dropped/picked up
            if (_pendingInventoryBroadcast)
            {
                _pendingInventoryBroadcast = false;
                BroadcastInventorySync();
            }

            if (_tick % HashCheckInterval == 0)
            {
                var snapshot = BuildSnapshot();
                var hashVal = WorldHasher.ComputeHash(snapshot);
                _log.LogInfo($"[Session] Host hash at tick {_tick}: {hashVal}  {WorldHasher.DiagnoseComponents(snapshot)}");
                var hash = new WorldHash
                {
                    Tick = _tick,
                    Hash = hashVal
                };
                _net.SendToAll(MessageType.HashCheck, hash);
            }
        }

        /// <summary>Populate KnownOreIds with all current ores to prevent first-tick flood.</summary>
        private void InitializeKnownOres()
        {
            DirtyTracker.KnownOreIds.Clear();
            _hostOreInstanceToNetId.Clear();
            _nextOreNetId = 1;
            foreach (var ore in OrePiece.AllOrePieces)
            {
                if (ore == null) continue;
                int id = ore.GetInstanceID();
                DirtyTracker.KnownOreIds.Add(id);
                _hostOreInstanceToNetId[id] = _nextOreNetId++;
            }
            _log.LogInfo($"[Session] Initialized {DirtyTracker.KnownOreIds.Count} known ores with network IDs");
        }

        /// <summary>Compare current OrePiece.AllOrePieces against known set to find spawned/removed.</summary>
        private void DetectOreChanges()
        {
            _oreCurrentIds.Clear();

            foreach (var ore in OrePiece.AllOrePieces)
            {
                if (ore == null) continue;
                int id = ore.GetInstanceID();
                _oreCurrentIds.Add(id);

                if (!DirtyTracker.KnownOreIds.Contains(id))
                {
                    // Assign a network ID for this new ore
                    int netId = _nextOreNetId++;
                    _hostOreInstanceToNetId[id] = netId;

                    var rb = ore.GetComponent<Rigidbody>();
                    DirtyTracker.SpawnedOrePieces.Add(new OrePieceState
                    {
                        NetworkId = netId,
                        ResourceType = (NetResourceType)(int)ore.ResourceType,
                        PieceType = (NetPieceType)(int)ore.PieceType,
                        IsPolished = ore.IsPolished,
                        Position = new NetVector3(ore.transform.position),
                        Rotation = new NetQuaternion(ore.transform.rotation),
                        Velocity = rb != null ? new NetVector3(rb.linearVelocity) : default,
                        SellValue = ore.GetSellValue()
                    });
                }
            }

            // Find removed ore — look up network IDs for cross-process sending
            foreach (int knownId in DirtyTracker.KnownOreIds)
            {
                if (!_oreCurrentIds.Contains(knownId))
                {
                    if (_hostOreInstanceToNetId.TryGetValue(knownId, out int netId))
                    {
                        DirtyTracker.RemovedOrePieceIds.Add(netId); // network ID, not instance ID
                        _hostOreInstanceToNetId.Remove(knownId);
                    }
                }
            }

            // Update known set for next tick
            DirtyTracker.KnownOreIds.Clear();
            foreach (int id in _oreCurrentIds)
                DirtyTracker.KnownOreIds.Add(id);
        }

        /// <summary>Check all tracked ores for position changes and send batched updates to clients.</summary>
        private void SendOrePositionUpdates()
        {
            _poolOrePositionUpdates.Clear();

            foreach (var ore in OrePiece.AllOrePieces)
            {
                if (ore == null) continue;
                int instanceId = ore.GetInstanceID();
                if (!_hostOreInstanceToNetId.TryGetValue(instanceId, out int netId))
                    continue;

                var pos = ore.transform.position;
                if (_lastSentOrePositions.TryGetValue(netId, out var lastPos))
                {
                    if (Vector3.SqrMagnitude(pos - lastPos) < OrePositionMoveThreshold * OrePositionMoveThreshold)
                        continue;
                }

                _lastSentOrePositions[netId] = pos;
                _poolOrePositionUpdates.Add(new OrePositionUpdate
                {
                    NetworkId = netId,
                    Position = new NetVector3(pos),
                    Rotation = new NetQuaternion(ore.transform.rotation)
                });
            }

            // Clean up entries for removed ores
            if (_poolOrePositionUpdates.Count > 0 || _poolRemovedOreIds.Count > 0)
            {
                foreach (var removedNetId in _poolRemovedOreIds)
                    _lastSentOrePositions.Remove(removedNetId);
            }

            if (_poolOrePositionUpdates.Count > 0)
                _net.SendToAll(MessageType.OrePositionBatch, _poolOrePositionUpdates);
        }

        /// <summary>Build and broadcast the authoritative tool list from the host's PlayerInventory.</summary>
        private void BroadcastInventorySync()
        {
            var invMsg = BuildInventorySyncMessage();
            if (invMsg != null)
                _net.SendToAll(MessageType.InventorySync, invMsg);
        }

        /// <summary>Send the inventory sync to a specific client (e.g. after snapshot).</summary>
        private void SendInventorySyncToClient(uint clientId)
        {
            var invMsg = BuildInventorySyncMessage();
            if (invMsg != null)
                _net.SendToClient(clientId, MessageType.InventorySync, invMsg);
        }

        /// <summary>Build an InventorySyncMessage from the host's PlayerInventory.</summary>
        private InventorySyncMessage BuildInventorySyncMessage()
        {
            if (_cachedPlayerController == null)
                _cachedPlayerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (_cachedPlayerController == null) return null;

            var inv = _cachedPlayerController.GetComponent<PlayerInventory>();
            if (inv == null) return null;

            var tools = new List<string>();
            foreach (var tool in inv.Items)
            {
                if (tool == null) continue;
                var name = tool.SavableObjectID.ToString();
                // ToolBuilder is a building-mode indicator, not a persistent inventory tool
                if (name == "ToolBuilder") continue;
                tools.Add(name);
            }

            return new InventorySyncMessage { Tools = tools.ToArray() };
        }

        /// <summary>
        /// Host: apply ore positions sent by a client who is grabbing/moving ores.
        /// Finds the host-side ore by network ID and updates its position.
        /// The host's own SendOrePositionUpdates will then relay to all clients.
        /// </summary>
        private void ApplyClientOrePositions(List<OrePositionUpdate> updates)
        {
            if (updates == null || updates.Count == 0) return;

            // Build reverse lookup: netId → OrePiece (only for net IDs we need)
            var needed = new HashSet<int>();
            foreach (var upd in updates) needed.Add(upd.NetworkId);

            var netIdToOre = new Dictionary<int, OrePiece>();
            foreach (var kv in _hostOreInstanceToNetId)
            {
                if (!needed.Contains(kv.Value)) continue;
                foreach (var ore in OrePiece.AllOrePieces)
                {
                    if (ore != null && ore.GetInstanceID() == kv.Key)
                    {
                        netIdToOre[kv.Value] = ore;
                        break;
                    }
                }
            }

            foreach (var upd in updates)
            {
                if (netIdToOre.TryGetValue(upd.NetworkId, out var ore) && ore != null)
                {
                    ore.transform.position = upd.Position.ToUnity();
                    ore.transform.rotation = upd.Rotation.ToUnity();
                    var rb = ore.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    _remotelyHeldOres[upd.NetworkId] = UnityEngine.Time.unscaledTime;
                }
            }
        }

        /// <summary>Release ores and crates back to physics after they've stopped being remotely updated.</summary>
        private void ReleaseStaleRemoteOres()
        {
            var now = UnityEngine.Time.unscaledTime;

            // Release stale ores
            if (_remotelyHeldOres.Count > 0)
            {
                var toRelease = new List<int>();
                foreach (var kv in _remotelyHeldOres)
                {
                    if (now - kv.Value > RemoteHoldTimeout)
                        toRelease.Add(kv.Key);
                }
                foreach (var netId in toRelease)
                {
                    _remotelyHeldOres.Remove(netId);
                    _oreTargetPositions.Remove(netId);
                    _oreTargetRotations.Remove(netId);
                    OrePiece foundOre = null;
                    if (MultiplayerState.IsHost)
                    {
                        foreach (var kv in _hostOreInstanceToNetId)
                        {
                            if (kv.Value != netId) continue;
                            foreach (var ore in OrePiece.AllOrePieces)
                            {
                                if (ore != null && ore.GetInstanceID() == kv.Key)
                                { foundOre = ore; break; }
                            }
                            break;
                        }
                    }
                    else if (_clientOreByNetId.TryGetValue(netId, out var clientOre))
                    {
                        foundOre = clientOre;
                    }
                    if (foundOre != null)
                    {
                        var rb = foundOre.GetComponent<Rigidbody>();
                        if (rb != null) rb.isKinematic = false;
                    }
                }
            }

            // Release stale crates
            if (_remotelyHeldCrates.Count > 0)
            {
                var toRelease = new List<int>();
                foreach (var kv in _remotelyHeldCrates)
                {
                    if (now - kv.Value > RemoteHoldTimeout)
                        toRelease.Add(kv.Key);
                }
                foreach (var netId in toRelease)
                {
                    _remotelyHeldCrates.Remove(netId);
                    _crateTargetPositions.Remove(netId);
                    _crateTargetRotations.Remove(netId);
                    if (MultiplayerState.IsHost)
                    {
                        // Find host crate by reverse lookup
                        int instanceId = -1;
                        foreach (var kv in _hostCrateInstanceToNetId)
                        {
                            if (kv.Value == netId) { instanceId = kv.Key; break; }
                        }
                        if (instanceId >= 0)
                        {
                            var allCrates = UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None);
                            foreach (var crate in allCrates)
                            {
                                if (crate != null && crate.GetInstanceID() == instanceId)
                                {
                                    var rb = crate.GetComponent<Rigidbody>();
                                    if (rb != null) rb.isKinematic = false;
                                    break;
                                }
                            }
                        }
                    }
                    else if (_clientCrateByNetId.TryGetValue(netId, out var clientCrate) && clientCrate != null)
                    {
                        var rb = clientCrate.GetComponent<Rigidbody>();
                        if (rb != null) rb.isKinematic = false;
                    }
                }
            }
        }

        // ── Client ore position tracking ──
        private readonly Dictionary<int, Vector3> _clientLastSentOrePos = new Dictionary<int, Vector3>(); // netId → last sent pos
        private readonly List<OrePositionUpdate> _clientOreUpdatePool = new List<OrePositionUpdate>();
        /// <summary>Net IDs of ores the client is currently holding/moving locally. Skip host position updates for these.</summary>
        private readonly HashSet<int> _clientLocallyHeldOres = new HashSet<int>();
        // ── Client crate position tracking ──
        private readonly Dictionary<int, Vector3> _clientLastSentCratePos = new Dictionary<int, Vector3>();
        private readonly List<CratePositionUpdate> _clientCrateUpdatePool = new List<CratePositionUpdate>();
        /// <summary>Net IDs of crates the client is currently holding/moving locally. Skip host position updates for these.</summary>
        private readonly HashSet<int> _clientLocallyHeldCrates = new HashSet<int>();

        /// <summary>
        /// Smoothly interpolate ore and crate positions toward their network targets.
        /// Call from Update (every frame) for smooth visual movement on the client.
        /// </summary>
        public void InterpolateItems()
        {
            if (!MultiplayerState.IsClient) return;

            float dt = UnityEngine.Time.deltaTime;
            float t = ItemInterpolationSpeed * dt;

            // Interpolate ores
            foreach (var kv in _oreTargetPositions)
            {
                if (_clientLocallyHeldOres.Contains(kv.Key)) continue;
                if (!_clientOreByNetId.TryGetValue(kv.Key, out var ore) || ore == null) continue;
                ore.transform.position = Vector3.Lerp(ore.transform.position, kv.Value, t);
                if (_oreTargetRotations.TryGetValue(kv.Key, out var targetRot))
                    ore.transform.rotation = Quaternion.Slerp(ore.transform.rotation, targetRot, t);
            }

            // Interpolate crates
            foreach (var kv in _crateTargetPositions)
            {
                if (_clientLocallyHeldCrates.Contains(kv.Key)) continue;
                if (!_clientCrateByNetId.TryGetValue(kv.Key, out var crate) || crate == null) continue;
                crate.transform.position = Vector3.Lerp(crate.transform.position, kv.Value, t);
                if (_crateTargetRotations.TryGetValue(kv.Key, out var targetRot))
                    crate.transform.rotation = Quaternion.Slerp(crate.transform.rotation, targetRot, t);
            }
        }

        private void TickClient()
        {
            _net?.Poll();

            // Retry handshake/sync if we've been waiting for world state
            if (_clientWaitingForSnapshot && _net != null && _net.IsRunning)
            {
                float elapsed = UnityEngine.Time.unscaledTime - _lastSyncRequestTime;
                if (elapsed >= SyncRetryInterval)
                {
                    _syncRetryCount++;
                    _lastSyncRequestTime = UnityEngine.Time.unscaledTime;
                    if (_syncRetryCount > MaxSyncRetries)
                    {
                        _log.LogError("[Session] Failed to receive world state from host after all retries. Disconnecting.");
                        LogEvent("Failed to sync with host.");
                        Stop();
                        return;
                    }
                    if (!_handshakeAckReceived)
                    {
                        _log.LogWarning($"[Session] No handshake ack from host — resending handshake (attempt {_syncRetryCount}/{MaxSyncRetries})");
                        _net.SendToHost(MessageType.Handshake, new HandshakeMessage
                        {
                            PlayerName = _pendingPlayerName ?? SteamClient.Name ?? "Player",
                            ModVersion = PluginInfo.Version,
                            SteamId = SteamClient.SteamId
                        });
                    }
                    else
                    {
                        _log.LogWarning($"[Session] No snapshot from host — sending resync request (attempt {_syncRetryCount}/{MaxSyncRetries})");
                        _net.SendToHost(MessageType.ResyncRequest, _tick);
                    }
                }
                return; // Don't send position updates while waiting for snapshot
            }

            if (!IsGameSceneReady()) return;

            // Process queued building spawns from reconciliation (batched across ticks)
            ProcessPendingBuildingSpawns();

            // Send position update every other tick (~10 times/sec at 20 tps)
            _tick++;

            if (_net != null && _net.IsRunning)
            {
                if (TryGetLocalPlayerPose(out var pos, out var rot, out var pc))
                {
                    if (pc != null)
                        DetectAndSendItemDrop(pc);

                    // Send player input + ore positions every 2 ticks (~10 times/sec at 20 tps)
                    if (_tick % OrePositionSyncInterval == 0)
                    {
                        if (pc != null)
                            SendClientOrePositions(pc);
                        string heldId = null;
                        var heldPos = default(NetVector3);
                        var heldRot = default(NetQuaternion);
                        if (pc != null)
                            TryGetHeldObjectState(pc, out heldId, out heldPos, out heldRot);

                        _net.SendToHost(MessageType.PlayerInput, new PlayerInputMessage
                        {
                            PlayerId = MultiplayerState.LocalPlayerId,
                            Position = new NetVector3(pos),
                            Rotation = new NetQuaternion(rot),
                            Sprinting = pc != null && GetSprintState(pc),
                            ClientTick = _tick,
                            EquippedTool = pc != null ? GetEquippedToolName(pc) : _lastValidToolName,
                            IsCrouching = pc != null && GetCrouchState(pc),
                            HeldObjectId = heldId,
                            HeldObjectPosition = heldPos,
                            HeldObjectRotation = heldRot
                        });
                    }
                }
                else if (_tick % 40 == 0)
                {
                    _log.LogWarning("[Session] Local player transform not found — cannot send position");
                }
            }

            // Release ores that haven't been remotely updated recently
            ReleaseStaleRemoteOres();

            // Periodically report inventory to host for save persistence
            if (_tick % 60 == 0 && _net != null && _net.IsRunning)
            {
                try { SendClientInventoryReport(); }
                catch (Exception ex) { _log.LogWarning($"[Session] Inventory report failed: {ex.Message}"); }
            }
        }

        /// <summary>
        /// Client: detect ores that the local player is holding/moving and send their
        /// positions to the host. Checks PlayerController.HeldObject for hand-grabs
        /// and scans nearby ores for magnet-held ones.
        /// </summary>
        private void SendClientOrePositions(PlayerController pc)
        {
            _clientOreUpdatePool.Clear();
            _clientLocallyHeldOres.Clear();
            _clientCrateUpdatePool.Clear();
            _clientLocallyHeldCrates.Clear();

            // Check for hand-grabbed object
            try
            {
                var heldObj = pc.HeldObject;
                if (heldObj != null)
                {
                    var ore = heldObj.GetComponent<OrePiece>();
                    if (ore != null)
                        TryAddClientOreUpdate(ore);

                    // Also check if the held object is a BreakableCrate
                    var crate = heldObj.GetComponent<BreakableCrate>();
                    if (crate != null)
                        TryAddClientCrateUpdate(crate);
                }
            }
            catch { /* HeldObject may not exist in this game version */ }

            // Check for magnet-held ores: use OrePiece.CurrentMagnetTool and scan ToolMagnet._heldBodies
            var playerPos = pc.transform.position;
            bool hasMagnet = false;
            ToolMagnet activeMagnet = null;
            try
            {
                var toolName = _lastValidToolName;
                if (string.IsNullOrEmpty(toolName)) toolName = GetEquippedToolName(pc);
                hasMagnet = !string.IsNullOrEmpty(toolName) &&
                            toolName.IndexOf("Magnet", System.StringComparison.OrdinalIgnoreCase) >= 0;
                if (hasMagnet)
                {
                    var inv = pc.GetComponent<PlayerInventory>();
                    if (inv != null && inv.ActiveTool != null)
                        activeMagnet = inv.ActiveTool.GetComponent<ToolMagnet>();
                }
            }
            catch { }

            // Build a set of rigidbodies held by the local magnet for fast lookup
            HashSet<Rigidbody> magnetHeldBodies = null;
            if (activeMagnet != null)
            {
                try
                {
                    var heldField = typeof(ToolMagnet).GetField("_heldBodies",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (heldField != null)
                    {
                        var held = heldField.GetValue(activeMagnet) as System.Collections.Generic.List<Rigidbody>;
                        if (held != null && held.Count > 0)
                            magnetHeldBodies = new HashSet<Rigidbody>(held);
                    }
                }
                catch { }
            }

            foreach (var kv in _clientOreByNetId)
            {
                var ore = kv.Value;
                if (ore == null) continue;

                float sqrDist = Vector3.SqrMagnitude(ore.transform.position - playerPos);

                // Primary check: OrePiece.CurrentMagnetTool is set when magnet grabs the ore
                if (ore.CurrentMagnetTool != null)
                {
                    // Force non-kinematic so the magnet spring can actually move the ore
                    var oreRb = ore.GetComponent<Rigidbody>();
                    if (oreRb != null && oreRb.isKinematic)
                    {
                        oreRb.isKinematic = false;
                        _remotelyHeldOres.Remove(kv.Key);
                        _oreTargetPositions.Remove(kv.Key);
                        _oreTargetRotations.Remove(kv.Key);
                    }
                    TryAddClientOreUpdate(ore);
                    continue;
                }

                // Secondary check: magnet _heldBodies contains this ore's rigidbody
                if (magnetHeldBodies != null && sqrDist < 225f)
                {
                    var oreRb = ore.GetComponent<Rigidbody>();
                    if (oreRb != null && magnetHeldBodies.Contains(oreRb))
                    {
                        if (oreRb.isKinematic)
                        {
                            oreRb.isKinematic = false;
                            _remotelyHeldOres.Remove(kv.Key);
                            _oreTargetPositions.Remove(kv.Key);
                            _oreTargetRotations.Remove(kv.Key);
                        }
                        TryAddClientOreUpdate(ore);
                        continue;
                    }
                }

                // Fallback: check for any joint on the ore itself
                if (sqrDist < 225f) // 15m
                {
                    var joint = ore.GetComponent<Joint>();
                    if (joint != null)
                    {
                        TryAddClientOreUpdate(ore);
                        continue;
                    }
                }

                // If magnet equipped, detect ores with velocity within range (being attracted)
                if (hasMagnet && sqrDist < 100f) // 10m
                {
                    var rb = ore.GetComponent<Rigidbody>();
                    if (rb != null && !rb.isKinematic && rb.linearVelocity.sqrMagnitude > 0.25f)
                    {
                        TryAddClientOreUpdate(ore);
                    }
                }
            }

            if (_clientOreUpdatePool.Count > 0)
                _net.SendToHost(MessageType.OrePositionBatch, _clientOreUpdatePool);

            // Check for magnet-held or hand-held crates too
            foreach (var kv in _clientCrateByNetId)
            {
                var crate = kv.Value;
                if (crate == null) continue;
                float sqrDist = Vector3.SqrMagnitude(crate.transform.position - playerPos);

                // Check magnet _heldBodies for crate's rigidbody
                if (magnetHeldBodies != null && sqrDist < 225f)
                {
                    var crateRb = crate.GetComponent<Rigidbody>();
                    if (crateRb != null && magnetHeldBodies.Contains(crateRb))
                    {
                        if (crateRb.isKinematic)
                        {
                            crateRb.isKinematic = false;
                            _remotelyHeldCrates.Remove(kv.Key);
                            _crateTargetPositions.Remove(kv.Key);
                            _crateTargetRotations.Remove(kv.Key);
                        }
                        TryAddClientCrateUpdate(crate);
                        continue;
                    }
                }

                // Check for any joint component (magnet/physics grab)
                if (sqrDist < 225f) // 15m
                {
                    var joint = crate.GetComponent<Joint>();
                    if (joint != null)
                    {
                        TryAddClientCrateUpdate(crate);
                        continue;
                    }
                }

                // If magnet equipped, detect crates with velocity within range
                if (hasMagnet && sqrDist < 100f) // 10m
                {
                    var rb = crate.GetComponent<Rigidbody>();
                    if (rb != null && !rb.isKinematic && rb.linearVelocity.sqrMagnitude > 0.25f)
                    {
                        TryAddClientCrateUpdate(crate);
                    }
                }
            }

            // Send crate position updates
            if (_clientCrateUpdatePool.Count > 0)
                _net.SendToHost(MessageType.CratePositionBatch, _clientCrateUpdatePool);
        }

        private void TryAddClientOreUpdate(OrePiece ore)
        {
            // Find network ID for this ore
            int netId = -1;
            foreach (var kv in _clientOreByNetId)
            {
                if (kv.Value == ore)
                {
                    netId = kv.Key;
                    break;
                }
            }
            if (netId < 0) return;

            // Mark this ore as locally held so host position updates are skipped
            _clientLocallyHeldOres.Add(netId);

            // Make sure the ore is non-kinematic so the client can move it
            var rb = ore.GetComponent<Rigidbody>();
            if (rb != null && rb.isKinematic)
                rb.isKinematic = false;

            var pos = ore.transform.position;
            if (_clientLastSentOrePos.TryGetValue(netId, out var lastPos))
            {
                if (Vector3.SqrMagnitude(pos - lastPos) < OrePositionMoveThreshold * OrePositionMoveThreshold)
                    return;
            }

            _clientLastSentOrePos[netId] = pos;
            _clientOreUpdatePool.Add(new OrePositionUpdate
            {
                NetworkId = netId,
                Position = new NetVector3(pos),
                Rotation = new NetQuaternion(ore.transform.rotation)
            });
        }

        private void TryAddClientCrateUpdate(BreakableCrate crate)
        {
            int netId = -1;
            foreach (var kv in _clientCrateByNetId)
            {
                if (kv.Value == crate) { netId = kv.Key; break; }
            }
            if (netId < 0) return;

            // Mark this crate as locally held so host position updates are skipped
            _clientLocallyHeldCrates.Add(netId);

            // Make sure the crate is non-kinematic so the client can move it
            var rb = crate.GetComponent<Rigidbody>();
            if (rb != null && rb.isKinematic)
                rb.isKinematic = false;

            var pos = crate.transform.position;
            if (_clientLastSentCratePos.TryGetValue(netId, out var lastPos))
            {
                if (Vector3.SqrMagnitude(pos - lastPos) < OrePositionMoveThreshold * OrePositionMoveThreshold)
                    return;
            }

            _clientLastSentCratePos[netId] = pos;
            _clientCrateUpdatePool.Add(new CratePositionUpdate
            {
                NetworkId = netId,
                Position = new NetVector3(pos),
                Rotation = new NetQuaternion(crate.transform.rotation)
            });
        }

        private void UpdateHostPlayerState()
        {
            var hostPlayer = _players.Find(p => p.PlayerId == 0);
            if (hostPlayer == null) return;

            if (TryGetLocalPlayerPose(out var pos, out var rot, out var pc))
            {
                hostPlayer.Position = new NetVector3(pos);
                hostPlayer.Rotation = new NetQuaternion(rot);
                hostPlayer.EquippedTool = pc != null ? GetEquippedToolName(pc) : _lastValidToolName;
                hostPlayer.IsCrouching = pc != null && GetCrouchState(pc);
                if (pc != null && TryGetHeldObjectState(pc, out var heldId, out var heldPos, out var heldRot))
                {
                    hostPlayer.HeldObjectId = heldId;
                    hostPlayer.HeldObjectPosition = heldPos;
                    hostPlayer.HeldObjectRotation = heldRot;
                }
                else
                {
                    hostPlayer.HeldObjectId = null;
                }
            }

            var eco = Singleton<EconomyManager>.Instance;
            if (eco != null) hostPlayer.Money = eco.Money;

            var research = Singleton<ResearchManager>.Instance;
            if (research != null) hostPlayer.ResearchTickets = research.ResearchTickets;
        }

        /// <summary>Read the SavableObjectID name of the player's currently held tool via PlayerInventory.</summary>
        private string _lastValidToolName;
        private int _toolNullTicks;
        private const int ToolNullDebounce = 20; // ~1s at 20tps before clearing tool

        private string GetEquippedToolName(PlayerController pc)
        {
            try
            {
                var inv = pc.GetComponent<PlayerInventory>();
                if (inv == null)
                    return _lastValidToolName;

                string name = null;

                // Primary source: ActiveTool
                try { name = TryGetToolIdFromAny(inv.ActiveTool); } catch { /* ignore */ }

                // Reflection fallback: different game builds may rename the active tool slot.
                if (string.IsNullOrEmpty(name))
                    name = TryGetToolIdFromMember(inv, "CurrentTool")
                        ?? TryGetToolIdFromMember(inv, "SelectedTool")
                        ?? TryGetToolIdFromMember(inv, "ActiveItem")
                        ?? TryGetToolIdFromMember(inv, "CurrentItem")
                        ?? TryGetToolIdFromMember(inv, "SelectedItem")
                        ?? TryGetToolIdFromMember(inv, "HeldTool");

                // Last fallback: some builds expose currently held object on PlayerController.
                // Skip physics-holdable objects (crates) — those are tracked via HeldObjectId, not EquippedTool.
                if (string.IsNullOrEmpty(name))
                {
                    try
                    {
                        var heldName = TryGetToolIdFromAny(pc.HeldObject);
                        if (!string.IsNullOrEmpty(heldName) &&
                            !heldName.StartsWith("BreakableCrate", System.StringComparison.OrdinalIgnoreCase))
                            name = heldName;
                    }
                    catch { /* ignore */ }
                }

                // Deep reflection fallback for game-version variance.
                if (string.IsNullOrEmpty(name))
                {
                    name = TryScanMembersForToolId(inv, 2);
                    if (string.IsNullOrEmpty(name))
                        name = TryScanMembersForToolId(pc, 2);
                }

                // Deterministic fallback: resolve from inventory list + selected index fields.
                if (string.IsNullOrEmpty(name))
                    name = TryGetToolIdFromInventorySelection(inv);

                // Last deterministic fallback: if pickaxe exists in inventory, prefer it over empty.
                if (string.IsNullOrEmpty(name))
                    name = TryGetDefaultToolIdFromInventory(inv);

                if (string.IsNullOrEmpty(name))
                {
                    // Keep last known valid tool to avoid remote visual flicker/disappear
                    // when the game temporarily reports no active tool.
                    _toolNullTicks++;
                    if (_tick % 180 == 0)
                    {
                        string heldType = null;
                        try { heldType = pc?.HeldObject?.GetType().Name; } catch { }
                        _log.LogWarning($"[Session] Unable to resolve equipped tool (invType={inv.GetType().Name}, heldType={heldType ?? "null"})");
                    }
                    return _lastValidToolName;
                }

                _toolNullTicks = 0;
                _lastValidToolName = name;
                // Periodic debug log every ~15 seconds (300 ticks at 20tps)
                if (_tick % 300 == 0)
                    _log.LogInfo($"[Session] EquippedTool: '{name}' (Items={inv.Items?.Count ?? 0})");
                return name;
            }
            catch { return _lastValidToolName; }
        }

        /// <summary>Try to read SavableObjectID-like value from an object using common field/property names.</summary>
        private static string TryGetToolIdFromAny(object obj)
        {
            if (obj == null) return null;

            if (obj is string s)
                return NormalizeToolId(s);

            if (obj is GameObject go)
            {
                var id = TryGetToolIdFromGameObject(go);
                if (!string.IsNullOrEmpty(id)) return id;

                var rb = go.GetComponent<Rigidbody>() ?? go.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    id = TryGetToolIdFromGameObject(rb.gameObject);
                    if (!string.IsNullOrEmpty(id)) return id;
                }
            }

            if (obj is Component comp)
            {
                var idFromComp = TryGetToolIdFromComponent(comp);
                if (!string.IsNullOrEmpty(idFromComp)) return idFromComp;

                var id = TryGetToolIdFromGameObject(comp.gameObject);
                if (!string.IsNullOrEmpty(id)) return id;

                var rb = comp.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    id = TryGetToolIdFromGameObject(rb.gameObject);
                    if (!string.IsNullOrEmpty(id)) return id;
                }
            }

            var type = obj.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // Most game tool objects expose SavableObjectID directly.
            foreach (var propName in SavableIdMemberNames)
            {
                var p = type.GetProperty(propName, flags);
                if (p == null || !p.CanRead) continue;
                try
                {
                    var val = p.GetValue(obj);
                    var norm = NormalizeToolId(val?.ToString());
                    if (!string.IsNullOrEmpty(norm)) return norm;
                }
                catch { }
            }

            foreach (var fieldName in SavableIdMemberNames)
            {
                var f = type.GetField(fieldName, flags);
                if (f == null) continue;
                try
                {
                    var val = f.GetValue(obj);
                    var norm = NormalizeToolId(val?.ToString());
                    if (!string.IsNullOrEmpty(norm)) return norm;
                }
                catch { }
            }

            // Method fallback used by several save-loadable game objects.
            try
            {
                var m = type.GetMethod("GetSavableObjectID", flags, null, Type.EmptyTypes, null);
                if (m != null)
                {
                    var val = m.Invoke(obj, null);
                    var norm = NormalizeToolId(val?.ToString());
                    if (!string.IsNullOrEmpty(norm)) return norm;
                }
            }
            catch { }

            return null;
        }

        /// <summary>Try reading SavableObjectID from any component on a GameObject.</summary>
        private static string TryGetToolIdFromGameObject(GameObject go)
        {
            if (go == null) return null;

            // Check the exact object first.
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var id = TryGetToolIdFromComponent(c);
                if (!string.IsNullOrEmpty(id)) return id;
            }

            // Grabs often target a child collider. Walk parents to find the actual tool/object component.
            var parentComps = go.GetComponentsInParent<Component>(true);
            foreach (var c in parentComps)
            {
                if (c == null) continue;
                var id = TryGetToolIdFromComponent(c);
                if (!string.IsNullOrEmpty(id)) return id;
            }
            return null;
        }

        private static readonly string[] SavableIdMemberNames =
        {
            "SavableObjectID", "savableObjectID", "SavableObjectId", "savableObjectId"
        };

        private static string TryGetToolIdFromComponent(Component comp)
        {
            if (comp == null) return null;
            var t = comp.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var propName in SavableIdMemberNames)
            {
                var p = t.GetProperty(propName, flags);
                if (p == null || !p.CanRead) continue;
                try
                {
                    var val = p.GetValue(comp);
                    var norm = NormalizeToolId(val?.ToString());
                    if (!string.IsNullOrEmpty(norm)) return norm;
                }
                catch { }
            }

            foreach (var fieldName in SavableIdMemberNames)
            {
                var f = t.GetField(fieldName, flags);
                if (f == null) continue;
                try
                {
                    var val = f.GetValue(comp);
                    var norm = NormalizeToolId(val?.ToString());
                    if (!string.IsNullOrEmpty(norm)) return norm;
                }
                catch { }
            }

            try
            {
                var m = t.GetMethod("GetSavableObjectID", flags, null, Type.EmptyTypes, null);
                if (m != null)
                {
                    var val = m.Invoke(comp, null);
                    var norm = NormalizeToolId(val?.ToString());
                    if (!string.IsNullOrEmpty(norm)) return norm;
                }
            }
            catch { }

            return null;
        }

        /// <summary>Try reading a member value from source and then extracting a tool ID from it.</summary>
        private static string TryGetToolIdFromMember(object source, string memberName)
        {
            if (source == null || string.IsNullOrEmpty(memberName)) return null;
            var t = source.GetType();

            var p = t.GetProperty(memberName);
            if (p != null)
            {
                var val = p.GetValue(source);
                var tool = TryGetToolIdFromAny(val);
                if (!string.IsNullOrEmpty(tool)) return tool;
            }

            var f = t.GetField(memberName);
            if (f != null)
            {
                var val = f.GetValue(source);
                var tool = TryGetToolIdFromAny(val);
                if (!string.IsNullOrEmpty(tool)) return tool;
            }

            return null;
        }

        /// <summary>Shallow recursive scan of likely members to find a SavableObjectID-like value.</summary>
        private static string TryScanMembersForToolId(object source, int depth)
        {
            if (source == null || depth < 0) return null;

            // Fast path on the object itself.
            var direct = TryGetToolIdFromAny(source);
            if (!string.IsNullOrEmpty(direct)) return direct;

            var t = source.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var p in t.GetProperties(flags))
            {
                if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
                if (!LooksLikeToolMember(p.Name)) continue;
                try
                {
                    var val = p.GetValue(source);
                    var id = TryGetToolIdFromAny(val);
                    if (!string.IsNullOrEmpty(id)) return id;
                    if (depth > 0)
                    {
                        id = TryScanMembersForToolId(val, depth - 1);
                        if (!string.IsNullOrEmpty(id)) return id;
                    }
                }
                catch { }
            }

            foreach (var f in t.GetFields(flags))
            {
                if (!LooksLikeToolMember(f.Name)) continue;
                try
                {
                    var val = f.GetValue(source);
                    var id = TryGetToolIdFromAny(val);
                    if (!string.IsNullOrEmpty(id)) return id;
                    if (depth > 0)
                    {
                        id = TryScanMembersForToolId(val, depth - 1);
                        if (!string.IsNullOrEmpty(id)) return id;
                    }
                }
                catch { }
            }

            return null;
        }

        private static bool LooksLikeToolMember(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            name = name.ToLowerInvariant();
            return name.Contains("tool") || name.Contains("item") || name.Contains("held")
                || name.Contains("active") || name.Contains("current") || name.Contains("selected")
                || name.Contains("equip") || name.Contains("object");
        }

        /// <summary>Try reading the currently physics-held object (e.g. lantern/tool moved in world).</summary>
        private static bool TryGetHeldObjectState(PlayerController pc, out string heldId, out NetVector3 heldPos, out NetQuaternion heldRot)
        {
            heldId = null;
            heldPos = default;
            heldRot = default;
            if (pc == null) return false;

            try
            {
                var heldObj = pc.HeldObject;
                if (heldObj == null) return false;

                heldId = TryGetToolIdFromAny(heldObj);
                if (string.IsNullOrEmpty(heldId))
                {
                    // Fallback: use object name/type so we can still sync world-held movement
                    // even if SavableObjectID isn't exposed on this build/object.
                    heldId = TryGetHeldObjectFallbackId(heldObj);
                }
                if (string.IsNullOrEmpty(heldId)) return false;

                var t = TryResolveHeldObjectTransform(heldObj);
                if (t == null) return false;

                heldPos = new NetVector3(t.position);
                heldRot = new NetQuaternion(t.rotation);
                return true;
            }
            catch
            {
                // Ignore and try reflection-based fallback below.
            }

            // Fallback: inspect PlayerInventory members for held/carried object references.
            try
            {
                var inv = pc.GetComponent<PlayerInventory>();
                if (inv != null && TryFindHeldObjectTransform(inv, out var t2, out heldId))
                {
                    heldPos = new NetVector3(t2.position);
                    heldRot = new NetQuaternion(t2.rotation);
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static string TryGetToolIdFromInventorySelection(PlayerInventory inv)
        {
            if (inv == null) return null;

            IList items = null;
            try { items = inv.Items as IList; } catch { }
            if (items == null || items.Count == 0) return null;

            int? idx = TryGetIntMember(inv,
                "SelectedToolIndex", "CurrentToolIndex", "ActiveToolIndex",
                "selectedToolIndex", "currentToolIndex", "activeToolIndex",
                "SelectedIndex", "CurrentIndex", "ActiveIndex",
                "selectedIndex", "currentIndex", "activeIndex");

            if (idx.HasValue && idx.Value >= 0 && idx.Value < items.Count)
            {
                var id = TryGetToolIdFromAny(items[idx.Value]);
                if (!string.IsNullOrEmpty(id)) return id;
            }

            return null;
        }

        private static string TryGetDefaultToolIdFromInventory(PlayerInventory inv)
        {
            if (inv == null) return null;
            IList items = null;
            try { items = inv.Items as IList; } catch { }
            if (items == null || items.Count == 0) return null;

            // Prefer pickaxe if present (most common baseline tool).
            foreach (var it in items)
            {
                var id = TryGetToolIdFromAny(it);
                if (string.Equals(id, "PickaxeBasic", StringComparison.OrdinalIgnoreCase))
                    return id;
            }

            // Otherwise first valid item.
            foreach (var it in items)
            {
                var id = TryGetToolIdFromAny(it);
                if (!string.IsNullOrEmpty(id)) return id;
            }

            return null;
        }

        private static int? TryGetIntMember(object source, params string[] names)
        {
            if (source == null || names == null) return null;
            var t = source.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var n in names)
            {
                if (string.IsNullOrEmpty(n)) continue;
                var p = t.GetProperty(n, flags);
                if (p != null && p.CanRead)
                {
                    try
                    {
                        var v = p.GetValue(source);
                        if (v is int pi) return pi;
                    }
                    catch { }
                }

                var f = t.GetField(n, flags);
                if (f != null)
                {
                    try
                    {
                        var v = f.GetValue(source);
                        if (v is int fi) return fi;
                    }
                    catch { }
                }
            }

            return null;
        }

        private static bool TryFindHeldObjectTransform(object source, out Transform tr, out string id)
        {
            tr = null;
            id = null;
            if (source == null) return false;

            var t = source.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var p in t.GetProperties(flags))
            {
                if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
                if (!LooksLikeToolMember(p.Name)) continue;
                object val;
                try { val = p.GetValue(source); } catch { continue; }
                if (val == null) continue;

                var toolId = TryGetToolIdFromAny(val);
                var tf = TryResolveHeldObjectTransform(val) ?? ExtractTransform(val);
                if (!string.IsNullOrEmpty(toolId) && tf != null)
                {
                    tr = tf;
                    id = toolId;
                    return true;
                }
            }

            foreach (var f in t.GetFields(flags))
            {
                if (!LooksLikeToolMember(f.Name)) continue;
                object val;
                try { val = f.GetValue(source); } catch { continue; }
                if (val == null) continue;

                var toolId = TryGetToolIdFromAny(val);
                var tf = TryResolveHeldObjectTransform(val) ?? ExtractTransform(val);
                if (!string.IsNullOrEmpty(toolId) && tf != null)
                {
                    tr = tf;
                    id = toolId;
                    return true;
                }
            }

            return false;
        }

        private static Transform ExtractTransform(object obj)
        {
            if (obj == null) return null;
            if (obj is GameObject go) return go.transform;
            if (obj is Component c) return c.transform;

            try
            {
                var tp = obj.GetType().GetProperty("transform") ?? obj.GetType().GetProperty("Transform");
                if (tp != null) return tp.GetValue(obj) as Transform;
            }
            catch { }
            return null;
        }

        private static Transform TryResolveHeldObjectTransform(object heldObj)
        {
            if (heldObj == null) return null;

            if (heldObj is GameObject go)
            {
                var rb = go.GetComponent<Rigidbody>() ?? go.GetComponentInParent<Rigidbody>();
                if (rb != null) return rb.transform;
                return go.transform;
            }

            if (heldObj is Component comp)
            {
                var rb = comp.GetComponent<Rigidbody>() ?? comp.GetComponentInParent<Rigidbody>();
                if (rb != null) return rb.transform;
                return comp.transform;
            }

            return ExtractTransform(heldObj);
        }

        private static string TryGetHeldObjectFallbackId(object heldObj)
        {
            if (heldObj == null) return null;
            try
            {
                if (heldObj is GameObject go)
                {
                    var extracted = TryGetToolIdFromGameObject(go);
                    if (!string.IsNullOrEmpty(extracted)) return extracted;

                    var rb = go.GetComponent<Rigidbody>() ?? go.GetComponentInParent<Rigidbody>();
                    if (rb != null)
                    {
                        extracted = TryGetToolIdFromGameObject(rb.gameObject);
                        if (!string.IsNullOrEmpty(extracted)) return extracted;
                    }

                    if (!string.IsNullOrEmpty(go.name)) return go.name;
                }
                if (heldObj is Component c)
                {
                    var extracted = TryGetToolIdFromComponent(c);
                    if (!string.IsNullOrEmpty(extracted)) return extracted;

                    var rb = c.GetComponent<Rigidbody>() ?? c.GetComponentInParent<Rigidbody>();
                    if (rb != null)
                    {
                        extracted = TryGetToolIdFromGameObject(rb.gameObject);
                        if (!string.IsNullOrEmpty(extracted)) return extracted;
                    }

                    if (c.gameObject != null && !string.IsNullOrEmpty(c.gameObject.name))
                        return c.gameObject.name;
                }
                return NormalizeToolId(heldObj.GetType().Name);
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeToolId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var value = raw.Trim();
            if (value.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(0, value.Length - "(Clone)".Length).Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Resolve the local player's pose reliably across scene/state transitions.
        /// Falls back to the main camera transform if PlayerController is temporarily unavailable.
        /// </summary>
        private bool TryGetLocalPlayerPose(out Vector3 position, out Quaternion rotation, out PlayerController pc)
        {
            if (_cachedPlayerController == null)
                _cachedPlayerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

            pc = _cachedPlayerController;
            if (pc != null)
            {
                var pcPos = pc.transform.position;
                var pcRot = pc.transform.rotation;

                // Some builds report a stale/default PlayerController transform on clients.
                // If it looks invalid, fall back to camera pose so remote movement still updates.
                var cam = Camera.main;
                if (MultiplayerState.IsClient && cam != null)
                {
                    var camPos = cam.transform.position;
                    bool pcLooksDefault = pcPos.sqrMagnitude < 0.25f && camPos.sqrMagnitude > 1f;
                    bool pcTooFarFromCamera = Vector3.SqrMagnitude(pcPos - camPos) > 64f;
                    if (pcLooksDefault || pcTooFarFromCamera)
                    {
                        if (_tick % 120 == 0)
                            _log.LogWarning($"[Session] Using camera pose fallback for client movement (pc=({pcPos.x:F1},{pcPos.y:F1},{pcPos.z:F1}) cam=({camPos.x:F1},{camPos.y:F1},{camPos.z:F1}))");
                        position = camPos;
                        rotation = cam.transform.rotation;
                        return true;
                    }
                }

                position = pcPos;
                rotation = pcRot;
                return true;
            }

            var mainCam = Camera.main;
            if (mainCam != null)
            {
                position = mainCam.transform.position;
                rotation = mainCam.transform.rotation;
                return true;
            }

            position = default;
            rotation = default;
            return false;
        }

        private static bool GetSprintState(PlayerController pc)
        {
            try
            {
                // Access sprint property via reflection to avoid compile error if API varies
                var prop = pc.GetType().GetProperty("IsSprinting")
                    ?? pc.GetType().GetProperty("isSprinting");
                if (prop != null) return (bool)prop.GetValue(pc);

                var field = pc.GetType().GetField("IsSprinting")
                    ?? pc.GetType().GetField("isSprinting")
                    ?? pc.GetType().GetField("_isSprinting");
                if (field != null) return (bool)field.GetValue(pc);
            }
            catch { /* property may not exist in this game version */ }
            return false;
        }

        private static bool GetCrouchState(PlayerController pc)
        {
            try
            {
                var prop = pc.GetType().GetProperty("IsCrouching")
                    ?? pc.GetType().GetProperty("isCrouching");
                if (prop != null) return (bool)prop.GetValue(pc);

                var field = pc.GetType().GetField("IsCrouching")
                    ?? pc.GetType().GetField("isCrouching")
                    ?? pc.GetType().GetField("_isCrouching");
                if (field != null) return (bool)field.GetValue(pc);
            }
            catch { /* property may not exist in this game version */ }
            return false;
        }

        // ── Host message router ──────────────────────

        private void HandleHostMessage(uint clientId, MessageType type, byte[] payload)
        {
            try
            {
                switch (type)
                {
                    case MessageType.Handshake:
                        var hs = NetSerializer.Deserialize<HandshakeMessage>(payload);
                        HandlePlayerJoined(clientId, hs);
                        break;
                    case MessageType.PlayerInput:
                        var input = NetSerializer.Deserialize<PlayerInputMessage>(payload);
                        HandlePlayerInput(clientId, input);
                        break;
                    case MessageType.PlaceBuilding:
                        var place = NetSerializer.Deserialize<PlaceBuildingMessage>(payload);
                        HandlePlaceBuilding(clientId, place);
                        break;
                    case MessageType.RemoveBuilding:
                        var remove = NetSerializer.Deserialize<RemoveBuildingMessage>(payload);
                        HandleRemoveBuilding(clientId, remove);
                        break;
                    case MessageType.MineNode:
                        var mine = NetSerializer.Deserialize<MineNodeMessage>(payload);
                        HandleMineNode(clientId, mine);
                        break;
                    case MessageType.CrateDamage:
                        var crate = NetSerializer.Deserialize<CrateDamageMessage>(payload);
                        HandleCrateDamage(clientId, crate);
                        break;
                    case MessageType.ResearchItem:
                        var research = NetSerializer.Deserialize<ResearchItemMessage>(payload);
                        HandleResearchItem(clientId, research);
                        break;
                    case MessageType.InteractBuilding:
                        var interact = NetSerializer.Deserialize<InteractBuildingMessage>(payload);
                        HandleInteractBuilding(clientId, interact);
                        break;
                    case MessageType.ResyncRequest:
                        _log.LogInfo($"[Session] Client {clientId} requested resync");
                        if (IsGameSceneReady())
                        {
                            var resyncSnap = BuildSnapshot();
                            var hostDiag = WorldHasher.DiagnoseComponents(resyncSnap);
                            _log.LogInfo($"[Session] Host snapshot for resync: {hostDiag}");
                            _net.SendToClient(clientId, MessageType.FullSnapshot, resyncSnap);
                        }
                        break;
                    case MessageType.OrePositionBatch:
                        var clientOrePos = NetSerializer.Deserialize<List<OrePositionUpdate>>(payload);
                        ApplyClientOrePositions(clientOrePos);
                        break;
                    case MessageType.CratePositionBatch:
                        var clientCratePos = NetSerializer.Deserialize<List<CratePositionUpdate>>(payload);
                        ApplyClientCratePositions(clientCratePos);
                        break;
                    case MessageType.ShopPurchaseNotify:
                        var purchaseMsg = NetSerializer.Deserialize<ShopPurchaseNotifyMessage>(payload);
                        HandleShopPurchaseNotify(clientId, purchaseMsg);
                        break;
                    case MessageType.ItemDropped:
                        var dropMsg = NetSerializer.Deserialize<ItemDroppedMessage>(payload);
                        HandleItemDropped(clientId, dropMsg);
                        break;
                    case MessageType.ClientInventoryReport:
                        var invReport = NetSerializer.Deserialize<ClientInventoryReportMessage>(payload);
                        HandleClientInventoryReport(clientId, invReport);
                        break;
                    case MessageType.SoundEvent:
                        var hostSoundMsg = NetSerializer.Deserialize<SoundEventMessage>(payload);
                        HandleSoundEventFromClient(clientId, hostSoundMsg);
                        break;
                    case MessageType.ToolDrop:
                        var toolDrop = NetSerializer.Deserialize<ToolDropMessage>(payload);
                        HandleToolDrop(clientId, toolDrop);
                        break;
                    case MessageType.ToolPickup:
                        var toolPickup = NetSerializer.Deserialize<ToolPickupMessage>(payload);
                        HandleToolPickup(clientId, toolPickup);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[Session] Error handling host message {type} from client {clientId}: {ex}");
            }
        }

        // ── Client message router ────────────────────

        private void HandleClientMessage(uint _, MessageType type, byte[] payload)
        {
            try
            {
                switch (type)
                {
                    case MessageType.HandshakeAck:
                        var ack = NetSerializer.Deserialize<HandshakeAckMessage>(payload);
                        if (!ack.Accepted)
                        {
                            var reason = !string.IsNullOrEmpty(ack.RejectionReason) ? ack.RejectionReason : "Rejected by host";
                            _log.LogWarning($"[Session] Handshake rejected: {reason}");
                            LogEvent($"Rejected: {reason}");
                            Stop();
                            break;
                        }
                        _handshakeAckReceived = true;
                        _log.LogInfo($"[Session] Handshake accepted, assigned player ID {ack.AssignedPlayerId}");
                        MultiplayerState.LocalPlayerId = ack.AssignedPlayerId;
                        // Load the host's gameplay scene if we're not already in it
                        if (!string.IsNullOrEmpty(ack.SceneName) && SceneManager.GetActiveScene().name != ack.SceneName)
                        {
                            _clientLoadingScene = true;
                            _log.LogInfo($"[Session] Loading host scene '{ack.SceneName}'...");
                            LogEvent("Loading level...");
                            SceneManager.LoadScene(ack.SceneName);
                        }
                        break;
                    case MessageType.FullSnapshot:
                        if (_clientLoadingScene) break;
                        _clientWaitingForSnapshot = false;
                        _log.LogInfo($"[Session] Received FullSnapshot ({payload?.Length ?? 0} bytes)");
                        var snap = NetSerializer.Deserialize<WorldSnapshot>(payload);
                        ApplySnapshot(snap);
                        break;
                    case MessageType.DeltaUpdate:
                        if (_clientLoadingScene || _clientWaitingForSnapshot) break;
                        var delta = NetSerializer.Deserialize<WorldDelta>(payload);
                        ApplyDelta(delta);
                        break;
                    case MessageType.HashCheck:
                        if (_clientLoadingScene || _clientWaitingForSnapshot) break;
                        var hash = NetSerializer.Deserialize<WorldHash>(payload);
                        CheckHash(hash);
                        break;
                    case MessageType.BuildingSpawned:
                        if (_clientWaitingForSnapshot) break;
                        var bld = NetSerializer.Deserialize<BuildingState>(payload);
                        OnRemoteBuildingSpawned(bld);
                        break;
                    case MessageType.BuildingRemoved:
                        if (_clientWaitingForSnapshot) break;
                        var bldRemoval = NetSerializer.Deserialize<BuildingRemovalInfo>(payload);
                        OnRemoteBuildingRemoved(bldRemoval);
                        break;
                    case MessageType.OreSpawned:
                        if (_clientWaitingForSnapshot) break;
                        var ore = NetSerializer.Deserialize<OrePieceState>(payload);
                        OnRemoteOreSpawned(ore);
                        break;
                    case MessageType.OreRemoved:
                        if (_clientWaitingForSnapshot) break;
                        var oreId = NetSerializer.Deserialize<int>(payload);
                        OnRemoteOreRemoved(oreId);
                        break;
                    case MessageType.OreSpawnedBatch:
                        if (_clientWaitingForSnapshot) break;
                        var oreBatch = NetSerializer.Deserialize<List<OrePieceState>>(payload);
                        foreach (var o in oreBatch) OnRemoteOreSpawned(o);
                        break;
                    case MessageType.OreRemovedBatch:
                        if (_clientWaitingForSnapshot) break;
                        var oreIdBatch = NetSerializer.Deserialize<List<int>>(payload);
                        foreach (var id in oreIdBatch) OnRemoteOreRemoved(id);
                        break;
                    case MessageType.MineNode:
                        if (_clientWaitingForSnapshot) break;
                        var mineMsg = NetSerializer.Deserialize<MineNodeMessage>(payload);
                        ApplyRemoteMining(mineMsg);
                        break;
                    case MessageType.CrateDamage:
                        if (_clientWaitingForSnapshot) break;
                        var crateMsg = NetSerializer.Deserialize<CrateDamageMessage>(payload);
                        ApplyRemoteCrateDamage(crateMsg);
                        break;
                    case MessageType.OrePositionBatch:
                        if (_clientWaitingForSnapshot) break;
                        var posUpdates = NetSerializer.Deserialize<List<OrePositionUpdate>>(payload);
                        ApplyOrePositionUpdates(posUpdates);
                        break;
                    case MessageType.CratePositionBatch:
                        if (_clientWaitingForSnapshot) break;
                        var crateUpdates = NetSerializer.Deserialize<List<CratePositionUpdate>>(payload);
                        ApplyCratePositionUpdates(crateUpdates);
                        break;
                    case MessageType.InventorySync:
                        var invSync = NetSerializer.Deserialize<InventorySyncMessage>(payload);
                        if (invSync?.Tools != null && invSync.Tools.Length > 0)
                            ReconcileToolInventory(invSync.Tools);
                        break;
                    case MessageType.ItemDropped:
                        if (_clientWaitingForSnapshot) break;
                        var clientDropMsg = NetSerializer.Deserialize<ItemDroppedMessage>(payload);
                        ApplyItemDropped(clientDropMsg);
                        break;
                    case MessageType.ToolDrop:
                        if (_clientWaitingForSnapshot) break;
                        var remoteToolDrop = NetSerializer.Deserialize<ToolDropMessage>(payload);
                        ApplyRemoteToolDrop(remoteToolDrop);
                        break;
                    case MessageType.ToolPickup:
                        if (_clientWaitingForSnapshot) break;
                        var remoteToolPickup = NetSerializer.Deserialize<ToolPickupMessage>(payload);
                        ApplyRemoteToolPickup(remoteToolPickup);
                        break;
                    case MessageType.SoundEvent:
                        var clientSoundMsg = NetSerializer.Deserialize<SoundEventMessage>(payload);
                        HandleSoundEventOnClient(clientSoundMsg);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[Session] Error handling client message {type}: {ex}");
            }
        }

        // ── Host event handlers ──────────────────────

        private void HandleClientConnected(uint clientId)
        {
            _log.LogInfo($"[Session] Steam client {clientId} connected, awaiting handshake...");
        }

        private void HandleClientDisconnected(uint clientId)
        {
            _pendingSnapshotClients.Remove(clientId);
            string name = "Unknown";
            if (_clientNames.TryGetValue(clientId, out var n))
            {
                name = n;
                _clientNames.Remove(clientId);
            }
            _players.RemoveAll(p => p.PlayerId == (int)clientId);
            _log.LogInfo($"[Session] Player '{name}' (ID {clientId}) left");
            LogLeave(name);
        }

        private void HandlePlayerJoined(uint clientId, HandshakeMessage handshake)
        {
            _log.LogInfo($"[Session] Received handshake from client {clientId}: '{handshake.PlayerName}' v{handshake.ModVersion}");

            // If this client already completed handshake, resend ack + snapshot (retry case)
            if (_clientNames.ContainsKey(clientId))
            {
                _log.LogInfo($"[Session] Client {clientId} already joined — resending ack and snapshot");
                _net.SendToClient(clientId, MessageType.HandshakeAck, new HandshakeAckMessage
                {
                    AssignedPlayerId = (int)clientId,
                    Accepted = true,
                    SceneName = SceneManager.GetActiveScene().name
                });
                if (IsGameSceneReady())
                {
                    var resnapshot = BuildSnapshot();
                    _log.LogInfo($"[Session] Sending snapshot to already-joined client {clientId}");
                    _net.SendToClient(clientId, MessageType.FullSnapshot, resnapshot);
                }
                return;
            }

            // Reject if host scene is still loading
            if (_pendingHost)
            {
                _log.LogInfo($"[Session] Player '{handshake.PlayerName}' connected while scene loading — rejecting with retry hint");
                _net.SendToClient(clientId, MessageType.HandshakeAck, new HandshakeAckMessage
                {
                    AssignedPlayerId = -1,
                    Accepted = false,
                    RejectionReason = "Host is still loading. Please try again in a moment."
                });
                return;
            }

            // Version mismatch check
            if (!string.IsNullOrEmpty(handshake.ModVersion) && handshake.ModVersion != PluginInfo.Version)
            {
                _log.LogWarning($"[Session] Player '{handshake.PlayerName}' has mod version {handshake.ModVersion}, expected {PluginInfo.Version}");
                _net.SendToClient(clientId, MessageType.HandshakeAck, new HandshakeAckMessage
                {
                    AssignedPlayerId = -1,
                    Accepted = false,
                    RejectionReason = $"Version mismatch: you have {handshake.ModVersion}, host has {PluginInfo.Version}"
                });
                return;
            }

            // 4-player limit check
            if (_players.Count >= MaxPlayers)
            {
                _log.LogWarning($"[Session] Player '{handshake.PlayerName}' rejected: server full ({_players.Count}/{MaxPlayers})");
                _net.SendToClient(clientId, MessageType.HandshakeAck, new HandshakeAckMessage
                {
                    AssignedPlayerId = -1,
                    Accepted = false,
                    RejectionReason = $"Server full ({_players.Count}/{MaxPlayers} players)"
                });
                return;
            }

            _clientNames[clientId] = handshake.PlayerName;
            if (handshake.SteamId != 0)
                _clientSteamIds[clientId] = handshake.SteamId;
            _players.Add(new PlayerState
            {
                PlayerId = (int)clientId,
                DisplayName = handshake.PlayerName
            });

            // Send ack with assigned ID and the scene the client needs to load
            var ackMsg = new HandshakeAckMessage
            {
                AssignedPlayerId = (int)clientId,
                Accepted = true,
                SceneName = SceneManager.GetActiveScene().name
            };
            _net.SendToClient(clientId, MessageType.HandshakeAck, ackMsg);
            _log.LogInfo($"[Session] Sent HandshakeAck to client {clientId} (scene='{ackMsg.SceneName}')");

            // Send full snapshot if scene is ready, otherwise queue for next tick
            if (IsGameSceneReady())
            {
                var snapshot = BuildSnapshot();
                _log.LogInfo($"[Session] Sending FullSnapshot to client {clientId}");
                _net.SendToClient(clientId, MessageType.FullSnapshot, snapshot);
            }
            else
            {
                _pendingSnapshotClients.Add(clientId);
                _log.LogInfo($"[Session] Scene not ready — queued snapshot for client {clientId}");
            }

            _log.LogInfo($"[Session] Player '{handshake.PlayerName}' (ID {clientId}) joined");
            LogJoin(handshake.PlayerName);

            // If host has saved inventory for this player, send it
            if (handshake.SteamId != 0 && _savedPlayerInventories.TryGetValue(handshake.SteamId, out var savedTools))
            {
                _log.LogInfo($"[Session] Sending saved inventory ({savedTools.Length} tools) to client {clientId} (Steam {handshake.SteamId})");
                _net.SendToClient(clientId, MessageType.InventorySync, new InventorySyncMessage { Tools = savedTools });
            }
        }

        private void HandlePlayerInput(uint clientId, PlayerInputMessage input)
        {
            var player = _players.Find(p => p.PlayerId == (int)clientId);
            if (player == null) return;
            player.Position = input.Position;
            player.Rotation = input.Rotation;

            var playerId = player.PlayerId;
            var incomingTool = NormalizeToolId(input.EquippedTool);
            var incomingHeld = NormalizeToolId(input.HeldObjectId);

            // If explicit tool is empty while the player is grabbing a world object,
            // prefer that object ID so remote visuals remain stable.
            if (string.IsNullOrEmpty(incomingTool) && !string.IsNullOrEmpty(incomingHeld))
                incomingTool = incomingHeld;

            if (string.IsNullOrEmpty(incomingTool))
            {
                int emptyTicks = 0;
                _hostEmptyToolTicksByPlayerId.TryGetValue(playerId, out emptyTicks);
                emptyTicks++;
                _hostEmptyToolTicksByPlayerId[playerId] = emptyTicks;

                if (_hostLastToolByPlayerId.TryGetValue(playerId, out var lastTool) &&
                    !string.IsNullOrEmpty(lastTool) &&
                    emptyTicks < HostToolEmptyDebounce)
                {
                    player.EquippedTool = lastTool;
                }
                else
                {
                    player.EquippedTool = null;
                }
            }
            else
            {
                _hostLastToolByPlayerId[playerId] = incomingTool;
                _hostEmptyToolTicksByPlayerId[playerId] = 0;
                player.EquippedTool = incomingTool;
            }

            player.IsCrouching = input.IsCrouching;
            player.HeldObjectId = incomingHeld;
            player.HeldObjectPosition = input.HeldObjectPosition;
            player.HeldObjectRotation = input.HeldObjectRotation;

            if (_tick % 120 == 0)
                _log.LogInfo($"[Session] Input P{clientId}: ({input.Position.X:F1},{input.Position.Y:F1},{input.Position.Z:F1}) tool='{player.EquippedTool}' held='{player.HeldObjectId}'");
        }

        private void HandlePlaceBuilding(uint clientId, PlaceBuildingMessage msg)
        {
            _log.LogInfo($"[Session] Player {clientId} placing '{msg.SavableObjectId}' at {msg.Position}");

            if (!System.Enum.TryParse<SavableObjectID>(msg.SavableObjectId, out var savableId))
            {
                _log.LogWarning($"[Session] Unknown SavableObjectID: {msg.SavableObjectId}");
                return;
            }

            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm == null) return;

            var prefab = slm.GetPrefab(savableId);
            if (prefab == null)
            {
                _log.LogWarning($"[Session] No prefab found for {savableId}");
                return;
            }

            var pos = msg.Position.ToUnity();
            var rot = msg.Rotation.ToUnity();

            BuildingPatch.NetworkBypass = true;
            try
            {
                var go = UnityEngine.Object.Instantiate(prefab, pos, rot);
                var bo = go.GetComponent<BuildingObject>();
                if (bo != null)
                {
                    // Broadcast the new building to all clients
                    var state = new BuildingState
                    {
                        LocalInstanceId = bo.GetInstanceID(),
                        SavableObjectId = msg.SavableObjectId,
                        Position = msg.Position,
                        Rotation = msg.Rotation
                    };
                    _net.SendToAll(MessageType.BuildingSpawned, state);
                }
            }
            finally { BuildingPatch.NetworkBypass = false; }
        }

        private void HandleRemoveBuilding(uint clientId, RemoveBuildingMessage msg)
        {
            _log.LogInfo($"[Session] Player {clientId} removing building '{msg.SavableObjectId}' at {msg.Position}");
            foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
            {
                if (bo.SavableObjectID.ToString() == msg.SavableObjectId &&
                    Vector3.Distance(bo.transform.position, msg.Position.ToUnity()) < 0.15f)
                {
                    BuildingPatch.NetworkBypass = true;
                    try { bo.TryTakeOrPack(); }
                    finally { BuildingPatch.NetworkBypass = false; }

                    var removalInfo = new BuildingRemovalInfo
                    {
                        Position = new NetVector3(bo.transform.position),
                        SavableObjectId = msg.SavableObjectId
                    };
                    DirtyTracker.RemovedBuildings.Add(removalInfo);

                    // Broadcast removal to all clients
                    _net.SendToAll(MessageType.BuildingRemoved, removalInfo);
                    break;
                }
            }
        }

        private void HandleMineNode(uint clientId, MineNodeMessage msg)
        {
            _log.LogInfo($"[Session] Player {clientId} mining at {msg.NodePosition}");
            OreNode closest = null;
            float closestDist = float.MaxValue;
            var nodePos = msg.NodePosition.ToUnity();
            var hitPos = msg.HitPosition.ToUnity();
            foreach (var node in UnityEngine.Object.FindObjectsByType<OreNode>(FindObjectsSortMode.None))
            {
                // Prefer matching by reported node position, but also consider hit position
                // to tolerate small host/client transform drift.
                float distNode = Vector3.Distance(node.transform.position, nodePos);
                float distHit = Vector3.Distance(node.transform.position, hitPos);
                float dist = Mathf.Min(distNode, distHit);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = node;
                }
            }
            if (closest != null && closestDist < 3f)
            {
                MiningPatch.NetworkBypass = true;
                try { closest.TakeDamage(msg.Damage, hitPos); }
                finally { MiningPatch.NetworkBypass = false; }

                // Host-applied remote mining must be explicitly rebroadcast to clients.
                // Postfix broadcast is bypassed in this code path via NetworkBypass.
                BroadcastMineNode(msg.NodePosition, msg.Damage, msg.HitPosition);
            }
            else
            {
                _log.LogWarning($"[Session] MineNode from player {clientId} had no nearby ore node (bestDist={closestDist:F2})");
            }
        }

        private void HandleCrateDamage(uint clientId, CrateDamageMessage msg)
        {
            _log.LogInfo($"[Session] Player {clientId} hitting crate at {msg.CratePosition}");
            BreakableCrate closest = null;
            float closestDist = float.MaxValue;
            foreach (var crate in UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None))
            {
                float dist = Vector3.Distance(crate.transform.position, msg.CratePosition.ToUnity());
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = crate;
                }
            }
            if (closest != null && closestDist < 2f)
            {
                // Capture position before TakeDamage (crate may be destroyed)
                var cratePos = new NetVector3(closest.transform.position);

                Patches.MachineProcessingPatch.CrateNetworkBypass = true;
                try { closest.TakeDamage(msg.Damage, msg.HitPosition.ToUnity()); }
                finally { Patches.MachineProcessingPatch.CrateNetworkBypass = false; }

                // Broadcast to ALL clients so they see the damage/break.
                // Crates are NOT tracked in building snapshots or delta sync —
                // this broadcast is the ONLY way clients learn about crate state changes.
                BroadcastCrateDamage(cratePos, msg.Damage, msg.HitPosition);
            }
        }

        private void HandleResearchItem(uint clientId, ResearchItemMessage msg)
        {
            _log.LogInfo($"[Session] Player {clientId} researching '{msg.ResearchItemId}'");
            var research = Singleton<ResearchManager>.Instance;
            if (research == null) return;

            if (!System.Enum.TryParse<SavableObjectID>(msg.ResearchItemId, out var savableId))
            {
                _log.LogWarning($"[Session] Unknown research ID: {msg.ResearchItemId}");
                return;
            }

            var itemDef = research.GetResearchItemByID(savableId);
            if (itemDef == null)
            {
                _log.LogWarning($"[Session] No ResearchItemDefinition for {savableId}");
                return;
            }

            ResearchPatch.NetworkBypass = true;
            try
            {
                research.ResearchItem(itemDef);
                DirtyTracker.ResearchDirty = true;
            }
            finally { ResearchPatch.NetworkBypass = false; }
        }

        private void HandleInteractBuilding(uint clientId, InteractBuildingMessage msg)
        {
            _log.LogInfo($"[Session] Player {clientId} interacting '{msg.Action}' at {msg.Position}");

            // ── Handle detonator actions first (they are NOT BuildingObjects) ──
            if (msg.Action == "triggerDetonator")
            {
                DetonatorTrigger bestTrigger = null;
                float bestDist = 50f;
                foreach (var dt in UnityEngine.Object.FindObjectsByType<DetonatorTrigger>(FindObjectsSortMode.None))
                {
                    if (dt == null || dt.HasTriggered) continue;
                    float d = Vector3.Distance(dt.transform.position, msg.Position.ToUnity());
                    if (d < bestDist) { bestDist = d; bestTrigger = dt; }
                }
                if (bestTrigger != null)
                {
                    _log.LogInfo($"[Session] Found DetonatorTrigger at dist={bestDist:F2}");
                    BuildingInteractionPatch.NetworkBypass = true;
                    try { bestTrigger.Interact(null); }
                    finally { BuildingInteractionPatch.NetworkBypass = false; }
                    // NetworkBypass suppresses the postfix dirty-tracking, so mark manually
                    var det = bestTrigger.GetComponentInParent<DetonatorExplosion>()
                           ?? bestTrigger.GetComponentInChildren<DetonatorExplosion>();
                    if (det != null) DirtyTracker.DirtyDetonatorIds.Add(det.DetonatorID);
                }
                else
                {
                    _log.LogWarning($"[Session] No DetonatorTrigger found near {msg.Position}");
                }
                return;
            }

            if (msg.Action == "buyDetonator")
            {
                DetonatorBuySign bestBuy = null;
                float bestBuyDist = 50f;
                foreach (var bs in UnityEngine.Object.FindObjectsByType<DetonatorBuySign>(FindObjectsSortMode.None))
                {
                    if (bs == null) continue;
                    float d = Vector3.Distance(bs.transform.position, msg.Position.ToUnity());
                    if (d < bestBuyDist) { bestBuyDist = d; bestBuy = bs; }
                }
                if (bestBuy != null)
                {
                    _log.LogInfo($"[Session] Found DetonatorBuySign at dist={bestBuyDist:F2}");
                    EconomyPatch.NetworkBypass = true;
                    BuildingInteractionPatch.NetworkBypass = true;
                    try { bestBuy.TryBuySign(); }
                    finally
                    {
                        EconomyPatch.NetworkBypass = false;
                        BuildingInteractionPatch.NetworkBypass = false;
                    }
                    // NetworkBypass suppresses the postfix dirty-tracking, so mark manually
                    var det = bestBuy.GetComponentInParent<DetonatorExplosion>()
                           ?? bestBuy.GetComponentInChildren<DetonatorExplosion>();
                    if (det != null) DirtyTracker.DirtyDetonatorIds.Add(det.DetonatorID);
                    DirtyTracker.MoneyDirty = true;
                }
                else
                {
                    _log.LogWarning($"[Session] No DetonatorBuySign found near {msg.Position}");
                }
                return;
            }

            // ── Standard BuildingObject actions ──
            // Find the closest building to the given position
            BuildingObject target = null;
            float closest = 1.0f;
            foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
            {
                if (bo.IsGhost) continue;
                float dist = Vector3.Distance(bo.transform.position, msg.Position.ToUnity());
                if (dist < closest)
                {
                    closest = dist;
                    target = bo;
                }
            }
            if (target == null)
            {
                _log.LogWarning($"[Session] No building found near {msg.Position} for action '{msg.Action}'");
                return;
            }

            switch (msg.Action)
            {
                case "toggle":
                    var belt = target.GetComponent<ConveyorBelt>();
                    if (belt != null)
                    {
                        belt.Disabled = !belt.Disabled;
                        DirtyTracker.DirtyBeltIds.Add(belt.GetInstanceID());
                    }
                    break;

                case "toggleAutoMiner":
                    var miner = target.GetComponent<AutoMiner>();
                    if (miner != null)
                    {
                        BuildingInteractionPatch.NetworkBypass = true;
                        try { miner.Toggle(!miner.Enabled); }
                        finally { BuildingInteractionPatch.NetworkBypass = false; }
                        DirtyTracker.DirtyMachineInstanceIds.Add(target.GetInstanceID());
                    }
                    break;

                case "toggleHatch":
                    var hatch = target.GetComponent<ChuteHatch>();
                    if (hatch != null)
                    {
                        BuildingInteractionPatch.NetworkBypass = true;
                        try { hatch.SetDirection(!hatch.IsClosed); }
                        finally { BuildingInteractionPatch.NetworkBypass = false; }
                        DirtyTracker.DirtyMachineInstanceIds.Add(target.GetInstanceID());
                    }
                    break;

                case "toggleBlocker":
                    var blocker = target.GetComponent<ConveyorBlockerT2>();
                    if (blocker != null)
                    {
                        BuildingInteractionPatch.NetworkBypass = true;
                        try { blocker.Toggle(); }
                        finally { BuildingInteractionPatch.NetworkBypass = false; }
                        DirtyTracker.DirtyMachineInstanceIds.Add(target.GetInstanceID());
                    }
                    break;

                case "editSign":
                    var sign = target.GetComponent<EditableSign>();
                    if (sign != null)
                    {
                        BuildingInteractionPatch.NetworkBypass = true;
                        try { sign.UpdateText(msg.Data ?? ""); }
                        finally { BuildingInteractionPatch.NetworkBypass = false; }
                        DirtyTracker.DirtyMachineInstanceIds.Add(target.GetInstanceID());
                    }
                    break;

                case "toggleOreSpawner":
                    var spawner = target.GetComponentInChildren<OreSpawnerMacine>();
                    if (spawner != null)
                    {
                        BuildingInteractionPatch.NetworkBypass = true;
                        try { spawner.Toggle(msg.Data == "on"); }
                        finally { BuildingInteractionPatch.NetworkBypass = false; }
                        DirtyTracker.DirtyMachineInstanceIds.Add(target.GetInstanceID());
                    }
                    break;

                case "configureOreSpawner":
                    var spawnerCfg = target.GetComponentInChildren<OreSpawnerMacine>();
                    if (spawnerCfg != null && msg.Data != null)
                    {
                        BuildingInteractionPatch.NetworkBypass = true;
                        try { spawnerCfg.LoadFromSave(msg.Data); }
                        finally { BuildingInteractionPatch.NetworkBypass = false; }
                        DirtyTracker.DirtyMachineInstanceIds.Add(target.GetInstanceID());
                    }
                    break;

                default:
                    // Fallback: mark machine dirty for CustomSaveData propagation
                    if (target is ICustomSaveDataProvider)
                        DirtyTracker.DirtyMachineInstanceIds.Add(target.GetInstanceID());
                    break;
            }
        }

        // ── Client state application ─────────────────

        private void ApplySnapshot(WorldSnapshot snapshot)
        {
            _log.LogInfo($"[Session] Received snapshot at tick {snapshot.Tick}");
            LogEvent("Received world snapshot from host.");

            if (snapshot.World != null)
            {
                EconomyPatch.NetworkBypass = true;
                ResearchPatch.NetworkBypass = true;
                try
                {
                    var eco = Singleton<EconomyManager>.Instance;
                    var research = Singleton<ResearchManager>.Instance;
                    if (eco != null) eco.SetMoney(snapshot.World.Money);
                    if (research != null)
                    {
                        // Sync completed research using the game's save-load API
                        if (snapshot.World.CompletedResearchIds != null)
                        {
                            var completedIds = new List<SavableObjectID>();
                            foreach (var idStr in snapshot.World.CompletedResearchIds)
                            {
                                if (System.Enum.TryParse<SavableObjectID>(idStr, out var sid))
                                    completedIds.Add(sid);
                            }
                            research.LoadFromSaveFile(completedIds);
                        }
                        research.SetResearchTickets(snapshot.World.ResearchTickets);
                    }
                }
                finally
                {
                    EconomyPatch.NetworkBypass = false;
                    ResearchPatch.NetworkBypass = false;
                }

                // Sync quest completions (QuestID enum, not SavableObjectID)
                var quests = Singleton<QuestManager>.Instance;
                if (quests != null)
                {
                    QuestPatch.NetworkBypass = true;
                    try
                    {
                        if (snapshot.World.CompletedQuestIds != null)
                        {
                            var hostCompleted = new HashSet<QuestID>();
                            foreach (var idStr in snapshot.World.CompletedQuestIds)
                            {
                                if (System.Enum.TryParse<QuestID>(idStr, out var qid))
                                    hostCompleted.Add(qid);
                            }
                            foreach (var quest in quests.AllQuests)
                            {
                                if (hostCompleted.Contains(quest.QuestID) && !quest.IsCompleted())
                                    quest.UnlockFromLoadingSaveFile();
                            }
                        }

                        // Activate and sync progress for active quests
                        if (snapshot.World.ActiveQuests != null)
                        {
                            foreach (var aq in snapshot.World.ActiveQuests)
                            {
                                if (System.Enum.TryParse<QuestID>(aq.QuestId, out var qid))
                                {
                                    var quest = quests.GetQuestByID(qid);
                                    if (quest != null && !quest.IsActive() && !quest.IsCompleted())
                                        quests.ForceActivateQuest(qid);
                                }
                            }
                            ApplyActiveQuestProgress(quests, snapshot.World.ActiveQuests);
                        }
                    }
                    finally { QuestPatch.NetworkBypass = false; }
                }

                // Sync shop purchases
                var eco2 = Singleton<EconomyManager>.Instance;
                if (snapshot.World.ShopPurchases != null && eco2?.ShopPurchases != null)
                {
                    eco2.ShopPurchases.Purchases.Clear();
                    foreach (var sp in snapshot.World.ShopPurchases)
                    {
                        if (System.Enum.TryParse<SavableObjectID>(sp.SavableObjectId, out var sid))
                            eco2.ShopPurchases.Purchases.Add(
                                new ShopObjectPurchaseEntry { SavableObjectID = sid, AmountPurchased = sp.Amount });
                    }
                }

                // Sync contracts
                var contracts = Singleton<ContractsManager>.Instance;
                if (contracts != null)
                    ApplyContractState(contracts, snapshot.World);

                // Sync game mode type (Standard/Sandbox)
                var gmm = Singleton<GamemodeManager>.Instance;
                if (gmm != null)
                    gmm.GameModeType = (GameModeType)snapshot.World.GameModeType;
            }

            // Update remote player visuals
            if (snapshot.Players != null)
                RemotePlayerManager.UpdatePlayers(snapshot.Players, MultiplayerState.LocalPlayerId);

            // Apply conveyor states
            if (snapshot.Conveyors != null)
                ApplyConveyorStates(snapshot.Conveyors);

            // Reconcile buildings: spawn missing, remove extras
            if (snapshot.Buildings != null)
                ReconcileBuildingsFromSnapshot(snapshot.Buildings);

            // Reconcile ores: clear local, respawn from host
            if (snapshot.OrePieces != null)
                ReconcileOresFromSnapshot(snapshot.OrePieces);

            // Reconcile crate positions from host
            if (snapshot.Crates != null)
                ReconcileCratesFromSnapshot(snapshot.Crates);

            // Apply detonator/explosion states (doors blown open, TNT purchased, etc.)
            if (snapshot.Detonators != null)
                ApplyDetonatorStates(snapshot.Detonators);
        }

        private bool _loggedFirstDelta;

        private void ApplyDelta(WorldDelta delta)
        {
            // Log the first delta with player data so we can confirm sync is working
            if (!_loggedFirstDelta && delta.PlayerUpdates != null && delta.PlayerUpdates.Count > 0)
            {
                _loggedFirstDelta = true;
                var sb = new System.Text.StringBuilder();
                sb.Append($"[Session] First delta received with {delta.PlayerUpdates.Count} players: ");
                foreach (var p in delta.PlayerUpdates)
                    sb.Append($"P{p.PlayerId}=({p.Position.X:F1},{p.Position.Y:F1},{p.Position.Z:F1}) ");
                _log.LogInfo(sb.ToString());
            }

            if (delta.World != null)
            {
                EconomyPatch.NetworkBypass = true;
                ResearchPatch.NetworkBypass = true;
                try
                {
                    var eco = Singleton<EconomyManager>.Instance;
                    var research = Singleton<ResearchManager>.Instance;
                    if (eco != null) eco.SetMoney(delta.World.Money);
                    if (research != null)
                    {
                        // Sync research whenever the host set differs from local
                        if (delta.World.CompletedResearchIds != null)
                        {
                            var completedIds = new List<SavableObjectID>();
                            foreach (var idStr in delta.World.CompletedResearchIds)
                            {
                                if (System.Enum.TryParse<SavableObjectID>(idStr, out var sid))
                                    completedIds.Add(sid);
                            }
                            // Only reload if the set actually differs (content comparison)
                            var localSet = new HashSet<SavableObjectID>(research.CompletedResearchItems);
                            var hostSet = new HashSet<SavableObjectID>(completedIds);
                            if (!localSet.SetEquals(hostSet))
                                research.LoadFromSaveFile(completedIds);
                        }
                        research.SetResearchTickets(delta.World.ResearchTickets);
                    }

                    // Sync quest completions (QuestID enum, not SavableObjectID)
                    var quests = Singleton<QuestManager>.Instance;
                    if (quests != null && delta.World.CompletedQuestIds != null)
                    {
                        QuestPatch.NetworkBypass = true;
                        try
                        {
                            var hostCompleted = new HashSet<QuestID>();
                            foreach (var idStr in delta.World.CompletedQuestIds)
                            {
                                if (System.Enum.TryParse<QuestID>(idStr, out var qid))
                                    hostCompleted.Add(qid);
                            }
                            foreach (var quest in quests.AllQuests)
                            {
                                if (hostCompleted.Contains(quest.QuestID) && !quest.IsCompleted())
                                    quest.UnlockFromLoadingSaveFile();
                            }

                            // Activate and sync progress for active quests
                            if (delta.World.ActiveQuests != null)
                            {
                                foreach (var aq in delta.World.ActiveQuests)
                                {
                                    if (System.Enum.TryParse<QuestID>(aq.QuestId, out var qid))
                                    {
                                        var quest = quests.GetQuestByID(qid);
                                        if (quest != null && !quest.IsActive() && !quest.IsCompleted())
                                            quests.ForceActivateQuest(qid);
                                    }
                                }
                                ApplyActiveQuestProgress(quests, delta.World.ActiveQuests);
                            }
                        }
                        finally { QuestPatch.NetworkBypass = false; }
                    }

                    // Sync shop purchases
                    if (delta.World.ShopPurchases != null && eco?.ShopPurchases != null)
                    {
                        eco.ShopPurchases.Purchases.Clear();
                        foreach (var sp in delta.World.ShopPurchases)
                        {
                            if (System.Enum.TryParse<SavableObjectID>(sp.SavableObjectId, out var sid))
                                eco.ShopPurchases.Purchases.Add(
                                    new ShopObjectPurchaseEntry { SavableObjectID = sid, AmountPurchased = sp.Amount });
                        }
                    }

                    // Sync contracts
                    var contracts = Singleton<ContractsManager>.Instance;
                    if (contracts != null)
                        ApplyContractState(contracts, delta.World);
                }
                finally
                {
                    EconomyPatch.NetworkBypass = false;
                    ResearchPatch.NetworkBypass = false;
                }
            }

            // Update remote player visuals from host-sent positions
            if (delta.PlayerUpdates != null)
                RemotePlayerManager.UpdatePlayers(delta.PlayerUpdates, MultiplayerState.LocalPlayerId);

            // Apply conveyor changes
            if (delta.ChangedConveyors != null && delta.ChangedConveyors.Count > 0)
                ApplyConveyorStates(delta.ChangedConveyors);

            // Apply building removals
            if (delta.RemovedBuildings != null)
            {
                foreach (var info in delta.RemovedBuildings)
                    OnRemoteBuildingRemoved(info);
            }

            // Apply building state updates (custom save data)
            if (delta.ChangedBuildings != null && delta.ChangedBuildings.Count > 0)
                ApplyBuildingUpdates(delta.ChangedBuildings);

            // Apply detonator state changes
            if (delta.ChangedDetonators != null && delta.ChangedDetonators.Count > 0)
                ApplyDetonatorStates(delta.ChangedDetonators);
        }

        private void ApplyActiveQuestProgress(QuestManager quests, ActiveQuestData[] activeQuests)
        {
            foreach (var aq in activeQuests)
            {
                if (!System.Enum.TryParse<QuestID>(aq.QuestId, out var qid)) continue;
                var quest = quests.GetQuestByID(qid);
                if (quest == null) continue;

                int ri = 0, ti = 0;
                foreach (var req in quest.QuestRequirements)
                {
                    if (req is ResourceQuestRequirement rr && aq.ResourceProgress != null && ri < aq.ResourceProgress.Length)
                    {
                        rr.CurrentAmount = aq.ResourceProgress[ri].CurrentAmount;
                        ri++;
                    }
                    else if (req is TriggeredQuestRequirement tr && aq.TriggeredProgress != null && ti < aq.TriggeredProgress.Length)
                    {
                        tr.CurrentAmount = aq.TriggeredProgress[ti].CurrentAmount;
                        ti++;
                    }
                }
            }
        }

        private void ApplyContractState(ContractsManager contracts, WorldState world)
        {
            QuestPatch.NetworkBypass = true;
            try
            {
                // Match active contract by name and sync progress
                if (world.ActiveContract != null)
                {
                    if (contracts.ActiveContract != null && contracts.ActiveContract.Name == world.ActiveContract.Name)
                    {
                        ApplyContractProgress(contracts.ActiveContract, world.ActiveContract);
                    }
                    else
                    {
                        // Find the matching contract in inactive list and activate it
                        var match = contracts.InactiveContracts?.FirstOrDefault(c => c.Name == world.ActiveContract.Name);
                        if (match != null)
                        {
                            contracts.SetContractActive(match);
                            ApplyContractProgress(contracts.ActiveContract, world.ActiveContract);
                        }
                    }
                }
                else if (contracts.ActiveContract != null)
                {
                    // Host has no active contract but we do — deactivate
                    contracts.SetContractInactive(contracts.ActiveContract);
                }

                // Sync inactive contract progress
                if (world.InactiveContracts != null)
                {
                    foreach (var wc in world.InactiveContracts)
                    {
                        var local = contracts.InactiveContracts?.FirstOrDefault(c => c.Name == wc.Name);
                        if (local != null)
                            ApplyContractProgress(local, wc);
                    }
                }
            }
            finally { QuestPatch.NetworkBypass = false; }
        }

        private void ApplyContractProgress(ContractInstance contract, ContractData data)
        {
            if (data.Progress == null) return;
            int ri = 0;
            foreach (var req in contract.QuestRequirements)
            {
                if (req is ResourceQuestRequirement rr && ri < data.Progress.Length)
                {
                    rr.CurrentAmount = data.Progress[ri].CurrentAmount;
                    ri++;
                }
            }
        }

        private void ApplyConveyorStates(List<ConveyorState> conveyors)
        {
            // Match belts by position + rotation (InstanceIDs differ across processes)
            foreach (var cs in conveyors)
            {
                ConveyorBelt match = null;
                float closest = 0.15f;
                foreach (var belt in ConveyorBelt.AllConveyorBelts)
                {
                    if (belt == null) continue;
                    float dist = Vector3.Distance(belt.transform.position, cs.Position.ToUnity());
                    if (dist < closest)
                    {
                        // Also verify rotation is close (within 15 degrees)
                        if (cs.Rotation.W != 0f && Quaternion.Angle(belt.transform.rotation, cs.Rotation.ToUnity()) > 15f)
                            continue;
                        closest = dist;
                        match = belt;
                    }
                }
                if (match != null)
                {
                    ConveyorPatch.NetworkBypass = true;
                    try
                    {
                        if (match.Speed != cs.Speed)
                            match.ChangeSpeed(cs.Speed);
                        match.Disabled = cs.Disabled;
                    }
                    finally { ConveyorPatch.NetworkBypass = false; }
                }
            }
        }

        /// <summary>Match snapshot buildings against local buildings, queue missing for batched spawn, remove extras.</summary>
        private void ReconcileBuildingsFromSnapshot(List<BuildingState> hostBuildings)
        {
            var localBuildings = new List<BuildingObject>(
                UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None));

            _log.LogInfo($"[Session] Building reconcile start: local={localBuildings.Count} host={hostBuildings?.Count ?? 0}");

            // Build a spatial lookup: key = "Type|X|Y|Z" (rounded to 0.1) → list of local buildings
            var localLookup = new Dictionary<string, List<BuildingObject>>();
            foreach (var lb in localBuildings)
            {
                if (lb == null || lb.IsGhost) continue;
                var key = BuildingSpatialKey(lb.SavableObjectID.ToString(), lb.transform.position);
                if (!localLookup.TryGetValue(key, out var list))
                {
                    list = new List<BuildingObject>(1);
                    localLookup[key] = list;
                }
                list.Add(lb);
            }

            var matched = new HashSet<int>();
            int matchedWithDataCount = 0;
            var toSpawn = new List<BuildingState>();

            foreach (var hb in hostBuildings)
            {
                var key = BuildingSpatialKey(hb.SavableObjectId, hb.Position.ToUnity());
                BuildingObject localMatch = null;

                if (localLookup.TryGetValue(key, out var candidates))
                {
                    foreach (var lb in candidates)
                    {
                        if (matched.Contains(lb.GetInstanceID())) continue;
                        if (Vector3.Distance(lb.transform.position, hb.Position.ToUnity()) < 0.15f)
                        {
                            localMatch = lb;
                            matched.Add(lb.GetInstanceID());
                            break;
                        }
                    }
                }

                if (localMatch != null)
                {
                    if (!string.IsNullOrEmpty(hb.CustomSaveData) && localMatch is ICustomSaveDataProvider provider)
                    {
                        provider.LoadFromSave(hb.CustomSaveData);
                        matchedWithDataCount++;
                    }
                }
                else
                {
                    toSpawn.Add(hb);
                }
            }

            // Collect buildings to remove (not present in host snapshot)
            var toRemove = new List<BuildingObject>();
            foreach (var lb in localBuildings)
            {
                if (lb == null || lb.IsGhost) continue;
                if (!matched.Contains(lb.GetInstanceID()))
                    toRemove.Add(lb);
            }

            // Remove extras immediately (cheap: just Destroy calls)
            BuildingPatch.NetworkBypass = true;
            try
            {
                foreach (var lb in toRemove)
                    UnityEngine.Object.Destroy(lb.gameObject);
            }
            finally { BuildingPatch.NetworkBypass = false; }

            // Queue spawns for batched processing across multiple ticks
            _pendingBuildingSpawns = new Queue<BuildingState>(toSpawn);

            _log.LogInfo($"[Session] Building reconcile: matched={matched.Count} dataSync={matchedWithDataCount} " +
                         $"queued={toSpawn.Count} removed={toRemove.Count} (batching {MaxBuildingSpawnsPerTick}/tick)");
        }

        /// <summary>Generate a spatial bucket key for building matching. Rounds to 0.1 units.</summary>
        private static string BuildingSpatialKey(string savableId, Vector3 pos)
        {
            // Round to nearest 0.1 for bucket matching
            int x = Mathf.RoundToInt(pos.x * 10);
            int y = Mathf.RoundToInt(pos.y * 10);
            int z = Mathf.RoundToInt(pos.z * 10);
            return $"{savableId}|{x}|{y}|{z}";
        }

        /// <summary>Incrementally reconcile ores: keep matching, remove extras, spawn missing.</summary>
        private void ReconcileOresFromSnapshot(List<OrePieceState> hostOres)
        {
            int removed = 0, spawned = 0, kept = 0;

            // First, delete ALL local ores not tracked by network ID (e.g. ores
            // auto-spawned by the game when the scene loaded, before any snapshot).
            var trackedInstances = new HashSet<OrePiece>();
            foreach (var kv in _clientOreByNetId)
            {
                if (kv.Value != null) trackedInstances.Add(kv.Value);
            }
            foreach (var ore in new List<OrePiece>(OrePiece.AllOrePieces))
            {
                if (ore != null && !trackedInstances.Contains(ore))
                {
                    ore.Delete();
                    removed++;
                }
            }

            if (hostOres == null || hostOres.Count == 0)
            {
                // Host has no ores — also clear tracked
                foreach (var kv in _clientOreByNetId)
                    if (kv.Value != null) kv.Value.Delete();
                _clientOreByNetId.Clear();
                _log.LogInfo($"[Session] Ore reconcile: removed all {removed} untracked + {_clientOreByNetId.Count} tracked (host has 0)");
                return;
            }

            // Build set of host network IDs for quick lookup
            var hostNetIds = new HashSet<int>();
            foreach (var ho in hostOres)
                hostNetIds.Add(ho.NetworkId);

            // Remove client ores that the host no longer has
            var toRemove = new List<int>();
            foreach (var kv in _clientOreByNetId)
            {
                if (!hostNetIds.Contains(kv.Key))
                {
                    if (kv.Value != null) kv.Value.Delete();
                    toRemove.Add(kv.Key);
                    removed++;
                }
                else
                {
                    kept++;
                }
            }
            foreach (var id in toRemove)
                _clientOreByNetId.Remove(id);

            // Spawn ores the host has that we don't
            foreach (var ho in hostOres)
            {
                if (!_clientOreByNetId.ContainsKey(ho.NetworkId))
                {
                    OnRemoteOreSpawned(ho);
                    spawned++;
                }
            }

            _log.LogInfo($"[Session] Ore reconcile: kept={kept} removed={removed} spawned={spawned} (host total={hostOres.Count})");
        }

        /// <summary>Apply building custom save data updates from delta.</summary>
        private void ApplyBuildingUpdates(List<BuildingState> buildings)
        {
            if (buildings.Count == 0) return;

            // Build spatial lookup once instead of calling FindObjectsByType per building
            var localLookup = new Dictionary<string, List<BuildingObject>>();
            foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
            {
                if (bo == null || bo.IsGhost) continue;
                var key = BuildingSpatialKey(bo.SavableObjectID.ToString(), bo.transform.position);
                if (!localLookup.TryGetValue(key, out var list))
                {
                    list = new List<BuildingObject>(1);
                    localLookup[key] = list;
                }
                list.Add(bo);
            }

            foreach (var state in buildings)
            {
                if (string.IsNullOrEmpty(state.CustomSaveData)) continue;
                var key = BuildingSpatialKey(state.SavableObjectId, state.Position.ToUnity());
                if (localLookup.TryGetValue(key, out var candidates))
                {
                    foreach (var bo in candidates)
                    {
                        if (Vector3.Distance(bo.transform.position, state.Position.ToUnity()) < 0.15f)
                        {
                            if (bo is ICustomSaveDataProvider provider)
                                provider.LoadFromSave(state.CustomSaveData);
                            break;
                        }
                    }
                }
            }
        }

        private void CheckHash(WorldHash hash)
        {
            // Skip hash check while buildings are still being batched in — snapshot would be incomplete
            if (_pendingBuildingSpawns != null && _pendingBuildingSpawns.Count > 0)
            {
                _log.LogInfo($"[Session] Hash check at tick {hash.Tick} — skipped ({_pendingBuildingSpawns.Count} buildings still spawning)");
                return;
            }

            _log.LogInfo($"[Session] Hash check at tick {hash.Tick}");

            try
            {
                // Build local snapshot and compare hash
                var localSnapshot = BuildClientSnapshot();
                var localHash = WorldHasher.ComputeHash(localSnapshot);

                if (localHash != hash.Hash)
                {
                    _consecutiveDesyncCount++;
                    // Log component-level details so we can pinpoint what diverged
                    var details = WorldHasher.DiagnoseComponents(localSnapshot);
                    _log.LogWarning($"[Session] Hash mismatch #{_consecutiveDesyncCount} at tick {hash.Tick}. Local={localHash} Host={hash.Hash}  Components: {details}");

                    if (_consecutiveDesyncCount >= ConsecutiveDesyncThreshold)
                    {
                        _log.LogWarning($"[Session] DESYNC confirmed after {_consecutiveDesyncCount} consecutive mismatches. Requesting resync.");
                        LogEvent("Desync detected — requesting resync from host.");
                        _net?.SendToHost(MessageType.ResyncRequest, hash.Tick);
                        _consecutiveDesyncCount = 0;
                    }
                }
                else
                {
                    if (_consecutiveDesyncCount > 0)
                        _log.LogInfo($"[Session] Hash matched after {_consecutiveDesyncCount} prior mismatch(es) — transient desync resolved.");
                    _consecutiveDesyncCount = 0;
                }
            }
            catch (System.Exception ex)
            {
                _log.LogError($"[Session] Hash check failed at tick {hash.Tick}: {ex}");
            }
        }

        /// <summary>Build a client-side snapshot for hash verification.</summary>
        private WorldSnapshot BuildClientSnapshot()
        {
            var eco = Singleton<EconomyManager>.Instance;
            var research = Singleton<ResearchManager>.Instance;
            var quests = Singleton<QuestManager>.Instance;
            var contracts = Singleton<ContractsManager>.Instance;

            var worldState = BuildWorldState(eco, research, quests, contracts);

            var buildings = new List<BuildingState>();
            foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
            {
                if (bo == null || bo.IsGhost) continue;
                buildings.Add(new BuildingState
                {
                    LocalInstanceId = 0, // Not used for cross-process hash
                    SavableObjectId = bo.SavableObjectID.ToString(),
                    Position = new NetVector3(bo.transform.position),
                    Rotation = new NetQuaternion(bo.transform.rotation)
                });
            }

            var orePieces = new List<OrePieceState>();
            foreach (var ore in OrePiece.AllOrePieces)
            {
                if (ore == null) continue;
                orePieces.Add(new OrePieceState
                {
                    NetworkId = 0, // Not used for hash
                    ResourceType = (NetResourceType)(int)ore.ResourceType,
                    PieceType = (NetPieceType)(int)ore.PieceType,
                    IsPolished = ore.IsPolished,
                    Position = new NetVector3(ore.transform.position)
                });
            }

            return new WorldSnapshot
            {
                World = worldState,
                Buildings = buildings,
                OrePieces = orePieces,
                Conveyors = BuildClientConveyorStates(),
                Tick = _tick
            };
        }

        /// <summary>Build conveyor states for client hash verification using FindObjectsByType for reliability.</summary>
        private List<ConveyorState> BuildClientConveyorStates()
        {
            var result = new List<ConveyorState>();
            foreach (var belt in UnityEngine.Object.FindObjectsByType<ConveyorBelt>(FindObjectsSortMode.None))
            {
                if (belt == null) continue;
                result.Add(new ConveyorState
                {
                    LocalInstanceId = belt.GetInstanceID(),
                    Speed = belt.Speed,
                    Disabled = belt.Disabled,
                    Position = new NetVector3(belt.transform.position),
                    Rotation = new NetQuaternion(belt.transform.rotation)
                });
            }
            return result;
        }

        private void OnRemoteBuildingSpawned(BuildingState building)
        {
            if (!System.Enum.TryParse<SavableObjectID>(building.SavableObjectId, out var savableId))
            {
                _log.LogWarning($"[Session] Unknown SavableObjectID: {building.SavableObjectId}");
                return;
            }

            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm == null) return;

            var prefab = slm.GetPrefab(savableId);
            if (prefab == null) return;

            BuildingPatch.NetworkBypass = true;
            try
            {
                var go = UnityEngine.Object.Instantiate(prefab, building.Position.ToUnity(), building.Rotation.ToUnity());
                // Apply custom save data if present
                if (!string.IsNullOrEmpty(building.CustomSaveData))
                {
                    var customProvider = go.GetComponentInChildren<ICustomSaveDataProvider>();
                    if (customProvider != null)
                        customProvider.LoadFromSave(building.CustomSaveData);
                }
            }
            finally { BuildingPatch.NetworkBypass = false; }
        }

        /// <summary>Process queued building spawns from ReconcileBuildingsFromSnapshot (max N per tick).</summary>
        private void ProcessPendingBuildingSpawns()
        {
            if (_pendingBuildingSpawns == null || _pendingBuildingSpawns.Count == 0) return;

            int count = 0;
            BuildingPatch.NetworkBypass = true;
            try
            {
                while (_pendingBuildingSpawns.Count > 0 && count < MaxBuildingSpawnsPerTick)
                {
                    var building = _pendingBuildingSpawns.Dequeue();
                    OnRemoteBuildingSpawned(building);
                    count++;
                }
            }
            finally { BuildingPatch.NetworkBypass = false; }

            if (_pendingBuildingSpawns.Count > 0)
                _log.LogInfo($"[Session] Batched building spawn: {count} this tick, {_pendingBuildingSpawns.Count} remaining");
            else
                _log.LogInfo($"[Session] Batched building spawn complete: {count} final batch");
        }

        private void OnRemoteBuildingRemoved(BuildingRemovalInfo info)
        {
            _log.LogInfo($"[Session] Remote building removed: {info.SavableObjectId} at {info.Position}");
            foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
            {
                if (bo.SavableObjectID.ToString() == info.SavableObjectId &&
                    Vector3.Distance(bo.transform.position, info.Position.ToUnity()) < 0.15f)
                {
                    UnityEngine.Object.Destroy(bo.gameObject);
                    break;
                }
            }
        }

        private void OnRemoteOreSpawned(OrePieceState ore)
        {

            var poolMgr = Singleton<OrePiecePoolManager>.Instance;
            if (poolMgr == null) return;

            var resourceType = (ResourceType)(int)ore.ResourceType;
            var pieceType = (PieceType)(int)ore.PieceType;
            var pos = ore.Position.ToUnity();
            var rot = ore.Rotation.ToUnity();

            var spawned = poolMgr.SpawnPooledOre(resourceType, pieceType, ore.IsPolished, pos, rot, null);
            if (spawned != null)
            {
                var rb = spawned.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = ore.Velocity.ToUnity();

                // Track by network ID for later removal
                _clientOreByNetId[ore.NetworkId] = spawned;
            }
        }

        private void OnRemoteOreRemoved(int networkId)
        {
            if (_clientOreByNetId.TryGetValue(networkId, out var ore))
            {
                if (ore != null) ore.Delete();
                _clientOreByNetId.Remove(networkId);
            }
            _oreTargetPositions.Remove(networkId);
            _oreTargetRotations.Remove(networkId);
            _remotelyHeldOres.Remove(networkId);
        }

        /// <summary>Apply ore position updates from host — smoothly moves ore pieces that have changed position.</summary>
        private void ApplyOrePositionUpdates(List<OrePositionUpdate> updates)
        {
            if (updates == null) return;
            foreach (var upd in updates)
            {
                // Skip ores the client is actively holding — don't let host override local grab
                if (_clientLocallyHeldOres.Contains(upd.NetworkId)) continue;

                if (_clientOreByNetId.TryGetValue(upd.NetworkId, out var ore) && ore != null)
                {
                    _oreTargetPositions[upd.NetworkId] = upd.Position.ToUnity();
                    _oreTargetRotations[upd.NetworkId] = upd.Rotation.ToUnity();
                    // Make kinematic while being remotely moved so physics doesn't fight interpolation
                    var rb = ore.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    _remotelyHeldOres[upd.NetworkId] = UnityEngine.Time.unscaledTime;
                }
            }
        }

        /// <summary>
        /// Reconcile the local PlayerInventory with the authoritative tool list from the host.
        /// Adds any missing tools and removes extras so both players share the same set.
        /// </summary>
        private void ReconcileToolInventory(string[] hostTools)
        {
            if (hostTools == null) return;

            if (_cachedPlayerController == null)
                _cachedPlayerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (_cachedPlayerController == null) return;

            var inv = _cachedPlayerController.GetComponent<PlayerInventory>();
            if (inv == null) return;

            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm == null) return;

            // Build a count map of host tools
            var hostToolCounts = new Dictionary<string, int>();
            foreach (var t in hostTools)
            {
                if (string.IsNullOrEmpty(t)) continue;
                // ToolBuilder cannot be instantiated on clients (GetIcon NRE)
                if (t == "ToolBuilder") continue;
                if (!hostToolCounts.ContainsKey(t))
                    hostToolCounts[t] = 0;
                hostToolCounts[t]++;
            }

            // Build a count map of local tools
            var localToolCounts = new Dictionary<string, int>();
            foreach (var tool in inv.Items)
            {
                if (tool == null) continue;
                var name = tool.SavableObjectID.ToString();
                if (!localToolCounts.ContainsKey(name))
                    localToolCounts[name] = 0;
                localToolCounts[name]++;
            }

            // Add missing tools
            foreach (var kv in hostToolCounts)
            {
                localToolCounts.TryGetValue(kv.Key, out int localCount);
                int toAdd = kv.Value - localCount;
                if (toAdd <= 0) continue;

                if (!System.Enum.TryParse<SavableObjectID>(kv.Key, out var savableId))
                    continue;
                var prefab = slm.GetPrefab(savableId);
                if (prefab == null) continue;

                for (int i = 0; i < toAdd; i++)
                {
                    var go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                    var tool = go.GetComponent<BaseHeldTool>();
                    if (tool != null)
                    {
                        try
                        {
                            inv.TryAddToInventory(tool);
                            _log.LogInfo($"[Session] Added tool '{kv.Key}' to inventory (shared sync)");
                        }
                        catch (System.Exception ex)
                        {
                            _log.LogWarning($"[Session] TryAddToInventory for '{kv.Key}' threw (icon init): {ex.GetType().Name}");
                            // Remove broken tool to prevent PlayerInventory.Update()->UpdateUI()->GetIcon() crash every frame
                            try { inv.Items.Remove(tool); } catch { }
                            UnityEngine.Object.Destroy(go);
                        }
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(go);
                    }
                }
            }

            // Remove excess tools (host dropped or removed a tool we still have)
            Patches.ToolPatch.NetworkBypass = true;
            try
            {
                foreach (var kv in localToolCounts)
                {
                    hostToolCounts.TryGetValue(kv.Key, out int hostCount);
                    int toRemove = kv.Value - hostCount;
                    if (toRemove <= 0) continue;
                    if (kv.Key == "ToolBuilder") continue;

                    if (!System.Enum.TryParse<SavableObjectID>(kv.Key, out var sid)) continue;

                    int removed = 0;
                    for (int i = inv.Items.Count - 1; i >= 0 && removed < toRemove; i--)
                    {
                        var tool = inv.Items[i];
                        if (tool != null && tool.SavableObjectID == sid)
                        {
                            var bht = tool.GetComponent<BaseHeldTool>();
                            if (bht != null)
                                inv.RemoveFromInventory(bht);
                            UnityEngine.Object.Destroy(tool.gameObject);
                            removed++;
                            _log.LogInfo($"[Session] Removed excess tool '{kv.Key}' from inventory (shared sync)");
                        }
                    }
                }
            }
            finally { Patches.ToolPatch.NetworkBypass = false; }
        }

        /// <summary>Host: store the client's inventory report for save persistence.</summary>
        private void HandleClientInventoryReport(uint clientId, ClientInventoryReportMessage msg)
        {
            if (msg == null || msg.SteamId == 0 || msg.Tools == null) return;
            _savedPlayerInventories[msg.SteamId] = msg.Tools;
        }

        /// <summary>Client: send our current tool list to the host for save persistence.</summary>
        private void SendClientInventoryReport()
        {
            if (_cachedPlayerController == null)
                _cachedPlayerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (_cachedPlayerController == null) return;

            var inv = _cachedPlayerController.GetComponent<PlayerInventory>();
            if (inv == null) return;

            var tools = new List<string>();
            foreach (var tool in inv.Items)
            {
                if (tool == null) continue;
                var name = tool.SavableObjectID.ToString();
                if (name == "ToolBuilder") continue;
                tools.Add(name);
            }

            _net.SendToHost(MessageType.ClientInventoryReport, new ClientInventoryReportMessage
            {
                SteamId = SteamClient.SteamId,
                Tools = tools.ToArray()
            });
        }

        /// <summary>Host: save per-player inventories to a companion JSON file alongside the game save.</summary>
        public void SaveMultiplayerCompanionData()
        {
            if (string.IsNullOrEmpty(_hostSaveFilePath)) return;

            // Also capture host's own inventory
            try
            {
                var hostInv = BuildInventorySyncMessage();
                if (hostInv?.Tools != null)
                    _savedPlayerInventories[SteamClient.SteamId] = hostInv.Tools;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[Session] Failed to capture host inventory: {ex.Message}");
            }

            if (_savedPlayerInventories.Count == 0) return;

            var companionPath = GetCompanionFilePath(_hostSaveFilePath);
            try
            {
                // Simple JSON: { "steamId": ["tool1","tool2"], ... }
                var sb = new System.Text.StringBuilder();
                sb.Append('{');
                bool first = true;
                foreach (var kv in _savedPlayerInventories)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append('"').Append(kv.Key).Append("\":[");
                    for (int i = 0; i < kv.Value.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append('"').Append(EscapeJsonString(kv.Value[i])).Append('"');
                    }
                    sb.Append(']');
                }
                sb.Append('}');
                File.WriteAllText(companionPath, sb.ToString());
                _log.LogInfo($"[Session] Saved multiplayer companion data ({_savedPlayerInventories.Count} players) to {companionPath}");
            }
            catch (Exception ex)
            {
                _log.LogError($"[Session] Failed to save companion data: {ex}");
            }
        }

        /// <summary>Host: load per-player inventories from the companion JSON file.</summary>
        private void LoadMultiplayerCompanionData()
        {
            _savedPlayerInventories.Clear();
            if (string.IsNullOrEmpty(_hostSaveFilePath)) return;

            var companionPath = GetCompanionFilePath(_hostSaveFilePath);
            if (!File.Exists(companionPath)) return;

            try
            {
                var json = File.ReadAllText(companionPath);
                // Minimal JSON parser for our simple format: { "steamId": ["tool1","tool2"], ... }
                ParseCompanionJson(json);
                _log.LogInfo($"[Session] Loaded multiplayer companion data ({_savedPlayerInventories.Count} players) from {companionPath}");
            }
            catch (Exception ex)
            {
                _log.LogError($"[Session] Failed to load companion data: {ex}");
            }
        }

        private static string GetCompanionFilePath(string saveFilePath)
        {
            var dir = Path.GetDirectoryName(saveFilePath);
            var name = Path.GetFileNameWithoutExtension(saveFilePath);
            return Path.Combine(dir, name + "_multiplayer.json");
        }

        private static string EscapeJsonString(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>Parse our simple companion JSON format into _savedPlayerInventories.</summary>
        private void ParseCompanionJson(string json)
        {
            // Format: {"76561198..":["Tool1","Tool2"],"76561199..":["Tool3"]}
            json = json.Trim();
            if (json.Length < 2 || json[0] != '{') return;
            json = json.Substring(1, json.Length - 2); // strip outer braces

            int pos = 0;
            while (pos < json.Length)
            {
                // Find key (steam ID)
                int keyStart = json.IndexOf('"', pos);
                if (keyStart < 0) break;
                int keyEnd = json.IndexOf('"', keyStart + 1);
                if (keyEnd < 0) break;
                var keyStr = json.Substring(keyStart + 1, keyEnd - keyStart - 1);

                // Find array start
                int arrStart = json.IndexOf('[', keyEnd);
                if (arrStart < 0) break;
                int arrEnd = json.IndexOf(']', arrStart);
                if (arrEnd < 0) break;

                var arrContent = json.Substring(arrStart + 1, arrEnd - arrStart - 1);
                var tools = new List<string>();
                if (arrContent.Trim().Length > 0)
                {
                    int tPos = 0;
                    while (tPos < arrContent.Length)
                    {
                        int tStart = arrContent.IndexOf('"', tPos);
                        if (tStart < 0) break;
                        int tEnd = arrContent.IndexOf('"', tStart + 1);
                        if (tEnd < 0) break;
                        tools.Add(arrContent.Substring(tStart + 1, tEnd - tStart - 1));
                        tPos = tEnd + 1;
                    }
                }

                if (ulong.TryParse(keyStr, out var steamId))
                    _savedPlayerInventories[steamId] = tools.ToArray();

                pos = arrEnd + 1;
            }
        }

        // ── Snapshot / delta builders ────────────────

        private WorldSnapshot BuildSnapshot()
        {
            var eco = Singleton<EconomyManager>.Instance;
            var research = Singleton<ResearchManager>.Instance;
            var quests = Singleton<QuestManager>.Instance;
            var contracts = Singleton<ContractsManager>.Instance;

            var worldState = BuildWorldState(eco, research, quests, contracts);

            // Collect all buildings
            var buildings = new List<BuildingState>();
            foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
            {
                if (bo.IsGhost) continue;
                var state = new BuildingState
                {
                    LocalInstanceId = bo.GetInstanceID(),
                    SavableObjectId = bo.SavableObjectID.ToString(),
                    Position = new NetVector3(bo.transform.position),
                    Rotation = new NetQuaternion(bo.transform.rotation)
                };
                if (bo is ICustomSaveDataProvider provider)
                    state.CustomSaveData = provider.GetCustomSaveData();
                buildings.Add(state);
            }

            // Collect all ore pieces — assign/use network IDs
            var orePieces = new List<OrePieceState>();
            foreach (var ore in OrePiece.AllOrePieces)
            {
                if (ore == null) continue;
                int localId = ore.GetInstanceID();

                // Ensure every ore has a network ID (snapshot might be called before DetectOreChanges)
                if (!_hostOreInstanceToNetId.TryGetValue(localId, out int netId))
                {
                    netId = _nextOreNetId++;
                    _hostOreInstanceToNetId[localId] = netId;
                }

                var rb = ore.GetComponent<Rigidbody>();
                orePieces.Add(new OrePieceState
                {
                    NetworkId = netId,
                    ResourceType = (NetResourceType)(int)ore.ResourceType,
                    PieceType = (NetPieceType)(int)ore.PieceType,
                    IsPolished = ore.IsPolished,
                    Position = new NetVector3(ore.transform.position),
                    Rotation = new NetQuaternion(ore.transform.rotation),
                    Velocity = rb != null ? new NetVector3(rb.linearVelocity) : default,
                    SellValue = ore.GetSellValue()
                });
            }

            return new WorldSnapshot
            {
                World = worldState,
                Players = new List<PlayerState>(_players),
                Buildings = buildings,
                OrePieces = orePieces,
                Conveyors = BuildConveyorStates(),
                Tick = _tick,
                Crates = BuildCrateStates(),
                Detonators = BuildDetonatorStates()
            };
        }

        private WorldDelta BuildDelta(HashSet<int> dirtyMachines, HashSet<int> dirtyBelts,
            HashSet<int> dirtyDetonators,
            List<BuildingRemovalInfo> removedBuildings, bool moneyDirty, bool researchDirty, bool questDirty,
            bool contractDirty, bool shopPurchaseDirty, bool activeQuestProgressDirty)
        {
            bool anythingDirty = moneyDirty ||
                                 researchDirty ||
                                 questDirty ||
                                 contractDirty ||
                                 shopPurchaseDirty ||
                                 activeQuestProgressDirty ||
                                 dirtyMachines.Count > 0 ||
                                 removedBuildings.Count > 0 ||
                                 dirtyBelts.Count > 0 ||
                                 dirtyDetonators.Count > 0;

            // Always send player positions when there are multiple players
            if (!anythingDirty && _players.Count <= 1)
                return null;

            bool worldDirty = moneyDirty || researchDirty || questDirty ||
                              contractDirty || shopPurchaseDirty || activeQuestProgressDirty;

            var eco = Singleton<EconomyManager>.Instance;
            var research = Singleton<ResearchManager>.Instance;
            var quests = Singleton<QuestManager>.Instance;
            var contracts = Singleton<ContractsManager>.Instance;

            return new WorldDelta
            {
                Tick = _tick,
                ChangedBuildings = BuildDirtyBuildings(dirtyMachines),
                RemovedBuildings = removedBuildings,
                ChangedOrePieces = new List<OrePieceState>(),
                RemovedOrePieceIds = new List<int>(),
                PlayerUpdates = new List<PlayerState>(_players),
                World = worldDirty ? BuildWorldState(eco, research, quests, contracts) : null,
                ChangedConveyors = BuildDirtyConveyors(dirtyBelts),
                ChangedDetonators = BuildDirtyDetonators(dirtyDetonators)
            };
        }

        private WorldState BuildWorldState(EconomyManager eco, ResearchManager research, QuestManager quests, ContractsManager contracts)
        {
            var state = new WorldState
            {
                Money = eco != null ? eco.Money : 0,
                ResearchTickets = research != null ? research.ResearchTickets : 0,
                GameModeType = (int)(Singleton<GamemodeManager>.Instance?.GameModeType ?? 0),
                CompletedResearchIds = research?.CompletedResearchItems?
                    .Select(id => id.ToString()).ToArray() ?? Array.Empty<string>(),
                CompletedQuestIds = quests?.GetCompletedQuestIDs()?
                    .Select(id => id.ToString()).ToArray() ?? Array.Empty<string>()
            };

            // Active quest progress
            if (quests != null)
            {
                var activeQuests = new List<ActiveQuestData>();
                foreach (var quest in quests.ActiveQuests)
                {
                    var rProgress = new List<ResourceRequirementProgress>();
                    var tProgress = new List<TriggeredRequirementProgress>();
                    foreach (var req in quest.QuestRequirements)
                    {
                        if (req is ResourceQuestRequirement rr)
                            rProgress.Add(new ResourceRequirementProgress
                            {
                                ResourceType = rr.ResourceType.ToString(),
                                PieceType = rr.PieceType.ToString(),
                                RequirePolished = rr.RequirePolishedResource,
                                CurrentAmount = rr.CurrentAmount
                            });
                        else if (req is TriggeredQuestRequirement tr)
                            tProgress.Add(new TriggeredRequirementProgress
                            {
                                Type = tr.TriggeredQuestRequirementType.ToString(),
                                CurrentAmount = tr.CurrentAmount
                            });
                    }
                    activeQuests.Add(new ActiveQuestData
                    {
                        QuestId = quest.QuestID.ToString(),
                        ResourceProgress = rProgress.ToArray(),
                        TriggeredProgress = tProgress.ToArray()
                    });
                }
                state.ActiveQuests = activeQuests.ToArray();
            }

            // Shop purchases
            if (eco?.ShopPurchases != null)
            {
                state.ShopPurchases = eco.ShopPurchases.Purchases?
                    .Select(p => new ShopPurchaseData
                    {
                        SavableObjectId = p.SavableObjectID.ToString(),
                        Amount = p.AmountPurchased
                    }).ToArray() ?? Array.Empty<ShopPurchaseData>();
            }

            // Contracts
            if (contracts != null)
            {
                if (contracts.ActiveContract != null)
                    state.ActiveContract = BuildContractData(contracts.ActiveContract);
                if (contracts.InactiveContracts != null)
                    state.InactiveContracts = contracts.InactiveContracts
                        .Select(c => BuildContractData(c)).ToArray();
            }

            return state;
        }

        private ContractData BuildContractData(ContractInstance contract)
        {
            var progress = new List<ResourceRequirementProgress>();
            foreach (var req in contract.QuestRequirements)
            {
                if (req is ResourceQuestRequirement rr)
                    progress.Add(new ResourceRequirementProgress
                    {
                        ResourceType = rr.ResourceType.ToString(),
                        PieceType = rr.PieceType.ToString(),
                        RequirePolished = rr.RequirePolishedResource,
                        CurrentAmount = rr.CurrentAmount
                    });
            }
            return new ContractData
            {
                Name = contract.Name,
                Progress = progress.ToArray(),
                RewardMoney = contract.RewardMoney
            };
        }

        private List<BuildingState> BuildDirtyBuildings(HashSet<int> dirtyMachines)
        {
            var result = new List<BuildingState>();
            if (dirtyMachines.Count == 0) return result;

            // Use per-tick cached building lookup instead of FindObjectsByType every call
            if (_cachedBuildingLookup == null)
            {
                var allBuildings = UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None);
                _cachedBuildingLookup = new Dictionary<int, BuildingObject>(allBuildings.Length);
                foreach (var bo in allBuildings)
                    if (bo != null && !bo.IsGhost) _cachedBuildingLookup[bo.GetInstanceID()] = bo;
            }

            foreach (var instanceId in dirtyMachines)
            {
                if (!_cachedBuildingLookup.TryGetValue(instanceId, out var bo)) continue;

                var state = new BuildingState
                {
                    LocalInstanceId = bo.GetInstanceID(),
                    SavableObjectId = bo.SavableObjectID.ToString(),
                    Position = new NetVector3(bo.transform.position),
                    Rotation = new NetQuaternion(bo.transform.rotation)
                };
                if (bo is ICustomSaveDataProvider provider)
                    state.CustomSaveData = provider.GetCustomSaveData();
                result.Add(state);
            }
            return result;
        }

        private List<ConveyorState> BuildConveyorStates()
        {
            var result = new List<ConveyorState>();
            foreach (var belt in ConveyorBelt.AllConveyorBelts)
            {
                if (belt == null) continue;
                result.Add(new ConveyorState
                {
                    LocalInstanceId = belt.GetInstanceID(),
                    Speed = belt.Speed,
                    Disabled = belt.Disabled,
                    Position = new NetVector3(belt.transform.position),
                    Rotation = new NetQuaternion(belt.transform.rotation)
                });
            }
            return result;
        }

        private List<CrateState> BuildCrateStates()
        {
            var result = new List<CrateState>();
            foreach (var crate in UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None))
            {
                if (crate == null) continue;
                int instanceId = crate.GetInstanceID();
                if (!_hostCrateInstanceToNetId.TryGetValue(instanceId, out int netId))
                {
                    netId = _nextCrateNetId++;
                    _hostCrateInstanceToNetId[instanceId] = netId;
                }
                result.Add(new CrateState
                {
                    NetworkId = netId,
                    Position = new NetVector3(crate.transform.position),
                    Rotation = new NetQuaternion(crate.transform.rotation)
                });
            }
            return result;
        }

        private List<ConveyorState> BuildDirtyConveyors(HashSet<int> dirtyBelts)
        {
            var result = new List<ConveyorState>();
            if (dirtyBelts.Count == 0) return result;

            foreach (var belt in ConveyorBelt.AllConveyorBelts)
            {
                if (belt == null) continue;
                if (!dirtyBelts.Contains(belt.GetInstanceID())) continue;
                result.Add(new ConveyorState
                {
                    LocalInstanceId = belt.GetInstanceID(),
                    Speed = belt.Speed,
                    Disabled = belt.Disabled,
                    Position = new NetVector3(belt.transform.position),
                    Rotation = new NetQuaternion(belt.transform.rotation)
                });
            }
            return result;
        }

        // ── Detonator state helpers ──

        private List<DetonatorState> BuildDetonatorStates()
        {
            var result = new List<DetonatorState>();
            foreach (var det in UnityEngine.Object.FindObjectsByType<DetonatorExplosion>(FindObjectsSortMode.None))
            {
                if (det == null) continue;
                result.Add(new DetonatorState
                {
                    DetonatorId = det.DetonatorID,
                    State = det.HasExploded() ? 2 : det.HasPurchased() ? 1 : 0,
                    SaveData = det.GetCustomSaveData()
                });
            }
            return result;
        }

        private List<DetonatorState> BuildDirtyDetonators(HashSet<int> dirtyIds)
        {
            var result = new List<DetonatorState>();
            if (dirtyIds.Count == 0) return result;

            foreach (var det in UnityEngine.Object.FindObjectsByType<DetonatorExplosion>(FindObjectsSortMode.None))
            {
                if (det == null) continue;
                if (!dirtyIds.Contains(det.DetonatorID)) continue;
                result.Add(new DetonatorState
                {
                    DetonatorId = det.DetonatorID,
                    State = det.HasExploded() ? 2 : det.HasPurchased() ? 1 : 0,
                    SaveData = det.GetCustomSaveData()
                });
            }
            return result;
        }

        private void ApplyDetonatorStates(List<DetonatorState> detonators)
        {
            if (detonators == null || detonators.Count == 0) return;

            var lookup = new Dictionary<int, DetonatorState>();
            foreach (var ds in detonators) lookup[ds.DetonatorId] = ds;

            foreach (var det in UnityEngine.Object.FindObjectsByType<DetonatorExplosion>(FindObjectsSortMode.None))
            {
                if (det == null) continue;
                if (!lookup.TryGetValue(det.DetonatorID, out var state)) continue;

                _log.LogInfo($"[Session] Applying detonator {det.DetonatorID} state={state.State}");

                // Use the game's own save/load mechanism to restore the full state,
                // which handles disabling walls, showing/hiding TNT, and setting the enum.
                if (!string.IsNullOrEmpty(state.SaveData))
                {
                    BuildingInteractionPatch.NetworkBypass = true;
                    try { det.LoadFromSave(state.SaveData); }
                    finally { BuildingInteractionPatch.NetworkBypass = false; }
                }
            }
        }

        // ── Client RPC helpers (called from patches) ─

        public void SendPlaceBuildingRPC(string savableObjectId, NetVector3 position, NetQuaternion rotation)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.PlaceBuilding, new PlaceBuildingMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                SavableObjectId = savableObjectId,
                Position = position,
                Rotation = rotation
            });
        }

        public void SendRemoveBuildingRPC(NetVector3 position, string savableObjectId)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.RemoveBuilding, new RemoveBuildingMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                Position = position,
                SavableObjectId = savableObjectId
            });
        }

        public void SendInteractBuildingRPC(int buildingInstanceId, string action)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.InteractBuilding, new InteractBuildingMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                BuildingInstanceId = buildingInstanceId,
                Action = action
            });
        }

        public void SendInteractBuildingByPosRPC(NetVector3 position, string action, string data = null)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.InteractBuilding, new InteractBuildingMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                Position = position,
                Action = action,
                Data = data
            });
        }

        /// <summary>Client: notify the host that we purchased tools from the shop so the host can add them too.</summary>
        public void NotifyHostOfPurchase(string[] purchasedTools, float totalCost)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.ShopPurchaseNotify, new ShopPurchaseNotifyMessage
            {
                PurchasedTools = purchasedTools,
                TotalCost = totalCost
            });
            _log.LogInfo($"[Session] Sent shop purchase notification: {purchasedTools.Length} tools, cost={totalCost}");
        }

        /// <summary>Client: tell host a tool was dropped, including position/physics data.</summary>
        public void SendToolDropRPC(string toolName, Vector3 pos, Quaternion rot, Vector3 vel)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.ToolDrop, new ToolDropMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                ToolName = toolName,
                Position = new NetVector3(pos),
                Rotation = new NetQuaternion(rot),
                Velocity = new NetVector3(vel)
            });
            _log.LogInfo($"[Session] Sent tool drop RPC: {toolName} at ({pos.x:F1},{pos.y:F1},{pos.z:F1})");
        }

        /// <summary>Client: tell host to add a tool to the shared inventory (tool was picked up).</summary>
        public void SendToolPickupRPC(string toolName)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.ToolPickup, new ToolPickupMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                ToolName = toolName
            });
            _log.LogInfo($"[Session] Sent tool pickup RPC: {toolName}");
        }

        private bool _pendingInventoryBroadcast;
        /// <summary>Schedule an inventory broadcast on the next tick (used after host tool changes).</summary>
        public void ScheduleInventoryBroadcast() { _pendingInventoryBroadcast = true; }

        /// <summary>Host: handle a client purchase notification — deduct shared money only (inventories are independent).</summary>
        private void HandleShopPurchaseNotify(uint clientId, ShopPurchaseNotifyMessage msg)
        {
            _log.LogInfo($"[Session] Client {clientId} spent {msg.TotalCost} at the shop");

            // Deduct money on the host (money is shared, inventory is not)
            var eco = Singleton<EconomyManager>.Instance;
            if (eco != null && msg.TotalCost > 0)
            {
                EconomyPatch.NetworkBypass = true;
                try { eco.AddMoney(-msg.TotalCost); }
                finally { EconomyPatch.NetworkBypass = false; }
                DirtyTracker.MoneyDirty = true;
            }
        }

        /// <summary>Host: a client dropped a tool — drop from host inventory at client's position, broadcast to all.</summary>
        private void HandleToolDrop(uint clientId, ToolDropMessage msg)
        {
            _log.LogInfo($"[Session] Client {clientId} dropped tool '{msg.ToolName}'");

            // Remove from host inventory and destroy (we'll spawn a fresh world copy)
            RemoveToolFromHostInventory(msg.ToolName);

            // Spawn a world-visible copy at the client's drop position
            SpawnWorldTool(msg.ToolName, msg.Position.ToUnity(), msg.Rotation.ToUnity(), msg.Velocity.ToUnity());

            // Broadcast to ALL clients so they see the world object
            // (the dropping client ignores their own PlayerId)
            _net.SendToAll(MessageType.ToolDrop, msg);

            BroadcastInventorySync();
        }

        /// <summary>Host: a client picked up a tool from the ground — add to host inventory, clean up world copies.</summary>
        private void HandleToolPickup(uint clientId, ToolPickupMessage msg)
        {
            _log.LogInfo($"[Session] Client {clientId} picked up tool '{msg.ToolName}'");

            // Destroy the world copy on the host side
            DestroyDroppedWorldTool(msg.ToolName);

            AddToolToHostInventory(msg.ToolName);

            // Broadcast to all clients so they destroy their world copies
            _net.SendToAll(MessageType.ToolPickup, msg);

            BroadcastInventorySync();
        }

        /// <summary>Host → all clients: a tool was dropped — spawn a world object.</summary>
        public void BroadcastToolWorldDrop(string toolName, Vector3 pos, Quaternion rot, Vector3 vel)
        {
            if (!MultiplayerState.IsHost || _net == null) return;
            _net.SendToAll(MessageType.ToolDrop, new ToolDropMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                ToolName = toolName,
                Position = new NetVector3(pos),
                Rotation = new NetQuaternion(rot),
                Velocity = new NetVector3(vel)
            });
        }

        /// <summary>Host → all clients: a tool was picked up — destroy world copies.</summary>
        public void BroadcastToolWorldPickup(string toolName)
        {
            if (!MultiplayerState.IsHost || _net == null) return;
            _net.SendToAll(MessageType.ToolPickup, new ToolPickupMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                ToolName = toolName
            });
        }

        /// <summary>Client: apply a remote tool drop — spawn a world-visible copy.</summary>
        private void ApplyRemoteToolDrop(ToolDropMessage msg)
        {
            // Don't double-spawn for our own drops (already visible locally)
            if (msg.PlayerId == MultiplayerState.LocalPlayerId) return;

            _log.LogInfo($"[Session] Remote tool drop: '{msg.ToolName}' at {msg.Position}");
            SpawnWorldTool(msg.ToolName, msg.Position.ToUnity(), msg.Rotation.ToUnity(), msg.Velocity.ToUnity());
        }

        /// <summary>Client: apply a remote tool pickup — destroy the matching world copy.</summary>
        private void ApplyRemoteToolPickup(ToolPickupMessage msg)
        {
            if (msg.PlayerId == MultiplayerState.LocalPlayerId) return;

            _log.LogInfo($"[Session] Remote tool pickup: '{msg.ToolName}'");
            DestroyDroppedWorldTool(msg.ToolName);
        }

        /// <summary>Instantiate a tool prefab as a world object with physics (visible on ground).</summary>
        private void SpawnWorldTool(string toolName, Vector3 pos, Quaternion rot, Vector3 vel)
        {
            if (!System.Enum.TryParse<SavableObjectID>(toolName, out var sid)) return;
            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm == null) return;

            var prefab = slm.GetPrefab(sid);
            if (prefab == null) { _log.LogWarning($"[Session] No prefab for '{toolName}'"); return; }

            var go = UnityEngine.Object.Instantiate(prefab, pos, rot);
            go.SetActive(true);

            // Show world model, hide view model
            var bht = go.GetComponent<BaseHeldTool>();
            if (bht != null)
            {
                bht.HideWorldModel(false);
                bht.HideViewModel(true);
                bht.Owner = null;
            }

            // Enable physics
            var rb = go.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                go.transform.parent = null;
                rb.isKinematic = false;
                rb.position = pos;
                rb.rotation = rot;
                rb.linearVelocity = vel;
            }

            _log.LogInfo($"[Session] Spawned world tool '{toolName}' at ({pos.x:F1},{pos.y:F1},{pos.z:F1})");
        }

        /// <summary>Find and destroy a loose (dropped) tool in the scene with a matching ID.</summary>
        private void DestroyDroppedWorldTool(string toolName)
        {
            if (!System.Enum.TryParse<SavableObjectID>(toolName, out var sid)) return;

            foreach (var bht in UnityEngine.Object.FindObjectsByType<BaseHeldTool>(FindObjectsSortMode.None))
            {
                if (bht == null) continue;
                if (bht.SavableObjectID != sid) continue;
                if (bht.Owner != null) continue; // Still held/in inventory — skip
                UnityEngine.Object.Destroy(bht.gameObject);
                _log.LogInfo($"[Session] Destroyed dropped world tool '{toolName}'");
                return;
            }
        }

        private void RemoveToolFromHostInventory(string toolName)
        {
            if (_cachedPlayerController == null)
                _cachedPlayerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (_cachedPlayerController == null) return;
            var inv = _cachedPlayerController.GetComponent<PlayerInventory>();
            if (inv == null) return;

            if (!System.Enum.TryParse<SavableObjectID>(toolName, out var sid)) return;

            // Find the first matching tool and silently remove it (no physics drop)
            for (int i = 0; i < inv.Items.Count; i++)
            {
                var tool = inv.Items[i];
                if (tool != null && tool.SavableObjectID == sid)
                {
                    var bht = tool.GetComponent<BaseHeldTool>();
                    if (bht != null)
                        inv.RemoveFromInventory(bht);
                    UnityEngine.Object.Destroy(tool.gameObject);
                    _log.LogInfo($"[Session] Removed '{toolName}' from host inventory at slot {i}");
                    return;
                }
            }
            _log.LogWarning($"[Session] Tool '{toolName}' not found in host inventory to remove");
        }

        private void AddToolToHostInventory(string toolName)
        {
            if (_cachedPlayerController == null)
                _cachedPlayerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (_cachedPlayerController == null) return;
            var inv = _cachedPlayerController.GetComponent<PlayerInventory>();
            if (inv == null) return;
            var slm = Singleton<SavingLoadingManager>.Instance;
            if (slm == null) return;

            if (!System.Enum.TryParse<SavableObjectID>(toolName, out var sid)) return;

            var prefabGo = slm.GetPrefab(sid);
            if (prefabGo == null) return;
            var toolPrefab = prefabGo.GetComponent<BaseHeldTool>();
            if (toolPrefab == null) return;

            var spawned = UnityEngine.Object.Instantiate(toolPrefab);
            Patches.ToolPatch.NetworkBypass = true;
            try { spawned.TryAddToInventory(); }
            finally { Patches.ToolPatch.NetworkBypass = false; }
            _log.LogInfo($"[Session] Added '{toolName}' to host inventory");
        }

        /// <summary>Host broadcasts a newly placed building to all clients.</summary>
        public void BroadcastBuildingSpawned(BuildingState state)
        {
            if (!MultiplayerState.IsHost || _net == null) return;
            _net.SendToAll(MessageType.BuildingSpawned, state);
        }

        public void SendMineNodeRPC(NetVector3 nodePos, float damage, NetVector3 hitPos)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.MineNode, new MineNodeMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                NodePosition = nodePos,
                Damage = damage,
                HitPosition = hitPos
            });
        }

        /// <summary>Host broadcasts a mining damage event to all clients so they see node damage/break.</summary>
        public void BroadcastMineNode(NetVector3 nodePos, float damage, NetVector3 hitPos)
        {
            if (!MultiplayerState.IsHost || _net == null) return;
            _net.SendToAll(MessageType.MineNode, new MineNodeMessage
            {
                PlayerId = 0,
                NodePosition = nodePos,
                Damage = damage,
                HitPosition = hitPos
            });
        }

        /// <summary>Client applies a remote mining event: finds closest node and deals damage locally.</summary>
        private void ApplyRemoteMining(MineNodeMessage msg)
        {
            OreNode closest = null;
            float closestDist = float.MaxValue;
            foreach (var node in UnityEngine.Object.FindObjectsByType<OreNode>(FindObjectsSortMode.None))
            {
                float dist = Vector3.Distance(node.transform.position, msg.NodePosition.ToUnity());
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = node;
                }
            }
            if (closest != null && closestDist < 1f)
            {
                // Count ores before so we can destroy any that spawn locally (host sends the real ones)
                int oreCountBefore = OrePiece.AllOrePieces?.Count ?? 0;

                MiningPatch.NetworkBypass = true;
                MiningPatch.SuppressOreSpawn = true;
                try { closest.TakeDamage(msg.Damage, msg.HitPosition.ToUnity()); }
                finally
                {
                    MiningPatch.NetworkBypass = false;
                    MiningPatch.SuppressOreSpawn = false;
                }

                // Destroy any ore pieces that were spawned locally (host will send the authoritative ones)
                var allOres = OrePiece.AllOrePieces;
                if (allOres != null && allOres.Count > oreCountBefore)
                {
                    for (int i = allOres.Count - 1; i >= oreCountBefore; i--)
                    {
                        var ore = allOres[i];
                        if (ore != null)
                        {
                            try { ore.Delete(); }
                            catch { UnityEngine.Object.Destroy(ore.gameObject); }
                        }
                    }
                }
            }
        }

        public void SendCrateDamageRPC(NetVector3 cratePos, float damage, NetVector3 hitPos)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.CrateDamage, new CrateDamageMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                CratePosition = cratePos,
                Damage = damage,
                HitPosition = hitPos
            });
        }

        /// <summary>Host broadcasts a crate damage event to all clients.</summary>
        public void BroadcastCrateDamage(NetVector3 cratePos, float damage, NetVector3 hitPos)
        {
            if (!MultiplayerState.IsHost || _net == null) return;
            _net.SendToAll(MessageType.CrateDamage, new CrateDamageMessage
            {
                PlayerId = 0,
                CratePosition = cratePos,
                Damage = damage,
                HitPosition = hitPos
            });
        }

        /// <summary>Client applies a remote crate damage event.</summary>
        private void ApplyRemoteCrateDamage(CrateDamageMessage msg)
        {
            _log.LogInfo($"[Session] Applying remote crate damage at {msg.CratePosition}, dmg={msg.Damage}");
            BreakableCrate closest = null;
            float closestDist = float.MaxValue;
            foreach (var crate in UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None))
            {
                float dist = Vector3.Distance(crate.transform.position, msg.CratePosition.ToUnity());
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = crate;
                }
            }
            if (closest != null && closestDist < 2f)
            {
                // Count ores before — crate may spawn ores when breaking,
                // but host sends the authoritative ones via OreSpawnedBatch
                int oreCountBefore = OrePiece.AllOrePieces?.Count ?? 0;

                Patches.MachineProcessingPatch.CrateNetworkBypass = true;
                try { closest.TakeDamage(msg.Damage, msg.HitPosition.ToUnity()); }
                finally { Patches.MachineProcessingPatch.CrateNetworkBypass = false; }

                // Destroy any ore pieces that were spawned locally (host will send the real ones)
                var allOres = OrePiece.AllOrePieces;
                if (allOres != null && allOres.Count > oreCountBefore)
                {
                    for (int i = allOres.Count - 1; i >= oreCountBefore; i--)
                    {
                        var ore = allOres[i];
                        if (ore != null)
                        {
                            try { ore.Delete(); }
                            catch { UnityEngine.Object.Destroy(ore.gameObject); }
                        }
                    }
                }
            }
        }

        /// <summary>Client: match host crate network IDs to local BreakableCrate objects and apply positions.</summary>
        private void ReconcileCratesFromSnapshot(List<CrateState> hostCrates)
        {
            _clientCrateByNetId.Clear();

            // Collect all local crates
            var localCrates = new List<BreakableCrate>(
                UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None));

            // For each host crate, find the closest local crate and assign the network ID
            var claimed = new HashSet<int>(); // local instanceIds already matched
            foreach (var hc in hostCrates)
            {
                BreakableCrate best = null;
                float bestDist = float.MaxValue;
                foreach (var lc in localCrates)
                {
                    if (lc == null || claimed.Contains(lc.GetInstanceID())) continue;
                    float dist = Vector3.Distance(lc.transform.position, hc.Position.ToUnity());
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = lc;
                    }
                }
                if (best != null)
                {
                    claimed.Add(best.GetInstanceID());
                    _clientCrateByNetId[hc.NetworkId] = best;
                    // Snap crate to host position
                    best.transform.position = hc.Position.ToUnity();
                    best.transform.rotation = hc.Rotation.ToUnity();
                    var rb = best.GetComponent<Rigidbody>();
                    if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                }
            }
            _log.LogInfo($"[Session] Reconciled {_clientCrateByNetId.Count} crates from snapshot");
        }

        /// <summary>Host: check all tracked crates for position changes and send batched updates.</summary>
        private void SendCratePositionUpdates()
        {
            _poolCratePositionUpdates.Clear();

            // Single scene scan — build instanceId → crate lookup
            var allCrates = UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None);
            var crateById = new Dictionary<int, BreakableCrate>(allCrates.Length);
            foreach (var crate in allCrates)
            {
                if (crate == null) continue;
                int instanceId = crate.GetInstanceID();
                crateById[instanceId] = crate;
                // Ensure newly-spawned crates get a network ID
                if (!_hostCrateInstanceToNetId.ContainsKey(instanceId))
                    _hostCrateInstanceToNetId[instanceId] = _nextCrateNetId++;
            }

            // Check for moved crates using the lookup
            foreach (var kv in _hostCrateInstanceToNetId)
            {
                if (!crateById.TryGetValue(kv.Key, out var found)) continue;

                var pos = found.transform.position;
                int netId = kv.Value;
                if (_lastSentCratePositions.TryGetValue(netId, out var lastPos))
                {
                    if (Vector3.SqrMagnitude(pos - lastPos) < OrePositionMoveThreshold * OrePositionMoveThreshold)
                        continue;
                }

                _lastSentCratePositions[netId] = pos;
                _poolCratePositionUpdates.Add(new CratePositionUpdate
                {
                    NetworkId = netId,
                    Position = new NetVector3(pos),
                    Rotation = new NetQuaternion(found.transform.rotation)
                });
            }

            if (_poolCratePositionUpdates.Count > 0)
                _net.SendToAll(MessageType.CratePositionBatch, _poolCratePositionUpdates);
        }

        /// <summary>Client: apply crate position updates from host using smooth interpolation.</summary>
        private void ApplyCratePositionUpdates(List<CratePositionUpdate> updates)
        {
            if (updates == null) return;
            foreach (var upd in updates)
            {
                // Skip crates the client is actively holding — don't let host override local grab
                if (_clientLocallyHeldCrates.Contains(upd.NetworkId)) continue;

                if (_clientCrateByNetId.TryGetValue(upd.NetworkId, out var crate) && crate != null)
                {
                    // Set interpolation targets instead of snapping
                    _crateTargetPositions[upd.NetworkId] = upd.Position.ToUnity();
                    _crateTargetRotations[upd.NetworkId] = upd.Rotation.ToUnity();
                    var rb = crate.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    _remotelyHeldCrates[upd.NetworkId] = UnityEngine.Time.unscaledTime;
                }
            }
        }

        /// <summary>Host: apply client-sent crate positions (client is grabbing/moving a crate).</summary>
        private void ApplyClientCratePositions(List<CratePositionUpdate> updates)
        {
            if (updates == null || updates.Count == 0) return;

            // Build a reverse lookup: netId → instanceId
            Dictionary<int, int> netIdToInstance = null;
            foreach (var upd in updates)
            {
                // Lazy-build reverse lookup on first iteration
                if (netIdToInstance == null)
                {
                    netIdToInstance = new Dictionary<int, int>(_hostCrateInstanceToNetId.Count);
                    foreach (var kv in _hostCrateInstanceToNetId)
                        netIdToInstance[kv.Value] = kv.Key;
                }
                if (!netIdToInstance.TryGetValue(upd.NetworkId, out int instanceId)) continue;

                // Single scene scan (cached for the batch)
                if (_cachedCrateLookup == null)
                {
                    var allCrates = UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None);
                    _cachedCrateLookup = new Dictionary<int, BreakableCrate>(allCrates.Length);
                    foreach (var c in allCrates)
                        if (c != null) _cachedCrateLookup[c.GetInstanceID()] = c;
                }

                if (!_cachedCrateLookup.TryGetValue(instanceId, out var found)) continue;
                found.transform.position = upd.Position.ToUnity();
                found.transform.rotation = upd.Rotation.ToUnity();
                var rb = found.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                _remotelyHeldCrates[upd.NetworkId] = UnityEngine.Time.unscaledTime;
            }
        }

        // ── Item drop/throw sync ──────────────────────

        /// <summary>Track previously held object to detect drops.</summary>
        private string _prevHeldObjectId;
        private Vector3 _prevHeldObjectPos;
        private Vector3 _prevHeldObjectPosOlder; // position from frame before _prevHeldObjectPos
        private Vector3 _prevHeldObjectVelocity; // cached velocity while still held (for throw detection)
        private Rigidbody _prevHeldObjectRb; // cached rigidbody for velocity tracking

        /// <summary>
        /// Detect when the local player drops/throws a held item and send velocity info
        /// so other clients see the item fly correctly.
        /// </summary>
        public void DetectAndSendItemDrop(PlayerController pc)
        {
            if (_net == null || !_net.IsRunning) return;

            string currentHeld = null;
            Rigidbody currentRb = null;
            try
            {
                var heldObj = pc?.HeldObject;
                if (heldObj != null)
                {
                    currentHeld = TryGetHeldObjectFallbackId(heldObj);
                    if (string.IsNullOrEmpty(currentHeld))
                        currentHeld = heldObj.name?.Replace("(Clone)", "").Trim();
                    currentRb = heldObj.GetComponent<Rigidbody>();
                }
            }
            catch { }

            // Detect transition from holding to not holding
            if (!string.IsNullOrEmpty(_prevHeldObjectId) && string.IsNullOrEmpty(currentHeld))
            {
                // Item was dropped - find it and send with cached velocity
                TrySendDropVelocity(_prevHeldObjectId, _prevHeldObjectPos, _prevHeldObjectVelocity);
            }

            _prevHeldObjectId = currentHeld;
            if (!string.IsNullOrEmpty(currentHeld))
            {
                Vector3 newPos = Vector3.zero;
                try { newPos = pc.HeldObject.transform.position; } catch { }

                // Cache velocity while still being held
                if (_prevHeldObjectRb != currentRb && currentRb != null)
                    _prevHeldObjectVelocity = Vector3.zero;
                _prevHeldObjectRb = currentRb;

                if (currentRb != null && !currentRb.isKinematic)
                {
                    _prevHeldObjectVelocity = currentRb.linearVelocity;
                }
                else
                {
                    // Estimate velocity from position delta (for kinematic held objects)
                    float dt = UnityEngine.Time.deltaTime;
                    if (dt > 0.001f && _prevHeldObjectPos != Vector3.zero)
                    {
                        var estimatedVel = (newPos - _prevHeldObjectPos) / dt;
                        _prevHeldObjectVelocity = Vector3.Lerp(_prevHeldObjectVelocity, estimatedVel, 0.5f);
                    }
                }

                _prevHeldObjectPosOlder = _prevHeldObjectPos;
                _prevHeldObjectPos = newPos;
            }
            else
            {
                _prevHeldObjectVelocity = Vector3.zero;
                _prevHeldObjectRb = null;
            }
        }

        private void TrySendDropVelocity(string itemId, Vector3 lastKnownPos, Vector3 cachedVelocity)
        {
            // Search for the recently-dropped item near the last known position
            float searchRadius = 5f;

            // Check ores
            if (MultiplayerState.IsClient)
            {
                foreach (var kv in _clientOreByNetId)
                {
                    var ore = kv.Value;
                    if (ore == null) continue;
                    if (Vector3.SqrMagnitude(ore.transform.position - lastKnownPos) > searchRadius * searchRadius) continue;
                    var rb = ore.GetComponent<Rigidbody>();
                    if (rb == null) continue;
                    SendItemDropMessage("ore", kv.Key, ore.transform, rb, cachedVelocity);
                    return;
                }
                foreach (var kv in _clientCrateByNetId)
                {
                    var crate = kv.Value;
                    if (crate == null) continue;
                    if (Vector3.SqrMagnitude(crate.transform.position - lastKnownPos) > searchRadius * searchRadius) continue;
                    var rb = crate.GetComponent<Rigidbody>();
                    if (rb == null) continue;
                    SendItemDropMessage("crate", kv.Key, crate.transform, rb, cachedVelocity);
                    return;
                }
            }
            else if (MultiplayerState.IsHost)
            {
                // Host: check ores near drop position
                foreach (var ore in OrePiece.AllOrePieces)
                {
                    if (ore == null) continue;
                    if (Vector3.SqrMagnitude(ore.transform.position - lastKnownPos) > searchRadius * searchRadius) continue;
                    if (!_hostOreInstanceToNetId.TryGetValue(ore.GetInstanceID(), out int netId)) continue;
                    var rb = ore.GetComponent<Rigidbody>();
                    if (rb == null) continue;
                    SendItemDropMessage("ore", netId, ore.transform, rb, cachedVelocity);
                    return;
                }
                // Host: check crates
                var allCrates = UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None);
                foreach (var crate in allCrates)
                {
                    if (crate == null) continue;
                    if (Vector3.SqrMagnitude(crate.transform.position - lastKnownPos) > searchRadius * searchRadius) continue;
                    if (!_hostCrateInstanceToNetId.TryGetValue(crate.GetInstanceID(), out int netId)) continue;
                    var rb = crate.GetComponent<Rigidbody>();
                    if (rb == null) continue;
                    SendItemDropMessage("crate", netId, crate.transform, rb, cachedVelocity);
                    return;
                }
            }
        }

        private void SendItemDropMessage(string itemType, int netId, Transform t, Rigidbody rb, Vector3 cachedVelocity)
        {
            // Use the actual rb velocity if available, otherwise fall back to cached
            var vel = rb.linearVelocity;
            if (vel.sqrMagnitude < 0.01f && cachedVelocity.sqrMagnitude > 0.01f)
                vel = cachedVelocity;

            var msg = new ItemDroppedMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                ItemType = itemType,
                NetworkId = netId,
                Position = new NetVector3(t.position),
                Rotation = new NetQuaternion(t.rotation),
                Velocity = new NetVector3(vel),
                AngularVelocity = new NetVector3(rb.angularVelocity)
            };

            // Ensure the item is non-kinematic locally so it responds to physics
            rb.isKinematic = false;

            if (MultiplayerState.IsHost)
                _net.SendToAll(MessageType.ItemDropped, msg);
            else
                _net.SendToHost(MessageType.ItemDropped, msg);
        }

        /// <summary>Host: receive drop from client, apply physics, broadcast to all.</summary>
        private void HandleItemDropped(uint clientId, ItemDroppedMessage msg)
        {
            if (msg.ItemType == "ore")
            {
                foreach (var kv in _hostOreInstanceToNetId)
                {
                    if (kv.Value != msg.NetworkId) continue;
                    foreach (var ore in OrePiece.AllOrePieces)
                    {
                        if (ore != null && ore.GetInstanceID() == kv.Key)
                        {
                            // Apply position first so velocity starts from correct place
                            ore.transform.position = msg.Position.ToUnity();
                            ore.transform.rotation = msg.Rotation.ToUnity();
                            var rb = ore.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = false;
                                rb.linearVelocity = msg.Velocity.ToUnity();
                                rb.angularVelocity = msg.AngularVelocity.ToUnity();
                            }
                            _remotelyHeldOres.Remove(msg.NetworkId);
                            break;
                        }
                    }
                    break;
                }
            }
            else if (msg.ItemType == "crate")
            {
                foreach (var kv in _hostCrateInstanceToNetId)
                {
                    if (kv.Value != msg.NetworkId) continue;
                    var allCrates = UnityEngine.Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None);
                    foreach (var crate in allCrates)
                    {
                        if (crate != null && crate.GetInstanceID() == kv.Key)
                        {
                            crate.transform.position = msg.Position.ToUnity();
                            crate.transform.rotation = msg.Rotation.ToUnity();
                            var rb = crate.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.isKinematic = false;
                                rb.linearVelocity = msg.Velocity.ToUnity();
                                rb.angularVelocity = msg.AngularVelocity.ToUnity();
                            }
                            _remotelyHeldCrates.Remove(msg.NetworkId);
                            break;
                        }
                    }
                    break;
                }
            }

            // Broadcast to all clients
            _net.SendToAll(MessageType.ItemDropped, msg);
        }

        /// <summary>Client: apply item drop velocity from host broadcast.</summary>
        private void ApplyItemDropped(ItemDroppedMessage msg)
        {
            // Don't apply our own drops
            if (msg.PlayerId == MultiplayerState.LocalPlayerId) return;

            if (msg.ItemType == "ore")
            {
                if (_clientOreByNetId.TryGetValue(msg.NetworkId, out var ore) && ore != null)
                {
                    ore.transform.position = msg.Position.ToUnity();
                    ore.transform.rotation = msg.Rotation.ToUnity();
                    var rb = ore.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.linearVelocity = msg.Velocity.ToUnity();
                        rb.angularVelocity = msg.AngularVelocity.ToUnity();
                    }
                    _remotelyHeldOres.Remove(msg.NetworkId);
                    _oreTargetPositions.Remove(msg.NetworkId);
                    _oreTargetRotations.Remove(msg.NetworkId);
                }
            }
            else if (msg.ItemType == "crate")
            {
                if (_clientCrateByNetId.TryGetValue(msg.NetworkId, out var crate) && crate != null)
                {
                    crate.transform.position = msg.Position.ToUnity();
                    crate.transform.rotation = msg.Rotation.ToUnity();
                    var rb = crate.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.linearVelocity = msg.Velocity.ToUnity();
                        rb.angularVelocity = msg.AngularVelocity.ToUnity();
                    }
                    _remotelyHeldCrates.Remove(msg.NetworkId);
                    _crateTargetPositions.Remove(msg.NetworkId);
                    _crateTargetRotations.Remove(msg.NetworkId);
                }
            }
        }

        public void SendResearchRPC(string researchItemId)
        {
            if (!MultiplayerState.IsClient || _net == null) return;
            _net.SendToHost(MessageType.ResearchItem, new ResearchItemMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                ResearchItemId = researchItemId
            });
        }

        // ── Debug / diagnostics ───────────────────────

        /// <summary>Current tick counter.</summary>
        public long CurrentTick => _tick;

        /// <summary>Number of connected players (including host).</summary>
        public int PlayerCount => _players.Count;

        /// <summary>Get a diagnostic summary string for the debug overlay.</summary>
        public string GetDebugInfo()
        {
            var role = MultiplayerState.CurrentRole.ToString();
            var phase = Phase.ToString();
            var players = _players.Count;
            var net = _net != null && _net.IsRunning ? "UP" : "DOWN";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Role: {role}  |  Phase: {phase}  |  Net: {net}");
            sb.AppendLine($"Tick: {_tick}  |  Players: {players}");

            if (MultiplayerState.IsHost)
            {
                var oreCount = OrePiece.AllOrePieces?.Count ?? 0;
                var buildingCount = 0;
                foreach (var bo in UnityEngine.Object.FindObjectsByType<BuildingObject>(FindObjectsSortMode.None))
                    if (bo != null && !bo.IsGhost) buildingCount++;
                var eco = Singleton<EconomyManager>.Instance;
                sb.AppendLine($"Buildings: {buildingCount}  |  Ores: {oreCount}  |  OreNetIds: {_hostOreInstanceToNetId.Count}");
                sb.AppendLine($"Money: {(eco != null ? eco.Money.ToString("N0") : "?")}  |  Desyncs: {_consecutiveDesyncCount}");

                foreach (var p in _players)
                    sb.AppendLine($"  P{p.PlayerId}: {p.DisplayName} @ ({p.Position.X:F1}, {p.Position.Y:F1}, {p.Position.Z:F1})");
            }
            else if (MultiplayerState.IsClient)
            {
                var trackedOres = _clientOreByNetId.Count;
                sb.AppendLine($"Tracked ores: {trackedOres}  |  Waiting snapshot: {_clientWaitingForSnapshot}");
                sb.AppendLine($"Scene ready: {_clientSceneReady}  |  Loading: {_clientLoadingScene}");
            }

            return sb.ToString();
        }

        /// <summary>Force a full resync: host rebuilds snapshot for all clients, client requests resync from host.</summary>
        public void ForceResync()
        {
            if (MultiplayerState.IsHost && _net != null)
            {
                if (!IsGameSceneReady()) { _log.LogWarning("[Debug] Scene not ready for resync"); return; }
                var snapshot = BuildSnapshot();
                _net.SendToAll(MessageType.FullSnapshot, snapshot);
                _log.LogInfo("[Debug] Force-sent full snapshot to all clients");
                LogEvent("Force resync sent to all clients.");
            }
            else if (MultiplayerState.IsClient && _net != null)
            {
                _clientWaitingForSnapshot = true;
                _net.SendToHost(MessageType.ResyncRequest, _tick);
                _log.LogInfo("[Debug] Requested resync from host");
                LogEvent("Requested resync from host.");
            }
        }

        /// <summary>Add debug money to the economy. Host-only; triggers money dirty flag for sync.</summary>
        public void AddDebugMoney(float amount)
        {
            if (!IsGameSceneReady()) return;
            var eco = Singleton<EconomyManager>.Instance;
            if (eco == null) return;

            EconomyPatch.NetworkBypass = true;
            try
            {
                eco.AddMoney(amount);
                DirtyTracker.MoneyDirty = true;
            }
            finally { EconomyPatch.NetworkBypass = false; }

            _log.LogInfo($"[Debug] Added ${amount:N0} debug money (total: ${eco.Money:N0})");
            LogEvent($"Added ${amount:N0} debug money.");
        }

        /// <summary>Log a detailed snapshot dump to BepInEx log for debugging.</summary>
        public void DumpSnapshot()
        {
            if (!IsGameSceneReady()) { _log.LogInfo("[Debug] Scene not ready"); return; }

            WorldSnapshot snapshot;
            if (MultiplayerState.IsHost)
                snapshot = BuildSnapshot();
            else
                snapshot = BuildClientSnapshot();

            var diag = WorldHasher.DiagnoseComponents(snapshot);
            var hash = WorldHasher.ComputeHash(snapshot);
            _log.LogInfo($"[Debug] === SNAPSHOT DUMP ===");
            _log.LogInfo($"[Debug] Hash: {hash}");
            _log.LogInfo($"[Debug] Components: {diag}");
            _log.LogInfo($"[Debug] Players: {snapshot.Players?.Count ?? 0}");
            _log.LogInfo($"[Debug] Buildings: {snapshot.Buildings?.Count ?? 0}");
            _log.LogInfo($"[Debug] Ores: {snapshot.OrePieces?.Count ?? 0}");
            _log.LogInfo($"[Debug] Conveyors: {snapshot.Conveyors?.Count ?? 0}");
            _log.LogInfo($"[Debug] Money: {(snapshot.World?.Money ?? 0f):N0}");
            _log.LogInfo($"[Debug] Research: {snapshot.World?.CompletedResearchIds?.Length ?? 0} completed");
            _log.LogInfo($"[Debug] Quests: {snapshot.World?.CompletedQuestIds?.Length ?? 0} completed, {snapshot.World?.ActiveQuests?.Length ?? 0} active");

            if (snapshot.Buildings != null)
            {
                foreach (var b in snapshot.Buildings)
                    _log.LogInfo($"[Debug]   Building: {b.SavableObjectId} @ ({b.Position.X:F1},{b.Position.Y:F1},{b.Position.Z:F1}) data={!string.IsNullOrEmpty(b.CustomSaveData)}");
            }

            LogEvent($"Snapshot dumped to log. Hash={hash}, B={snapshot.Buildings?.Count ?? 0}, O={snapshot.OrePieces?.Count ?? 0}");
        }

        // ── Debug bot ─────────────────────────────────

        /// <summary>
        /// Toggle debug bot. Cycles: Off → Mirror → Cycle → Off.
        /// Works without a full multiplayer session (solo debug mode).
        /// </summary>
        public void ToggleDebugBot()
        {
            if (_debugBotActive && _debugBotMirrorMode && _debugBotStatic)
            {
                // Static Mirror → Moving Mirror
                _debugBotStatic = false;
                _debugBotAngle = 0f;
                _log.LogInfo("[Session] Debug bot switched to MOVING MIRROR mode");
                LogEvent("Test bot: moving mirror (circles you)");
                return;
            }
            else if (_debugBotActive && _debugBotMirrorMode && !_debugBotStatic)
            {
                // Moving Mirror → Cycle
                _debugBotMirrorMode = false;
                _debugBotAngle = 0f;
                _log.LogInfo("[Session] Debug bot switched to CYCLE mode");
                LogEvent("Test bot: cycle mode (tools rotate every 4s)");
                return;
            }
            else if (_debugBotActive && !_debugBotMirrorMode)
            {
                // Cycle → Off
                _debugBotActive = false;
                _players.RemoveAll(p => p.PlayerId == DebugBotPlayerId);
                RemotePlayerManager.Clear();
                if (!_debugBotSoloMode)
                    RemotePlayerManager.UpdatePlayers(_players, MultiplayerState.LocalPlayerId);
                _debugBotSoloMode = false;
                _debugBotLastTool = null;
                _log.LogInfo("[Session] Debug bot removed");
                LogEvent("Test bot left.");
                return;
            }

            // Off → Static Mirror (spawn)
            _debugBotActive = true;
            _debugBotMirrorMode = true;
            _debugBotStatic = true;
            _debugBotLastTool = null;
            _debugBotAngle = 0f;

            // If not in a session, set up solo mode so the bot still renders
            bool inSession = MultiplayerState.IsOnline;
            if (!inSession)
            {
                _debugBotSoloMode = true;
                // Add a local player entry if not present
                if (!_players.Exists(p => p.PlayerId == 0))
                {
                    _players.Add(new PlayerState
                    {
                        PlayerId = 0,
                        DisplayName = "You"
                    });
                }
            }

            _players.Add(new PlayerState
            {
                PlayerId = DebugBotPlayerId,
                DisplayName = "TestBot"
            });
            _log.LogInfo("[Session] Debug bot spawned in STATIC MIRROR mode" + (_debugBotSoloMode ? " (solo)" : ""));
            LogEvent("Test bot joined (static mirror — stands in front of you).");
        }

        public bool IsDebugBotActive => _debugBotActive;
        public bool IsDebugBotMirrorMode => _debugBotMirrorMode;
        public bool IsDebugBotStatic => _debugBotStatic;
        public string DebugBotModeName => !_debugBotActive ? "Off"
            : (_debugBotMirrorMode && _debugBotStatic ? "Static"
            : (_debugBotMirrorMode ? "Mirror" : "Cycle"));

        private void UpdateDebugBot()
        {
            var bot = _players.Find(p => p.PlayerId == DebugBotPlayerId);
            if (bot == null) return;

            // Run tool prefab audit once on first bot tick
            if (!_debugBotAuditDone)
            {
                _debugBotAuditDone = true;
                RemotePlayerManager.AuditAllToolPrefabs();
            }

            // In solo mode, keep our local player entry up to date
            if (_debugBotSoloMode)
            {
                var localPlayer = _players.Find(p => p.PlayerId == 0);
                if (localPlayer != null && TryGetLocalPlayerPose(out var lPos, out var lRot, out var lPc))
                {
                    localPlayer.Position = new NetVector3(lPos);
                    localPlayer.Rotation = new NetQuaternion(lRot);
                }
            }

            var hostPlayer = _players.Find(p => p.PlayerId == 0);
            var center = hostPlayer?.Position.ToUnity() ?? Vector3.zero;

            if (_debugBotStatic)
            {
                // Static mode: place bot 3m in front of the player, facing the player
                var hostRot = hostPlayer?.Rotation.ToUnity() ?? Quaternion.identity;
                var forward = hostRot * Vector3.forward;
                var botPos = center + forward * 3f;
                bot.Position = new NetVector3(botPos);
                // Face back toward the player
                bot.Rotation = new NetQuaternion(Quaternion.LookRotation(-forward));
            }
            else
            {
                // Circle around the host player at radius 4, ~0.25 rev/sec
                _debugBotAngle += 0.25f * (1f / 20f) * 2f * Mathf.PI; // 20 tps assumed
                if (_debugBotAngle > 2f * Mathf.PI) _debugBotAngle -= 2f * Mathf.PI;

                bot.Position = new NetVector3(
                    center.x + Mathf.Cos(_debugBotAngle) * 4f,
                    center.y,
                    center.z + Mathf.Sin(_debugBotAngle) * 4f);

                // Face the direction of movement
                float nextAngle = _debugBotAngle + 0.05f;
                var forward = new Vector3(
                    Mathf.Cos(nextAngle) * 4f - Mathf.Cos(_debugBotAngle) * 4f,
                    0,
                    Mathf.Sin(nextAngle) * 4f - Mathf.Sin(_debugBotAngle) * 4f);
                if (forward.sqrMagnitude > 0.001f)
                    bot.Rotation = new NetQuaternion(Quaternion.LookRotation(forward));
            }

            // Equipment: mirror local player or cycle through preset list
            if (_debugBotMirrorMode)
            {
                // Mirror: copy whatever the local player has equipped
                if (TryGetLocalPlayerPose(out _, out _, out var pc) && pc != null)
                {
                    var resolvedTool = GetEquippedToolName(pc);
                    // Use sticky debounce: only update if we got a valid tool name.
                    // GetEquippedToolName often returns null due to API limitations.
                    if (!string.IsNullOrEmpty(resolvedTool))
                        _debugBotLastTool = resolvedTool;
                    bot.EquippedTool = _debugBotLastTool;

                    bot.IsCrouching = GetCrouchState(pc);
                    if (TryGetHeldObjectState(pc, out var heldId, out var heldPos, out var heldRot))
                    {
                        bot.HeldObjectId = heldId;
                        // Offset held object position to match bot's offset from player
                        var playerPos = hostPlayer?.Position.ToUnity() ?? Vector3.zero;
                        var botPos = bot.Position.ToUnity();
                        var offset = botPos - playerPos;
                        bot.HeldObjectPosition = new NetVector3(heldPos.ToUnity() + offset);
                        bot.HeldObjectRotation = heldRot;
                    }
                    else
                    {
                        bot.HeldObjectId = null;
                    }

                    // Periodic diagnostic log every ~3 seconds
                    if (_tick % 60 == 0)
                        _log.LogInfo($"[DebugBot] Mirror: tool='{bot.EquippedTool}' heldObj='{bot.HeldObjectId}' crouch={bot.IsCrouching}");
                }
                else
                {
                    if (_tick % 60 == 0)
                        _log.LogWarning("[DebugBot] Mirror: no PlayerController found");
                }
            }
            else
            {
                // Cycle through tools every 4 seconds so host can see all visuals
                int toolIndex = ((int)(Time.time / 4f)) % DebugBotTools.Length;
                bot.EquippedTool = DebugBotTools[toolIndex];
                bot.HeldObjectId = null;
                bot.IsCrouching = false;
            }
        }

        /// <summary>Tick only the debug bot visual system (for solo mode when not in a real session).</summary>
        public void TickDebugBotOnly()
        {
            if (!_debugBotActive || !_debugBotSoloMode) return;
            _tick++; // keep tick counter going for periodic logs
            UpdateDebugBot();
            RemotePlayerManager.UpdatePlayers(_players, 0);
        }

        // ── Lifecycle ────────────────────────────────

        public void Stop()
        {
            _log.LogInfo($"[Session] Stop() called from:\n{System.Environment.StackTrace}");
            LeaveLobby();
            _net?.Stop();
            _net = null;
            _pendingHost = false;
            _clientLoadingScene = false;
            _clientSceneReady = false;
            _clientWaitingForConnect = false;
            _clientWaitingForSnapshot = false;
            _handshakeAckReceived = false;
            _syncRetryCount = 0;
            _consecutiveDesyncCount = 0;
            _debugBotActive = false;
            _debugBotSoloMode = false;
            _debugBotLastTool = null;
            bool wasOnline = MultiplayerState.IsOnline || Phase != LobbyPhase.None;
            Phase = LobbyPhase.None;
            MultiplayerState.CurrentRole = MultiplayerState.Role.Offline;
            _players.Clear();
            _clientNames.Clear();
            _cachedPlayerController = null;
            _loggedFirstDelta = false;
            _hostOreInstanceToNetId.Clear();
            _clientOreByNetId.Clear();
            _lastSentOrePositions.Clear();
            _clientLastSentOrePos.Clear();
            _clientLastSentCratePos.Clear();
            _clientLocallyHeldCrates.Clear();
            _oreTargetPositions.Clear();
            _oreTargetRotations.Clear();
            _crateTargetPositions.Clear();
            _crateTargetRotations.Clear();
            _remotelyHeldOres.Clear();
            _remotelyHeldCrates.Clear();
            _prevHeldObjectId = null;
            _prevHeldObjectVelocity = Vector3.zero;
            _prevHeldObjectRb = null;
            _lastValidToolName = null;
            _toolNullTicks = 0;
            _nextOreNetId = 1;
            _savedPlayerInventories.Clear();
            _clientSteamIds.Clear();
            _hostSaveFilePath = null;
            LobbyPlayerNames.Clear();
            RemotePlayerManager.Clear();
            _log.LogInfo("[Session] Stopped");
            if (wasOnline) LogEvent("Multiplayer session ended.");
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
            Stop();
            SteamClient.Shutdown();
            if (Instance == this) Instance = null;
        }

        // ── Steam Lobby ────────────────────────────────

        /// <summary>Current Steam lobby (if hosting). Used for friend invites.</summary>
        public Steamworks.Data.Lobby? CurrentLobby => _lobby;

        private void LeaveLobby()
        {
            if (_lobby.HasValue)
            {
                _lobby.Value.Leave();
                _lobby = null;
                _log.LogInfo("[Session] Left Steam Lobby");
            }
        }

        /// <summary>Invite a specific friend to the current lobby.</summary>
        public bool InviteFriendToLobby(SteamId friendId)
        {
            if (!_lobby.HasValue) return false;
            return _lobby.Value.InviteFriend(friendId);
        }

        private void OnLobbyInvite(Friend friend, Steamworks.Data.Lobby lobby)
        {
            _log.LogInfo($"[Session] Received lobby invite from {friend.Name} (lobby {lobby.Id})");
            LogEvent($"Lobby invite from {friend.Name}");
        }

        private void OnLobbyMemberJoined(Steamworks.Data.Lobby lobby, Friend friend)
        {
            _log.LogInfo($"[Session] {friend.Name} joined the lobby");
            RefreshLobbyPlayers();
        }

        private void OnLobbyMemberLeave(Steamworks.Data.Lobby lobby, Friend friend)
        {
            _log.LogInfo($"[Session] {friend.Name} left the lobby");
            RefreshLobbyPlayers();
        }

        private void OnLobbyDataChanged(Steamworks.Data.Lobby lobby)
        {
            // Client: check if host launched the game
            if (Phase == LobbyPhase.InLobby && _lobby.HasValue && lobby.Id == _lobby.Value.Id)
            {
                var state = lobby.GetData("state");
                if (state == "launching" || state == "ingame")
                {
                    HandleLobbyLaunch();
                }
            }
        }

        private void OnGameLobbyJoinRequested(Steamworks.Data.Lobby lobby, SteamId friendId)
        {
            _log.LogInfo($"[Session] Friend {friendId} wants to join via lobby invite/overlay");

            // If already in a session, ignore
            if (Phase != LobbyPhase.None)
            {
                _log.LogWarning($"[Session] Already in phase {Phase}, ignoring lobby join request");
                return;
            }

            // Join the lobby (not P2P — just the Steam lobby)
            var playerName = SteamClient.Name ?? "Player";
            JoinLobbyDirect(lobby, playerName);
        }

        // ── Sound event sync ────────────────────────

        /// <summary>Broadcast a sound event to all other players.</summary>
        public void BroadcastSoundEvent(string soundName, Vector3 position)
        {
            if (_net == null || !_net.IsRunning) return;
            var msg = new SoundEventMessage
            {
                PlayerId = MultiplayerState.LocalPlayerId,
                SoundName = soundName,
                Position = new NetVector3(position)
            };
            if (MultiplayerState.IsHost)
                _net.SendToAll(MessageType.SoundEvent, msg);
            else
                _net.SendToHost(MessageType.SoundEvent, msg);
        }

        /// <summary>Host: relay a client's sound event to all other clients.</summary>
        private void HandleSoundEventFromClient(uint clientId, SoundEventMessage msg)
        {
            // Play locally on host
            Patches.SoundPatch.PlayRemoteSound(msg.SoundName, msg.Position.ToUnity());
            // Relay to all clients (they'll skip if it's their own)
            _net.SendToAll(MessageType.SoundEvent, msg);
        }

        /// <summary>Client: play a sound event received from host.</summary>
        private void HandleSoundEventOnClient(SoundEventMessage msg)
        {
            if (msg.PlayerId == MultiplayerState.LocalPlayerId) return; // skip own sounds
            Patches.SoundPatch.PlayRemoteSound(msg.SoundName, msg.Position.ToUnity());
        }

        // ── Event log helpers ────────────────────────

        private static void LogEvent(string msg)
        {
            try { EventLog.Instance?.LogSystem(msg); }
            catch (System.Exception) { /* never let logging crash the session */ }
        }

        private static void LogJoin(string name)
        {
            EventLog.Instance?.LogJoin(name);
        }

        private static void LogLeave(string name)
        {
            EventLog.Instance?.LogLeave(name);
        }
    }
}
