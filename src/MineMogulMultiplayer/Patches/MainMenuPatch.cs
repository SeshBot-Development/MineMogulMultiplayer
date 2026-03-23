using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineMogulMultiplayer.Patches
{
    /// <summary>
    /// Injects a "Multiplayer" button into the main menu's left-side button list.
    /// Clones an existing menu button so it inherits the game's native font, size and style,
    /// then repositions buttons using RectTransform (the game uses absolute positioning, not layout groups).
    /// </summary>
    [HarmonyPatch(typeof(MainMenu), "OnEnable")]
    internal static class MainMenuPatch
    {
        static void Postfix(MainMenu __instance)
        {
            try
            {
                if (__instance.MainUIPanel == null) return;
                if (__instance.MainUIPanel.transform.Find("MultiplayerButton") != null) return;

                InjectButton(__instance.MainUIPanel.transform);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[MineMogulMP] MainMenuPatch error: {ex}");
            }
        }

        private static void InjectButton(Transform panel)
        {
            // Collect all button children with their positions so we can compute spacing
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

            // Sort by Y position descending (higher Y = higher on screen in typical UI)
            buttons.Sort((a, b) => b.rt.anchoredPosition.y.CompareTo(a.rt.anchoredPosition.y));

            // Calculate vertical spacing between buttons
            float spacing = Mathf.Abs(buttons[0].rt.anchoredPosition.y - buttons[1].rt.anchoredPosition.y);
            if (spacing < 5f) spacing = 50f; // fallback

            // Find insertion point: before any social/URL buttons or Exit —
            // the game has Wishlist, Discord, Steam Discussions, Feedback Survey
            // buttons that open web pages; we want Multiplayer above all of those.
            int insertBeforeIdx = -1;
            foreach (string keyword in new[] {
                "Wishlist", "Discord", "Steam", "Feedback", "Survey",
                "Community", "Support", "Bug", "Exit", "Quit" })
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

            // Clone the button above the insertion point (e.g., "Settings")
            int templateIdx = Mathf.Max(0, insertBeforeIdx - 1);
            var templateRt = buttons[templateIdx].rt;

            var clone = Object.Instantiate(templateRt.gameObject, panel);
            clone.name = "MultiplayerButton";

            // Set label text
            foreach (var tmp in clone.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.text = "Multiplayer";
            foreach (var txt in clone.GetComponentsInChildren<Text>(true))
                txt.text = "Multiplayer";

            var btn2 = clone.GetComponent<Button>();
            if (btn2 != null)
            {
                // Replace the entire event to clear persistent (serialized) listeners
                // that may open URLs — RemoveAllListeners only removes runtime ones.
                btn2.onClick = new Button.ButtonClickedEvent();
                btn2.onClick.AddListener(OpenPanel);
            }

            // Remove any extra components on the clone that could open a browser
            foreach (var comp in clone.GetComponents<Component>())
            {
                if (comp == null) continue;
                var typeName = comp.GetType().Name;
                if (typeName.IndexOf("URL", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    typeName.IndexOf("Link", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    typeName.IndexOf("Web", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    typeName.IndexOf("Browser", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    typeName.IndexOf("Open", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Object.Destroy(comp);
                }
            }

            // Position the new button: same X as template, Y shifted down by one spacing unit
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
            txt.fontSize = 36;
            txt.fontStyle = FontStyles.Bold;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Left;
            btn.targetGraphic = txt;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 50);
        }

        private static void OpenPanel()
        {
            var panel = Plugin.Instance?.MultiplayerPanel;
            if (panel != null) panel.Show();
        }
    }
}
