using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TsOnline
{
    /// <summary>
    /// Runtime uGUI: command buttons, target/skill pick, HP/SP, gold ข่มธาตุ! / gray ธาตุต้าน popups.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        TurnManager _turns;
        readonly List<string> _log = new List<string>();
        readonly List<Popup> _popups = new List<Popup>();

        Font _font;
        Canvas _canvas;
        Text _banner;
        Text _logText;
        RectTransform _commands;
        RectTransform _skills;
        RectTransform _targets;
        RectTransform _partyHud;
        RectTransform _enemyHud;
        struct Popup
        {
            public Vector3 world;
            public string text;
            public Color color;
            public float age;
        }

        public void Bind(TurnManager turns)
        {
            _turns = turns;
            _turns.OnChanged += Rebuild;
            _turns.OnLog += AppendLog;
            _turns.OnPopup += (unit, text, color) =>
            {
                if (unit == null)
                    return;
                _popups.Add(new Popup
                {
                    world = unit.transform.position + Vector3.up * 0.7f,
                    text = text,
                    color = color,
                    age = 0f
                });
            };
            BuildCanvas();
            Rebuild();
        }

        void Update()
        {
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                Popup p = _popups[i];
                p.age += Time.deltaTime;
                p.world += Vector3.up * Time.deltaTime * 0.6f;
                _popups[i] = p;
                if (p.age > 1.3f)
                    _popups.RemoveAt(i);
            }
        }

        void OnGUI()
        {
            if (Camera.main == null)
                return;
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            for (int i = 0; i < _popups.Count; i++)
            {
                Popup p = _popups[i];
                Vector3 screen = Camera.main.WorldToScreenPoint(p.world);
                if (screen.z < 0f)
                    continue;
                style.normal.textColor = p.color;
                float y = Screen.height - screen.y;
                GUI.Label(new Rect(screen.x - 90f, y - 18f, 180f, 36f), p.text, style);
            }
        }

        void BuildCanvas()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_font == null)
                _font = Font.CreateDynamicFontFromOSFont("Liberation Sans", 16);

            EnsureEventSystem();

            var root = new GameObject("BattleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            _canvas = root.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 20;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;

            _banner = MakeText(root.transform, "Banner", 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -28), new Vector2(900, 40));
            _banner.fontStyle = FontStyle.Bold;

            _logText = MakeText(root.transform, "Log", 13, TextAnchor.LowerLeft, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 118), new Vector2(980, 96));
            _logText.color = new Color(0.85f, 0.88f, 0.9f);

            _partyHud = MakePanel(root.transform, "PartyHud", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(150, 0), new Vector2(280, 360));
            _enemyHud = MakePanel(root.transform, "EnemyHud", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-150, 0), new Vector2(280, 360));

            _commands = MakePanel(root.transform, "Commands", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 52), new Vector2(760, 72));
            _skills = MakePanel(root.transform, "Skills", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 52), new Vector2(860, 72));
            _targets = MakePanel(root.transform, "Targets", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 52), new Vector2(860, 72));
        }

        void Rebuild()
        {
            if (_turns == null || _banner == null)
                return;

            _banner.text = _turns.Banner ?? "";
            _banner.color = _turns.State == BattleState.Ended
                ? new Color(1f, 0.85f, 0.4f)
                : Color.white;

            RefreshHud(_partyHud, _turns.PlayerUnits);
            RefreshHud(_enemyHud, _turns.EnemyUnits);
            RefreshLog();

            ClearKids(_commands);
            ClearKids(_skills);
            ClearKids(_targets);
            _commands.gameObject.SetActive(false);
            _skills.gameObject.SetActive(false);
            _targets.gameObject.SetActive(false);

            if (_turns.State == BattleState.AwaitingCommand && _turns.CurrentActor != null && _turns.CurrentActor.isPlayer)
            {
                _commands.gameObject.SetActive(true);
                LayoutButtons(_commands, new[]
                {
                    Btn("โจมตีปกติ", new Color(0.55f, 0.28f, 0.22f), () => _turns.ChooseCommand(BattleCommand.Attack)),
                    Btn("สกิล", new Color(0.22f, 0.38f, 0.62f), () => _turns.ChooseCommand(BattleCommand.Skill)),
                    Btn("ไอเทม", new Color(0.35f, 0.35f, 0.22f), () => _turns.ChooseCommand(BattleCommand.Item)),
                    Btn("ป้องกัน", new Color(0.25f, 0.45f, 0.32f), () => _turns.ChooseCommand(BattleCommand.Defend)),
                    Btn("หนี", new Color(0.28f, 0.28f, 0.32f), () => _turns.ChooseCommand(BattleCommand.Escape))
                });
            }
            else if (_turns.State == BattleState.AwaitingSkill && _turns.CurrentActor != null)
            {
                _skills.gameObject.SetActive(true);
                var list = new List<BtnSpec>();
                SkillDefinition[] skills = _turns.CurrentActor.Skills;
                for (int i = 0; i < skills.Length; i++)
                {
                    SkillDefinition skill = skills[i];
                    if (skill == null)
                        continue;
                    bool ok = SkillSystem.CanUse(_turns.CurrentActor, skill);
                    string label = skill.displayName + "  SP " + skill.spCost;
                    Color col = ok ? new Color(0.2f, 0.4f, 0.65f) : new Color(0.2f, 0.2f, 0.22f);
                    SkillDefinition captured = skill;
                    list.Add(Btn(label, col, () =>
                    {
                        if (ok)
                            _turns.ChooseSkill(captured);
                    }));
                }

                list.Add(Btn("ยกเลิก", new Color(0.3f, 0.3f, 0.32f), () => _turns.CancelToCommands()));
                LayoutButtons(_skills, list.ToArray());
            }
            else if (_turns.State == BattleState.AwaitingTarget)
            {
                _targets.gameObject.SetActive(true);
                var list = new List<BtnSpec>();
                List<BattleUnit> options = _turns.CurrentTargetOptions();
                for (int i = 0; i < options.Count; i++)
                {
                    BattleUnit unit = options[i];
                    list.Add(Btn(unit.ShortName + "  HP " + unit.currentHp, unit.ElementColor() * 0.8f, () => _turns.ChooseTarget(unit)));
                }

                list.Add(Btn("ยกเลิก", new Color(0.3f, 0.3f, 0.32f), () => _turns.CancelToCommands()));
                LayoutButtons(_targets, list.ToArray());
            }
        }

        void RefreshHud(RectTransform root, List<BattleUnit> units)
        {
            ClearKids(root);
            float y = 140f;
            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];
                var card = MakePanel(root, "Card" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(260, 100));
                var img = card.GetComponent<Image>();
                img.color = unit != null && unit == _turns.CurrentActor
                    ? new Color(0.18f, 0.22f, 0.16f, 0.92f)
                    : new Color(0.08f, 0.09f, 0.11f, 0.82f);

                string title = unit == null ? "-" : unit.ShortName + "  " + ThaiElement(unit.Element);
                var name = MakeText(card, "N", 14, TextAnchor.MiddleLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(240, 22));
                name.text = title;
                name.color = unit != null && unit.IsAlive ? Color.white : new Color(1f, 0.4f, 0.4f);

                if (unit != null)
                {
                    MakeBar(card, "HP", new Vector2(0, -40), unit.currentHp, unit.stats.hp, new Color(0.75f, 0.22f, 0.22f), "HP " + unit.currentHp + "/" + unit.stats.hp);
                    MakeBar(card, "SP", new Vector2(0, -68), unit.currentSp, Mathf.Max(1, unit.stats.sp), new Color(0.22f, 0.45f, 0.82f), "SP " + unit.currentSp + "/" + unit.stats.sp);
                    if (unit.isDefending)
                    {
                        var d = MakeText(card, "D", 11, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -16), new Vector2(80, 18));
                        d.text = "ป้องกัน";
                        d.color = new Color(0.6f, 0.95f, 0.7f);
                    }
                }

                y -= 110f;
            }
        }

        void MakeBar(Transform parent, string id, Vector2 pos, int current, int max, Color fill, string label)
        {
            var bg = MakePanel((RectTransform)parent, id, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), pos, new Vector2(232, 18));
            bg.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.14f, 1f);
            float pct = max <= 0 ? 0f : Mathf.Clamp01(current / (float)max);
            var bar = MakePanel(bg, "Fill", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(116f * pct, 0), new Vector2(232f * pct, 18));
            bar.GetComponent<Image>().color = fill;
            var t = MakeText(bg, "L", 11, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(232, 18));
            t.text = label;
        }

        void AppendLog(string line)
        {
            _log.Add(line);
            if (_log.Count > 8)
                _log.RemoveAt(0);
            RefreshLog();
        }

        void RefreshLog()
        {
            if (_logText == null)
                return;
            var sb = new StringBuilder();
            for (int i = 0; i < _log.Count; i++)
            {
                if (i > 0)
                    sb.Append('\n');
                sb.Append(_log[i]);
            }

            _logText.text = sb.ToString();
        }

        struct BtnSpec
        {
            public string Label;
            public Color Color;
            public UnityEngine.Events.UnityAction Click;
        }

        static BtnSpec Btn(string label, Color color, UnityEngine.Events.UnityAction click)
        {
            return new BtnSpec { Label = label, Color = color, Click = click };
        }

        void LayoutButtons(RectTransform parent, BtnSpec[] buttons)
        {
            float width = 140f;
            float gap = 10f;
            float total = buttons.Length * width + (buttons.Length - 1) * gap;
            float x = -total * 0.5f + width * 0.5f;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button b = MakeButton(parent, buttons[i].Label, buttons[i].Color, buttons[i].Click);
                var rt = b.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x, 0);
                rt.sizeDelta = new Vector2(width, 48);
                x += width + gap;
            }
        }

        Button MakeButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction click)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(140, 48);
            go.GetComponent<Image>().color = color;
            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = color * 1.15f;
            colors.pressedColor = color * 0.8f;
            btn.colors = colors;
            btn.onClick.AddListener(click);
            var text = MakeText(go.transform, "T", 15, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 48));
            text.text = label;
            text.raycastTarget = false;
            return btn;
        }

        Text MakeText(Transform parent, string name, int size, TextAnchor anchor, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = go.GetComponent<Text>();
            t.font = _font;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        RectTransform MakePanel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f, 0.55f);
            return rt;
        }

        static void ClearKids(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                Destroy(t.GetChild(i).gameObject);
        }

        static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }

        static string ThaiElement(ElementType e)
        {
            switch (e)
            {
                case ElementType.Earth: return "ดิน";
                case ElementType.Water: return "น้ำ";
                case ElementType.Fire: return "ไฟ";
                case ElementType.Wind: return "ลม";
                default: return "-";
            }
        }
    }
}
