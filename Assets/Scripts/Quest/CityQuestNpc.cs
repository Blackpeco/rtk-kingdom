using UnityEngine;

namespace TsOnline
{
    /// <summary>ยายเมือง — talk (E) to accept / turn in the one forest quest.</summary>
    public class CityQuestNpc : MonoBehaviour
    {
        bool _near;
        bool _talking;
        string _line = "";

        public static GameObject Spawn()
        {
            var go = new GameObject("Npc_Grandma");
            go.transform.position = new Vector3(-7.5f, 1.7f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeQuad(new Color(0.55f, 0.42f, 0.78f), 14);
            sr.sortingOrder = 5;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.7f;
            go.AddComponent<CityQuestNpc>();

            var label = new GameObject("Name");
            label.transform.SetParent(go.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = "ยายเมือง";
            tm.characterSize = 0.14f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontSize = 22;
            tm.color = Color.white;
            return go;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerWorldController>() != null)
                _near = true;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerWorldController>() != null)
            {
                _near = false;
                _talking = false;
                PlayerWorldController.DialogueOpen = false;
            }
        }

        void Update()
        {
            if (_near && !_talking && Input.GetKeyDown(KeyCode.E))
            {
                _talking = true;
                _line = Greeting();
            }

            if (_talking && Input.GetKeyDown(KeyCode.Escape))
            {
                _talking = false;
                PlayerWorldController.DialogueOpen = false;
            }

            PlayerWorldController.DialogueOpen = _talking;
        }

        void OnDisable()
        {
            PlayerWorldController.DialogueOpen = false;
        }

        void OnGUI()
        {
            if (_near && !_talking)
            {
                Vector3 screen = Camera.main != null
                    ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.9f)
                    : Vector3.zero;
                if (Camera.main != null && screen.z > 0f)
                    GUI.Label(new Rect(screen.x - 70, Screen.height - screen.y - 8, 140, 22), "กด E เพื่อคุย");
            }

            if (!_talking)
                return;

            float w = 520f;
            float h = 168f;
            var box = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 24, w, h);
            GUI.Box(box, "");
            GUI.Label(new Rect(box.x + 16, box.y + 10, w - 32, 24), "ยายเมือง");
            GUI.Label(new Rect(box.x + 16, box.y + 38, w - 32, 60), _line);

            if (QuestTracker.Phase == QuestPhase.None)
            {
                if (GUI.Button(new Rect(box.x + 16, box.y + h - 48, 160, 36), "รับเควสต์"))
                {
                    QuestTracker.Accept();
                    _line = "ป่าด้านขวาไม่สงบ… ชนะการรบในป่า 2 ครั้ง แล้วกลับมาหาฉัน";
                    SaveService.Save(null);
                }
            }
            else if (QuestTracker.Phase == QuestPhase.Ready)
            {
                if (GUI.Button(new Rect(box.x + 16, box.y + h - 48, 160, 36), "ส่งเควสต์"))
                {
                    string msg;
                    if (QuestTracker.TryComplete(out msg))
                    {
                        _line = "ดีมาก เด็ก ๆ  " + msg;
                        SaveService.Save(null);
                    }
                }
            }

            if (GUI.Button(new Rect(box.x + w - 132, box.y + h - 48, 116, 36), "ปิด"))
            {
                _talking = false;
                PlayerWorldController.DialogueOpen = false;
            }
        }

        static string Greeting()
        {
            switch (QuestTracker.Phase)
            {
                case QuestPhase.None:
                    return "ป่าด้านขวารบกวนชาวบ้าน… ช่วยไปชนะการรบในป่า 2 ครั้งให้หน่อยได้ไหม";
                case QuestPhase.Active:
                    return "ยังไม่ครบ  " + QuestTracker.ForestWins + "/" + QuestTracker.WinsNeeded
                        + "  ชนะในป่าแล้วค่อยกลับมา";
                case QuestPhase.Ready:
                    return "กลับมาแล้ว! ส่งเควสต์ได้ — มีสมุนไพรและค่าประสบการณ์ให้";
                default:
                    return string.IsNullOrEmpty(QuestTracker.LastReward)
                        ? "ขอบใจนะ ป่าสงบขึ้นแล้ว"
                        : "ขอบใจนะ  " + QuestTracker.LastReward;
            }
        }
    }
}
