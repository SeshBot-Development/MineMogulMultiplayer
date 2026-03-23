using HarmonyLib;
using MineMogulMultiplayer.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Injects a "Multiplayer" button into the in-game pause menu's button list.
    /// Clones an existing menu button so it inherits the game's native font, size and style.
    /// Repositions buttons via RectTransform (the game uses absolute positioning, not layout groups).
    /// When multiplayer is active, prevents the game from freezing (Time.timeScale = 0).
    /// </summary>
    [HarmonyPatch(typeof(PauseMenu), "OnEnable")]
    internal static class PauseMenuPatch
    {
        static void Postfix(PauseMenu __instance)
        {
            try
            {
                if (MultiplayerState.IsOnline)
                    Time.timeScale = 1f;

                if (__instance.MainUIPanel == null) return;
                if (__instance.MainUIPanel.transform.Find("MultiplayerButton") != null) return;

                InjectButton(__instance.MainUIPanel.transform);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[MineMogulMP] PauseMenuPatch error: {ex}");
            }
        }

        private static void InjectButton(Transform panel)
        {
            // Collect all button children with positions to compute spacing
            var buttons = new System.Collections.Generic.List<(RectTransform rt, string text)>();
            for (int i = 0; i < panel.childCount; i++)
            {
                var child = panel.GetChild(i);
                var btn = child.GetComponent<Button>();
                if (btn == null) continue;
                var rt = child.GetComponent<RectTransform>();
                if (rt == null) continue;
                string text = "";
                var tmp = child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null) text = tmp.text;
                buttons.Add((rt, text));
            }

            if (buttons.Count < 2) { CreateFallbackButton(panel); return; }

            // Sort by Y position descending (higher Y = higher on screen)
            buttons.Sort((a, b) => b.rt.anchoredPosition.y.CompareTo(a.rt.anchoredPosition.y));

            // Calculate vertical spacing between consecutive buttons
            float spacing = Mathf.Abs(buttons[0].rt.anchoredPosition.y - buttons[1].rt.anchoredPosition.y);
            if (spacing < 5f) spacing = 45f;

            // Find insertion point: before any social/URL buttons or Exit —
            // the game has Wishlist, Discord, Steam Discussions buttons
            // that open web pages; we want Multiplayer above all of those.
            int insertBeforeIdx = -1;
            foreach (string keyword in new[] {
                "Wishlist", "Discord", "Steam", "Feedback", "Survey",
                "Community", "Support", "Bug",
                "Main Menu", "Exit", "Quit" })
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].text.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        insertBeforeIdx = i;
                        break;
                    }
                }
                if (insertBeforeIdx >= 0) break;
            }
            if (insertBeforeIdx < 0) insertBeforeIdx = buttons.Count - 1;

            // Clone the button above the insertion point (e.g. "Settings")
            int templateIdx = Mathf.Max(0, insertBeforeIdx - 1);
            var templateRt = buttons[templateIdx].rt;

            var clone = Object.Instantiate(templateRt.gameObject, panel);
            clone.name = "MultiplayerButton";

            foreach (var tmp in clone.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.text = "Multiplayer";
            foreach (var txt in clone.GetComponentsInChildren<Text>(true))
                txt.text = "Multiplayer";

            var btn2 = clone.GetComponent<Button>();
            if (btn2 != null)
            {
                btn2.onClick.RemoveAllListeners();
                btn2.onClick.AddListener(OpenPanel);
            }

            // Position new button: same X as template, Y shifted down by one spacing unit
            var cloneRt = clone.GetComponent<RectTransform>();
            cloneRt.anchoredPosition = new Vector2(
                templateRt.anchoredPosition.x,
                templateRt.anchoredPosition.y - spacing);

            // Push all buttons at or below the insertion point down by one spacing unit
            for (int i = insertBeforeIdx; i < buttons.Count; i++)
            {
                var brt = buttons[i].rt;
                brt.anchoredPosition = new Vector2(
                    brt.anchoredPosition.x,
                    brt.anchoredPosition.y - spacing);
            }

            clone.SetActive(true);
        }

        private static void CreateFallbackButton(Transform parent)
        {
            var go = new GameObject("MultiplayerButton", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.85f, 0.3f, 1f);
            cb.pressedColor = new Color(0.8f, 0.68f, 0.24f, 1f);
            cb.fadeDuration = 0.1f;
            btn.colors = cb;
            btn.onClick.AddListener(OpenPanel);

            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = "Multiplayer";
            txt.fontSize = 28;
            txt.fontStyle = FontStyles.Bold;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Left;
            btn.targetGraphic = txt;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 45);
        }

        private static void OpenPanel()
        {
            var panel = Plugin.Instance?.MultiplayerPanel;
            if (panel != null) panel.Show();
        }
    }

    /// <summary>
    /// When multiplayer is active, prevent OnDisable from calling OnGamePauseToggled(false)
    /// since we never actually paused. Also ensures timeScale stays at 1.
    /// </summary>
    [HarmonyPatch(typeof(PauseMenu), "OnDisable")]
    internal static class PauseMenuDisablePatch
    {
        static bool Prefix()
        {
            if (MultiplayerState.IsOnline)
            {
                Time.timeScale = 1f;
                Singleton<SettingsManager>.Instance?.SetVsyncAndFPSLimit();
                return false;
            }
            return true;
        }
    }
}
