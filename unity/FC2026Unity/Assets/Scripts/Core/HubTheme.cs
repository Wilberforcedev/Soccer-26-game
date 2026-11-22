using System;
using System.Collections.Generic;
using UnityEngine;

namespace FC2026.Core
{
    /// <summary>
    /// Flat IMGUI palette shared by the career hub and the in-match HUD so both
    /// surfaces use the same button hierarchy and colour language. All shapes
    /// are generated at runtime: rounded nine-slice panels, pill tabs, crest
    /// medallions and a stadium-lit backdrop textured in navy and cyan.
    /// </summary>
    internal static class HubTheme
    {
        private const int ShapeSize = 64;

        private static readonly Dictionary<RoundKey, Texture2D> RoundCache = new();
        private static readonly Dictionary<Color32, Texture2D> SolidTextureCache = new();
        private static readonly Dictionary<(Color32 fill, Color32 edge), Texture2D> CircleCache = new();
        private static Texture2D circleTexture;
        private static Texture2D ringTexture;
        private static Texture2D playGlyphTexture;
        private static Texture2D backdropGradient;
        private static Texture2D stageGlow;
        private static Texture2D vignette;
        private static GUIStyle chipStyle;
        private static int chipStyleFontSize = -1;

        public static readonly Color32 TextColor = C(0xEA, 0xF3, 0xFB);
        public static readonly Color32 SecondaryTextColor = C(0x93, 0xAC, 0xC3);
        public static readonly Color32 AccentTextColor = C(0x4F, 0xDB, 0xFF);
        public static readonly Color32 GoldTextColor = C(0xF6, 0xDA, 0x8A);
        public static readonly Color32 PanelColor = new Color32(0x06, 0x12, 0x22, 0xF6);
        public static readonly Color32 TableHeaderColor = C(0x0B, 0x2D, 0x49);
        public static readonly Color32 TableRowColor = C(0x09, 0x19, 0x2B);
        public static readonly Color32 TableAltRowColor = C(0x07, 0x14, 0x23);
        public static readonly Color32 HighlightColor = C(0x0C, 0x3B, 0x58);
        public static readonly Color32 AccentColor = C(0x1B, 0xC8, 0xF6);
        public static readonly Color32 PositiveColor = C(0x39, 0xD9, 0x8C);
        public static readonly Color32 WarningColor = C(0xF2, 0xB0, 0x3C);
        public static readonly Color32 DangerTextColor = C(0xFF, 0x7D, 0x8C);

