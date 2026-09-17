using UnityEngine;

namespace TsOnline
{
    /// <summary>ยายเมือง — talk (E) to accept / turn in the one forest quest. Not shown while the party panel is open.</summary>
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
            sr.sprite = WorldArt.MakeShape(new Color(0.62f, 0.42f, 0.88f), 22, WorldArt.Shape.Triangle);
            sr.sortingOrder = 5;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.7f;
            go.AddComponent<CityQuestNpc>();
            WorldArt.MakeLabel(go.transform, "ยายเมือง", new Vector3(0f, 0.72f, 0f), Color.white, 0.13f, 24);
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
            if (!PlayerWorldController.CanOpenDialogue)
            {
                _talking = false;
                PlayerWorldController.DialogueOpen = false;
                return;
            }

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
            if (!PlayerWorldController.CanOpenDialogue)
                return;

            if (_near && !_talking)
            {
                Vector3 screen = Camera.main != null
                    ? Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.9f)
                    : Vector3.zero;
                if (Camera.main != null && screen.z > 0f)
                    GUI.Label(new Rect(screen.x - 80, Screen.height - screen.y - 10, 160, 26), "กด E เพื่อคุย", UiTheme.Body());
            }

            if (!_talking)
                return;

            float w = Mathf.Min(560f, Mathf.Max(280f, Screen.width - 40f));
            float h = 188f;
            var box = new Rect((Screen.width - w) * 0.5f, Mathf.Max(12f, Screen.height - h - 24), w, h);
            UiTheme.DrawPanel(box);
            GUI.Label(new Rect(box.x + 16, box.y + 10, w - 32, 28), "ยายเมือง", UiTheme.Title());
            GUI.Label(new Rect(box.x + 16, box.y + 44, w - 32, 72), _line, UiTheme.Body());

            float inner = w - 32f;
            float acceptW = Mathf.Min(168f, inner * 0.48f);
            float closeW = Mathf.Min(124f, inner * 0.40f);
            if (QuestTracker.Phase == QuestPhase.None)
            {
                if (GUI.Button(new Rect(box.x + 16, box.y + h - 52, acceptW, 40), "รับเควสต์", UiTheme.Button()))
                {
                    QuestTracker.Accept();
                    _line = "ป่าด้านขวาไม่สงบ… ชนะการรบในป่า 2 ครั้ง แล้วกลับมาหาฉัน";
                    SaveService.Save(null);
                }
            }
            else if (QuestTracker.Phase == QuestPhase.Ready)
            {
                if (GUI.Button(new Rect(box.x + 16, box.y + h - 52, acceptW, 40), "ส่งเควสต์", UiTheme.Button()))
                {
                    string msg;
                    if (QuestTracker.TryComplete(out msg))
                    {
                        _line = "ดีมาก เด็ก ๆ  " + msg;
                        SaveService.Save(null);
                    }
                }
            }

            if (GUI.Button(new Rect(box.x + w - 16 - closeW, box.y + h - 52, closeW, 40), "ปิด", UiTheme.Button()))
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
