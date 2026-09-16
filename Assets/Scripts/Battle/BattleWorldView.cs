using UnityEngine;

namespace TsOnline
{
    /// <summary>Colored quad placeholders: players left, enemies right.</summary>
    public static class BattleWorldView
    {
        public static void AttachPlaceholder(BattleUnit unit, int slotIndex, int slotCount)
        {
            if (unit == null)
                return;

            float y = slotCount <= 1 ? 0f : Mathf.Lerp(2.1f, -2.1f, slotIndex / (float)(slotCount - 1));
            float x = unit.isPlayer ? -5.1f : 5.1f;
            unit.transform.position = new Vector3(x, y, 0f);

            var sr = unit.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = MakeQuad(unit.ElementColor());
            sr.sortingOrder = 2;
            unit.body = sr;

            var label = new GameObject("Label");
            label.transform.SetParent(unit.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            var text = label.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null)
                text.font = font;
            text.text = unit.ShortName;
            text.characterSize = 0.18f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            text.fontSize = 24;
        }

        static Sprite MakeQuad(Color color)
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[16 * 16];
            Color edge = color * 0.45f;
            edge.a = 1f;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool border = x == 0 || y == 0 || x == 15 || y == 15;
                    pixels[y * 16 + x] = border ? edge : color;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
