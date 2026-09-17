using UnityEngine;

namespace TsOnline
{
    /// <summary>Colored shape placeholders: players left (diamond), enemies right (square).</summary>
    public static class BattleWorldView
    {
        public static void BuildArena()
        {
            Sprite ally = WorldArt.MakeArenaFloor(true);
            Sprite enemy = WorldArt.MakeArenaFloor(false);
            FillFloor("AllyFloor", -8.0f, -2.4f, -3.6f, 3.6f, ally, 0);
            FillFloor("EnemyFloor", 2.4f, 8.0f, -3.6f, 3.6f, enemy, 0);
            PlaceFloor("AllyTrim", new Vector3(-2.5f, 0f, 1f), new Vector3(0.14f, 7.2f, 1f),
                new Color(0.28f, 0.42f, 0.58f), 1);
            PlaceFloor("EnemyTrim", new Vector3(2.5f, 0f, 1f), new Vector3(0.14f, 7.2f, 1f),
                new Color(0.58f, 0.28f, 0.22f), 1);
            PlaceFloor("MidLine", new Vector3(0f, 0f, 1f), new Vector3(0.12f, 7.2f, 1f),
                new Color(0.72f, 0.62f, 0.32f), 2);
            PlaceFloor("MidGlow", new Vector3(0f, 0f, 1f), new Vector3(0.28f, 7.2f, 1f),
                new Color(0.55f, 0.48f, 0.22f, 0.55f), 1);
            WorldArt.MakeLabel(null, "ฝ่ายเรา", new Vector3(-5.2f, 3.55f, 0f), new Color(0.70f, 0.88f, 1f), 0.16f, 26);
            WorldArt.MakeLabel(null, "ศัตรู", new Vector3(5.2f, 3.55f, 0f), new Color(1f, 0.72f, 0.68f), 0.16f, 26);
        }

        public static void AttachPlaceholder(BattleUnit unit, int slotIndex, int slotCount)
        {
            if (unit == null)
                return;

            float y = slotCount <= 1 ? 0f : Mathf.Lerp(2.1f, -2.1f, slotIndex / (float)(slotCount - 1));
            float x = unit.isPlayer ? -5.1f : 5.1f;
            unit.transform.position = new Vector3(x, y, 0f);

            var sr = unit.gameObject.AddComponent<SpriteRenderer>();
            Color col = unit.ElementColor();
            WorldArt.ActorMark mark = unit.isPlayer ? WorldArt.ActorMark.Lead : WorldArt.ActorMark.None;
            sr.sprite = WorldArt.MakeShape(col, 24, unit.isPlayer ? WorldArt.Shape.Diamond : WorldArt.Shape.Square,
                true, mark);
            sr.sortingOrder = 3;
            unit.body = sr;
            WorldArt.AttachShadow(unit.transform, 3, 1.2f);

            WorldArt.MakeLabel(unit.transform, unit.ShortName, new Vector3(0f, 0.90f, 0f), Color.white, 0.15f, 24);
            WorldArt.MakeLabel(unit.transform, CreatedHero.Thai(unit.Element), new Vector3(0f, 0.64f, 0f),
                Color.Lerp(col, Color.white, 0.35f), 0.11f, 22);
        }

        static void FillFloor(string rootName, float xMin, float xMax, float yMin, float yMax, Sprite sprite, int sort)
        {
            var root = new GameObject(rootName);
            const float step = 1f;
            for (float y = yMin; y < yMax - 0.01f; y += step)
            {
                for (float x = xMin; x < xMax - 0.01f; x += step)
                {
                    var go = new GameObject("tile");
                    go.transform.SetParent(root.transform, false);
                    go.transform.position = new Vector3(x + step * 0.5f, y + step * 0.5f, 1f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = sort;
                }
            }
        }

        static void PlaceFloor(string name, Vector3 pos, Vector3 scale, Color color, int sort)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeQuad(color, 12);
            sr.color = new Color(color.r, color.g, color.b, color.a);
            sr.sortingOrder = sort;
        }
    }
}
