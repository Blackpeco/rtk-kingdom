using UnityEngine;

namespace TsOnline
{
    public class WorldHUD : MonoBehaviour
    {
        string _hint = "เมือง (ซ้าย) ปลอดภัย  ·  ป่า (ขวา) ชนมอนสเตอร์เพื่อเข้า Battle   WASD / ลูกศร";

        void OnGUI()
        {
            var box = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            var title = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            title.normal.textColor = Color.white;

            GUI.Box(new Rect(16, 12, 620, 78), "", box);
            GUI.Label(new Rect(28, 18, 596, 28), "World — Step 4", title);
            GUI.Label(new Rect(28, 46, 596, 36), _hint + "   P = ปาร์ตี้");

            if (EncounterContext.LastEnd == BattleEndKind.None)
                return;

            string result;
            switch (EncounterContext.LastEnd)
            {
                case BattleEndKind.Win: result = "ผลรบล่าสุด: ชนะ — กลับจุดเดิม"; break;
                case BattleEndKind.Lose: result = "ผลรบล่าสุด: แพ้ — กลับจุดเดิม (ทดสอบ)"; break;
                case BattleEndKind.Escape: result = "ผลรบล่าสุด: หนีรอด"; break;
                default: result = ""; break;
            }

            GUI.Label(new Rect(16, 96, 480, 24), result);
        }
    }
}
