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
        Texture2D _previewTex;
        ElementType _previewEl = (ElementType)(-1);

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

            // Bottom-anchored confirm + hint so the button never sits on the copy on short Game views.
            float confirmY = box.y + h - 56;
            bool hasErr = !string.IsNullOrEmpty(_error);
            float errY = hasErr ? confirmY - 30 : confirmY;
            float hintY = errY - 44;
            float topEnd = hintY - 8;

            GUI.Label(new Rect(box.x + 28, box.y + 12, w - 56, 28), "สร้างตัวละคร", UiTheme.Title());
            GUI.Label(new Rect(box.x + 28, box.y + 46, 80, 28), "ชื่อ", UiTheme.Body());
            var field = new GUIStyle(GUI.skin.textField) { fontSize = 18 };
            _name = GUI.TextField(new Rect(box.x + 110, box.y + 44, w - 154, 32), _name ?? "", field);

            GUI.Label(new Rect(box.x + 28, box.y + 84, w - 56, 22), "เลือกธาตุ", UiTheme.Body());

            float elY = box.y + 108;
            float leftover = Mathf.Max(120f, topEnd - elY);
            float previewH = leftover >= 210f ? 58f : 0f;
            float statH = leftover < 180f ? 56f : 72f;
            float roleH = 24f;
            float elH = Mathf.Clamp(leftover - statH - roleH - previewH - 14f, 56f, 96f);
            float roleY = elY + elH + 6;
            float previewY = roleY + roleH + 4;
            float statY = previewH > 0f ? previewY + previewH + 4 : roleY + roleH + 4;

            float btnW = (w - 80f) / 4f;
            for (int i = 0; i < Choices.Length; i++)
            {
                ElementType el = Choices[i];
                var r = new Rect(box.x + 28 + i * (btnW + 8), elY, btnW, elH);
                DrawElementButton(r, el);
            }

            UnitStats s = CreatedHero.StatsFor(_element);
            var role = new GUIStyle(UiTheme.Title()) { fontSize = 20 };
            role.normal.textColor = Color.Lerp(CreatedHero.ColorOf(_element), Color.white, 0.35f);
            GUI.Label(new Rect(box.x + 28, roleY, w - 56, roleH), CreatedHero.RoleBlurb(_element), role);

            if (previewH > 0f)
                DrawPreview(new Rect(box.x + 28, previewY, w - 56, previewH), s);

            DrawStatRow(box.x + 28, statY, w - 56, s, _element, statH);

            GUI.Label(new Rect(box.x + 28, hintY, w - 56, 40),
                "ตัวนี้เป็นหัวหน้าปาร์ตี้  ขุนพล 6 คนยังปลดล็อกในแผงปาร์ตี้ (P) ภายหลัง", UiTheme.Hint());

            if (hasErr)
            {
                var err = new GUIStyle(UiTheme.Body());
                err.normal.textColor = new Color(1f, 0.45f, 0.4f);
                GUI.Label(new Rect(box.x + 28, errY, w - 56, 26), _error, err);
            }

            if (GUI.Button(new Rect(box.x + w * 0.5f - 110, confirmY, 220, 46), "เริ่มเดินทาง", UiTheme.Button()))
                Confirm();
        }

        void DrawPreview(Rect r, UnitStats s)
        {
            Color col = CreatedHero.ColorOf(_element);
            UiTheme.DrawFramedPanel(r, new Color(0.09f, 0.10f, 0.13f, 0.96f), col);
            EnsurePreview();
            if (_previewTex != null)
                GUI.DrawTexture(new Rect(r.x + 12, r.y + 6, r.height - 10, r.height - 10), _previewTex, ScaleMode.ScaleToFit, true);
            var name = new GUIStyle(UiTheme.Title()) { fontSize = 18 };
            name.normal.textColor = Color.Lerp(col, Color.white, 0.3f);
            string shown = string.IsNullOrWhiteSpace(_name) ? "ผู้กล้า" : _name.Trim();
            GUI.Label(new Rect(r.x + r.height + 8, r.y + 6, r.width - r.height - 20, 22),
                "หัวหน้า  ·  " + shown + "  ·  " + CreatedHero.Thai(_element), name);
            GUI.Label(new Rect(r.x + r.height + 8, r.y + 28, r.width - r.height - 20, 22),
                "HP " + s.hp + "   SP " + s.sp + "   ATK " + s.atk + "   AGI " + s.agi, UiTheme.Hint());
        }

        void EnsurePreview()
        {
            if (_previewTex != null && _previewEl == _element)
                return;
            Sprite spr = WorldArt.MakeShape(CreatedHero.ColorOf(_element), 48, WorldArt.Shape.Diamond, true,
                WorldArt.ActorMark.Lead);
            _previewTex = spr != null ? spr.texture : null;
            _previewEl = _element;
        }

        void DrawElementButton(Rect r, ElementType el)
        {
            Color col = CreatedHero.ColorOf(el);
            bool selected = _element == el;
            UiTheme.DrawFill(new Rect(r.x - 1f, r.y - 1f, r.width + 2f, r.height + 2f),
                selected ? Color.white : new Color(0f, 0f, 0f, 0.7f));
            UiTheme.DrawFill(r, selected ? Color.Lerp(col, Color.white, 0.16f) : col * 0.72f);
            UiTheme.DrawFill(new Rect(r.x + 4, r.y + 4, r.width - 8, 3f), Color.Lerp(col, Color.white, 0.45f));
            DrawElementGlyph(new Rect(r.x + r.width * 0.5f - 10, r.y + r.height * 0.42f - 8, 20, 16), el, col);

            var thai = new GUIStyle(UiTheme.Title())
            {
                fontSize = r.height < 80f ? 22 : 26,
                alignment = TextAnchor.MiddleCenter
            };
            thai.normal.textColor = Color.white;
            var en = new GUIStyle(UiTheme.Hint()) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
            GUI.Label(new Rect(r.x, r.y + 8, r.width, 28), CreatedHero.Thai(el), thai);
            GUI.Label(new Rect(r.x, r.y + r.height * 0.58f, r.width, 18), CreatedHero.English(el), en);
            if (selected)
                GUI.Label(new Rect(r.x, r.y + r.height - 20, r.width, 18), "▸ เลือกแล้ว", en);

            if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                _element = el;
        }

        static void DrawElementGlyph(Rect r, ElementType el, Color col)
        {
            Color ink = Color.Lerp(col, Color.white, 0.55f);
            switch (el)
            {
                case ElementType.Earth:
                    UiTheme.DrawFill(new Rect(r.x, r.y + 8, r.width, 6), ink);
                    UiTheme.DrawFill(new Rect(r.x + 4, r.y + 3, r.width - 8, 6), ink);
                    UiTheme.DrawFill(new Rect(r.x + 8, r.y, 4, 6), ink);
                    break;
                case ElementType.Water:
                    UiTheme.DrawFill(new Rect(r.x + 7, r.y, 6, 6), ink);
                    UiTheme.DrawFill(new Rect(r.x + 4, r.y + 5, 12, 8), ink);
                    break;
                case ElementType.Fire:
                    UiTheme.DrawFill(new Rect(r.x + 8, r.y, 4, 5), ink);
                    UiTheme.DrawFill(new Rect(r.x + 5, r.y + 4, 10, 6), ink);
                    UiTheme.DrawFill(new Rect(r.x + 3, r.y + 9, 14, 6), ink);
                    break;
                default:
                    UiTheme.DrawFill(new Rect(r.x + 1, r.y + 2, r.width - 4, 3), ink);
                    UiTheme.DrawFill(new Rect(r.x + 3, r.y + 7, r.width - 6, 3), ink);
                    UiTheme.DrawFill(new Rect(r.x + 5, r.y + 12, r.width - 8, 3), ink);
                    break;
            }
        }

        static void DrawStatRow(float x, float y, float width, UnitStats s, ElementType el, float height = 72f)
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
                var r = new Rect(x + i * cell, y, cell - 8f, height);
                Color bg = hot[i]
                    ? Color.Lerp(CreatedHero.ColorOf(el), new Color(0.12f, 0.12f, 0.14f), 0.45f)
                    : new Color(0.12f, 0.13f, 0.16f, 0.95f);
                UiTheme.DrawFill(new Rect(r.x - 1f, r.y - 1f, r.width + 2f, r.height + 2f),
                    hot[i] ? CreatedHero.ColorOf(el) : new Color(0f, 0f, 0f, 0.55f));
                UiTheme.DrawFill(r, bg);
                var name = new GUIStyle(UiTheme.Hint()) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
                var val = new GUIStyle(UiTheme.Title()) { alignment = TextAnchor.MiddleCenter, fontSize = height < 64f ? 18 : 22 };
                if (hot[i])
                    val.normal.textColor = Color.white;
                GUI.Label(new Rect(r.x, r.y + 4, r.width, 20), names[i], name);
                GUI.Label(new Rect(r.x, r.y + height * 0.38f, r.width, height * 0.55f), vals[i].ToString(), val);
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
