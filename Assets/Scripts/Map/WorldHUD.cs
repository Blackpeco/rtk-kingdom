using UnityEngine;

namespace TsOnline
{
    public class WorldHUD : MonoBehaviour
    {
        string _hint = "เมือง (ซ้าย) ปลอดภัย  ·  ป่า (ขวา) ชนมอนสเตอร์   WASD / ลูกศร   E = คุย   P = ปาร์ตี้";

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
            var box = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            var title = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            title.normal.textColor = Color.white;

            PartyMember lead = PartyManager.Ensure().Party.Count > 0 ? PartyManager.Ensure().Party[0] : null;
            string leadTitle = lead != null
                ? lead.ShortName + "  " + CreatedHero.Thai(lead.Element)
                : "World";

            GUI.Box(new Rect(16, 12, 640, 118), "", box);
            GUI.Label(new Rect(28, 16, 616, 26), leadTitle, title);
            GUI.Label(new Rect(28, 42, 616, 28), _hint);
            GUI.Label(new Rect(28, 70, 616, 24), QuestTracker.ObjectiveLine()
                + "   ·   สมุนไพร ×" + InventoryService.CountOf(InventoryService.HerbId));

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

                GUI.Label(new Rect(28, 94, 500, 22), result);
            }

            if (GUI.Button(new Rect(Screen.width - 240, 54, 108, 32), "บันทึก (F5)"))
                SaveService.Save(null);
            if (GUI.Button(new Rect(Screen.width - 126, 54, 110, 32), "โหลด (F9)"))
            {
                if (SaveService.TryLoad())
                    SaveService.TryApplyWorldPosition(SaveService.FindPlayer());
            }
            if (GUI.Button(new Rect(Screen.width - 240, 90, 224, 28), "เกมใหม่ / ลบเซฟ (F8)"))
                SaveService.ResetRuntimeAndCreate();

            if (!string.IsNullOrEmpty(SaveService.LastMessage)
                && Time.realtimeSinceStartup - SaveService.LastMessageAt < 2.5f)
            {
                GUI.Label(new Rect(Screen.width - 240, 122, 224, 22), SaveService.LastMessage);
            }
        }
    }
}
