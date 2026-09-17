using UnityEngine;

namespace TsOnline
{
    /// <summary>Runtime placeholder sprites + world labels. Visual only.</summary>
    public static class WorldArt
    {
        public enum Shape
        {
            Square,
            Circle,
            Diamond,
            Triangle,
            Blob
        }

        public static Sprite MakeQuad(Color color, int size)
        {
            return MakeShape(color, size, Shape.Square, false);
        }

        public static Sprite MakeShape(Color color, int size, Shape shape, bool highlight = true)
        {
            size = Mathf.Max(10, size);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            Color edge = color * 0.32f;
            edge.a = 1f;
            Color hi = Color.Lerp(color, Color.white, 0.42f);
            hi.a = 1f;
            Color fill = color;
            fill.a = 1f;
            float c = (size - 1) * 0.5f;
            float r = size * 0.40f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    bool inside = Inside(shape, size, x, y, dx, dy, r);
                    bool ring = Inside(shape, size, x, y, dx, dy, r + 1.35f);
                    if (!inside)
                    {
                        pixels[y * size + x] = ring ? edge : Color.clear;
                        continue;
                    }

                    Color px = fill;
                    if (highlight && y > size * 0.58f && x < size * 0.58f)
                        px = Color.Lerp(fill, hi, 0.40f);
                    bool rim = NearEdge(shape, size, x, y, dx, dy, r);
                    pixels[y * size + x] = rim ? edge : px;
                }
            }

            if (shape == Shape.Blob)
                DrawFace(pixels, size, c);

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeGround(bool city, int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (city)
                    {
                        int cell = 8;
                        bool mortar = (x % cell == 0) || (y % cell == 0);
                        int ox = (y / cell) % 2 == 0 ? 0 : cell / 2;
                        bool mortarShift = ((x + ox) % cell == 0) || (y % cell == 0);
                        float n = Hash(x, y);
                        Color stone = Color.Lerp(
                            new Color(0.46f, 0.44f, 0.38f),
                            new Color(0.62f, 0.58f, 0.48f),
                            n);
                        pixels[y * size + x] = mortarShift ? new Color(0.30f, 0.28f, 0.24f) : stone;
                    }
                    else
                    {
                        float n = Hash(x, y);
                        Color grass = Color.Lerp(
                            new Color(0.08f, 0.22f, 0.10f),
                            new Color(0.16f, 0.40f, 0.16f),
                            n);
                        if (((x + y * 3) % 17) == 0)
                            grass = new Color(0.10f, 0.28f, 0.12f);
                        pixels[y * size + x] = grass;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static TextMesh MakeLabel(Transform parent, string text, Vector3 local, Color color, float charSize = 0.15f, int fontSize = 26)
        {
            var go = new GameObject("Label");
            if (parent != null)
                go.transform.SetParent(parent, false);
            go.transform.localPosition = local;

            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(go.transform, false);
            shadow.transform.localPosition = new Vector3(0.02f, -0.02f, 0.01f);
            ApplyText(shadow.AddComponent<TextMesh>(), text, charSize, fontSize, new Color(0f, 0f, 0f, 0.75f));

            var tm = go.AddComponent<TextMesh>();
            ApplyText(tm, text, charSize, fontSize, color);
            return tm;
        }

        public static void ApplyText(TextMesh tm, string text, float charSize, int fontSize, Color color)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
                tm.font = font;
            tm.text = text;
            tm.characterSize = charSize;
            tm.fontSize = fontSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            var mr = tm.GetComponent<MeshRenderer>();
            if (mr != null && tm.font != null && tm.font.material != null)
                mr.sharedMaterial = tm.font.material;
        }

        static bool Inside(Shape shape, int size, int x, int y, float dx, float dy, float r)
        {
            switch (shape)
            {
                case Shape.Square:
                    return x > 0 && y > 0 && x < size - 1 && y < size - 1;
                case Shape.Circle:
                case Shape.Blob:
                    return dx * dx + dy * dy <= r * r;
                case Shape.Diamond:
                    return Mathf.Abs(dx) + Mathf.Abs(dy) <= r;
                case Shape.Triangle:
                    {
                        float ny = y / (float)(size - 1);
                        float half = (1f - ny) * (size * 0.42f);
                        return y > 1 && Mathf.Abs(dx) <= half;
                    }
                default:
                    return false;
            }
        }

        static bool NearEdge(Shape shape, int size, int x, int y, float dx, float dy, float r)
        {
            switch (shape)
            {
                case Shape.Square:
                    return x <= 1 || y <= 1 || x >= size - 2 || y >= size - 2;
                case Shape.Circle:
                case Shape.Blob:
                    return dx * dx + dy * dy > (r - 1.2f) * (r - 1.2f);
                case Shape.Diamond:
                    return Mathf.Abs(dx) + Mathf.Abs(dy) > r - 1.2f;
                case Shape.Triangle:
                    {
                        float ny = y / (float)(size - 1);
                        float half = (1f - ny) * (size * 0.42f);
                        return Mathf.Abs(dx) > half - 1.3f || y <= 2;
                    }
                default:
                    return false;
            }
        }

        static void DrawFace(Color[] pixels, int size, float c)
        {
            int eyeY = Mathf.RoundToInt(c + size * 0.08f);
            Plot(pixels, size, Mathf.RoundToInt(c - 3), eyeY, new Color(0.12f, 0.06f, 0.10f, 1f));
            Plot(pixels, size, Mathf.RoundToInt(c + 3), eyeY, new Color(0.12f, 0.06f, 0.10f, 1f));
            Plot(pixels, size, Mathf.RoundToInt(c), Mathf.RoundToInt(c - 2), new Color(0.55f, 0.18f, 0.28f, 1f));
        }

        static void Plot(Color[] pixels, int size, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= size || y >= size)
                return;
            pixels[y * size + x] = c;
        }

        static float Hash(int x, int y)
        {
            int n = x * 374761393 + y * 668265263;
            n = (n ^ (n >> 13)) * 1274126177;
            return ((n ^ (n >> 16)) & 255) / 255f;
        }
    }
}
