using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using MineMogulMultiplayer.Core;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MineMogulMultiplayer.UI
{
    /// <summary>
    /// Lobby-first multiplayer panel. Accessible from main menu (F9), pause menu, or Multiplayer button.
    /// Flow: Create/Join lobby → see player list → host launches save → everyone loads in.
    /// </summary>
    public class MultiplayerPanel : MonoBehaviour
    {
        private ManualLogSource _log;
        private SessionManager _session;
        private ConfigEntry<string> _cfgPlayerName;

        // Root objects
        private Canvas _canvas;
        private GameObject _root;

        // Status bar
        private TextMeshProUGUI _steamStatusText;

        // Pages (only one visible at a time)
        private GameObject _pageOffline;    // Create/Join lobby
        private GameObject _pageLobby;      // In lobby (player list, save picker for host, waiting for client)
        private GameObject _pageInGame;     // In game (status + stop)

        // Offline page
        private Button _createLobbyBtn;
        private TMP_InputField _steamIdInput;
        private Button _joinBtn;
        private TextMeshProUGUI _offlineStatus;
        private RectTransform _friendsContent;
        private readonly List<GameObject> _friendRows = new List<GameObject>();

        // Lobby page
        private TextMeshProUGUI _lobbyTitle;
        private RectTransform _playerListContent;
        private readonly List<GameObject> _playerRows = new List<GameObject>();
        private Button _inviteBtn;
        private Button _leaveLobbyBtn;
        private GameObject _savePickerSection;       // host only
        private RectTransform _saveListContent;
        private readonly List<GameObject> _saveRows = new List<GameObject>();
        private TextMeshProUGUI _lobbyStatus;

        // In-game page
        private TextMeshProUGUI _inGameStatus;
        private Button _stopBtn;
        private Button _debugBotBtn;
        private TextMeshProUGUI _debugBotBtnLabel;
        private TextMeshProUGUI _debugInfoText;

        // Debug overlay (always visible during gameplay when toggled)
        private Canvas _debugOverlayCanvas;
        private TextMeshProUGUI _debugOverlayText;
        private bool _debugOverlayVisible;

        // State
        private bool _visible;
        private float _refreshTimer;
        private const float RefreshInterval = 2f;
        private bool _saveListDirty = true;

        public bool IsVisible => _visible;

        public void Init(ManualLogSource log, SessionManager session, ConfigEntry<string> cfgPlayerName)
        {
            _log = log;
            _session = session;
            _cfgPlayerName = cfgPlayerName;

            _session.OnLobbyPlayersChanged += OnLobbyPlayersChanged;
            _session.OnLobbyLaunched += OnLobbyLaunched;

            BuildUI();
            Hide();
        }

        // ── Show / Hide ──

        public void Show()
        {
            if (_canvas == null) return;
            _visible = true;
            _saveListDirty = true;
            _canvas.gameObject.SetActive(true);
            RefreshAll();
        }

        public void Hide()
        {
            if (_canvas == null) return;
            _visible = false;
            _canvas.gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (_visible) Hide(); else Show();
        }

        // ── Unity lifecycle ──

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
                Toggle();

            // F10 toggles debug overlay
            if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
                ToggleDebugOverlay();

            // Update debug overlay every frame if visible
            if (_debugOverlayVisible && _debugOverlayCanvas != null && _session != null)
                _debugOverlayText.text = _session.GetDebugInfo();

            if (!_visible) return;

            _refreshTimer += Time.unscaledDeltaTime;
            if (_refreshTimer >= RefreshInterval)
            {
                _refreshTimer = 0;
                RefreshAll();
            }
        }

        private void OnDestroy()
        {
            if (_session != null)
            {
                _session.OnLobbyPlayersChanged -= OnLobbyPlayersChanged;
                _session.OnLobbyLaunched -= OnLobbyLaunched;
            }
            if (_debugOverlayCanvas != null)
                Destroy(_debugOverlayCanvas.gameObject);
        }

        // ── Build entire UI ──

        private void BuildUI()
        {
            // Try to harvest game assets (font, sprites) for native look
            UIFactory.HarvestGameAssets();

            _canvas = UIFactory.CreateScreenCanvas("MP_Canvas", 150);

            // Backdrop (click to close)
            var backdrop = UIFactory.CreatePanel(_canvas.transform, "Backdrop", new Color(0, 0, 0, 0.55f));
            Stretch(backdrop);
            var backdropBtn = backdrop.gameObject.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(Hide);

            // Main panel — centered, compact 420×560
            _root = new GameObject("MP_Panel", typeof(RectTransform));
            _root.transform.SetParent(_canvas.transform, false);
            var rootRt = _root.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.sizeDelta = new Vector2(420, 560);
            var rootImg = _root.AddComponent<Image>();
            rootImg.color = UIFactory.PanelBg;
            UIFactory.ApplyRounded(rootImg);
            UIFactory.AddVerticalLayout(rootRt, 0, 0, 0, 0, 0);

            // Ambient glow
            var glow = _root.AddComponent<Shadow>();
            glow.effectColor = new Color(UIFactory.AccentGold.r, UIFactory.AccentGold.g, UIFactory.AccentGold.b, 0.18f);
            glow.effectDistance = new Vector2(0, -4f);
            var outline = _root.AddComponent<Outline>();
            outline.effectColor = UIFactory.AccentGoldDim * new Color(1, 1, 1, 0.35f);
            outline.effectDistance = new Vector2(1, -1);

            // Header bar
            BuildHeader(rootRt);

            // Steam status strip
            var statusBar = UIFactory.CreatePanel(rootRt, "StatusBar", UIFactory.PanelBgLight, true);
            var statusLe = statusBar.gameObject.AddComponent<LayoutElement>();
            statusLe.preferredHeight = 22; statusLe.minHeight = 22;
            _steamStatusText = UIFactory.CreateText(statusBar, "SteamLabel", "", 10, TextAlignmentOptions.Center);
            Stretch(_steamStatusText.GetComponent<RectTransform>());

            // Gold accent line under header
            UIFactory.AddDivider(rootRt, 2);

            // Pages container (fills remaining space)
            var pages = UIFactory.CreatePanel(rootRt, "Pages", Color.clear);
            pages.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

            _pageOffline = BuildOfflinePage(pages);
            _pageLobby = BuildLobbyPage(pages);
            _pageInGame = BuildInGamePage(pages);
        }

        private void BuildHeader(RectTransform parent)
        {
            var header = UIFactory.CreatePanel(parent, "Header", UIFactory.HeaderBg, false);
            var headerLe = header.gameObject.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 24; headerLe.minHeight = 24; headerLe.flexibleHeight = 0;
            var hlg = UIFactory.AddHorizontalLayout(header, 0);
            hlg.padding = new RectOffset(8, 4, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var title = UIFactory.CreateText(header, "Title", "MULTIPLAYER", 11, TextAlignmentOptions.MidlineLeft);
            title.fontStyle = FontStyles.Bold;
            title.color = UIFactory.AccentGold;
            title.characterSpacing = 2;
            var titleLe = title.gameObject.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1;
            titleLe.preferredHeight = 24; titleLe.minHeight = 24;

            // Version label next to title
            var verText = UIFactory.CreateText(header, "Version", $"v{PluginInfo.Version}", 9, TextAlignmentOptions.MidlineRight);
            verText.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            var verLe = verText.gameObject.AddComponent<LayoutElement>();
            verLe.preferredWidth = 40; verLe.preferredHeight = 24; verLe.flexibleWidth = 0;

            // Small X close button — plain text style
            var closeBtnGo = new GameObject("CloseBtn", typeof(RectTransform));
            closeBtnGo.transform.SetParent(header, false);
            var closeLe = closeBtnGo.AddComponent<LayoutElement>();
            closeLe.preferredWidth = 24; closeLe.preferredHeight = 24;
            closeLe.minHeight = 24; closeLe.flexibleWidth = 0;
            var closeImg = closeBtnGo.AddComponent<Image>();
            closeImg.color = new Color(1f, 1f, 1f, 0f); // invisible bg
            var closeBtn = closeBtnGo.AddComponent<Button>();
            closeBtn.transition = Selectable.Transition.ColorTint;
            closeBtn.targetGraphic = closeImg;
            var cb = closeBtn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.3f, 0.3f, 0.3f);
            cb.pressedColor = new Color(1f, 0.2f, 0.2f, 0.5f);
            closeBtn.colors = cb;
            closeBtn.onClick.AddListener(Hide);
            var closeTxt = UIFactory.CreateText(closeBtnGo.transform, "X", "\u00D7", 16, TextAlignmentOptions.Center);
            closeTxt.color = new Color(0.8f, 0.8f, 0.8f);
            var closeTxtRt = closeTxt.GetComponent<RectTransform>();
            closeTxtRt.anchorMin = Vector2.zero; closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.offsetMin = Vector2.zero; closeTxtRt.offsetMax = Vector2.zero;
        }

        // ── OFFLINE PAGE (Create/Join) ──

        /// <summary>
        /// Creates a page container that fills the parent and scrolls its content vertically.
        /// Returns the page root (for SetActive toggling) and the content transform to add elements to.
        /// </summary>
        private (GameObject page, RectTransform content) BuildScrollablePage(RectTransform parent, string name,
            int padL = 12, int padR = 12, int padT = 10, int padB = 8, float spacing = 4)
        {
            var page = new GameObject(name, typeof(RectTransform));
            page.transform.SetParent(parent, false);
            Stretch(page.GetComponent<RectTransform>());
            page.AddComponent<RectMask2D>();

            // Scrollable content that grows to fit children
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(page.transform, false);
            var cRt = content.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 1);
            cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(0.5f, 1);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(padL, padR, padT, padB);
            vlg.spacing = spacing;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = page.AddComponent<ScrollRect>();
            scroll.content = cRt;
            scroll.viewport = page.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            return (page, cRt);
        }

        private GameObject BuildOfflinePage(RectTransform parent)
        {
            var (page, rt) = BuildScrollablePage(parent, "OfflinePage", 12, 12, 10, 8, 4);

            // Host section
            var hostLabel = UIFactory.CreateText(rt, "HostLabel", "HOST A GAME", 13);
            hostLabel.fontStyle = FontStyles.Bold;
            hostLabel.color = UIFactory.AccentGold;
            UIFactory.CreateText(rt, "HostDesc",
                "Create a lobby, invite friends, then launch a save.", 11).color = UIFactory.TextDim;

            _createLobbyBtn = UIFactory.CreateButton(rt, "CreateLobbyBtn", "Create Lobby",
                UIFactory.ButtonNormal, 13, 32);
            _createLobbyBtn.onClick.AddListener(OnCreateLobbyPressed);

            UIFactory.AddDivider(rt);

            // Join section
            var joinLabel = UIFactory.CreateText(rt, "JoinLabel", "JOIN A GAME", 13);
            joinLabel.fontStyle = FontStyles.Bold;
            joinLabel.color = UIFactory.AccentGold;
            UIFactory.CreateText(rt, "JoinDesc",
                "Enter the host's Steam ID, or accept a Steam invite.", 11).color = UIFactory.TextDim;

            _steamIdInput = UIFactory.CreateInputField(rt, "SteamIdInput",
                "e.g. 76561198012345678", 12, 30);
            _steamIdInput.contentType = TMP_InputField.ContentType.IntegerNumber;

            _joinBtn = UIFactory.CreateButton(rt, "JoinBtn", "Join Lobby",
                UIFactory.ButtonPrimary, 13, 30);
            _joinBtn.onClick.AddListener(OnJoinPressed);

            _offlineStatus = UIFactory.CreateText(rt, "Status", "", 11);
            _offlineStatus.color = UIFactory.TextDim;

            UIFactory.AddDivider(rt);

            // Friends online
            var friendsLabel = UIFactory.CreateText(rt, "FriendsLabel", "FRIENDS ONLINE", 12);
            friendsLabel.fontStyle = FontStyles.Bold;
            friendsLabel.color = UIFactory.TextColor;
            var (_, content) = UIFactory.CreateScrollView(rt, "FriendsList", 140);
            _friendsContent = content;

            return page;
        }

        // ── LOBBY PAGE (Player list + save picker for host) ──

        private GameObject BuildLobbyPage(RectTransform parent)
        {
            var (page, rt) = BuildScrollablePage(parent, "LobbyPage", 14, 14, 12, 10, 6);

            _lobbyTitle = UIFactory.CreateText(rt, "LobbyTitle", "LOBBY", 15);
            _lobbyTitle.fontStyle = FontStyles.Bold;
            _lobbyTitle.color = UIFactory.AccentGold;

            _lobbyStatus = UIFactory.CreateText(rt, "LobbyStatus", "", 12);
            _lobbyStatus.color = UIFactory.TextDim;

            UIFactory.AddDivider(rt);

            // Player list
            var playersLabel = UIFactory.CreateText(rt, "PlayersLabel", "PLAYERS", 12);
            playersLabel.fontStyle = FontStyles.Bold;
            playersLabel.color = UIFactory.TextColor;
            var (_, playerContent) = UIFactory.CreateScrollView(rt, "PlayerList", 70);
            _playerListContent = playerContent;

            // Invite button
            _inviteBtn = UIFactory.CreateButton(rt, "InviteBtn", "Invite Friends (Steam Overlay)",
                UIFactory.ButtonSecondary, 12, 28);
            _inviteBtn.onClick.AddListener(OnInvitePressed);

            UIFactory.AddDivider(rt);

            // Save picker section (host only)
            _savePickerSection = new GameObject("SavePicker", typeof(RectTransform));
            _savePickerSection.transform.SetParent(rt, false);
            var spRt = _savePickerSection.GetComponent<RectTransform>();
            _savePickerSection.AddComponent<LayoutElement>().flexibleHeight = 1;
            UIFactory.AddVerticalLayout(spRt, 0, 0, 0, 0, 4);

            var saveLabel = UIFactory.CreateText(spRt, "SaveLabel", "SELECT A SAVE", 12);
            saveLabel.fontStyle = FontStyles.Bold;
            saveLabel.color = UIFactory.TextColor;

            var refreshBtn = UIFactory.CreateButton(spRt, "RefreshSaves", "Refresh",
                UIFactory.ButtonSecondary, 11, 24);
            refreshBtn.onClick.AddListener(RefreshSaveList);

            var (_, saveContent) = UIFactory.CreateScrollView(spRt, "SavesList", 130);
            _saveListContent = saveContent;

            // Leave button
            UIFactory.AddSpacer(rt, 2);
            _leaveLobbyBtn = UIFactory.CreateButton(rt, "LeaveBtn", "Leave Lobby",
                UIFactory.ButtonDanger, 13, 32);
            _leaveLobbyBtn.onClick.AddListener(OnLeaveLobbyPressed);

            return page;
        }

        // ── IN-GAME PAGE ──

        private GameObject BuildInGamePage(RectTransform parent)
        {
            var (page, rt) = BuildScrollablePage(parent, "InGamePage", 14, 14, 14, 10, 4);

            _inGameStatus = UIFactory.CreateText(rt, "Status", "", 15);
            _inGameStatus.fontStyle = FontStyles.Bold;

            UIFactory.CreateText(rt, "Desc",
                "Session is active. Manage your connection below.", 11).color = UIFactory.TextDim;

            UIFactory.AddDivider(rt);

            // Invite button (in-game)
            var invBtn = UIFactory.CreateButton(rt, "InviteInGame", "Invite Friends (Steam Overlay)",
                UIFactory.ButtonSecondary, 12, 28);
            invBtn.onClick.AddListener(OnInvitePressed);

            UIFactory.AddSpacer(rt, 2);

            _stopBtn = UIFactory.CreateButton(rt, "StopBtn", "Disconnect / Stop",
                UIFactory.ButtonDanger, 13, 32);
            _stopBtn.onClick.AddListener(OnStopPressed);

            // ── Debug Tools Section ──
            UIFactory.AddSpacer(rt, 4);
            UIFactory.AddDivider(rt);
            var debugLabel = UIFactory.CreateText(rt, "DebugHeader", "DEBUG TOOLS", 11);
            debugLabel.fontStyle = FontStyles.Bold;
            debugLabel.color = UIFactory.AccentGoldDim;
            debugLabel.characterSpacing = 3;

            // Test Bot button
            _debugBotBtn = UIFactory.CreateButton(rt, "DebugBotBtn", "Spawn Test Bot",
                new Color(0.18f, 0.35f, 0.50f, 1f), 11, 26);
            _debugBotBtnLabel = _debugBotBtn.GetComponentInChildren<TextMeshProUGUI>();
            _debugBotBtn.onClick.AddListener(OnDebugBotPressed);

            // Force Resync button
            var resyncBtn = UIFactory.CreateButton(rt, "ResyncBtn", "Force Resync",
                new Color(0.35f, 0.30f, 0.18f, 1f), 11, 26);
            resyncBtn.onClick.AddListener(OnForceResyncPressed);

            // Dump Snapshot button
            var dumpBtn = UIFactory.CreateButton(rt, "DumpBtn", "Dump Snapshot to Log",
                new Color(0.35f, 0.30f, 0.18f, 1f), 11, 26);
            dumpBtn.onClick.AddListener(OnDumpSnapshotPressed);

            // Toggle overlay button
            var overlayBtn = UIFactory.CreateButton(rt, "OverlayBtn", "Toggle Stats Overlay (F10)",
                new Color(0.35f, 0.30f, 0.18f, 1f), 11, 26);
            overlayBtn.onClick.AddListener(ToggleDebugOverlay);

            // Add Money button — useful for testing purchases in multiplayer
            var moneyBtn = UIFactory.CreateButton(rt, "MoneyBtn", "Add $1000",
                new Color(0.18f, 0.42f, 0.18f, 1f), 11, 26);
            moneyBtn.onClick.AddListener(OnAddMoneyPressed);

            // Live debug info text
            UIFactory.AddSpacer(rt, 2);
            _debugInfoText = UIFactory.CreateText(rt, "DebugInfo", "", 9);
            _debugInfoText.color = UIFactory.TextDim;
            _debugInfoText.textWrappingMode = TMPro.TextWrappingModes.Normal;

            return page;
        }

        // ── Button callbacks ──

        private void OnCreateLobbyPressed()
        {
            if (_session == null || !_session.SteamReady)
            {
                _offlineStatus.text = "<color=#CC4444>Steam not connected.</color>";
                return;
            }
            _session.CreateLobby(_cfgPlayerName.Value);
            _offlineStatus.text = "Creating lobby...";
        }

        private void OnJoinPressed()
        {
            if (_session == null || !_session.SteamReady)
            {
                _offlineStatus.text = "<color=#CC4444>Steam not connected.</color>";
                return;
            }
            var text = _steamIdInput.text.Trim();
            if (ulong.TryParse(text, out ulong steamId))
            {
                _session.JoinLobbyByHostId(steamId, _cfgPlayerName.Value);
                _offlineStatus.text = $"Searching for lobby...";
            }
            else
            {
                _offlineStatus.text = "<color=#CC4444>Invalid Steam ID.</color>";
            }
        }

        private void OnInvitePressed()
        {
            if (!SteamClient.IsValid) return;
            var lobby = _session?.CurrentLobby;
            if (lobby.HasValue)
            {
                SteamFriends.OpenGameInviteOverlay(lobby.Value.Id);
            }
            else
            {
                _log?.LogWarning("[UI] No lobby to invite to");
            }
        }

        private void OnLeaveLobbyPressed()
        {
            _session?.LeaveLobbyAndReset();
            _saveListDirty = true;
            RefreshAll();
        }

        private void OnStopPressed()
        {
            _session?.Stop();
            RefreshAll();
        }

        private void OnDebugBotPressed()
        {
            _session?.ToggleDebugBot();
            UpdateDebugBotButton();
        }

        private void OnForceResyncPressed()
        {
            _session?.ForceResync();
        }

        private void OnDumpSnapshotPressed()
        {
            _session?.DumpSnapshot();
        }

        private void OnAddMoneyPressed()
        {
            _session?.AddDebugMoney(1000f);
        }

        private void ToggleDebugOverlay()
        {
            _debugOverlayVisible = !_debugOverlayVisible;
            if (_debugOverlayVisible && _debugOverlayCanvas == null)
                BuildDebugOverlay();
            if (_debugOverlayCanvas != null)
                _debugOverlayCanvas.gameObject.SetActive(_debugOverlayVisible);
        }

        private void BuildDebugOverlay()
        {
            // Small always-on-top overlay in the top-left corner
            var canvasGo = new GameObject("MP_DebugOverlay");
            canvasGo.transform.SetParent(transform, false);
            _debugOverlayCanvas = canvasGo.AddComponent<Canvas>();
            _debugOverlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _debugOverlayCanvas.sortingOrder = 200;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

            // Background panel
            var bg = new GameObject("BG", typeof(RectTransform));
            bg.transform.SetParent(canvasGo.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 1);
            bgRt.anchorMax = new Vector2(0, 1);
            bgRt.pivot = new Vector2(0, 1);
            bgRt.anchoredPosition = new Vector2(10, -10);
            bgRt.sizeDelta = new Vector2(420, 160);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.04f, 0.03f, 0.85f);
            UIFactory.ApplyRounded(bgImg);

            // Text
            _debugOverlayText = UIFactory.CreateText(bg.transform, "DebugText", "", 11);
            _debugOverlayText.color = new Color(0.7f, 0.9f, 0.6f, 1f); // green-tint monospace feel
            _debugOverlayText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            _debugOverlayText.overflowMode = TextOverflowModes.Truncate;
            var textRt = _debugOverlayText.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8, 6);
            textRt.offsetMax = new Vector2(-8, -6);
        }

        private void UpdateDebugBotButton()
        {
            if (_debugBotBtnLabel == null || _session == null) return;
            _debugBotBtnLabel.text = _session.IsDebugBotActive ? "Remove Test Bot" : "Spawn Test Bot";
        }

        private void OnSaveSelected(string fullFilePath, string levelId)
        {
            if (_session == null || !_session.SteamReady || _session.Phase != SessionManager.LobbyPhase.InLobby) return;

            // Resolve scene name
            string sceneName = null;
            var levelMgr = Singleton<LevelManager>.Instance;
            if (levelMgr != null)
            {
                var levelInfo = levelMgr.GetLevelByID(levelId);
                if (levelInfo != null)
                    sceneName = levelInfo.SceneName;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                _lobbyStatus.text = $"<color=#CC4444>Could not resolve scene for '{levelId}'</color>";
                return;
            }

            _session.LaunchGame(fullFilePath, sceneName, _cfgPlayerName.Value);
            _lobbyStatus.text = $"<color=#{ColorUtility.ToHtmlStringRGB(UIFactory.StatusGreen)}>Launching game...</color>";
            Hide();
        }

        private void JoinFriend(ulong steamId)
        {
            if (_session == null || !_session.SteamReady) return;
            _session.JoinLobbyByHostId(steamId, _cfgPlayerName.Value);
            _offlineStatus.text = $"Searching for lobby...";
        }

        // ── Event callbacks ──

        private void OnLobbyPlayersChanged()
        {
            if (_visible) RefreshPlayerList();
        }

        private void OnLobbyLaunched()
        {
            Hide();
        }

        // ── Refresh ──

        private void RefreshAll()
        {
            bool steamOk = _session != null && _session.SteamReady;

            // Steam status bar
            if (steamOk)
                _steamStatusText.text = $"Steam: {SteamClient.Name}  \u00b7  ID: {SteamClient.SteamId}";
            else
                _steamStatusText.text = "Steam: NOT CONNECTED";
            _steamStatusText.color = steamOk ? UIFactory.FriendOnline : UIFactory.ButtonDanger;

            // Show correct page
            var phase = _session?.Phase ?? SessionManager.LobbyPhase.None;

            _pageOffline.SetActive(phase == SessionManager.LobbyPhase.None);
            _pageLobby.SetActive(phase == SessionManager.LobbyPhase.InLobby);
            _pageInGame.SetActive(phase == SessionManager.LobbyPhase.InGame || phase == SessionManager.LobbyPhase.Launching);

            if (phase == SessionManager.LobbyPhase.None)
            {
                _createLobbyBtn.interactable = steamOk;
                _joinBtn.interactable = steamOk;
                RefreshFriendsList();
            }
            else if (phase == SessionManager.LobbyPhase.InLobby)
            {
                RefreshLobbyPage();
            }
            else if (phase == SessionManager.LobbyPhase.Launching)
            {
                _inGameStatus.text = $"<color=#{ColorUtility.ToHtmlStringRGB(UIFactory.AccentGold)}>Loading game...</color>";
            }
            else if (phase == SessionManager.LobbyPhase.InGame)
            {
                bool isHost = MultiplayerState.IsHost;
                _inGameStatus.text = isHost
                    ? $"<color=#{ColorUtility.ToHtmlStringRGB(UIFactory.StatusGreen)}>HOSTING</color>"
                    : $"<color=#{ColorUtility.ToHtmlStringRGB(UIFactory.FriendOnline)}>CONNECTED</color>  as client";

                // Update debug info in panel
                UpdateDebugBotButton();
                if (_debugInfoText != null && _session != null)
                    _debugInfoText.text = _session.GetDebugInfo();
            }
        }

        private void RefreshLobbyPage()
        {
            RefreshPlayerList();

            bool isLobbyOwner = _session?.CurrentLobby?.Owner.Id == SteamClient.SteamId;
            _savePickerSection.SetActive(isLobbyOwner);

            if (isLobbyOwner)
            {
                _lobbyTitle.text = "YOUR LOBBY";
                _lobbyStatus.text = "Select a save and launch when everyone is ready.";
                RefreshSaveList();
            }
            else
            {
                _lobbyTitle.text = "LOBBY";
                _lobbyStatus.text = "Waiting for the host to launch the game...";
            }
        }

        private void RefreshPlayerList()
        {
            foreach (var row in _playerRows)
                Destroy(row);
            _playerRows.Clear();

            if (_session == null) return;
            var names = _session.LobbyPlayerNames;
            for (int i = 0; i < names.Count; i++)
            {
                var row = UIFactory.CreatePanel(_playerListContent, "Player", UIFactory.RowBg, true);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
                UIFactory.AddHorizontalLayout(row, 10).childAlignment = TextAnchor.MiddleLeft;

                // Status dot
                var dot = UIFactory.CreatePanel(row, "Dot", UIFactory.StatusGreen);
                var dotLe = dot.gameObject.AddComponent<LayoutElement>();
                dotLe.preferredWidth = 8; dotLe.preferredHeight = 8; dotLe.minWidth = 8;

                var nameText = UIFactory.CreateText(row, "Name", names[i], 14);
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

        // ── Save file list ──

        private void RefreshSaveList()
        {
            if (_saveListContent == null) return;
            if (!_saveListDirty) return;
            _saveListDirty = false;

            foreach (var row in _saveRows) Destroy(row);
            _saveRows.Clear();

            try
            {
                var saves = SavingLoadingManager.GetAllSaveFileHeaderFileCombos();

                if (saves == null || saves.Count == 0)
                {
                    var empty = UIFactory.CreateText(_saveListContent, "NoSaves", "No save files found.", 14, TextAlignmentOptions.Center);
                    empty.color = UIFactory.TextDim;
                    _saveRows.Add(empty.gameObject);
                    return;
                }

                foreach (var combo in saves)
                    _saveRows.Add(BuildSaveRow(combo));
            }
            catch (Exception ex)
            {
                _log?.LogError($"[UI] Failed to list saves: {ex}");
                var err = UIFactory.CreateText(_saveListContent, "Error", $"Error: {ex.Message}", 12);
                err.color = UIFactory.ButtonDanger;
                _saveRows.Add(err.gameObject);
            }
        }

        private GameObject BuildSaveRow(SaveFileHeaderFileCombo combo)
        {
            var header = combo.SaveFileHeader;
            var row = UIFactory.CreatePanel(_saveListContent, "Save", UIFactory.RowBg, true);
            var rowLe = row.gameObject.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 50; rowLe.minHeight = 50;
            UIFactory.AddHorizontalLayout(row, 10).childAlignment = TextAnchor.MiddleLeft;

            var infoCol = new GameObject("Info", typeof(RectTransform));
            infoCol.transform.SetParent(row, false);
            infoCol.AddComponent<LayoutElement>().flexibleWidth = 1;
            UIFactory.AddVerticalLayout(infoCol.GetComponent<RectTransform>(), 6, 6, 4, 4, 1);

            var nameText = UIFactory.CreateText(infoCol.GetComponent<RectTransform>(), "Name", header.SaveFileName, 14);
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = UIFactory.AccentGold;

            var detail = $"${header.Money:N0}  \u00b7  {header.LevelID}  \u00b7  {header.SaveTimestamp}";
            UIFactory.CreateText(infoCol.GetComponent<RectTransform>(), "Detail", detail, 10).color = UIFactory.TextDim;

            var launchBtn = UIFactory.CreateButton(row, "LaunchBtn", "LAUNCH",
                UIFactory.ButtonNormal, 13, 38);
            var lLe = launchBtn.gameObject.GetComponent<LayoutElement>();
            lLe.preferredWidth = 76; lLe.flexibleWidth = 0;

            string filePath = combo.FullFilePath;
            string levelId = header.LevelID;
            launchBtn.onClick.AddListener(() => OnSaveSelected(filePath, levelId));

            return row.gameObject;
        }

        // ── Friends list ──

        private void RefreshFriendsList()
        {
            if (!SteamClient.IsValid || _friendsContent == null) return;

            foreach (var row in _friendRows) Destroy(row);
            _friendRows.Clear();

            foreach (var friend in SteamFriends.GetFriends())
            {
                bool inGame = friend.IsPlayingThisGame;
                bool online = friend.IsOnline || friend.IsAway || friend.IsBusy;
                if (!online && !inGame) continue;

                var row = UIFactory.CreatePanel(_friendsContent, "Friend", UIFactory.RowBg, true);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 32;
                UIFactory.AddHorizontalLayout(row, 8).childAlignment = TextAnchor.MiddleLeft;

                var dot = UIFactory.CreatePanel(row, "Dot", inGame ? UIFactory.StatusGreen : UIFactory.FriendOnline);
                var dotLe = dot.gameObject.AddComponent<LayoutElement>();
                dotLe.preferredWidth = 6; dotLe.preferredHeight = 6; dotLe.minWidth = 6;

                var nameText = UIFactory.CreateText(row, "Name", friend.Name, 13);
                nameText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
                nameText.color = inGame ? UIFactory.StatusGreen : UIFactory.FriendOnline;

                if (inGame)
                {
                    var joinBtn = UIFactory.CreateButton(row, "JoinBtn", "Join", UIFactory.ButtonPrimary, 11, 24);
                    joinBtn.gameObject.GetComponent<LayoutElement>().preferredWidth = 46;
                    ulong id = friend.Id;
                    joinBtn.onClick.AddListener(() => JoinFriend(id));
                }
                else
                {
                    var invBtn = UIFactory.CreateButton(row, "InviteBtn", "Invite", UIFactory.ButtonSecondary, 11, 24);
                    invBtn.gameObject.GetComponent<LayoutElement>().preferredWidth = 50;
                }

                _friendRows.Add(row.gameObject);
            }
        }

        // ── Helpers ──

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
