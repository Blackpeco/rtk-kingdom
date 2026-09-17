using UnityEngine;
using UnityEngine.SceneManagement;

namespace TsOnline
{
    /// <summary>Name + element picker. Confirm writes the created lead and loads World.</summary>
    public class CharacterCreateUI : MonoBehaviour
    {
        string _name = "ผู้กล้า";
        ElementType _element = ElementType.Fire;
        string _error = "";

        static readonly ElementType[] Choices =
        {
            ElementType.Earth, ElementType.Water, ElementType.Fire, ElementType.Wind
        };

        void Start()
        {
            ApplyCamera();
        }

        void OnGUI()
        {
            float w = Mathf.Min(780f, Screen.width - 40f);
            float h = Mathf.Min(620f, Screen.height - 40f);
            var box = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            UiTheme.DrawPanel(box);

            GUI.Label(new Rect(box.x + 28, box.y + 16, w - 56, 32), "สร้างตัวละคร", UiTheme.Title());
            GUI.Label(new Rect(box.x + 28, box.y + 56, 80, 28), "ชื่อ", UiTheme.Body());
            var field = new GUIStyle(GUI.skin.textField) { fontSize = 18 };
            _name = GUI.TextField(new Rect(box.x + 110, box.y + 54, w - 154, 32), _name ?? "", field);

            GUI.Label(new Rect(box.x + 28, box.y + 100, w - 56, 24), "เลือกธาตุ", UiTheme.Body());

            float btnW = (w - 80f) / 4f;
            for (int i = 0; i < Choices.Length; i++)
            {
                ElementType el = Choices[i];
                var r = new Rect(box.x + 28 + i * (btnW + 8), box.y + 128, btnW, 96);
                DrawElementButton(r, el);
            }

            UnitStats s = CreatedHero.StatsFor(_element);
            var role = new GUIStyle(UiTheme.Title()) { fontSize = 20 };
            role.normal.textColor = Color.Lerp(CreatedHero.ColorOf(_element), Color.white, 0.35f);
            GUI.Label(new Rect(box.x + 28, box.y + 240, w - 56, 28), CreatedHero.RoleBlurb(_element), role);

            DrawStatRow(box.x + 28, box.y + 280, w - 56, s, _element);

            GUI.Label(new Rect(box.x + 28, box.y + 368, w - 56, 44),
                "ตัวนี้เป็นหัวหน้าปาร์ตี้  ขุนพล 6 คนยังปลดล็อกในแผงปาร์ตี้ (P) ภายหลัง", UiTheme.Hint());

            if (!string.IsNullOrEmpty(_error))
            {
                var err = new GUIStyle(UiTheme.Body());
                err.normal.textColor = new Color(1f, 0.45f, 0.4f);
                GUI.Label(new Rect(box.x + 28, box.y + 416, w - 56, 26), _error, err);
            }

            if (GUI.Button(new Rect(box.x + w * 0.5f - 110, box.y + h - 64, 220, 46), "เริ่มเดินทาง", UiTheme.Button()))
                Confirm();
        }

        void DrawElementButton(Rect r, ElementType el)
        {
            Color col = CreatedHero.ColorOf(el);
            bool selected = _element == el;
            UiTheme.DrawFill(r, selected ? Color.Lerp(col, Color.white, 0.18f) : col * 0.75f);
            if (selected)
            {
                UiTheme.DrawFill(new Rect(r.x, r.y, r.width, 4f), Color.white);
                UiTheme.DrawFill(new Rect(r.x, r.yMax - 4f, r.width, 4f), Color.white);
            }

            var thai = new GUIStyle(UiTheme.Title())
            {
                fontSize = 26,
                alignment = TextAnchor.MiddleCenter
            };
            thai.normal.textColor = Color.white;
            var en = new GUIStyle(UiTheme.Hint()) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
            GUI.Label(new Rect(r.x, r.y + 10, r.width, 36), CreatedHero.Thai(el), thai);
            GUI.Label(new Rect(r.x, r.y + 44, r.width, 22), CreatedHero.English(el), en);
            if (selected)
                GUI.Label(new Rect(r.x, r.y + 66, r.width, 22), "▸ เลือกแล้ว", en);

            if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                _element = el;
        }

        static void DrawStatRow(float x, float y, float width, UnitStats s, ElementType el)
        {
            string[] names = { "HP", "SP", "ATK", "INT", "DEF", "AGI" };
            int[] vals = { s.hp, s.sp, s.atk, s.intel, s.def, s.agi };
            bool[] hot =
            {
                el == ElementType.Earth,
                el == ElementType.Water,
                el == ElementType.Fire,
                el == ElementType.Water,
                el == ElementType.Earth,
                el == ElementType.Wind
            };
            float cell = width / 6f;
            for (int i = 0; i < 6; i++)
            {
                var r = new Rect(x + i * cell, y, cell - 8f, 72f);
                Color bg = hot[i]
                    ? Color.Lerp(CreatedHero.ColorOf(el), new Color(0.12f, 0.12f, 0.14f), 0.45f)
                    : new Color(0.12f, 0.13f, 0.16f, 0.95f);
                UiTheme.DrawFill(r, bg);
                var name = new GUIStyle(UiTheme.Hint()) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
                var val = new GUIStyle(UiTheme.Title()) { alignment = TextAnchor.MiddleCenter, fontSize = 22 };
                if (hot[i])
                    val.normal.textColor = Color.white;
                GUI.Label(new Rect(r.x, r.y + 6, r.width, 22), names[i], name);
                GUI.Label(new Rect(r.x, r.y + 28, r.width, 36), vals[i].ToString(), val);
            }
        }

        void Confirm()
        {
            string n = (_name ?? "").Trim();
            if (n.Length < 1)
            {
                _error = "ใส่ชื่อก่อน";
                return;
            }

            if (n.Length > 16)
                n = n.Substring(0, 16);

            PartyManager.Ensure().BeginNewGame(n, _element);
            SaveService.SessionHydrated = true;
            SaveService.Save(new Vector3(-6.2f, 0f, 0f));
            SceneManager.LoadScene("World");
        }

        static void ApplyCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
    }
}
