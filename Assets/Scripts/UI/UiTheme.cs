using UnityEngine;

namespace TsOnline
{
    /// <summary>Shared OnGUI readability helpers. Visual only — no gameplay state.</summary>
    public static class UiTheme
    {
        public const float WorldHudHeight = 188f;
        public const float WorldHudBottom = 208f;

        public static readonly Color Panel = new Color(0.07f, 0.08f, 0.10f, 0.92f);
        public static readonly Color PanelWarm = new Color(0.14f, 0.09f, 0.08f, 0.92f);
        public static readonly Color Hp = new Color(0.22f, 0.78f, 0.36f, 1f);
        public static readonly Color Sp = new Color(0.22f, 0.50f, 0.95f, 1f);
        public static readonly Color HpBack = new Color(0.10f, 0.16f, 0.10f, 1f);
        public static readonly Color SpBack = new Color(0.08f, 0.10f, 0.18f, 1f);
        public static readonly Color LeadGold = new Color(1f, 0.86f, 0.38f, 1f);
        public static readonly Color Advantage = new Color(1f, 0.84f, 0.18f, 1f);
        public static readonly Color Resist = new Color(0.78f, 0.80f, 0.86f, 1f);

        static Texture2D _white;
        static GUIStyle _title;
        static GUIStyle _body;
        static GUIStyle _hint;
        static GUIStyle _tiny;
        static GUIStyle _button;
        static GUIStyle _box;
        static int _cachedSkin;

        public static Texture2D White
        {
            get
            {
                if (_white == null)
                {
                    _white = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _white.SetPixel(0, 0, Color.white);
                    _white.Apply();
                    _white.hideFlags = HideFlags.HideAndDontSave;
                }

                return _white;
            }
        }

        public static void DrawFill(Rect r, Color color)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(r, White);
            GUI.color = prev;
        }

        public static void DrawPanel(Rect r, Color? color = null)
        {
            DrawFill(r, color ?? Panel);
            DrawFill(new Rect(r.x, r.y, r.width, 2f), new Color(1f, 1f, 1f, 0.12f));
        }

        public static void DrawBar(Rect r, int current, int max, Color fill, Color back, string label, GUIStyle labelStyle)
        {
            DrawFill(r, back);
            float pct = max <= 0 ? 0f : Mathf.Clamp01(current / (float)max);
            if (pct > 0f)
                DrawFill(new Rect(r.x + 2f, r.y + 2f, (r.width - 4f) * pct, r.height - 4f), fill);
            if (labelStyle != null && !string.IsNullOrEmpty(label))
                GUI.Label(r, label, labelStyle);
        }

        public static GUIStyle Title()
        {
            Ensure();
            return _title;
        }

        public static GUIStyle Body()
        {
            Ensure();
            return _body;
        }

        public static GUIStyle Hint()
        {
            Ensure();
            return _hint;
        }

        public static GUIStyle Tiny()
        {
            Ensure();
            return _tiny;
        }

        public static GUIStyle Button()
        {
            Ensure();
            return _button;
        }

        public static GUIStyle Box()
        {
            Ensure();
            return _box;
        }

        public static Color Element(ElementType e)
        {
            return CreatedHero.ColorOf(e);
        }

        /// <summary>Single-line ellipsis so long names do not spill into the next column.</summary>
        public static string Ellipsis(string text, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            if (style == null || maxWidth <= 0f)
                return text;
            if (style.CalcSize(new GUIContent(text)).x <= maxWidth)
                return text;

            const string dots = "…";
            for (int i = text.Length - 1; i >= 1; i--)
            {
                string cut = text.Substring(0, i) + dots;
                if (style.CalcSize(new GUIContent(cut)).x <= maxWidth)
                    return cut;
            }

            return dots;
        }

        static void Ensure()
        {
            int id = GUI.skin != null ? GUI.skin.GetHashCode() : 0;
            if (_title != null && _cachedSkin == id)
                return;
            _cachedSkin = id;

            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            _title.normal.textColor = Color.white;

            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            _body.normal.textColor = new Color(0.93f, 0.94f, 0.96f);

            _hint = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            _hint.normal.textColor = new Color(0.82f, 0.86f, 0.90f);

            _tiny = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            _tiny.normal.textColor = Color.white;

            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            _box = new GUIStyle(GUI.skin.box)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
        }
    }
}
