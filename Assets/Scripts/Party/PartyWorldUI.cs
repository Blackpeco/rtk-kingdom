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
            float rowH = 26f;
            float h = 34f + n * rowH;
            float top = UiTheme.WorldHudBottom;
            var box = new Rect(16, top, 460, h + 10);
            UiTheme.DrawPanel(box);
            GUI.Label(new Rect(box.x + 10, box.y + 6, 440, 22),
                "ปาร์ตี้ (สูงสุด 5)  —  แถวนี้คือลำดับในโลก / AGI เรียงตาในรบ", UiTheme.Hint());
            float y = box.y + 32f;
            for (int i = 0; i < n; i++)
            {
                PartyMember m = pm.Party[i];
                if (m == null)
                    continue;
                UnitStats s = m.EffectiveStats;
                Color el = CreatedHero.ColorOf(m.Element);
                UiTheme.DrawFill(new Rect(box.x + 8, y + 4, 8, 18), el);
                string names = m.IsCreatedLead
                    ? m.ShortName + "  " + CreatedHero.Thai(m.Element)
                    : m.ShortName + " / " + m.ThaiName;
                var line = new GUIStyle(UiTheme.Body()) { fontSize = 15 };
                GUI.Label(new Rect(box.x + 22, y, 250, 24),
                    (i + 1) + ". " + names, line);
                var vitals = new GUIStyle(UiTheme.Tiny()) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
                vitals.normal.textColor = UiTheme.Hp;
                GUI.Label(new Rect(box.x + 272, y, 180, 24),
                    "Lv" + m.level + "   HP " + m.currentHp + "/" + s.hp
                    + (m.unspentPoints > 0 ? "  +" + m.unspentPoints : ""),
                    vitals);
                y += rowH;
            }

            if (!string.IsNullOrEmpty(pm.LastRewardSummary) && EncounterContext.LastEnd == BattleEndKind.Win)
                GUI.Label(new Rect(16, top + h + 16, 520, 24), pm.LastRewardSummary, UiTheme.Hint());
        }

        void DrawToggle(PartyManager pm)
        {
            string label = _open ? "ปิดปาร์ตี้ (P)" : "ปาร์ตี้ / เลเวลอัพ (P)";
            if (pm.PendingPoints() > 0)
                label += "  [" + pm.PendingPoints() + " pts]";
            if (GUI.Button(new Rect(Screen.width - 268, 12, 248, 40), label, UiTheme.Button()))
                _open = !_open;
        }

        void DrawPanel(PartyManager pm)
        {
            float w = Mathf.Min(960f, Screen.width - 32f);
            float h = Mathf.Min(600f, Screen.height - 40f);
            var box = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            UiTheme.DrawPanel(box);
            GUI.Label(new Rect(box.x + 18, box.y + 10, w - 36, 28),
                "ปาร์ตี้ — สลับลำดับ / เพิ่มจาก 6 ขุนพล   (หัวหน้าเอาออกไม่ได้ · ตาในรบเรียง AGI)",
                UiTheme.Title());

            float col = (w - 48f) * 0.5f;
            GUI.Label(new Rect(box.x + 18, box.y + 44, col, 22),
                "กำลังเดินทาง (" + pm.Party.Count + "/" + PartyManager.MaxParty + ")", UiTheme.Body());
            for (int i = 0; i < pm.Party.Count; i++)
            {
                PartyMember m = pm.Party[i];
                float y = box.y + 72 + i * 42;
                if (m != null)
                    UiTheme.DrawFill(new Rect(box.x + 18, y + 6, 8, 26), CreatedHero.ColorOf(m.Element));
                if (GUI.Button(new Rect(box.x + 30, y, col - 180, 38), LineLabel(m, true), UiTheme.Button()))
                    _selected = RosterIndex(pm, m);
                if (GUI.Button(new Rect(box.x + 18 + col - 164, y, 46, 38), "▲", UiTheme.Button()))
                    pm.MoveParty(i, -1);
                if (GUI.Button(new Rect(box.x + 18 + col - 114, y, 46, 38), "▼", UiTheme.Button()))
                    pm.MoveParty(i, 1);
                if (m != null && m.IsCreatedLead)
                {
                    UiTheme.DrawFill(new Rect(box.x + 18 + col - 64, y + 4, 56, 30), new Color(0.72f, 0.52f, 0.12f, 0.95f));
                    var badge = new GUIStyle(UiTheme.Tiny()) { fontStyle = FontStyle.Bold, fontSize = 12 };
                    badge.normal.textColor = UiTheme.LeadGold;
                    GUI.Label(new Rect(box.x + 18 + col - 64, y + 4, 56, 30), "หัวหน้า", badge);
                }
                else if (GUI.Button(new Rect(box.x + 18 + col - 64, y, 56, 38), "ออก", UiTheme.Button()))
                    pm.TryRemoveFromParty(m);
            }

            GUI.Label(new Rect(box.x + 28 + col, box.y + 44, col, 22), "ปลดล็อกแล้ว (6 ขุนพลตัวอย่าง)", UiTheme.Body());
            for (int i = 0; i < pm.Roster.Count; i++)
            {
                PartyMember m = pm.Roster[i];
                float y = box.y + 72 + i * 42;
                bool inParty = pm.InParty(m);
                string extra = inParty ? "  [ในปาร์ตี้]" : "";
                if (GUI.Button(new Rect(box.x + 28 + col, y, col - 100, 38), LineLabel(m, false) + extra, UiTheme.Button()))
                    _selected = i;
                if (!inParty && GUI.Button(new Rect(box.x + 28 + col + col - 96, y, 88, 38), "เข้า", UiTheme.Button()))
                    pm.TryAddToParty(m);
            }

            DrawAlloc(pm, new Rect(box.x + 18, box.y + h - 176, w - 36, 158));
        }

        void DrawAlloc(PartyManager pm, Rect area)
        {
            UiTheme.DrawPanel(area, new Color(0.09f, 0.10f, 0.13f, 0.96f));
            PartyMember m = Selected(pm);
            if (m == null)
            {
                GUI.Label(new Rect(area.x + 14, area.y + 14, area.width - 28, 40),
                    "เลือกขุนพลด้านบนเพื่อดูค่าสถานะและแจกแต้มเลเวลอัพ", UiTheme.Body());
                return;
            }

            UnitStats s = m.EffectiveStats;
            GUI.Label(new Rect(area.x + 14, area.y + 8, area.width - 28, 24),
                m.ShortName + " / " + m.ThaiName
                + "  Lv " + m.level + "  EXP " + m.exp + "/" + ExpLevelSystem.ExpToNext(m.level)
                + "  แต้มเหลือ " + m.unspentPoints, UiTheme.Title());

            var hp = new GUIStyle(UiTheme.Body());
            hp.normal.textColor = UiTheme.Hp;
            GUI.Label(new Rect(area.x + 14, area.y + 36, 220, 24),
                "HP " + m.currentHp + "/" + s.hp, hp);
            var sp = new GUIStyle(UiTheme.Body());
            sp.normal.textColor = UiTheme.Sp;
            GUI.Label(new Rect(area.x + 230, area.y + 36, 160, 24),
                "SP " + m.currentSp + "/" + s.sp, sp);
            GUI.Label(new Rect(area.x + 400, area.y + 36, area.width - 420, 24),
                "ATK " + s.atk + "  INT " + s.intel + "  DEF " + s.def + "  AGI " + s.agi, UiTheme.Body());

            if (m.unspentPoints <= 0)
            {
                GUI.Label(new Rect(area.x + 14, area.y + 68, area.width - 28, 40),
                    "ยังไม่มีแต้ม — ชนะการรบเพื่อได้ EXP (เลเวลละ 3 แต้ม จน Lv " + ExpLevelSystem.MaxLevel + ")",
                    UiTheme.Hint());
                return;
            }

            DrawStatBtn(m, area.x + 14, area.y + 70, "HP +" + ExpLevelSystem.HpPerPoint, ExpLevelSystem.StatKind.Hp);
            DrawStatBtn(m, area.x + 132, area.y + 70, "SP +" + ExpLevelSystem.SpPerPoint, ExpLevelSystem.StatKind.Sp);
            DrawStatBtn(m, area.x + 250, area.y + 70, "ATK +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Atk);
            DrawStatBtn(m, area.x + 368, area.y + 70, "INT +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Intel);
            DrawStatBtn(m, area.x + 486, area.y + 70, "DEF +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Def);
            DrawStatBtn(m, area.x + 604, area.y + 70, "AGI +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Agi);
            GUI.Label(new Rect(area.x + 14, area.y + 112, area.width - 28, 36),
                "แต้มเข้าสู่สมาชิกปาร์ตี้ทันที — รบครั้งถัดไปใช้ค่าสถานะใหม่ (รวม HP/SP ปัจจุบัน)", UiTheme.Hint());
        }

        static void DrawStatBtn(PartyMember m, float x, float y, string label, ExpLevelSystem.StatKind stat)
        {
            if (GUI.Button(new Rect(x, y, 110, 36), label, UiTheme.Button()))
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
