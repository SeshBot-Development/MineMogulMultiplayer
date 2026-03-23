using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineMogulMultiplayer.UI
{
    /// <summary>
    /// Persistent on-screen log that shows multiplayer events (server started, player joined, etc.).
    /// Appears in the bottom-left corner during gameplay.
    /// </summary>
    public class EventLog : MonoBehaviour
    {
        private Canvas _canvas;
        private RectTransform _container;
        private readonly List<LogEntry> _entries = new List<LogEntry>();
        private const int MaxEntries = 12;
        private const float FadeDuration = 1.5f;
        private const float DisplayDuration = 8f;

        private struct LogEntry
        {
            public GameObject Go;
            public TextMeshProUGUI Text;
            public CanvasGroup Group;
            public float SpawnTime;
        }

        public static EventLog Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            BuildUI();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildUI()
        {
            // Separate canvas so it never conflicts with other UI
            var canvasGo = new GameObject("MP_EventLogCanvas");
            Object.DontDestroyOnLoad(canvasGo);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            // No GraphicRaycaster — this log should never block input

            // Container: bottom-left, grows upward
            var containerGo = new GameObject("Container", typeof(RectTransform));
            containerGo.transform.SetParent(canvasGo.transform, false);
            _container = containerGo.GetComponent<RectTransform>();
            _container.anchorMin = new Vector2(0, 0);
            _container.anchorMax = new Vector2(0, 0);
            _container.pivot = new Vector2(0, 0);
            _container.anchoredPosition = new Vector2(16, 16);
            _container.sizeDelta = new Vector2(500, 400);

            var vlg = containerGo.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.spacing = 2;
            vlg.padding = new RectOffset(0, 0, 0, 0);

            var csf = containerGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                float age = now - entry.SpawnTime;

                if (age > DisplayDuration + FadeDuration)
                {
                    Destroy(entry.Go);
                    _entries.RemoveAt(i);
                }
                else if (age > DisplayDuration)
                {
                    float fade = 1f - ((age - DisplayDuration) / FadeDuration);
                    entry.Group.alpha = Mathf.Clamp01(fade);
                }
            }
        }

        /// <summary>
        /// Add a message to the event log. Thread-safe to call from anywhere.
        /// </summary>
        public void Log(string message, Color? color = null)
        {
            try
            {
                if (_container == null) return;

                // Trim old entries
                while (_entries.Count >= MaxEntries)
                {
                    if (_entries[0].Go != null) Destroy(_entries[0].Go);
                    _entries.RemoveAt(0);
                }

                var go = new GameObject("LogEntry", typeof(RectTransform));
                go.transform.SetParent(_container, false);

                var group = go.AddComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;

                // Rounded background for readability
                var bg = go.AddComponent<Image>();
                if (bg != null)
                {
                    bg.color = new Color(0, 0, 0, 0.45f);
                    UIFactory.ApplyRounded(bg);
                }

                var tmp = go.AddComponent<TextMeshProUGUI>();
                if (tmp == null) { Destroy(go); return; }
                tmp.text = message ?? "";
                tmp.fontSize = 15;
                tmp.color = color ?? UIFactory.TextColor;
                tmp.alignment = TextAlignmentOptions.Left;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.margin = new Vector4(6, 2, 6, 2);
                tmp.raycastTarget = false;

                var le = go.AddComponent<LayoutElement>();
                if (le != null) le.preferredHeight = -1; // auto-size

                _entries.Add(new LogEntry
                {
                    Go = go,
                    Text = tmp,
                    Group = group,
                    SpawnTime = Time.unscaledTime
                });
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MP EventLog] Log failed: {ex.Message}");
            }
        }

        /// <summary>Shortcut for multiplayer system messages (gold colored).</summary>
        public void LogSystem(string message)
        {
            Log($"[MP] {message}", UIFactory.AccentGold);
        }

        /// <summary>Shortcut for player join/leave events (green/red).</summary>
        public void LogJoin(string playerName)
        {
            Log($"+ {playerName} joined", new Color(0.35f, 0.85f, 0.35f, 1f));
        }

        public void LogLeave(string playerName)
        {
            Log($"- {playerName} left", new Color(0.85f, 0.35f, 0.35f, 1f));
        }

        public void LogError(string message)
        {
            Log($"[ERROR] {message}", UIFactory.ButtonDanger);
        }
    }
}
