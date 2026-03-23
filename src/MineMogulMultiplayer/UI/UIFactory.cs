using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineMogulMultiplayer.UI
{
    /// <summary>
    /// Runtime helper for building Unity UI elements without prefabs.
    /// Harvests actual game UI assets (sprites, fonts, button styles) at runtime
    /// for a native look, falling back to procedural generation if unavailable.
    /// </summary>
    public static class UIFactory
    {
        // ── Palette ──────────────────────────────────
        // Warm dark-earth tones — matches MineMogul's mine/factory feel
        public static readonly Color PanelBg          = new Color(0.09f, 0.08f, 0.07f, 0.97f);
        public static readonly Color PanelBgLight     = new Color(0.12f, 0.11f, 0.09f, 1f);
        public static readonly Color HeaderBg         = new Color(0.06f, 0.05f, 0.04f, 1f);
        public static readonly Color RowBg            = new Color(0.15f, 0.13f, 0.11f, 1f);
        public static readonly Color RowBgHover       = new Color(0.20f, 0.17f, 0.14f, 1f);

        // Buttons — earthy, muted tones that feel in-game
        public static readonly Color ButtonPrimary    = new Color(0.28f, 0.48f, 0.30f, 1f);
        public static readonly Color ButtonDanger     = new Color(0.55f, 0.20f, 0.16f, 1f);
        public static readonly Color ButtonSecondary  = new Color(0.22f, 0.19f, 0.16f, 1f);
        public static readonly Color ButtonGhost      = new Color(0.16f, 0.14f, 0.12f, 0.6f);

        // Backwards-compatible alias
        public static readonly Color ButtonNormal     = ButtonPrimary;

        // Input
        public static readonly Color InputFieldBg     = new Color(0.06f, 0.05f, 0.04f, 1f);
        public static readonly Color InputBorder      = new Color(0.30f, 0.25f, 0.18f, 1f);

        // Typography
        public static readonly Color TextColor        = new Color(0.90f, 0.87f, 0.82f, 1f);
        public static readonly Color TextDim          = new Color(0.55f, 0.50f, 0.44f, 1f);
        public static readonly Color TextMuted        = new Color(0.40f, 0.36f, 0.30f, 1f);

        // Accent
        public static readonly Color AccentGold       = new Color(0.92f, 0.75f, 0.25f, 1f);
        public static readonly Color AccentGoldDim    = new Color(0.65f, 0.52f, 0.16f, 1f);
        public static readonly Color FriendOnline     = new Color(0.40f, 0.68f, 0.90f, 1f);
        public static readonly Color StatusGreen      = new Color(0.35f, 0.72f, 0.42f, 1f);

        // Scrollbar
        public static readonly Color ScrollbarBg      = new Color(0.10f, 0.08f, 0.06f, 0.5f);
        public static readonly Color ScrollbarHandle   = new Color(0.32f, 0.27f, 0.20f, 0.8f);

        // Divider
        public static readonly Color DividerColor     = new Color(0.65f, 0.52f, 0.16f, 0.25f);

        // ── Harvested Game Assets ──────────────────

        /// <summary>Sprite harvested from a real game button (panel bg / button bg). Null if not yet harvested.</summary>
        public static Sprite GameButtonSprite { get; private set; }

        /// <summary>TMP font asset harvested from a real game button label. Null if not yet harvested.</summary>
        public static TMP_FontAsset GameFont { get; private set; }

        /// <summary>Button color block harvested from a real game button. Null colors if not harvested.</summary>
        public static ColorBlock? GameButtonColors { get; private set; }

        private static bool _harvested;

        /// <summary>
        /// Scan the scene for real game UI elements and harvest their assets.
        /// Call this once after the game scene is loaded (e.g. from main menu).
        /// Safe to call multiple times — only harvests once.
        /// </summary>
        public static void HarvestGameAssets()
        {
            if (_harvested) return;
            _harvested = true;

            try
            {
                // Find any Button in the scene — game menus (MainMenu, PauseMenu) have buttons
                foreach (var btn in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                {
                    if (btn == null) continue;

                    // Harvest sprite from the button's Image
                    if (GameButtonSprite == null)
                    {
                        var img = btn.GetComponent<Image>();
                        if (img != null && img.sprite != null)
                            GameButtonSprite = img.sprite;
                    }

                    // Harvest font from the button's TMPro text
                    if (GameFont == null)
                    {
                        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (tmp != null && tmp.font != null)
                            GameFont = tmp.font;
                    }

                    // Harvest button color transition
                    if (!GameButtonColors.HasValue)
                        GameButtonColors = btn.colors;

                    if (GameButtonSprite != null && GameFont != null && GameButtonColors.HasValue)
                        break;
                }
            }
            catch { /* Scene may not have buttons yet — use fallbacks */ }
        }

        // ── Rounded Rectangle Sprite (procedural 9-slice) ──

        private static Sprite _roundedSprite;

        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite == null)
                    _roundedSprite = GenerateRoundedSprite(64, 64, 12);
                return _roundedSprite;
            }
        }

        private static Sprite GenerateRoundedSprite(int w, int h, int r)
        {
            var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float alpha = 1f;
                    if ((x < r || x >= w - r) && (y < r || y >= h - r))
                    {
                        float cx = x < r ? r : w - r - 1;
                        float cy = y < r ? r : h - r - 1;
                        float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        if (dist > r)
                            alpha = 0f;
                        else if (dist > r - 1.5f)
                            alpha = 1f - (dist - (r - 1.5f)) / 1.5f;
                    }
                    pixels[y * w + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        }

        public static void ApplyRounded(Image img)
        {
            if (img == null) return;
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
        }

        // ── Canvas ──

        public static Canvas CreateScreenCanvas(string name, int sortOrder = 100)
        {
            var go = new GameObject(name);
            Object.DontDestroyOnLoad(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ── Panel ──

        public static RectTransform CreatePanel(Transform parent, string name, Color bg, bool rounded = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bg;
            if (rounded) ApplyRounded(img);
            return go.GetComponent<RectTransform>();
        }

        // ── Text (TMPro) ──

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text,
            int fontSize = 16, TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = TextColor;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            if (GameFont != null) tmp.font = GameFont;
            return tmp;
        }

        // ── Button (with proper hover/press color tinting) ──

        public static Button CreateButton(Transform parent, string name, string label,
            Color bgColor, int fontSize = 14, float height = 36f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;
            ApplyRounded(img);

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            cb.pressedColor = new Color(0.80f, 0.80f, 0.80f, 1f);
            cb.selectedColor = Color.white;
            cb.fadeDuration = 0.12f;
            btn.colors = cb;
            btn.targetGraphic = img;

            // Soft glow shadow
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.35f);
            shadow.effectDistance = new Vector2(0, -2f);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;

            // Label
            var txt = CreateText(go.transform, "Label", label, fontSize, TextAlignmentOptions.Center);
            txt.fontStyle = FontStyles.Bold;
            var txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(10, 0);
            txtRt.offsetMax = new Vector2(-10, 0);

            return btn;
        }

        // ── Input Field (TMPro) ──

        public static TMP_InputField CreateInputField(Transform parent, string name,
            string placeholder = "", int fontSize = 15, float height = 38f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var bgImg = go.AddComponent<Image>();
            bgImg.color = InputFieldBg;
            ApplyRounded(bgImg);

            // Subtle border via outline
            var outline = go.AddComponent<Outline>();
            outline.effectColor = InputBorder;
            outline.effectDistance = new Vector2(1, -1);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;

            // Text area
            var textArea = new GameObject("Text Area", typeof(RectTransform));
            textArea.transform.SetParent(go.transform, false);
            textArea.AddComponent<RectMask2D>();
            var taRt = textArea.GetComponent<RectTransform>();
            taRt.anchorMin = Vector2.zero;
            taRt.anchorMax = Vector2.one;
            taRt.offsetMin = new Vector2(10, 2);
            taRt.offsetMax = new Vector2(-10, -2);

            // Placeholder
            var phGo = new GameObject("Placeholder", typeof(RectTransform));
            phGo.transform.SetParent(textArea.transform, false);
            var ph = phGo.AddComponent<TextMeshProUGUI>();
            ph.text = placeholder;
            ph.fontSize = fontSize;
            ph.color = TextMuted;
            ph.fontStyle = FontStyles.Italic;
            var phRt = phGo.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = Vector2.zero;
            phRt.offsetMax = Vector2.zero;

            // Input text
            var txtGo = new GameObject("Text", typeof(RectTransform));
            txtGo.transform.SetParent(textArea.transform, false);
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.fontSize = fontSize;
            txt.color = TextColor;
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            var input = go.AddComponent<TMP_InputField>();
            input.textViewport = taRt;
            input.textComponent = txt;
            input.placeholder = ph;
            input.fontAsset = txt.font;
            input.pointSize = fontSize;

            return input;
        }

        // ── Scroll View ──

        public static (ScrollRect scroll, RectTransform content) CreateScrollView(
            Transform parent, string name, float height = 200f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var goImg = go.AddComponent<Image>();
            goImg.color = new Color(0, 0, 0, 0.15f);
            ApplyRounded(goImg);
            go.AddComponent<RectMask2D>();
            var goLe = go.AddComponent<LayoutElement>();
            goLe.preferredHeight = height;
            goLe.flexibleHeight = 1;

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(go.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = new Vector2(0, 0);
            contentRt.offsetMax = new Vector2(-8, 0);
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Scrollbar — slim modern style
            var sbGo = new GameObject("Scrollbar", typeof(RectTransform));
            sbGo.transform.SetParent(go.transform, false);
            var sbImg = sbGo.AddComponent<Image>();
            sbImg.color = ScrollbarBg;
            var sbRt = sbGo.GetComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1, 0);
            sbRt.anchorMax = new Vector2(1, 1);
            sbRt.pivot = new Vector2(1, 0.5f);
            sbRt.sizeDelta = new Vector2(6, 0);

            var handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(sbGo.transform, false);
            var haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.offsetMin = Vector2.zero;
            haRt.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var hImg = handle.AddComponent<Image>();
            hImg.color = ScrollbarHandle;
            var hRt = handle.GetComponent<RectTransform>();
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = new Vector2(1, 0.3f);
            hRt.offsetMin = Vector2.zero;
            hRt.offsetMax = Vector2.zero;

            var sb = sbGo.AddComponent<Scrollbar>();
            sb.handleRect = hRt;
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.targetGraphic = hImg;

            var scroll = go.AddComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = go.GetComponent<RectTransform>();
            scroll.verticalScrollbar = sb;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            return (scroll, contentRt);
        }

        // ── Vertical Layout Group ──

        public static VerticalLayoutGroup AddVerticalLayout(RectTransform rt,
            int padL = 12, int padR = 12, int padT = 12, int padB = 12, float spacing = 8)
        {
            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(padL, padR, padT, padB);
            vlg.spacing = spacing;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            return vlg;
        }

        // ── Horizontal Layout Group ──

        public static HorizontalLayoutGroup AddHorizontalLayout(RectTransform rt,
            float spacing = 8)
        {
            var hlg = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            return hlg;
        }

        // ── Spacer ──

        public static void AddSpacer(Transform parent, float height)
        {
            var go = new GameObject("Spacer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }

        // ── Divider line ──

        public static void AddDivider(Transform parent, float height = 1f)
        {
            var go = new GameObject("Divider", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = DividerColor;
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
        }
    }
}
