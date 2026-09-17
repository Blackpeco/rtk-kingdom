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

        public enum ActorMark
        {
            None,
            Lead,
            Patoyo,
            Grandma,
            Wolf,
            Bandit,
            Frog
        }

        public static Sprite MakeQuad(Color color, int size)
        {
            return MakeShape(color, size, Shape.Square, false);
        }

        public static Sprite MakeShape(Color color, int size, Shape shape, bool highlight = true,
            ActorMark mark = ActorMark.None)
        {
            size = Mathf.Max(12, size);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            Color edge = color * 0.28f;
            edge.a = 1f;
            Color hi = Color.Lerp(color, Color.white, 0.46f);
            hi.a = 1f;
            Color fill = color;
            fill.a = 1f;
            Color shade = Color.Lerp(color, Color.black, 0.28f);
            shade.a = 1f;
            float c = (size - 1) * 0.5f;
            float r = size * 0.40f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    bool inside = Inside(shape, size, x, y, dx, dy, r);
                    bool ring = Inside(shape, size, x, y, dx, dy, r + 1.45f);
                    if (!inside)
                    {
                        pixels[y * size + x] = ring ? edge : Color.clear;
                        continue;
                    }

                    Color px = fill;
                    if (highlight && y > size * 0.58f && x < size * 0.58f)
                        px = Color.Lerp(fill, hi, 0.42f);
                    else if (highlight && y < size * 0.34f)
                        px = Color.Lerp(fill, shade, 0.22f);
                    bool rim = NearEdge(shape, size, x, y, dx, dy, r);
                    pixels[y * size + x] = rim ? edge : px;
                }
            }

            DrawMark(pixels, size, c, r, mark, fill, hi, shade);
            if (mark == ActorMark.Patoyo || shape == Shape.Blob)
                DrawPatoyoFace(pixels, size, c);

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeGround(bool city, int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Hash(x, y);
                    float n2 = Hash(x + 17, y + 9);
                    if (city)
                    {
                        int cell = 8;
                        int ox = (y / cell) % 2 == 0 ? 0 : cell / 2;
                        bool mortar = ((x + ox) % cell == 0) || (y % cell == 0);
                        int cx = ((x + ox) / cell) & 7;
                        int cy = (y / cell) & 7;
                        float stoneN = Hash(cx * 13 + x, cy * 7 + y);
                        Color stone = Color.Lerp(
                            new Color(0.44f, 0.41f, 0.35f),
                            new Color(0.68f, 0.63f, 0.52f),
                            stoneN);
                        if (n2 > 0.86f)
                            stone = Color.Lerp(stone, new Color(0.34f, 0.38f, 0.30f), 0.45f);
                        if (n > 0.93f)
                            stone = Color.Lerp(stone, new Color(0.72f, 0.70f, 0.62f), 0.55f);
                        Color grout = n > 0.7f
                            ? new Color(0.26f, 0.28f, 0.20f)
                            : new Color(0.28f, 0.25f, 0.21f);
                        pixels[y * size + x] = mortar ? grout : stone;
                    }
                    else
                    {
                        Color grass = Color.Lerp(
                            new Color(0.07f, 0.20f, 0.09f),
                            new Color(0.14f, 0.38f, 0.14f),
                            n);
                        if (n2 > 0.88f)
                            grass = new Color(0.22f, 0.18f, 0.10f);
                        else if (((x + y * 3) % 13) == 0)
                            grass = new Color(0.18f, 0.42f, 0.16f);
                        else if (((x * 5 + y) % 19) == 0)
                            grass = new Color(0.28f, 0.36f, 0.12f);
                        if ((x % 7 == 2) && n > 0.55f && n2 < 0.4f)
                            grass = Color.Lerp(grass, new Color(0.12f, 0.32f, 0.10f), 0.6f);
                        pixels[y * size + x] = grass;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeRoad(int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Hash(x + 3, y + 11);
                    float ny = y / (float)(size - 1);
                    Color dirt = Color.Lerp(
                        new Color(0.50f, 0.38f, 0.22f),
                        new Color(0.70f, 0.55f, 0.34f),
                        n);
                    if (ny < 0.18f || ny > 0.82f)
                        dirt = Color.Lerp(dirt, new Color(0.32f, 0.24f, 0.14f), 0.55f);
                    if (Mathf.Abs(ny - 0.38f) < 0.06f || Mathf.Abs(ny - 0.62f) < 0.06f)
                        dirt = Color.Lerp(dirt, new Color(0.36f, 0.26f, 0.14f), 0.5f);
                    if (((x + y) % 11) == 0)
                        dirt = Color.Lerp(dirt, new Color(0.42f, 0.40f, 0.36f), 0.4f);
                    pixels[y * size + x] = dirt;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeArenaFloor(bool ally, int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            var pixels = new Color[size * size];
            Color a = ally ? new Color(0.12f, 0.18f, 0.28f) : new Color(0.28f, 0.12f, 0.10f);
            Color b = ally ? new Color(0.16f, 0.24f, 0.36f) : new Color(0.36f, 0.16f, 0.12f);
            Color line = ally ? new Color(0.22f, 0.32f, 0.46f) : new Color(0.46f, 0.22f, 0.18f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Hash(x + (ally ? 1 : 8), y);
                    Color c = Color.Lerp(a, b, n);
                    if (x % 8 == 0 || y % 8 == 0)
                        c = Color.Lerp(c, line, 0.45f);
                    if (x == 0 || y == 0)
                        c = Color.Lerp(c, Color.black, 0.25f);
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeHouse(Color wall, int size = 32)
        {
            size = Mathf.Max(16, size);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            Color roof = Color.Lerp(wall, new Color(0.35f, 0.16f, 0.12f), 0.55f);
            Color door = new Color(0.18f, 0.12f, 0.10f, 1f);
            Color win = new Color(0.85f, 0.78f, 0.40f, 1f);
            Color edge = wall * 0.25f;
            edge.a = 1f;
            float c = (size - 1) * 0.5f;
            int roofTop = Mathf.RoundToInt(size * 0.08f);
            int eave = Mathf.RoundToInt(size * 0.48f);
            int wallTop = Mathf.RoundToInt(size * 0.36f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    if (y >= roofTop && y <= eave)
                    {
                        float t = (y - roofTop) / (float)Mathf.Max(1, eave - roofTop);
                        float half = t * (size * 0.42f);
                        if (Mathf.Abs(dx) <= half)
                        {
                            bool rim = Mathf.Abs(dx) > half - 1.2f || y <= roofTop + 1;
                            pixels[y * size + x] = rim ? edge : Color.Lerp(roof, Color.white, y > eave - 3 ? 0.08f : 0.18f);
                        }
                    }

                    if (y >= wallTop && y < size - 1 && x > 3 && x < size - 4)
                    {
                        bool rim = x <= 4 || x >= size - 5 || y >= size - 2;
                        Color px = Color.Lerp(wall, Color.white, (x + y) % 5 == 0 ? 0.08f : 0f);
                        pixels[y * size + x] = rim ? edge : px;
                    }
                }
            }

            int doorW = Mathf.Max(3, size / 7);
            int doorH = Mathf.Max(5, size / 4);
            FillRect(pixels, size, (int)c - doorW / 2, 1, (int)c + doorW / 2, doorH, door);
            int wx = Mathf.RoundToInt(c - size * 0.22f);
            int wy = Mathf.RoundToInt(size * 0.20f);
            FillRect(pixels, size, wx, wy, wx + 3, wy + 3, win);
            FillRect(pixels, size, wx + 8, wy, wx + 11, wy + 3, win);
            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeTree(Color foliage, int size = 32)
        {
            size = Mathf.Max(16, size);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            Color trunk = new Color(0.32f, 0.20f, 0.10f, 1f);
            Color dark = Color.Lerp(foliage, Color.black, 0.35f);
            Color hi = Color.Lerp(foliage, new Color(0.35f, 0.55f, 0.22f), 0.35f);
            float c = (size - 1) * 0.5f;
            int trunkW = Mathf.Max(2, size / 8);
            FillRect(pixels, size, (int)c - trunkW, 1, (int)c + trunkW, Mathf.RoundToInt(size * 0.38f), trunk);
            DrawCanopy(pixels, size, c, size * 0.22f, size * 0.92f, foliage, dark, hi);
            DrawCanopy(pixels, size, c, size * 0.38f, size * 0.78f, Color.Lerp(foliage, hi, 0.25f), dark, hi);
            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeShadow(int size = 24)
        {
            size = Mathf.Max(8, size);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c) / (c * 0.92f);
                    float dy = (y - c) / (c * 0.55f);
                    float d = dx * dx + dy * dy;
                    float a = d >= 1f ? 0f : Mathf.Clamp01((1f - d) * 0.45f);
                    pixels[y * size + x] = new Color(0f, 0f, 0f, a);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static void AttachShadow(Transform parent, int sorting, float scale = 1.15f)
        {
            if (parent == null)
                return;
            var go = new GameObject("Shadow");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0.02f, -0.30f, 0.02f);
            go.transform.localScale = new Vector3(scale, scale * 0.42f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeShadow();
            sr.sortingOrder = Mathf.Max(0, sorting - 1);
        }

        public static TextMesh MakeLabel(Transform parent, string text, Vector3 local, Color color, float charSize = 0.15f, int fontSize = 26)
        {
            var go = new GameObject("Label");
            if (parent != null)
                go.transform.SetParent(parent, false);
            go.transform.localPosition = local;

            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(go.transform, false);
            shadow.transform.localPosition = new Vector3(0.03f, -0.03f, 0.01f);
            ApplyText(shadow.AddComponent<TextMesh>(), text, charSize, fontSize, new Color(0f, 0f, 0f, 0.84f));

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

        static void DrawMark(Color[] pixels, int size, float c, float r, ActorMark mark, Color fill, Color hi, Color shade)
        {
            if (mark == ActorMark.None || mark == ActorMark.Patoyo)
                return;

            Color ink = new Color(0.12f, 0.08f, 0.08f, 1f);
            Color skin = new Color(0.93f, 0.80f, 0.66f, 1f);
            switch (mark)
            {
                case ActorMark.Lead:
                    FillCircle(pixels, size, c, c + r * 0.42f, size * 0.16f, skin);
                    Plot(pixels, size, Mathf.RoundToInt(c - 2), Mathf.RoundToInt(c + r * 0.44f), ink);
                    Plot(pixels, size, Mathf.RoundToInt(c + 2), Mathf.RoundToInt(c + r * 0.44f), ink);
                    FillRect(pixels, size, Mathf.RoundToInt(c - 4), Mathf.RoundToInt(c - 1),
                        Mathf.RoundToInt(c + 4), Mathf.RoundToInt(c), shade);
                    break;
                case ActorMark.Grandma:
                    FillCircle(pixels, size, c, c + r * 0.55f, size * 0.16f, skin);
                    FillRect(pixels, size, Mathf.RoundToInt(c - 6), Mathf.RoundToInt(c + r * 0.68f),
                        Mathf.RoundToInt(c + 6), Mathf.RoundToInt(c + r * 0.82f), shade);
                    Plot(pixels, size, Mathf.RoundToInt(c - 2), Mathf.RoundToInt(c + r * 0.55f), ink);
                    Plot(pixels, size, Mathf.RoundToInt(c + 2), Mathf.RoundToInt(c + r * 0.55f), ink);
                    break;
                case ActorMark.Wolf:
                    FillTriangle(pixels, size, c - 5, size * 0.72f, size * 0.96f, shade);
                    FillTriangle(pixels, size, c + 5, size * 0.72f, size * 0.96f, shade);
                    Plot(pixels, size, Mathf.RoundToInt(c - 3), Mathf.RoundToInt(c + 1), ink);
                    Plot(pixels, size, Mathf.RoundToInt(c + 3), Mathf.RoundToInt(c + 1), ink);
                    break;
                case ActorMark.Bandit:
                    FillRect(pixels, size, Mathf.RoundToInt(c - 5), Mathf.RoundToInt(size * 0.62f),
                        Mathf.RoundToInt(c + 5), Mathf.RoundToInt(size * 0.88f), skin);
                    FillRect(pixels, size, Mathf.RoundToInt(c - 5), Mathf.RoundToInt(size * 0.78f),
                        Mathf.RoundToInt(c + 5), Mathf.RoundToInt(size * 0.88f), shade);
                    Plot(pixels, size, Mathf.RoundToInt(c - 2), Mathf.RoundToInt(size * 0.70f), ink);
                    Plot(pixels, size, Mathf.RoundToInt(c + 2), Mathf.RoundToInt(size * 0.70f), ink);
                    break;
                case ActorMark.Frog:
                    FillCircle(pixels, size, c - 5, c + 5, 3.2f, hi);
                    FillCircle(pixels, size, c + 5, c + 5, 3.2f, hi);
                    Plot(pixels, size, Mathf.RoundToInt(c - 5), Mathf.RoundToInt(c + 5), ink);
                    Plot(pixels, size, Mathf.RoundToInt(c + 5), Mathf.RoundToInt(c + 5), ink);
                    FillRect(pixels, size, Mathf.RoundToInt(c - 4), Mathf.RoundToInt(c - 3),
                        Mathf.RoundToInt(c + 4), Mathf.RoundToInt(c - 2), ink);
                    break;
            }
        }

        static void DrawPatoyoFace(Color[] pixels, int size, float c)
        {
            int eyeY = Mathf.RoundToInt(c + size * 0.10f);
            Color ink = new Color(0.14f, 0.06f, 0.10f, 1f);
            Color blush = new Color(0.95f, 0.42f, 0.55f, 1f);
            Color shine = new Color(1f, 0.86f, 0.90f, 1f);
            FillCircle(pixels, size, c - 4, c + size * 0.16f, 2.2f, shine);
            Plot(pixels, size, Mathf.RoundToInt(c - 3), eyeY, ink);
            Plot(pixels, size, Mathf.RoundToInt(c + 3), eyeY, ink);
            Plot(pixels, size, Mathf.RoundToInt(c - 6), eyeY - 2, blush);
            Plot(pixels, size, Mathf.RoundToInt(c + 6), eyeY - 2, blush);
            FillRect(pixels, size, Mathf.RoundToInt(c - 2), Mathf.RoundToInt(c - 3),
                Mathf.RoundToInt(c + 2), Mathf.RoundToInt(c - 2), new Color(0.55f, 0.18f, 0.28f, 1f));
        }

        static void DrawCanopy(Color[] pixels, int size, float cx, float yBot, float yTop, Color fill, Color edge, Color hi)
        {
            int bot = Mathf.Clamp(Mathf.RoundToInt(yBot), 0, size - 1);
            int top = Mathf.Clamp(Mathf.RoundToInt(yTop), 0, size - 1);
            if (top <= bot)
                return;
            float mid = (bot + top) * 0.5f;
            for (int y = bot; y <= top; y++)
            {
                float u = 1f - Mathf.Abs(y - mid) / Mathf.Max(1f, (top - bot) * 0.5f);
                float half = u * (size * 0.38f);
                for (int x = Mathf.RoundToInt(cx - half); x <= Mathf.RoundToInt(cx + half); x++)
                {
                    bool rim = x <= cx - half + 1.1f || x >= cx + half - 1.1f || y == bot || y == top;
                    Color px = rim ? edge : ((x + y) % 5 == 0 ? hi : fill);
                    Plot(pixels, size, x, y, px);
                }
            }
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

        static void FillCircle(Color[] pixels, int size, float cx, float cy, float radius, Color color)
        {
            int x0 = Mathf.FloorToInt(cx - radius);
            int x1 = Mathf.CeilToInt(cx + radius);
            int y0 = Mathf.FloorToInt(cy - radius);
            int y1 = Mathf.CeilToInt(cy + radius);
            float r2 = radius * radius;
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= r2)
                        Plot(pixels, size, x, y, color);
                }
            }
        }

        static void FillTriangle(Color[] pixels, int size, float cx, float yTop, float yBot, Color color)
        {
            int top = Mathf.RoundToInt(yTop);
            int bot = Mathf.RoundToInt(yBot);
            if (bot == top)
                return;
            for (int y = Mathf.Min(top, bot); y <= Mathf.Max(top, bot); y++)
            {
                float t = (y - yTop) / (yBot - yTop);
                float half = Mathf.Abs(t) * 3.2f;
                for (int x = Mathf.RoundToInt(cx - half); x <= Mathf.RoundToInt(cx + half); x++)
                    Plot(pixels, size, x, y, color);
            }
        }

        static void FillRect(Color[] pixels, int size, int x0, int y0, int x1, int y1, Color color)
        {
            int xa = Mathf.Min(x0, x1);
            int xb = Mathf.Max(x0, x1);
            int ya = Mathf.Min(y0, y1);
            int yb = Mathf.Max(y0, y1);
            for (int y = ya; y <= yb; y++)
            {
                for (int x = xa; x <= xb; x++)
                    Plot(pixels, size, x, y, color);
            }
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
