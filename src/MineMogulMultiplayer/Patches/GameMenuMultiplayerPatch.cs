using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MineMogulMultiplayer.Core;
using MineMogulMultiplayer.UI;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Injects a "Host Multiplayer" toggle into the game's Load Save and New Game menus.
    /// When toggled on, pressing Load/Start creates a Steam lobby and shows an overlay
    /// where the host can invite friends and click Launch when ready.
    /// The existing standalone Multiplayer panel remains as a fallback.
    /// </summary>
    [HarmonyPatch]
    internal static class GameMenuMultiplayerPatch
    {
        // ── Shared toggle state (persists across menu open/close) ──
        private static bool _multiplayerToggled;

        // ── Lobby overlay ──
        private static GameObject _overlayRoot;
        private static TextMeshProUGUI _statusText;
        private static Transform _playerListContent;
        private static readonly List<GameObject> _playerRows = new List<GameObject>();

        // Pending launch context
        private static string _pendingSaveFilePath;
        private static string _pendingSceneName;
        private static string _pendingSaveName;
        private static bool _pendingIsNewGame;
        private static GameModeType _pendingGameMode;

        // Coroutine driver (we need a MonoBehaviour to run coroutines)
        private static LobbyPoller _poller;

        // ════════════════════════════════════════════
        //  LoadingMenu patches
        // ════════════════════════════════════════════

        [HarmonyPatch(typeof(LoadingMenu), "OnEnable")]
        [HarmonyPostfix]
        public static void LoadingMenu_OnEnable_Post(LoadingMenu __instance)
        {
            try { InjectToggle(__instance.transform, "LoadingMenu"); }
            catch (Exception ex) { Debug.LogError($"[MineMogulMP] LoadingMenu toggle inject: {ex}"); }
        }

        [HarmonyPatch(typeof(LoadingMenu), "OnLoadGamePressed")]
        [HarmonyPrefix]
        public static bool LoadingMenu_OnLoadGamePressed_Pre(LoadingMenu __instance)
        {
            if (!_multiplayerToggled) return true; // single-player — let original run

            // Read selected save from private fields
            var headerField = AccessTools.Field(typeof(LoadingMenu), "_SelectedSaveFileHeader");
            var pathField = AccessTools.Field(typeof(LoadingMenu), "_selectedSaveFileFullPath");
            if (headerField == null || pathField == null) return true;

            var header = headerField.GetValue(__instance) as SaveFileHeader;
            var filePath = pathField.GetValue(__instance) as string;
            if (header == null || string.IsNullOrEmpty(filePath)) return true;

            // Resolve scene
            string sceneName = ResolveSceneName(header.LevelID);
            if (sceneName == null)
            {
                ShowOverlayStatus($"<color=#CC4444>Cannot resolve scene for '{header.LevelID}'</color>");
                return false;
            }

            _pendingSaveFilePath = filePath;
            _pendingSceneName = sceneName;
            _pendingIsNewGame = false;
            _pendingSaveName = null;
            _pendingGameMode = header.GameMode;

            BeginLobbyFlow();
            return false; // skip original
        }

        // ════════════════════════════════════════════
        //  NewGameMenu patches
        // ════════════════════════════════════════════

        [HarmonyPatch(typeof(NewGameMenu), "OnEnable")]
        [HarmonyPostfix]
        public static void NewGameMenu_OnEnable_Post(NewGameMenu __instance)
        {
            try { InjectToggle(__instance.transform, "NewGameMenu"); }
            catch (Exception ex) { Debug.LogError($"[MineMogulMP] NewGameMenu toggle inject: {ex}"); }
        }

        [HarmonyPatch(typeof(NewGameMenu), "OnConfirmNewGamePressed")]
        [HarmonyPrefix]
        public static bool NewGameMenu_OnConfirmNewGamePressed_Pre(NewGameMenu __instance)
        {
            if (!_multiplayerToggled) return true;

            var nameField = AccessTools.Field(typeof(NewGameMenu), "_newSaveFileNameInputField");
            var levelField = AccessTools.Field(typeof(NewGameMenu), "_selectedLevelInfo");
            var modeField = AccessTools.Field(typeof(NewGameMenu), "_selectedGameMode");
            if (nameField == null || levelField == null || modeField == null) return true;

            var inputField = nameField.GetValue(__instance) as TMP_InputField;
            var levelInfo = levelField.GetValue(__instance) as LevelInfo;
            if (inputField == null || levelInfo == null || string.IsNullOrEmpty(inputField.text)) return true;

            // Apply game mode before lobby (matches original behaviour)
            var gameMode = (GameModeType)modeField.GetValue(__instance);
            var gmm = Singleton<GamemodeManager>.Instance;
            if (gmm != null) gmm.GameModeType = gameMode;

            _pendingSaveName = inputField.text;
            _pendingSceneName = levelInfo.SceneName;
            _pendingIsNewGame = true;
            _pendingSaveFilePath = null;

            BeginLobbyFlow();
            return false;
        }

        // ════════════════════════════════════════════
        //  Toggle injection
        // ════════════════════════════════════════════

        private static void InjectToggle(Transform parent, string menuId)
        {
            string goName = $"MP_Toggle_{menuId}";
            if (parent.Find(goName) != null) return; // already injected

            UIFactory.HarvestGameAssets();

            // Container row
            var row = new GameObject(goName, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 8);
            rt.sizeDelta = new Vector2(-20, 36);

            var bg = row.AddComponent<Image>();
            bg.color = UIFactory.PanelBgLight;
            UIFactory.ApplyRounded(bg);

            // Subtle gold border
            var outline = row.AddComponent<Outline>();
            outline.effectColor = UIFactory.AccentGoldDim * new Color(1, 1, 1, 0.3f);
            outline.effectDistance = new Vector2(1, -1);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 4, 4);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Checkbox background
            var checkBgGo = new GameObject("ChkBg", typeof(RectTransform));
            checkBgGo.transform.SetParent(row.transform, false);
            var chkBgImg = checkBgGo.AddComponent<Image>();
            chkBgImg.color = UIFactory.RowBg;
            UIFactory.ApplyRounded(chkBgImg);
            var chkBgLe = checkBgGo.AddComponent<LayoutElement>();
            chkBgLe.preferredWidth = 22; chkBgLe.preferredHeight = 22;

            // Checkmark
            var chkGo = new GameObject("Chk", typeof(RectTransform));
            chkGo.transform.SetParent(checkBgGo.transform, false);
            var chkImg = chkGo.AddComponent<Image>();
            chkImg.color = UIFactory.StatusGreen;
            var chkRt = chkGo.GetComponent<RectTransform>();
            chkRt.anchorMin = new Vector2(0.18f, 0.18f);
            chkRt.anchorMax = new Vector2(0.82f, 0.82f);
            chkRt.offsetMin = chkRt.offsetMax = Vector2.zero;

            // Toggle component
            var toggle = checkBgGo.AddComponent<Toggle>();
            toggle.graphic = chkImg;
            toggle.targetGraphic = chkBgImg;
            toggle.isOn = _multiplayerToggled;

            // Label
            var label = UIFactory.CreateText(row.transform, "Lbl", "HOST MULTIPLAYER", 14, TextAlignmentOptions.MidlineLeft);
            label.fontStyle = FontStyles.Bold;
            label.color = _multiplayerToggled ? UIFactory.StatusGreen : UIFactory.AccentGold;
            label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            toggle.onValueChanged.AddListener((bool on) =>
            {
                _multiplayerToggled = on;
                label.color = on ? UIFactory.StatusGreen : UIFactory.AccentGold;
            });
        }

        // ════════════════════════════════════════════
        //  Lobby flow
        // ════════════════════════════════════════════

        private static void BeginLobbyFlow()
        {
            var session = SessionManager.Instance;
            if (session == null || !session.SteamReady)
            {
                ShowOverlayStatus("<color=#CC4444>Steam not connected.</color>");
                return;
            }

            if (session.Phase == SessionManager.LobbyPhase.None)
            {
                string name = Plugin.Instance?.PlayerName ?? "Host";
                session.CreateLobby(name);
            }

            ShowOverlay();

            // Start polling coroutine
            EnsurePoller();
            _poller.StartPolling();
        }

        // ════════════════════════════════════════════
        //  Overlay UI
        // ════════════════════════════════════════════

        private static void ShowOverlay()
        {
            if (_overlayRoot != null) { _overlayRoot.SetActive(true); RefreshPlayerList(); return; }

            UIFactory.HarvestGameAssets();

            // Canvas
            _overlayRoot = new GameObject("MP_MenuLobbyOverlay");
            UnityEngine.Object.DontDestroyOnLoad(_overlayRoot);
            var canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = _overlayRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _overlayRoot.AddComponent<GraphicRaycaster>();

            // Dim background
            var dimBg = new GameObject("DimBG", typeof(RectTransform));
            dimBg.transform.SetParent(_overlayRoot.transform, false);
            var dimImg = dimBg.AddComponent<Image>();
            dimImg.color = new Color(0, 0, 0, 0.70f);
            var dimRt = dimBg.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;

            // Center panel — matches MultiplayerPanel style
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(_overlayRoot.transform, false);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = UIFactory.PanelBg;
            UIFactory.ApplyRounded(panelImg);
            var pRt = panel.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0.5f, 0.5f);
            pRt.anchorMax = new Vector2(0.5f, 0.5f);
            pRt.pivot = new Vector2(0.5f, 0.5f);
            pRt.sizeDelta = new Vector2(440, 480);

            // Gold outline and glow
            var glow = panel.AddComponent<Shadow>();
            glow.effectColor = new Color(UIFactory.AccentGold.r, UIFactory.AccentGold.g, UIFactory.AccentGold.b, 0.18f);
            glow.effectDistance = new Vector2(0, -4f);
            var panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = UIFactory.AccentGoldDim * new Color(1, 1, 1, 0.35f);
            panelOutline.effectDistance = new Vector2(1, -1);

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 0;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // ── Header bar ──
            var header = UIFactory.CreatePanel(pRt, "Header", UIFactory.HeaderBg, false);
            var headerLe = header.gameObject.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 28; headerLe.minHeight = 28;
            var hhlg = UIFactory.AddHorizontalLayout(header, 0);
            hhlg.padding = new RectOffset(14, 14, 0, 0);
            hhlg.childAlignment = TextAnchor.MiddleLeft;

            var title = UIFactory.CreateText(header, "Title", "MULTIPLAYER LOBBY", 12, TextAlignmentOptions.MidlineLeft);
            title.fontStyle = FontStyles.Bold;
            title.color = UIFactory.AccentGold;
            title.characterSpacing = 2;
            title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var verText = UIFactory.CreateText(header, "Ver", $"v{PluginInfo.Version}", 9, TextAlignmentOptions.MidlineRight);
            verText.color = UIFactory.TextDim;
            verText.gameObject.AddComponent<LayoutElement>().preferredWidth = 40;

            // Gold accent line
            UIFactory.AddDivider(pRt, 2);

            // ── Status strip ──
            var statusBar = UIFactory.CreatePanel(pRt, "StatusBar", UIFactory.PanelBgLight, false);
            statusBar.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
            _statusText = UIFactory.CreateText(statusBar, "Status", "Creating lobby...", 11, TextAlignmentOptions.Center);
            _statusText.color = UIFactory.TextDim;
            var stRt = _statusText.GetComponent<RectTransform>();
            stRt.anchorMin = Vector2.zero; stRt.anchorMax = Vector2.one;
            stRt.offsetMin = new Vector2(10, 0); stRt.offsetMax = new Vector2(-10, 0);

            // ── Content area (padded) ──
            var content = UIFactory.CreatePanel(pRt, "Content", Color.clear);
            content.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            var cVlg = UIFactory.AddVerticalLayout(content, 16, 16, 12, 12, 8);

            // Players section
            var playersLabel = UIFactory.CreateText(content, "PlayersLabel", "PLAYERS", 12);
            playersLabel.fontStyle = FontStyles.Bold;
            playersLabel.color = UIFactory.TextColor;

            // Player list scroll area
            var (scroll, playerContent) = UIFactory.CreateScrollView(content, "PlayerScroll", 160);
            _playerListContent = playerContent;

            UIFactory.AddDivider(content);

            // Invite button
            var inviteBtn = UIFactory.CreateButton(content, "InviteBtn", "Invite Friends  (Steam Overlay)",
                UIFactory.ButtonSecondary, 13, 34);
            inviteBtn.onClick.AddListener(() =>
            {
                var lobby = SessionManager.Instance?.CurrentLobby;
                if (lobby.HasValue)
                    SteamFriends.OpenGameInviteOverlay(lobby.Value.Id);
            });

            UIFactory.AddSpacer(content, 4);

            // ── Button row ──
            var btnRow = new GameObject("BtnRow", typeof(RectTransform));
            btnRow.transform.SetParent(content, false);
            var btnRowLe = btnRow.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 36; btnRowLe.minHeight = 36;
            var bhlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            bhlg.spacing = 12;
            bhlg.childForceExpandWidth = true;
            bhlg.childForceExpandHeight = true;
            bhlg.childControlWidth = true;
            bhlg.childControlHeight = true;

            var cancelBtn = UIFactory.CreateButton(btnRow.GetComponent<RectTransform>(), "CancelBtn", "Cancel",
                UIFactory.ButtonDanger, 13, 36);
            cancelBtn.onClick.AddListener(OnCancelClicked);

            var launchBtn = UIFactory.CreateButton(btnRow.GetComponent<RectTransform>(), "LaunchBtn", "Launch Game",
                UIFactory.ButtonPrimary, 13, 36);
            launchBtn.onClick.AddListener(OnLaunchClicked);

            RefreshPlayerList();
        }

        private static void HideOverlay()
        {
            if (_overlayRoot != null) _overlayRoot.SetActive(false);
            if (_poller != null) _poller.StopPolling();
        }

        private static void OnLaunchClicked()
        {
            var session = SessionManager.Instance;
            if (session == null || session.Phase != SessionManager.LobbyPhase.InLobby)
            {
                ShowOverlayStatus("<color=#CC4444>Lobby not ready — wait a moment.</color>");
                return;
            }

            string playerName = Plugin.Instance?.PlayerName ?? "Host";

            if (_pendingIsNewGame)
                session.LaunchNewGame(_pendingSaveName, _pendingSceneName, playerName);
            else
                session.LaunchGame(_pendingSaveFilePath, _pendingSceneName, playerName, _pendingGameMode);

            ShowOverlayStatus("<color=#88CC88>Launching...</color>");
            HideOverlay();
        }

        private static void OnCancelClicked()
        {
            HideOverlay();
            var s = SessionManager.Instance;
            if (s != null && s.IsInLobby)
                s.LeaveLobbyAndReset();
        }

        // ════════════════════════════════════════════
        //  Player list refresh
        // ════════════════════════════════════════════

        internal static void RefreshPlayerList()
        {
            if (_playerListContent == null) return;
            foreach (var r in _playerRows) UnityEngine.Object.Destroy(r);
            _playerRows.Clear();

            var session = SessionManager.Instance;
            if (session == null) return;

            // Update lobby status text
            if (session.Phase == SessionManager.LobbyPhase.InLobby)
            {
                int count = session.LobbyPlayerNames.Count;
                ShowOverlayStatus($"Lobby ready \u2014 {count} player{(count != 1 ? "s" : "")}. Invite friends, then click Launch.");
            }
            else if (session.Phase == SessionManager.LobbyPhase.None)
            {
                ShowOverlayStatus("Creating lobby...");
            }

            for (int i = 0; i < session.LobbyPlayerNames.Count; i++)
            {
                var playerName = session.LobbyPlayerNames[i];
                var row = UIFactory.CreatePanel(_playerListContent, "Player", UIFactory.RowBg, true);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 32;
                var hlg = UIFactory.AddHorizontalLayout(row, 10);
                hlg.padding = new RectOffset(10, 10, 0, 0);
                hlg.childAlignment = TextAnchor.MiddleLeft;

                // Status dot
                var dot = UIFactory.CreatePanel(row, "Dot", UIFactory.StatusGreen);
                var dotLe = dot.gameObject.AddComponent<LayoutElement>();
                dotLe.preferredWidth = 8; dotLe.preferredHeight = 8; dotLe.minWidth = 8;

                var nameText = UIFactory.CreateText(row, "Name", playerName, 14);
                nameText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

                if (i == 0)
                {
                    var hostLabel = UIFactory.CreateText(row, "Host", "HOST", 11);
                    hostLabel.color = UIFactory.AccentGold;
                    hostLabel.fontStyle = FontStyles.Bold;
                    hostLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 40;
                }

                _playerRows.Add(row.gameObject);
            }
        }

        private static void ShowOverlayStatus(string text)
        {
            if (_statusText != null) _statusText.text = text;
        }

        // ════════════════════════════════════════════
        //  Scene name resolution (with fallback)
        // ════════════════════════════════════════════

        private static string ResolveSceneName(string levelId)
        {
            var levelMgr = Singleton<LevelManager>.Instance;
            if (levelMgr != null)
            {
                var info = levelMgr.GetLevelByID(levelId);
                if (info != null) return info.SceneName;
            }

            // Fallback: check build settings for Gameplay_{levelId} or {levelId}
            foreach (var candidate in new[] { $"Gameplay_{levelId}", levelId })
            {
                for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
                {
                    var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                    if (System.IO.Path.GetFileNameWithoutExtension(path)
                            .Equals(candidate, StringComparison.OrdinalIgnoreCase))
                        return candidate;
                }
            }
            return null;
        }

        // ════════════════════════════════════════════
        //  Coroutine poller (refreshes player list while overlay is visible)
        // ════════════════════════════════════════════

        private static void EnsurePoller()
        {
            if (_poller != null) return;
            var go = new GameObject("MP_LobbyPoller");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _poller = go.AddComponent<LobbyPoller>();
        }

        /// <summary>Tiny MB that polls lobby player changes while the overlay is open.</summary>
        private class LobbyPoller : MonoBehaviour
        {
            private Coroutine _co;

            public void StartPolling()
            {
                if (_co != null) StopCoroutine(_co);
                _co = StartCoroutine(PollLoop());
            }

            public void StopPolling()
            {
                if (_co != null) { StopCoroutine(_co); _co = null; }
            }

            private IEnumerator PollLoop()
            {
                int lastCount = -1;
                while (true)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    var s = SessionManager.Instance;
                    if (s == null) continue;
                    s.RefreshLobbyPlayers();
                    if (s.LobbyPlayerNames.Count != lastCount)
                    {
                        lastCount = s.LobbyPlayerNames.Count;
                        RefreshPlayerList();
                    }
                }
            }
        }
    }
}