        private static readonly Color32 PanelTop = C(0x0A, 0x1B, 0x30);
        private static readonly Color32 PanelBottom = C(0x05, 0x0E, 0x1B);
        private static readonly Color32 PanelEdge = C(0x1B, 0x4D, 0x70);
        private static readonly Color32 CardTop = C(0x0B, 0x1F, 0x36);
        private static readonly Color32 CardBottom = C(0x08, 0x15, 0x25);
        private static readonly Color32 CardEdge = C(0x17, 0x44, 0x62);
        private static readonly Color32 CardHoverTop = C(0x12, 0x33, 0x55);
        private static readonly Color32 CardHoverBottom = C(0x0D, 0x24, 0x41);
        private static readonly Color32 CardHoverEdge = C(0x2F, 0xA0, 0xCB);
        private static readonly Color32 HeroTop = C(0x13, 0x53, 0x7A);
        private static readonly Color32 HeroBottom = C(0x0A, 0x2C, 0x45);
        private static readonly Color32 HeroEdge = C(0x3F, 0xC6, 0xF0);
        private static readonly Color32 SecondaryTop = C(0x0D, 0x23, 0x39);
        private static readonly Color32 SecondaryBottom = C(0x09, 0x17, 0x28);
        private static readonly Color32 SecondaryHoverTop = C(0x15, 0x37, 0x58);
        private static readonly Color32 SecondaryActiveTop = C(0x0A, 0x1B, 0x2D);
        private static readonly Color32 PrimaryTop = C(0x1B, 0xC8, 0xF6);
        private static readonly Color32 PrimaryBottom = C(0x0A, 0x78, 0xB3);
        private static readonly Color32 PrimaryEdge = C(0x8D, 0xEF, 0xFF);
        private static readonly Color32 PrimaryHoverTop = C(0x33, 0xD8, 0xFF);
        private static readonly Color32 PrimaryActiveTop = C(0x0B, 0x8C, 0xC4);
        private static readonly Color32 AccentTop = C(0x0E, 0x8F, 0xC4);
        private static readonly Color32 AccentBottom = C(0x08, 0x5E, 0x8B);
        private static readonly Color32 AccentEdge = C(0x7C, 0xE6, 0xFF);
        private static readonly Color32 AccentHoverTop = C(0x18, 0xB0, 0xE8);
        private static readonly Color32 AccentActiveTop = C(0x0A, 0x6E, 0x9B);
        private static readonly Color32 DangerTop = C(0x8E, 0x30, 0x3C);
        private static readonly Color32 DangerBottom = C(0x5C, 0x1B, 0x25);
        private static readonly Color32 DangerEdge = C(0xD8, 0x5A, 0x6D);
        private static readonly Color32 DangerHoverTop = C(0xA8, 0x3C, 0x4A);
        private static readonly Color32 DangerActiveTop = C(0x6E, 0x23, 0x2E);
        private static readonly Color32 RowTop = C(0x0A, 0x1C, 0x30);
        private static readonly Color32 RowBottom = C(0x08, 0x15, 0x25);
        private static readonly Color32 RowEdge = C(0x14, 0x3B, 0x56);
        private static readonly Color32 RowHoverTop = C(0x11, 0x30, 0x4E);
        private static readonly Color32 RowActiveTop = C(0x07, 0x12, 0x1F);
        private static readonly Color32 RowSelectedTop = C(0x0C, 0x3E, 0x5C);
        private static readonly Color32 RowSelectedBottom = C(0x08, 0x2A, 0x41);
        private static readonly Color32 RowSelectedEdge = C(0x3F, 0xC6, 0xF0);
        private static readonly Color32 RowSelectedHoverTop = C(0x11, 0x51, 0x74);

        private static Color32 C(int r, int g, int b) => new Color32((byte)r, (byte)g, (byte)b, 0xFF);

        public static int ResponsiveFontSize(float heightDivisor, int minimum, int maximum)
        {
            var divisor = Mathf.Max(1f, heightDivisor);
            return Mathf.Clamp(Mathf.RoundToInt(Screen.height / divisor), minimum, maximum);
        }

