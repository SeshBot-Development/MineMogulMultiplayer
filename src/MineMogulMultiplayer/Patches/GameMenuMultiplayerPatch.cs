using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MineMogulMultiplayer.Core;
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

            // Container row
            var row = new GameObject(goName, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 8);
            rt.sizeDelta = new Vector2(-20, 32);

            var bg = row.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.17f, 0.92f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 3, 3);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Checkbox background
            var checkBgGo = new GameObject("ChkBg", typeof(RectTransform));
            checkBgGo.transform.SetParent(row.transform, false);
            var chkBgImg = checkBgGo.AddComponent<Image>();
            chkBgImg.color = new Color(0.22f, 0.22f, 0.28f, 1f);
            var chkBgLe = checkBgGo.AddComponent<LayoutElement>();
            chkBgLe.preferredWidth = 20; chkBgLe.preferredHeight = 20;

            // Checkmark
            var chkGo = new GameObject("Chk", typeof(RectTransform));
            chkGo.transform.SetParent(checkBgGo.transform, false);
            var chkImg = chkGo.AddComponent<Image>();
            chkImg.color = new Color(0.35f, 0.82f, 0.35f, 1f);
            var chkRt = chkGo.GetComponent<RectTransform>();
            chkRt.anchorMin = new Vector2(0.15f, 0.15f);
            chkRt.anchorMax = new Vector2(0.85f, 0.85f);
            chkRt.offsetMin = chkRt.offsetMax = Vector2.zero;

            // Toggle component (on the checkbox bg)
            var toggle = checkBgGo.AddComponent<Toggle>();
            toggle.graphic = chkImg;
            toggle.targetGraphic = chkBgImg;
            toggle.isOn = _multiplayerToggled;

            // Label
            var labelGo = new GameObject("Lbl", typeof(RectTransform));
            labelGo.transform.SetParent(row.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = "HOST MULTIPLAYER";
            label.fontSize = 13;
            label.fontStyle = FontStyles.Bold;
            label.color = _multiplayerToggled
                ? new Color(0.35f, 0.82f, 0.35f, 1f)
                : new Color(0.85f, 0.8f, 0.45f, 1f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            labelGo.AddComponent<LayoutElement>().flexibleWidth = 1;

            toggle.onValueChanged.AddListener((bool on) =>
            {
                _multiplayerToggled = on;
                label.color = on
                    ? new Color(0.35f, 0.82f, 0.35f, 1f)
                    : new Color(0.85f, 0.8f, 0.45f, 1f);
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

            // Canvas
            _overlayRoot = new GameObject("MP_MenuLobbyOverlay");
            UnityEngine.Object.DontDestroyOnLoad(_overlayRoot);
            var canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = _overlayRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            _overlayRoot.AddComponent<GraphicRaycaster>();

            // Dim background
            MakeStretchedImage(_overlayRoot.transform, "DimBG", new Color(0, 0, 0, 0.65f));

            // Center panel
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(_overlayRoot.transform, false);
            panel.AddComponent<Image>().color = new Color(0.10f, 0.10f, 0.14f, 0.97f);
            var pRt = panel.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0.3f, 0.2f);
            pRt.anchorMax = new Vector2(0.7f, 0.8f);
            pRt.offsetMin = pRt.offsetMax = Vector2.zero;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 10;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            // Title
            AddText(pRt, "MULTIPLAYER LOBBY", 22, FontStyles.Bold, new Color(0.85f, 0.8f, 0.45f))
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 30;

            // Status
            _statusText = AddText(pRt, "Creating lobby...", 13, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));
            _statusText.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;

            // Player list scroll area
            var scrollGo = new GameObject("Scroll", typeof(RectTransform));
            scrollGo.transform.SetParent(pRt, false);
            scrollGo.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.10f, 1f);
            scrollGo.AddComponent<LayoutElement>().flexibleHeight = 1;
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.vertical = true; scroll.horizontal = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(scrollGo.transform, false);
            _playerListContent = content.transform;
            var cRt = content.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 1); cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(0.5f, 1); cRt.sizeDelta = Vector2.zero;
            var cVlg = content.AddComponent<VerticalLayoutGroup>();
            cVlg.spacing = 3; cVlg.padding = new RectOffset(6, 6, 6, 6);
            cVlg.childForceExpandWidth = true; cVlg.childForceExpandHeight = false;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = cRt;

            // Invite button
            AddButton(pRt, "INVITE FRIEND", new Color(0.22f, 0.42f, 0.68f), 36, () =>
            {
                var lobby = SessionManager.Instance?.CurrentLobby;
                if (lobby.HasValue)
                    SteamFriends.OpenGameInviteOverlay(lobby.Value.Id);
            });

            // Button row
            var btnRow = new GameObject("BtnRow", typeof(RectTransform));
            btnRow.transform.SetParent(pRt, false);
            btnRow.AddComponent<LayoutElement>().preferredHeight = 40;
            var bhlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            bhlg.spacing = 12; bhlg.childForceExpandWidth = true;

            AddButton(btnRow.GetComponent<RectTransform>(), "CANCEL", new Color(0.5f, 0.18f, 0.18f), 0, OnCancelClicked);
            AddButton(btnRow.GetComponent<RectTransform>(), "LAUNCH", new Color(0.18f, 0.52f, 0.18f), 0, OnLaunchClicked);

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
                session.LaunchGame(_pendingSaveFilePath, _pendingSceneName, playerName);

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
                ShowOverlayStatus($"Lobby ready — {count} player{(count != 1 ? "s" : "")}. Invite friends, then click <b>LAUNCH</b>.");
            }
            else if (session.Phase == SessionManager.LobbyPhase.None)
            {
                ShowOverlayStatus("Creating lobby...");
            }

            foreach (var name in session.LobbyPlayerNames)
            {
                var rowGo = new GameObject("PR", typeof(RectTransform));
                rowGo.transform.SetParent(_playerListContent, false);
                rowGo.AddComponent<Image>().color = new Color(0.14f, 0.14f, 0.18f, 1f);
                rowGo.AddComponent<LayoutElement>().preferredHeight = 28;

                var hlg2 = rowGo.AddComponent<HorizontalLayoutGroup>();
                hlg2.padding = new RectOffset(10, 10, 2, 2);
                hlg2.childAlignment = TextAnchor.MiddleLeft;

                // Green dot
                var dot = new GameObject("Dot", typeof(RectTransform));
                dot.transform.SetParent(rowGo.transform, false);
                dot.AddComponent<Image>().color = new Color(0.35f, 0.82f, 0.35f, 1f);
                var dotLe = dot.AddComponent<LayoutElement>();
                dotLe.preferredWidth = 8; dotLe.preferredHeight = 8;

                AddText(rowGo.GetComponent<RectTransform>(), $"  {name}", 14, FontStyles.Normal, Color.white);
                _playerRows.Add(rowGo);
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
        //  UI helpers
        // ════════════════════════════════════════════

        private static TextMeshProUGUI AddText(RectTransform parent, string text, int size,
            FontStyles style, Color color)
        {
            var go = new GameObject("T", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style; tmp.color = color;
            return tmp;
        }

        private static void AddButton(RectTransform parent, string text, Color bg, int height, Action onClick)
        {
            var go = new GameObject("Btn", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bg;
            if (height > 0)
                go.AddComponent<LayoutElement>().preferredHeight = height;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var c = btn.colors;
            c.highlightedColor = bg * 1.2f; c.pressedColor = bg * 0.8f;
            btn.colors = c;
            btn.onClick.AddListener(() => onClick());

            var lbl = AddText(go.GetComponent<RectTransform>(), text, 14, FontStyles.Bold, Color.white);
            lbl.alignment = TextAlignmentOptions.Center;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        }

        private static void MakeStretchedImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
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
