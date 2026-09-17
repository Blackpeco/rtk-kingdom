using UnityEngine;

namespace TsOnline
{
    /// <summary>World party strip + lineup / level-up panel (P or ปาร์ตี้). Does not open while ยายเมือง dialog is up.</summary>
    public class PartyWorldUI : MonoBehaviour
    {
        bool _open;
        int _selected = -1;
        Vector2 _partyScroll;
        Vector2 _rosterScroll;

        void Start()
        {
            if (PartyManager.Ensure().PendingPoints() > 0 && PlayerWorldController.CanOpenParty)
                _open = true;
            PlayerWorldController.PartyMenuOpen = _open;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                TryToggle();
            else if (_open && Input.GetKeyDown(KeyCode.Escape))
                _open = false;
            PlayerWorldController.PartyMenuOpen = _open;
        }

        void TryToggle()
        {
            if (_open)
                _open = false;
            else if (PlayerWorldController.CanOpenParty)
                _open = true;
            PlayerWorldController.PartyMenuOpen = _open;
        }

        void OnDisable()
        {
            PlayerWorldController.PartyMenuOpen = false;
        }

        void OnGUI()
        {
            PlayerWorldController.PartyMenuOpen = _open;
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
            UiTheme.DrawFramedPanel(box, UiTheme.Panel, UiTheme.Accent);
            GUI.Label(new Rect(box.x + 12, box.y + 6, 436, 22),
                "ปาร์ตี้ (สูงสุด 5)  —  แถวนี้คือลำดับในโลก / AGI เรียงตาในรบ", UiTheme.Hint());
            var line = new GUIStyle(UiTheme.Body()) { fontSize = 15, wordWrap = false };
            float y = box.y + 32f;
            var barLabel = new GUIStyle(UiTheme.Tiny()) { fontSize = 11 };
            for (int i = 0; i < n; i++)
            {
                PartyMember m = pm.Party[i];
                if (m == null)
                    continue;
                UnitStats s = m.EffectiveStats;
                Color el = CreatedHero.ColorOf(m.Element);
                if (i % 2 == 0)
                    UiTheme.DrawFill(new Rect(box.x + 6, y, box.width - 12, rowH - 2), new Color(1f, 1f, 1f, 0.04f));
                UiTheme.DrawChip(new Rect(box.x + 10, y + 5, 14, 14), el);
                string names = m.IsCreatedLead
                    ? m.ShortName + "  " + CreatedHero.Thai(m.Element)
                    : m.ShortName + " / " + m.ThaiName;
                GUI.Label(new Rect(box.x + 28, y, 200, 24),
                    UiTheme.Ellipsis((i + 1) + ". " + names, line, 200f), line);
                var lv = new GUIStyle(UiTheme.Tiny()) { alignment = TextAnchor.MiddleLeft, fontSize = 13 };
                GUI.Label(new Rect(box.x + 228, y, 44, 24), "Lv" + m.level, lv);
                UiTheme.DrawBar(new Rect(box.x + 272, y + 5, 120, 16),
                    m.currentHp, s.hp, UiTheme.Hp, UiTheme.HpBack,
                    m.currentHp + "/" + s.hp, barLabel);
                if (m.unspentPoints > 0)
                    GUI.Label(new Rect(box.x + 396, y, 50, 24), "+" + m.unspentPoints, lv);
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
                TryToggle();
        }

        void DrawPanel(PartyManager pm)
        {
            float w = Mathf.Min(960f, Screen.width - 32f);
            float h = Mathf.Min(620f, Screen.height - 40f);
            var box = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            UiTheme.DrawFramedPanel(box, UiTheme.Panel, UiTheme.LeadGold);
            GUI.Label(new Rect(box.x + 18, box.y + 8, w - 36, 26), "ปาร์ตี้", UiTheme.Title());
            GUI.Label(new Rect(box.x + 18, box.y + 34, w - 36, 22),
                "สลับลำดับ / เพิ่มจาก 6 ขุนพล · หัวหน้าเอาออกไม่ได้ · ตาในรบเรียง AGI",
                UiTheme.Hint());

            float col = (w - 48f) * 0.5f;
            GUI.Label(new Rect(box.x + 18, box.y + 58, col, 20),
                "กำลังเดินทาง (" + pm.Party.Count + "/" + PartyManager.MaxParty + ")", UiTheme.Body());
            GUI.Label(new Rect(box.x + 28 + col, box.y + 58, col, 20),
                "ปลดล็อกแล้ว (6 ขุนพลตัวอย่าง)", UiTheme.Body());

            bool wrapStats = (w - 36f) < 740f;
            float allocH = wrapStats ? 200f : 168f;
            float listTop = box.y + 80;
            float allocY = box.y + h - allocH - 10;
            float listH = Mathf.Max(72f, allocY - listTop - 8);

            DrawPartyColumn(pm, box.x + 16, listTop, col + 4, listH);
            DrawRosterColumn(pm, box.x + 24 + col, listTop, col + 4, listH);
            DrawAlloc(pm, new Rect(box.x + 18, allocY, w - 36, allocH), wrapStats);
        }

        void DrawPartyColumn(PartyManager pm, float x, float y, float width, float height)
        {
            float inner = Mathf.Max(220f, width - 20f);
            float contentH = Mathf.Max(height, pm.Party.Count * 42f);
            _partyScroll = GUI.BeginScrollView(new Rect(x, y, width, height), _partyScroll,
                new Rect(0, 0, inner, contentH));
            for (int i = 0; i < pm.Party.Count; i++)
            {
                PartyMember m = pm.Party[i];
                float rowY = i * 42;
                bool selected = m != null && RosterIndex(pm, m) == _selected;
                UiTheme.DrawFill(new Rect(0, rowY, inner, 40),
                    selected ? new Color(0.28f, 0.22f, 0.10f, 0.55f)
                    : (i % 2 == 0 ? new Color(1f, 1f, 1f, 0.04f) : new Color(0f, 0f, 0f, 0.10f)));
                if (m != null)
                    UiTheme.DrawChip(new Rect(2, rowY + 10, 16, 16), CreatedHero.ColorOf(m.Element));
                if (GUI.Button(new Rect(22, rowY, inner - 192, 38), LineLabel(m, true), UiTheme.Button()))
                    _selected = RosterIndex(pm, m);
                if (GUI.Button(new Rect(inner - 166, rowY, 46, 38), "▲", UiTheme.Button()))
                    pm.MoveParty(i, -1);
                if (GUI.Button(new Rect(inner - 116, rowY, 46, 38), "▼", UiTheme.Button()))
                    pm.MoveParty(i, 1);
                if (m != null && m.IsCreatedLead)
                    DrawLeadBadge(new Rect(inner - 66, rowY + 4, 56, 30));
                else if (GUI.Button(new Rect(inner - 66, rowY, 56, 38), "ออก", UiTheme.Button()))
                    pm.TryRemoveFromParty(m);
            }

            GUI.EndScrollView();
        }

        void DrawRosterColumn(PartyManager pm, float x, float y, float width, float height)
        {
            float inner = Mathf.Max(180f, width - 20f);
            float contentH = Mathf.Max(height, pm.Roster.Count * 42f);
            _rosterScroll = GUI.BeginScrollView(new Rect(x, y, width, height), _rosterScroll,
                new Rect(0, 0, inner, contentH));
            for (int i = 0; i < pm.Roster.Count; i++)
            {
                PartyMember m = pm.Roster[i];
                float rowY = i * 42;
                bool inParty = pm.InParty(m);
                bool selected = i == _selected;
                UiTheme.DrawFill(new Rect(0, rowY, inner, 40),
                    selected ? new Color(0.28f, 0.22f, 0.10f, 0.55f)
                    : (i % 2 == 0 ? new Color(1f, 1f, 1f, 0.04f) : new Color(0f, 0f, 0f, 0.10f)));
                if (m != null)
                    UiTheme.DrawChip(new Rect(2, rowY + 10, 16, 16), CreatedHero.ColorOf(m.Element));
                string extra = inParty ? "  [ในปาร์ตี้]" : "";
                if (GUI.Button(new Rect(22, rowY, inner - 118, 38), LineLabel(m, false) + extra, UiTheme.Button()))
                    _selected = i;
                if (!inParty && GUI.Button(new Rect(inner - 92, rowY, 88, 38), "เข้า", UiTheme.Button()))
                    pm.TryAddToParty(m);
            }

            GUI.EndScrollView();
        }

        void DrawAlloc(PartyManager pm, Rect area, bool wrapStats)
        {
            UiTheme.DrawFramedPanel(area, new Color(0.09f, 0.10f, 0.13f, 0.96f), UiTheme.Accent);
            PartyMember m = Selected(pm);
            if (m == null)
            {
                GUI.Label(new Rect(area.x + 14, area.y + 14, area.width - 28, 40),
                    "เลือกขุนพลด้านบนเพื่อดูค่าสถานะและแจกแต้มเลเวลอัพ", UiTheme.Body());
                return;
            }

            UnitStats s = m.EffectiveStats;
            var nameStyle = new GUIStyle(UiTheme.Body()) { fontStyle = FontStyle.Bold, wordWrap = false };
            GUI.Label(new Rect(area.x + 14, area.y + 8, area.width - 28, 24),
                UiTheme.Ellipsis(
                    m.ShortName + " / " + m.ThaiName
                    + "  Lv " + m.level + "  EXP " + m.exp + "/" + ExpLevelSystem.ExpToNext(m.level)
                    + "  แต้มเหลือ " + m.unspentPoints,
                    nameStyle, area.width - 28),
                nameStyle);

            var barLabel = new GUIStyle(UiTheme.Tiny()) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            UiTheme.DrawBar(new Rect(area.x + 14, area.y + 34, 200, 18),
                m.currentHp, s.hp, UiTheme.Hp, UiTheme.HpBack,
                "HP " + m.currentHp + "/" + s.hp, barLabel);
            UiTheme.DrawBar(new Rect(area.x + 222, area.y + 34, 170, 18),
                m.currentSp, Mathf.Max(1, s.sp), UiTheme.Sp, UiTheme.SpBack,
                "SP " + m.currentSp + "/" + s.sp, barLabel);
            GUI.Label(new Rect(area.x + 400, area.y + 32, Mathf.Max(80f, area.width - 420), 24),
                "ATK " + s.atk + "  INT " + s.intel + "  DEF " + s.def + "  AGI " + s.agi, UiTheme.Body());

            if (m.unspentPoints <= 0)
            {
                GUI.Label(new Rect(area.x + 14, area.y + 68, area.width - 28, 40),
                    "ยังไม่มีแต้ม — ชนะการรบเพื่อได้ EXP (เลเวลละ 3 แต้ม จน Lv " + ExpLevelSystem.MaxLevel + ")",
                    UiTheme.Hint());
                return;
            }

            float btnY = area.y + 68;
            if (!wrapStats)
            {
                DrawStatBtn(m, area.x + 14, btnY, "HP +" + ExpLevelSystem.HpPerPoint, ExpLevelSystem.StatKind.Hp);
                DrawStatBtn(m, area.x + 132, btnY, "SP +" + ExpLevelSystem.SpPerPoint, ExpLevelSystem.StatKind.Sp);
                DrawStatBtn(m, area.x + 250, btnY, "ATK +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Atk);
                DrawStatBtn(m, area.x + 368, btnY, "INT +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Intel);
                DrawStatBtn(m, area.x + 486, btnY, "DEF +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Def);
                DrawStatBtn(m, area.x + 604, btnY, "AGI +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Agi);
                GUI.Label(new Rect(area.x + 14, area.y + 112, area.width - 28, 36),
                    "แต้มเข้าสู่สมาชิกปาร์ตี้ทันที — รบครั้งถัดไปใช้ค่าสถานะใหม่ (รวม HP/SP ปัจจุบัน)", UiTheme.Hint());
                return;
            }

            float cell = (area.width - 28f) / 3f;
            DrawStatBtn(m, area.x + 14, btnY, "HP +" + ExpLevelSystem.HpPerPoint, ExpLevelSystem.StatKind.Hp, cell - 8f);
            DrawStatBtn(m, area.x + 14 + cell, btnY, "SP +" + ExpLevelSystem.SpPerPoint, ExpLevelSystem.StatKind.Sp, cell - 8f);
            DrawStatBtn(m, area.x + 14 + cell * 2, btnY, "ATK +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Atk, cell - 8f);
            DrawStatBtn(m, area.x + 14, btnY + 40, "INT +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Intel, cell - 8f);
            DrawStatBtn(m, area.x + 14 + cell, btnY + 40, "DEF +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Def, cell - 8f);
            DrawStatBtn(m, area.x + 14 + cell * 2, btnY + 40, "AGI +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Agi, cell - 8f);
            GUI.Label(new Rect(area.x + 14, area.y + 152, area.width - 28, 36),
                "แต้มเข้าสู่สมาชิกปาร์ตี้ทันที — รบครั้งถัดไปใช้ค่าสถานะใหม่ (รวม HP/SP ปัจจุบัน)", UiTheme.Hint());
        }

        static void DrawLeadBadge(Rect r)
        {
            UiTheme.DrawFill(r, new Color(0.20f, 0.15f, 0.05f, 0.96f));
            UiTheme.DrawBorder(r, UiTheme.LeadGold, 2f);
            var badge = new GUIStyle(UiTheme.Tiny()) { fontStyle = FontStyle.Bold, fontSize = 12 };
            badge.normal.textColor = UiTheme.LeadGold;
            GUI.Label(r, "หัวหน้า", badge);
        }

        static void DrawStatBtn(PartyMember m, float x, float y, string label, ExpLevelSystem.StatKind stat, float width = 110f)
        {
            if (GUI.Button(new Rect(x, y, width, 36), label, UiTheme.Button()))
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
