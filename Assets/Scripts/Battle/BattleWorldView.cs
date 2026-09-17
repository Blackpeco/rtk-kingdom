using UnityEngine;

namespace TsOnline
{
    /// <summary>Colored shape placeholders: players left (diamond), enemies right (square).</summary>
    public static class BattleWorldView
    {
        public static void BuildArena()
        {
            PlaceFloor("AllyFloor", new Vector3(-5.2f, 0f, 1f), new Vector3(5.6f, 7.2f, 1f),
                new Color(0.14f, 0.20f, 0.30f), 0);
            PlaceFloor("EnemyFloor", new Vector3(5.2f, 0f, 1f), new Vector3(5.6f, 7.2f, 1f),
                new Color(0.30f, 0.14f, 0.12f), 0);
            PlaceFloor("MidLine", new Vector3(0f, 0f, 1f), new Vector3(0.16f, 7.2f, 1f),
                new Color(0.55f, 0.52f, 0.40f), 1);
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
            sr.sprite = WorldArt.MakeShape(col, 22, unit.isPlayer ? WorldArt.Shape.Diamond : WorldArt.Shape.Square);
            sr.sortingOrder = 2;
            unit.body = sr;

            WorldArt.MakeLabel(unit.transform, unit.ShortName, new Vector3(0f, 0.88f, 0f), Color.white, 0.15f, 24);
            WorldArt.MakeLabel(unit.transform, CreatedHero.Thai(unit.Element), new Vector3(0f, 0.62f, 0f),
                Color.Lerp(col, Color.white, 0.35f), 0.11f, 22);
        }

        static void PlaceFloor(string name, Vector3 pos, Vector3 scale, Color color, int sort)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeQuad(color, 12);
            sr.sortingOrder = sort;
        }
    }
}