        public static GUIStyle Label(
            int fontSize,
            Color textColor,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            bool bold = false,
            bool wordWrap = false)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontSize = fontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                wordWrap = wordWrap,
                normal = { textColor = textColor }
            };
        }

        public static Rect CenteredPanel(float maxWidth, float maxHeight, float margin = 16f)
        {
            var safe = Screen.safeArea;
            var width = Mathf.Min(maxWidth, Mathf.Max(0f, safe.width - margin * 2f));
            var height = Mathf.Min(maxHeight, Mathf.Max(0f, safe.height - margin * 2f));
            var top = Screen.height - safe.yMax;
            return new Rect(
                safe.xMin + (safe.width - width) * 0.5f,
                top + (safe.height - height) * 0.5f,
                width,
                height);
        }

        /// <summary>Stadium-lit backdrop: navy vertical gradient, cyan floodlight and vignette.</summary>
        public static void DrawScreenBackground()
        {
            EnsureBackdrop();
            var full = new Rect(0f, 0f, Screen.width, Screen.height);
            GUI.color = Color.white;
            GUI.DrawTexture(full, backdropGradient);
            GUI.color = new Color(0.12f, 0.62f, 0.92f, 0.30f);
            GUI.DrawTexture(new Rect(
                -Screen.width * 0.18f, -Screen.height * 0.5f,
                Screen.width * 1.36f, Screen.height * 0.95f), stageGlow);
            GUI.color = new Color(0f, 0f, 0f, 0.52f);
            GUI.DrawTexture(full, vignette);
            GUI.color = Color.white;
        }

        public static GUIStyle Panel()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(24, 24, 24, 24),
                padding = new RectOffset(0, 0, 0, 0)
            };
            Apply(style, PanelTop, PanelBottom, PanelEdge, 22);
            return style;
        }

        public static GUIStyle Card()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(20, 20, 20, 20),
                padding = new RectOffset(0, 0, 0, 0)
            };
            Apply(style, CardTop, CardBottom, CardEdge, 16);
            return style;
        }

        public static GUIStyle HeroCard()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(20, 20, 20, 20),
                padding = new RectOffset(0, 0, 0, 0)
            };
            Apply(style, HeroTop, HeroBottom, HeroEdge, 16);
            return style;
        }

        public static GUIStyle Pitch()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(20, 20, 20, 20),
                padding = new RectOffset(0, 0, 0, 0)
            };
            Apply(style, C(0x11, 0x6B, 0x43), C(0x06, 0x36, 0x22), C(0x2C, 0xC2, 0x7E), 18);
            return style;
        }

        public static GUIStyle PitchCard()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(12, 12, 12, 12),
                padding = new RectOffset(0, 0, 0, 0)
            };
            Apply(style, CardTop, CardBottom, CardEdge, 10);
            return style;
        }

        /// <summary>Touchline, halfway line, centre circle and penalty areas on a pitch card.</summary>
        public static void PitchLines(Rect pitch)
        {
            var previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.26f);
            var thickness = Mathf.Max(1.5f, pitch.height * 0.004f);
            var inset = pitch.width * 0.045f;
            var inner = new Rect(pitch.x + inset, pitch.y + inset,
                pitch.width - inset * 2f, pitch.height - inset * 2f);
            var white = SolidTexture(Color.white);

            GUI.DrawTexture(new Rect(inner.x, inner.y, inner.width, thickness), white);
            GUI.DrawTexture(new Rect(inner.x, inner.yMax - thickness, inner.width, thickness), white);
            GUI.DrawTexture(new Rect(inner.x, inner.y, thickness, inner.height), white);
            GUI.DrawTexture(new Rect(inner.xMax - thickness, inner.y, thickness, inner.height), white);

            var centerX = inner.x + inner.width * 0.5f;
            var centerY = inner.y + inner.height * 0.5f;
            GUI.DrawTexture(new Rect(inner.x, centerY - thickness * 0.5f, inner.width, thickness), white);

            var circle = Mathf.Clamp(Mathf.Min(inner.width, inner.height) * 0.26f, 24f, 180f);
            GUI.DrawTexture(new Rect(centerX - circle * 0.5f, centerY - circle * 0.5f, circle, circle),
                RingTexture());
            var spot = Mathf.Max(3f, circle * 0.06f);
            GUI.DrawTexture(new Rect(centerX - spot * 0.5f, centerY - spot * 0.5f, spot, spot),
                CircleTexture());

            var boxWidth = inner.width * 0.52f;
            var boxHeight = inner.height * 0.14f;
            var boxX = centerX - boxWidth * 0.5f;
            for (var side = 0; side < 2; side++)
            {
                var lineY = side == 0 ? inner.y + boxHeight : inner.yMax - boxHeight;
                GUI.DrawTexture(new Rect(boxX, lineY, boxWidth, thickness), white);
                GUI.DrawTexture(new Rect(boxX, side == 0 ? inner.y : lineY, thickness, boxHeight), white);
                GUI.DrawTexture(new Rect(boxX + boxWidth - thickness, side == 0 ? inner.y : lineY,
                    thickness, boxHeight), white);
            }

            GUI.color = previous;
        }

        public static GUIStyle IconButton(int fontSize)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(18, 18, 18, 18),
                padding = new RectOffset(4, 4, 2, 2)
            };
            Apply(style, CardTop, CardBottom, HeroEdge, 14);
            SetText(style, AccentTextColor);
            return style;
        }

        /// <summary>Round HUD button built from a generated disc texture.</summary>
        public static GUIStyle CircleButton(int fontSize, bool primary = false)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(8, 8, 4, 4)
            };
            var fill = primary ? PrimaryBottom : CardBottom;
            var edge = primary ? PrimaryEdge : C(0x2E, 0x7F, 0xA8);
            var hoverFill = primary ? PrimaryHoverTop : SecondaryHoverTop;
            var hoverEdge = primary ? (Color32)Color.white : AccentTextColor;
            var activeFill = primary ? PrimaryActiveTop : C(0x07, 0x12, 0x1F);
            style.normal.background = CircleDiscTexture(fill, edge);
            style.hover.background = CircleDiscTexture(hoverFill, hoverEdge);
            style.active.background = CircleDiscTexture(activeFill, edge);
            style.focused.background = style.hover.background;
            SetText(style, primary ? Color.white : (Color)AccentTextColor);
            return style;
        }

        public static GUIStyle SecondaryButton(int fontSize)
        {
            return RoundedButton(fontSize, SecondaryTop, SecondaryBottom, PanelEdge,
                SecondaryHoverTop, SecondaryActiveTop, TextColor, 14);
        }

        public static GUIStyle PrimaryButton(int fontSize)
        {
            return RoundedButton(fontSize, PrimaryTop, PrimaryBottom, PrimaryEdge,
                PrimaryHoverTop, PrimaryActiveTop, Color.white, 14);
        }

        public static GUIStyle AccentButton(int fontSize)
        {
            return RoundedButton(fontSize, AccentTop, AccentBottom, AccentEdge,
                AccentHoverTop, AccentActiveTop, Color.white, 14);
        }

        public static GUIStyle DangerButton(int fontSize)
        {
            return RoundedButton(fontSize, DangerTop, DangerBottom, DangerEdge,
                DangerHoverTop, DangerActiveTop, Color.white, 14);
        }

        public static GUIStyle Tab(int fontSize, bool selected)
        {
            return selected ? AccentButton(fontSize) : SecondaryButton(fontSize);
        }

        public static GUIStyle Row(int fontSize, bool selected)
        {
            return BuildRow(fontSize,
                selected ? RowSelectedTop : RowTop,
                selected ? RowSelectedBottom : RowBottom,
                selected ? RowSelectedHoverTop : RowHoverTop,
                selected ? RowActiveTop : RowActiveTop,
                selected ? GoldTextColor : TextColor,
                selected ? RowSelectedEdge : RowEdge);
        }

        /// <summary>Invisible click layer that only tints on hover so card content stays visible.</summary>
        public static GUIStyle Hitbox()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                border = new RectOffset(18, 18, 18, 18),
                padding = new RectOffset(0, 0, 0, 0)
            };
            style.normal.background = RoundedTexture(Transparent, Transparent, Transparent, 14);
            style.hover.background = RoundedTexture(new Color32(0x4F, 0xDB, 0xFF, 0x1F),
                new Color32(0x4F, 0xDB, 0xFF, 0x12), new Color32(0x4F, 0xDB, 0xFF, 0x3A), 14);
            style.active.background = RoundedTexture(new Color32(0x4F, 0xDB, 0xFF, 0x38),
                new Color32(0x4F, 0xDB, 0xFF, 0x22), new Color32(0x4F, 0xDB, 0xFF, 0x66), 14);
            style.focused.background = style.hover.background;
            style.normal.textColor = Color.clear;
            style.hover.textColor = Color.clear;
            style.active.textColor = Color.clear;
            style.focused.textColor = Color.clear;
            return style;
        }

        public static void Chip(Rect rect, string text, Color textColor, int fontSize)
        {
            if (chipStyle == null || chipStyleFontSize != fontSize)
            {
                chipStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = fontSize,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    border = new RectOffset(16, 16, 16, 16),
                    padding = new RectOffset(8, 8, 2, 2)
                };
                Apply(chipStyle, CardTop, CardBottom, CardEdge, 12);
                chipStyleFontSize = fontSize;
            }

            chipStyle.normal.textColor = textColor;
            GUI.Box(rect, text, chipStyle);
        }

        public static void Crest(Rect rect, Color32 primary, Color32 secondary, string initials, int fontSize)
        {
            var previous = GUI.color;
            GUI.color = primary;
            GUI.DrawTexture(rect, CircleTexture());
            GUI.color = secondary;
            GUI.DrawTexture(rect, RingTexture());
            GUI.color = Color.white;
            if (!string.IsNullOrEmpty(initials))
            {
                var style = Label(fontSize, TextColor, TextAnchor.MiddleCenter, true);
                GUI.Label(rect, initials, style);
            }
            GUI.color = previous;
        }

        public static void Divider(Rect rect, Color32 color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, SolidTexture(Color.white));
            GUI.color = previous;
        }

        public static void Bar(Rect track, float value, Color32 fill)
        {
            var previous = GUI.color;
            var shape = RoundedShape(Mathf.Clamp(Mathf.RoundToInt(track.height * 0.5f), 2, 10));
            GUI.color = new Color(0.02f, 0.05f, 0.09f, 0.92f);
            GUI.DrawTexture(track, shape);
            value = Mathf.Clamp01(value);
            if (value > 0f)
            {
                GUI.color = fill;
                GUI.DrawTexture(new Rect(track.x, track.y, track.width * value, track.height), shape);
            }
            GUI.color = previous;
        }

        public static Texture2D CircleTexture()
        {
            if (circleTexture != null)
                return circleTexture;
            circleTexture = CreateMask(96, (distance, radius) => Mathf.Clamp01(radius - distance + 0.75f));
            return circleTexture;
        }

        /// <summary>Filled disc with a rim, used for round HUD buttons.</summary>
        public static Texture2D CircleDiscTexture(Color32 fill, Color32 edge)
        {
            if (CircleCache.TryGetValue((fill, edge), out var cached) && cached != null)
                return cached;

            const int size = 96;
            var texture = NewTexture(size);
            var center = (size - 1) * 0.5f;
            var radius = size * 0.5f - 1f;
            var rimStart = radius - Mathf.Max(2f, size * 0.045f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = Mathf.Clamp01(radius - distance + 0.75f);
                var color = distance >= rimStart ? edge : fill;
                texture.SetPixel(x, y, new Color(color.r / 255f, color.g / 255f, color.b / 255f,
                    color.a / 255f * alpha));
            }

            texture.Apply(false, true);
            CircleCache[(fill, edge)] = texture;
            return texture;
        }

        public static Texture2D RingTexture()
        {
            if (ringTexture != null)
                return ringTexture;
            const int size = 96;
            ringTexture = CreateMask(size, (distance, outer) =>
            {
                var outside = Mathf.Clamp01(outer - distance + 0.75f);
                var inside = Mathf.Clamp01(distance - (outer - size * 0.07f));
                return outside * inside;
            });
            return ringTexture;
        }

        public static Texture2D PlayTexture()
        {
            if (playGlyphTexture != null)
                return playGlyphTexture;
            const int size = 64;
            var texture = NewTexture(size);
            var ax = size * 0.30f;
            var ay = size * 0.24f;
            var cx = size * 0.78f;
            var cy = size * 0.50f;
            var otherY = size * 0.76f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var px = x + 0.5f;
                var py = y + 0.5f;
                var d1 = (px - ax) * (ay - otherY) - (ax - ax) * (py - otherY);
                var d2 = (px - cx) * (otherY - cy) - (ax - cx) * (py - cy);
                var d3 = (px - ax) * (cy - ay) - (cx - ax) * (py - ay);
                var hasNegative = d1 < 0f || d2 < 0f || d3 < 0f;
                var hasPositive = d1 > 0f || d2 > 0f || d3 > 0f;
                var inside = !(hasNegative && hasPositive);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, inside ? 1f : 0f));
            }
            texture.Apply(false, true);
            playGlyphTexture = texture;
            return playGlyphTexture;
        }

        /// <summary>White rounded silhouette for tinting kit and crest artwork.</summary>
        public static Texture2D RoundedShape(int radius)
        {
            return RoundedTexture(new Color32(0xFF, 0xFF, 0xFF, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF),
                Transparent, Mathf.Clamp(radius, 0, 30));
        }

        public static Texture2D SolidTexture(Color color)
        {
            var colorKey = (Color32)color;
            if (SolidTextureCache.TryGetValue(colorKey, out var cached) && cached != null)
                return cached;

            // Palette colours are reused across screens, so keep one tiny texture per colour.
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, colorKey);
            texture.Apply();
            SolidTextureCache[colorKey] = texture;
            return texture;
        }

        public static GUIStyle BuildButton(int fontSize, Color background, Color hover, Color active, Color text)
        {
            return RoundedButton(fontSize, (Color32)background, (Color32)background, BorderColor,
                hover, active, text, 14);
        }

        public static GUIStyle BuildRow(int fontSize, Color background, Color hover, Color active, Color text)
        {
            return BuildRow(fontSize, (Color32)background, (Color32)background, (Color32)hover,
                (Color32)active, text, BorderColor);
        }

        private static Color32 BorderColor => PanelEdge;

        private static GUIStyle BuildRow(int fontSize, Color32 top, Color32 bottom, Color32 hover,
            Color32 active, Color text, Color32 edge)
        {
            var style = RoundedButton(fontSize, top, bottom, edge, hover, active, text, 10);
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;
            style.wordWrap = false;
            style.padding = new RectOffset(16, 12, 2, 2);
            return style;
        }

        private static GUIStyle RoundedButton(int fontSize, Color32 top, Color32 bottom, Color32 edge,
            Color32 hover, Color32 active, Color text, int radius)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(radius + 2, radius + 2, radius + 2, radius + 2),
                padding = new RectOffset(10, 10, 4, 4)
            };
            style.normal.background = RoundedTexture(top, bottom, edge, radius);
            style.hover.background = RoundedTexture(hover, hover, edge, radius);
            style.active.background = RoundedTexture(active, active, edge, radius);
            style.focused.background = style.hover.background;
            SetText(style, text);
            return style;
        }

        private static void Apply(GUIStyle style, Color32 top, Color32 bottom, Color32 edge, int radius)
        {
            var texture = RoundedTexture(top, bottom, edge, radius);
            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.focused.background = texture;
        }

        private static void SetText(GUIStyle style, Color text)
        {
            style.normal.textColor = text;
            style.hover.textColor = text;
            style.active.textColor = text;
            style.focused.textColor = text;
        }

        private static Texture2D RoundedTexture(Color32 top, Color32 bottom, Color32 edge, int radius)
        {
            var key = new RoundKey(top, bottom, edge, radius);
            if (RoundCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            var texture = NewTexture(ShapeSize);
            var topColor = (Color)top;
            var bottomColor = (Color)bottom;
            var edgeColor = (Color)edge;
            for (var y = 0; y < ShapeSize; y++)
            {
                var mix = y / (ShapeSize - 1f);
                var fill = new Color(
                    topColor.r + (bottomColor.r - topColor.r) * mix,
                    topColor.g + (bottomColor.g - topColor.g) * mix,
                    topColor.b + (bottomColor.b - topColor.b) * mix,
                    topColor.a + (bottomColor.a - topColor.a) * mix);
                for (var x = 0; x < ShapeSize; x++)
                {
                    var px = x + 0.5f;
                    var py = y + 0.5f;
                    var qx = Mathf.Min(px, ShapeSize - px);
                    var qy = Mathf.Min(py, ShapeSize - py);
                    var alpha = 1f;
                    var cornerDistance = 0f;
                    if (qx < radius && qy < radius)
                    {
                        var dx = radius - qx;
                        var dy = radius - qy;
                        cornerDistance = Mathf.Sqrt(dx * dx + dy * dy);
                        alpha = Mathf.Clamp01(radius + 0.5f - cornerDistance);
                    }

                    if (alpha <= 0.002f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    var isEdge = qx <= 1.4f || qy <= 1.4f || cornerDistance > radius - 1.6f;
                    var color = isEdge ? edgeColor : fill;
                    texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
                }
            }

            texture.Apply(false, true);
            RoundCache[key] = texture;
            return texture;
        }

        private static Texture2D CreateMask(int size, Func<float, float, float> alpha)
        {
            var texture = NewTexture(size);
            var center = (size - 1) * 0.5f;
            var radius = size * 0.5f - 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha(distance, radius)));
            }

            texture.Apply(false, true);
            return texture;
        }

        private static Texture2D NewTexture(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear
            };
        }

        private static void EnsureBackdrop()
        {
            if (backdropGradient == null)
            {
                const int height = 256;
                backdropGradient = new Texture2D(1, height, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear
                };
                var top = new Color(0x07 / 255f, 0x11 / 255f, 0x1F / 255f, 1f);
                var mid = new Color(0x04 / 255f, 0x0B / 255f, 0x16 / 255f, 1f);
                var bottom = new Color(0x02 / 255f, 0x05 / 255f, 0x0A / 255f, 1f);
                for (var y = 0; y < height; y++)
                {
                    var t = y / (height - 1f);
                    var color = t < 0.5f
                        ? new Color(
                            bottom.r + (mid.r - bottom.r) * (t * 2f),
                            bottom.g + (mid.g - bottom.g) * (t * 2f),
                            bottom.b + (mid.b - bottom.b) * (t * 2f), 1f)
                        : new Color(
                            mid.r + (top.r - mid.r) * ((t - 0.5f) * 2f),
                            mid.g + (top.g - mid.g) * ((t - 0.5f) * 2f),
                            mid.b + (top.b - mid.b) * ((t - 0.5f) * 2f), 1f);
                    backdropGradient.SetPixel(0, y, color);
                }
                backdropGradient.Apply(false, true);
            }

            if (stageGlow == null)
            {
                const int size = 128;
                stageGlow = NewTexture(size);
                var center = (size - 1) * 0.5f;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - center) / (size * 0.5f);
                    var dy = (y - center) / (size * 0.5f);
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    var falloff = Mathf.Clamp01(1f - distance);
                    stageGlow.SetPixel(x, y, new Color(1f, 1f, 1f, falloff * falloff * 0.9f));
                }
                stageGlow.Apply(false, true);
            }

            if (vignette == null)
            {
                const int size = 128;
                vignette = NewTexture(size);
                var center = (size - 1) * 0.5f;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - center) / (size * 0.5f);
                    var dy = (y - center) / (size * 0.5f);
                    var distance = Mathf.Sqrt(dx * dx + dy * dy) * 0.72f;
                    vignette.SetPixel(x, y, new Color(0f, 0f, 0f,
                        Mathf.Clamp01((distance - 0.55f) / 0.45f) * 0.9f));
                }
                vignette.Apply(false, true);
            }
        }

        private static readonly Color32 Transparent = new Color32(0x00, 0x00, 0x00, 0x00);

        private readonly struct RoundKey : IEquatable<RoundKey>
        {
            private readonly Color32 top;
            private readonly Color32 bottom;
            private readonly Color32 edge;
            private readonly int radius;

            public RoundKey(Color32 top, Color32 bottom, Color32 edge, int radius)
            {
                this.top = top;
                this.bottom = bottom;
                this.edge = edge;
                this.radius = radius;
            }

            public bool Equals(RoundKey other)
            {
                return top.r == other.top.r && top.g == other.top.g && top.b == other.top.b &&
                       top.a == other.top.a &&
                       bottom.r == other.bottom.r && bottom.g == other.bottom.g &&
                       bottom.b == other.bottom.b && bottom.a == other.bottom.a &&
                       edge.r == other.edge.r && edge.g == other.edge.g &&
                       edge.b == other.edge.b && edge.a == other.edge.a &&
                       radius == other.radius;
            }

            public override bool Equals(object obj) => obj is RoundKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = top.r * 31 + top.g;
                    hash = hash * 31 + top.b;
                    hash = hash * 31 + bottom.r;
                    hash = hash * 31 + bottom.g;
                    hash = hash * 31 + bottom.b;
                    hash = hash * 31 + edge.r;
                    hash = hash * 31 + edge.g;
                    hash = hash * 31 + edge.b;
                    hash = hash * 31 + radius;
                    return hash;
                }
            }
        }
    }
}
