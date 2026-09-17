using UnityEngine;

namespace TsOnline
{
    public class WorldHUD : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
                SaveService.Save(null);
            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (SaveService.TryLoad())
                    SaveService.TryApplyWorldPosition(SaveService.FindPlayer());
            }
            if (Input.GetKeyDown(KeyCode.F8))
                SaveService.ResetRuntimeAndCreate();
        }

        void OnGUI()
        {
            PartyMember lead = PartyManager.Ensure().Party.Count > 0 ? PartyManager.Ensure().Party[0] : null;
            float boxW = Mathf.Min(720f, Screen.width - 280f);
            var box = new Rect(16, 12, boxW, UiTheme.WorldHudHeight);
            UiTheme.DrawPanel(box);

            string leadTitle = lead != null
                ? lead.ShortName + "   " + CreatedHero.Thai(lead.Element)
                : "World";
            var title = new GUIStyle(UiTheme.Title());
            if (lead != null)
                title.normal.textColor = Color.Lerp(CreatedHero.ColorOf(lead.Element), Color.white, 0.35f);
            GUI.Label(new Rect(box.x + 14, box.y + 8, box.width - 28, 28), leadTitle, title);

            if (lead != null)
            {
                UnitStats s = lead.EffectiveStats;
                var barLabel = new GUIStyle(UiTheme.Tiny()) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
                UiTheme.DrawBar(new Rect(box.x + 14, box.y + 40, box.width - 28, 20),
                    lead.currentHp, s.hp, UiTheme.Hp, UiTheme.HpBack,
                    "HP  " + lead.currentHp + " / " + s.hp, barLabel);
                UiTheme.DrawBar(new Rect(box.x + 14, box.y + 64, box.width - 28, 16),
                    lead.currentSp, Mathf.Max(1, s.sp), UiTheme.Sp, UiTheme.SpBack,
                    "SP  " + lead.currentSp + " / " + s.sp, barLabel);
            }

            // Two reserved lines so wrap never collides with the gold quest band.
            GUI.Label(new Rect(box.x + 14, box.y + 86, box.width - 28, 20),
                "เมือง (ซ้าย) ปลอดภัย  ·  ป่า (ขวา) สุ่มสู้", UiTheme.Hint());
            GUI.Label(new Rect(box.x + 14, box.y + 106, box.width - 28, 20),
                "WASD / ลูกศร    E = คุย    P = ปาร์ตี้", UiTheme.Hint());

            var quest = new GUIStyle(UiTheme.Body()) { fontStyle = FontStyle.Bold };
            quest.normal.textColor = new Color(1f, 0.92f, 0.62f);
            GUI.Label(new Rect(box.x + 14, box.y + 128, box.width - 28, 24),
                QuestTracker.ObjectiveLine() + "    สมุนไพร ×" + InventoryService.CountOf(InventoryService.HerbId),
                quest);

            if (EncounterContext.LastEnd != BattleEndKind.None)
            {
                string result;
                switch (EncounterContext.LastEnd)
                {
                    case BattleEndKind.Win: result = "ผลรบล่าสุด: ชนะ — กลับจุดเดิม"; break;
                    case BattleEndKind.Lose: result = "ผลรบล่าสุด: แพ้ — กลับจุดเดิม (ทดสอบ)"; break;
                    case BattleEndKind.Escape: result = "ผลรบล่าสุด: หนีรอด"; break;
                    default: result = ""; break;
                }

                GUI.Label(new Rect(box.x + 14, box.y + 154, box.width - 28, 22), result, UiTheme.Body());
            }

            // Hide OnGUI save/new-game while the party modal is open so F8 cannot steal เข้า/ออก
            // clicks at 1024×576 / 1280×720. F5 / F9 / F8 keys in Update still work.
            if (PlayerWorldController.PartyMenuOpen)
            {
                if (!string.IsNullOrEmpty(SaveService.LastMessage)
                    && Time.realtimeSinceStartup - SaveService.LastMessageAt < 2.5f)
                {
                    GUI.Label(new Rect(Screen.width - 268, 56, 248, 24), SaveService.LastMessage, UiTheme.Hint());
                }

                return;
            }

            float sx = Screen.width - 268f;
            if (GUI.Button(new Rect(sx, 58, 120, 36), "บันทึก (F5)", UiTheme.Button()))
                SaveService.Save(null);
            if (GUI.Button(new Rect(sx + 128, 58, 120, 36), "โหลด (F9)", UiTheme.Button()))
            {
                if (SaveService.TryLoad())
                    SaveService.TryApplyWorldPosition(SaveService.FindPlayer());
            }
            if (GUI.Button(new Rect(sx, 98, 248, 34), "เกมใหม่ / ลบเซฟ (F8)", UiTheme.Button()))
                SaveService.ResetRuntimeAndCreate();

            if (!string.IsNullOrEmpty(SaveService.LastMessage)
                && Time.realtimeSinceStartup - SaveService.LastMessageAt < 2.5f)
            {
                GUI.Label(new Rect(sx, 136, 248, 24), SaveService.LastMessage, UiTheme.Hint());
            }
        }
    }
}
