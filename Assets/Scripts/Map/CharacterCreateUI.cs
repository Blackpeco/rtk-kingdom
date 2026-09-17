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
            float w = Mathf.Min(720f, Screen.width - 40f);
            float h = Mathf.Min(560f, Screen.height - 40f);
            var box = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(box, "");

            GUI.Label(new Rect(box.x + 24, box.y + 16, w - 48, 28), "สร้างตัวละคร");
            GUI.Label(new Rect(box.x + 24, box.y + 48, 80, 24), "ชื่อ");
            _name = GUI.TextField(new Rect(box.x + 100, box.y + 46, w - 140, 28), _name ?? "");

            GUI.Label(new Rect(box.x + 24, box.y + 88, w - 48, 22), "เลือกธาตุ  (ดิน น้ำ ไฟ ลม)");

            float btnW = (w - 64f) / 4f;
            for (int i = 0; i < Choices.Length; i++)
            {
                ElementType el = Choices[i];
                var r = new Rect(box.x + 24 + i * (btnW + 8), box.y + 116, btnW, 72);
                Color prev = GUI.color;
                GUI.color = CreatedHero.ColorOf(el);
                string label = CreatedHero.English(el) + "\n" + CreatedHero.Thai(el);
                if (_element == el)
                    label = "▸ " + label;
                if (GUI.Button(r, label))
                    _element = el;
                GUI.color = prev;
            }

            UnitStats s = CreatedHero.StatsFor(_element);
            GUI.Label(new Rect(box.x + 24, box.y + 200, w - 48, 28), CreatedHero.RoleBlurb(_element));
            GUI.Label(new Rect(box.x + 24, box.y + 232, w - 48, 48),
                "HP " + s.hp + "   SP " + s.sp + "   ATK " + s.atk
                + "   INT " + s.intel + "   DEF " + s.def + "   AGI " + s.agi);

            GUI.Label(new Rect(box.x + 24, box.y + 288, w - 48, 40),
                "ตัวนี้เป็นหัวหน้าปาร์ตี้  ขุนพล 6 คนยังปลดล็อกในแผงปาร์ตี้ (P) ภายหลัง");

            if (!string.IsNullOrEmpty(_error))
                GUI.Label(new Rect(box.x + 24, box.y + 336, w - 48, 24), _error);

            if (GUI.Button(new Rect(box.x + w * 0.5f - 90, box.y + h - 56, 180, 40), "เริ่มเดินทาง"))
                Confirm();
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
