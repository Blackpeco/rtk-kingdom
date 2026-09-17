using UnityEngine;

namespace TsOnline
{
    /// <summary>World party strip + lineup / level-up panel (P or ปาร์ตี้).</summary>
    public class PartyWorldUI : MonoBehaviour
    {
        bool _open;
        int _selected = -1;

        void Start()
        {
            if (PartyManager.Ensure().PendingPoints() > 0)
                _open = true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                _open = !_open;
            else if (_open && Input.GetKeyDown(KeyCode.Escape))
                _open = false;
            PlayerWorldController.PartyMenuOpen = _open;
        }

        void OnDisable()
        {
            PlayerWorldController.PartyMenuOpen = false;
        }

        void OnGUI()
        {
            PartyManager pm = PartyManager.Ensure();
            DrawStrip(pm);
            DrawToggle(pm);
            if (_open)
                DrawPanel(pm);
        }

        void DrawStrip(PartyManager pm)
        {
            int n = pm.Party.Count;
            float h = 28f + n * 22f;
            GUI.Box(new Rect(16, 168, 420, h + 8), "");
            GUI.Label(new Rect(24, 172, 400, 22), "ปาร์ตี้ (สูงสุด 5)  —  แถวนี้คือลำดับในโลก / AGI เรียงตาในรบ");
            float y = 194f;
            for (int i = 0; i < n; i++)
            {
                PartyMember m = pm.Party[i];
                if (m == null)
                    continue;
                UnitStats s = m.EffectiveStats;
                string pts = m.unspentPoints > 0 ? "  +" + m.unspentPoints + " pts" : "";
                string names = m.IsCreatedLead
                    ? m.ShortName + "  " + CreatedHero.Thai(m.Element)
                    : m.ShortName + " / " + m.ThaiName;
                GUI.Label(new Rect(24, y, 404, 20),
                    (i + 1) + ". " + names
                    + "  Lv" + m.level
                    + "  HP " + m.currentHp + "/" + s.hp
                    + "  SP " + m.currentSp + "/" + s.sp
                    + pts);
                y += 22f;
            }

            if (!string.IsNullOrEmpty(pm.LastRewardSummary) && EncounterContext.LastEnd == BattleEndKind.Win)
                GUI.Label(new Rect(16, 168 + h + 12, 520, 22), pm.LastRewardSummary);
        }

        void DrawToggle(PartyManager pm)
        {
            string label = _open ? "ปิดปาร์ตี้ (P)" : "ปาร์ตี้ / เลเวลอัพ (P)";
            if (pm.PendingPoints() > 0)
                label += "  [" + pm.PendingPoints() + " pts]";
            if (GUI.Button(new Rect(Screen.width - 240, 12, 224, 36), label))
                _open = !_open;
        }

        void DrawPanel(PartyManager pm)
        {
            float w = Mathf.Min(920f, Screen.width - 32f);
            float h = Mathf.Min(560f, Screen.height - 48f);
            var box = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(box, "");
            GUI.Label(new Rect(box.x + 16, box.y + 10, w - 32, 24),
                "ปาร์ตี้ — สลับลำดับ / เพิ่มจาก 6 ขุนพล   (ตาในรบยังเรียงตาม AGI)");

            float col = (w - 48f) * 0.5f;
            GUI.Label(new Rect(box.x + 16, box.y + 40, col, 20), "กำลังเดินทาง (" + pm.Party.Count + "/" + PartyManager.MaxParty + ")");
            for (int i = 0; i < pm.Party.Count; i++)
            {
                PartyMember m = pm.Party[i];
                float y = box.y + 64 + i * 36;
                if (GUI.Button(new Rect(box.x + 16, y, col - 150, 32), LineLabel(m, true)))
                    _selected = RosterIndex(pm, m);
                if (GUI.Button(new Rect(box.x + 16 + col - 146, y, 44, 32), "▲"))
                    pm.MoveParty(i, -1);
                if (GUI.Button(new Rect(box.x + 16 + col - 98, y, 44, 32), "▼"))
                    pm.MoveParty(i, 1);
                if (GUI.Button(new Rect(box.x + 16 + col - 50, y, 44, 32), "ออก"))
                    pm.TryRemoveFromParty(m);
            }

            GUI.Label(new Rect(box.x + 24 + col, box.y + 40, col, 20), "ปลดล็อกแล้ว (6 ขุนพลตัวอย่าง)");
            for (int i = 0; i < pm.Roster.Count; i++)
            {
                PartyMember m = pm.Roster[i];
                float y = box.y + 64 + i * 36;
                bool inParty = pm.InParty(m);
                string extra = inParty ? "  [ในปาร์ตี้]" : "";
                if (GUI.Button(new Rect(box.x + 24 + col, y, col - 90, 32), LineLabel(m, false) + extra))
                    _selected = i;
                if (!inParty && GUI.Button(new Rect(box.x + 24 + col + col - 86, y, 80, 32), "เข้า"))
                    pm.TryAddToParty(m);
            }

            DrawAlloc(pm, new Rect(box.x + 16, box.y + h - 168, w - 32, 152));
        }

        void DrawAlloc(PartyManager pm, Rect area)
        {
            GUI.Box(area, "");
            PartyMember m = Selected(pm);
            if (m == null)
            {
                GUI.Label(new Rect(area.x + 12, area.y + 12, area.width - 24, 40),
                    "เลือกขุนพลด้านบนเพื่อดูค่าสถานะและแจกแต้มเลเวลอัพ");
                return;
            }

            UnitStats s = m.EffectiveStats;
            GUI.Label(new Rect(area.x + 12, area.y + 8, area.width - 24, 22),
                m.ShortName + " / " + m.ThaiName
                + "  Lv " + m.level + "  EXP " + m.exp + "/" + ExpLevelSystem.ExpToNext(m.level)
                + "  แต้มเหลือ " + m.unspentPoints);

            GUI.Label(new Rect(area.x + 12, area.y + 32, area.width - 24, 22),
                "HP " + m.currentHp + "/" + s.hp
                + "   SP " + m.currentSp + "/" + s.sp
                + "   ATK " + s.atk + "  INT " + s.intel + "  DEF " + s.def + "  AGI " + s.agi);

            if (m.unspentPoints <= 0)
            {
                GUI.Label(new Rect(area.x + 12, area.y + 58, area.width - 24, 40),
                    "ยังไม่มีแต้ม — ชนะการรบเพื่อได้ EXP (เลเวลละ 3 แต้ม จน Lv " + ExpLevelSystem.MaxLevel + ")");
                return;
            }

            DrawStatBtn(m, area.x + 12, area.y + 60, "HP +" + ExpLevelSystem.HpPerPoint, ExpLevelSystem.StatKind.Hp);
            DrawStatBtn(m, area.x + 122, area.y + 60, "SP +" + ExpLevelSystem.SpPerPoint, ExpLevelSystem.StatKind.Sp);
            DrawStatBtn(m, area.x + 232, area.y + 60, "ATK +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Atk);
            DrawStatBtn(m, area.x + 342, area.y + 60, "INT +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Intel);
            DrawStatBtn(m, area.x + 452, area.y + 60, "DEF +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Def);
            DrawStatBtn(m, area.x + 562, area.y + 60, "AGI +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Agi);
            GUI.Label(new Rect(area.x + 12, area.y + 100, area.width - 24, 40),
                "แต้มเข้าสู่สมาชิกปาร์ตี้ทันที — รบครั้งถัดไปใช้ค่าสถานะใหม่ (รวม HP/SP ปัจจุบัน)");
        }

        static void DrawStatBtn(PartyMember m, float x, float y, string label, ExpLevelSystem.StatKind stat)
        {
            if (GUI.Button(new Rect(x, y, 104, 32), label))
                ExpLevelSystem.SpendPoint(m, stat);
        }

        static string LineLabel(PartyMember m, bool showVitals)
        {
            if (m == null)
                return "-";
            string s = m.IsCreatedLead
                ? m.ShortName + "  " + CreatedHero.Thai(m.Element) + "  Lv" + m.level
                : m.ShortName + " / " + m.ThaiName + "  Lv" + m.level;
            if (showVitals)
            {
                UnitStats st = m.EffectiveStats;
                s += "  HP " + m.currentHp + "/" + st.hp;
            }

            if (m.unspentPoints > 0)
                s += "  +" + m.unspentPoints + " pts";
            return s;
        }

        static int RosterIndex(PartyManager pm, PartyMember m)
        {
            return pm.Roster.IndexOf(m);
        }

        PartyMember Selected(PartyManager pm)
        {
            if (_selected >= 0 && _selected < pm.Roster.Count)
                return pm.Roster[_selected];
            if (pm.Party.Count > 0)
                return pm.Party[0];
            return null;
        }
    }
}
