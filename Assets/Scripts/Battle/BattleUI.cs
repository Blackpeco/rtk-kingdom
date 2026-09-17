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
        AutoBattleController _auto;
        readonly List<string> _log = new List<string>();
        readonly List<Popup> _popups = new List<Popup>();
        bool _showLevelUp;
        int _levelPick;
        System.Action _afterLevelUp;
        bool _persistSaveAfterLevelUp;

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

        public void Bind(TurnManager turns, AutoBattleController auto = null)
        {
            _turns = turns;
            _auto = auto;
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

        public void NoteRewards(string summary)
        {
            if (!string.IsNullOrEmpty(summary))
                AppendLog(summary);
            if (_banner != null && !string.IsNullOrEmpty(summary))
                _banner.text = (_turns != null ? _turns.Banner : "") + "  ·  " + summary;
        }

        public void OpenLevelUp(System.Action afterDone, bool persistSave = false)
        {
            _showLevelUp = true;
            _afterLevelUp = afterDone;
            _persistSaveAfterLevelUp = persistSave;
            PartyManager pm = PartyManager.Ensure();
            _levelPick = 0;
            for (int i = 0; i < pm.Party.Count; i++)
            {
                if (pm.Party[i] != null && pm.Party[i].unspentPoints > 0)
                {
                    _levelPick = i;
                    break;
                }
            }
        }

        void OnGUI()
        {
            DrawAutoToggles();
            if (_showLevelUp)
                DrawLevelUp();

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

        void DrawAutoToggles()
        {
            if (_auto == null)
                return;
            float x = Screen.width - 276f;
            GUI.Box(new Rect(x, 8, 264, 118), "");
            GUI.Label(new Rect(x + 10, 12, 244, 20), "Auto Battle");
            _auto.AutoAttack = GUI.Toggle(new Rect(x + 10, 34, 120, 22), _auto.AutoAttack, "Auto Attack");
            _auto.AutoHeal = GUI.Toggle(new Rect(x + 130, 34, 120, 22), _auto.AutoHeal, "Auto Heal");
            GUI.Label(new Rect(x + 10, 58, 150, 22), "Heal ถ้า HP < " + Mathf.RoundToInt(_auto.HealThresholdPercent) + "%");
            if (GUI.Button(new Rect(x + 168, 56, 36, 24), "−"))
                _auto.HealThresholdPercent = Mathf.Max(10f, _auto.HealThresholdPercent - 10f);
            if (GUI.Button(new Rect(x + 210, 56, 36, 24), "+"))
                _auto.HealThresholdPercent = Mathf.Min(100f, _auto.HealThresholdPercent + 10f);
            GUI.Label(new Rect(x + 10, 86, 244, 22),
                "ปาโต้เยา รักษาเหลือ " + PatoyoHelper.ChargesLeft + "/" + PatoyoHelper.MaxCharges
                + "   สมุนไพร ×" + InventoryService.CountOf(InventoryService.HerbId));
        }

        void DrawLevelUp()
        {
            PartyManager pm = PartyManager.Ensure();
            float w = Mathf.Min(640f, Screen.width - 40f);
            float h = 300f;
            var box = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.5f - h * 0.5f, w, h);
            GUI.Box(box, "");
            GUI.Label(new Rect(box.x + 16, box.y + 10, w - 32, 24),
                string.IsNullOrEmpty(pm.LastRewardSummary) ? "เลเวลอัพ — แจกแต้มสถานะ" : pm.LastRewardSummary);

            if (pm.Party.Count == 0)
            {
                if (GUI.Button(new Rect(box.x + w * 0.5f - 70, box.y + h - 48, 140, 36), ContinueLabel()))
                    FinishLevelUp();
                return;
            }

            _levelPick = Mathf.Clamp(_levelPick, 0, pm.Party.Count - 1);
            float tabX = box.x + 16;
            for (int i = 0; i < pm.Party.Count; i++)
            {
                PartyMember tab = pm.Party[i];
                string t = tab.ShortName + (tab.unspentPoints > 0 ? " +" + tab.unspentPoints : "");
                if (GUI.Button(new Rect(tabX, box.y + 40, 110, 28), t))
                    _levelPick = i;
                tabX += 114;
            }

            PartyMember m = pm.Party[_levelPick];
            UnitStats s = m.EffectiveStats;
            GUI.Label(new Rect(box.x + 16, box.y + 78, w - 32, 22),
                m.ShortName + " / " + m.ThaiName + "  Lv " + m.level
                + "  EXP " + m.exp + "/" + ExpLevelSystem.ExpToNext(m.level)
                + "  แต้ม " + m.unspentPoints);
            GUI.Label(new Rect(box.x + 16, box.y + 102, w - 32, 22),
                "HP " + m.currentHp + "/" + s.hp + "   SP " + m.currentSp + "/" + s.sp
                + "   ATK " + s.atk + "  INT " + s.intel + "  DEF " + s.def + "  AGI " + s.agi);

            if (m.unspentPoints > 0)
            {
                DrawAllocBtn(m, box.x + 16, box.y + 136, "HP +" + ExpLevelSystem.HpPerPoint, ExpLevelSystem.StatKind.Hp);
                DrawAllocBtn(m, box.x + 116, box.y + 136, "SP +" + ExpLevelSystem.SpPerPoint, ExpLevelSystem.StatKind.Sp);
                DrawAllocBtn(m, box.x + 216, box.y + 136, "ATK +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Atk);
                DrawAllocBtn(m, box.x + 316, box.y + 136, "INT +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Intel);
                DrawAllocBtn(m, box.x + 416, box.y + 136, "DEF +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Def);
                DrawAllocBtn(m, box.x + 516, box.y + 136, "AGI +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Agi);
            }
            else
            {
                GUI.Label(new Rect(box.x + 16, box.y + 140, w - 32, 28), "ขุนพลนี้ไม่มีแต้มเหลือ — เลือกแท็บอื่นหรือดำเนินการต่อ");
            }

            if (GUI.Button(new Rect(box.x + w * 0.5f - 80, box.y + h - 48, 160, 36), ContinueLabel()))
                FinishLevelUp();
        }

        static void DrawAllocBtn(PartyMember m, float x, float y, string label, ExpLevelSystem.StatKind stat)
        {
            if (GUI.Button(new Rect(x, y, 96, 32), label))
                ExpLevelSystem.SpendPoint(m, stat);
        }

        string ContinueLabel()
        {
            return _afterLevelUp != null ? "กลับโลก" : "ดำเนินการต่อ";
        }

        void FinishLevelUp()
        {
            _showLevelUp = false;
            if (_persistSaveAfterLevelUp)
                SaveService.Save(EncounterContext.ReturnPosition);
            _persistSaveAfterLevelUp = false;
            System.Action done = _afterLevelUp;
            _afterLevelUp = null;
            if (done != null)
                done();
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

            _commands = MakePanel(root.transform, "Commands", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 52), new Vector2(920, 72));
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
                bool patoyoOk = PatoyoHelper.ChargesLeft > 0;
                bool herbOk = InventoryService.CountOf(InventoryService.HerbId) > 0;
                LayoutButtons(_commands, new[]
                {
                    Btn("โจมตีปกติ", new Color(0.55f, 0.28f, 0.22f), () => _turns.ChooseCommand(BattleCommand.Attack)),
                    Btn("สกิล", new Color(0.22f, 0.38f, 0.62f), () => _turns.ChooseCommand(BattleCommand.Skill)),
                    Btn("ปาโต้เยา", patoyoOk ? new Color(0.72f, 0.32f, 0.48f) : new Color(0.22f, 0.2f, 0.22f),
                        () => _turns.ChooseCommand(BattleCommand.Patoyo)),
                    Btn(herbOk ? "สมุนไพร ×" + InventoryService.CountOf(InventoryService.HerbId) : "ไอเทม",
                        herbOk ? new Color(0.35f, 0.45f, 0.22f) : new Color(0.28f, 0.28f, 0.22f),
                        () => _turns.ChooseCommand(BattleCommand.Item)),
                    Btn("ป้องกัน", new Color(0.25f, 0.45f, 0.32f), () => _turns.ChooseCommand(BattleCommand.Defend)),
                    Btn("หนี", new Color(0.28f, 0.28f, 0.32f), () => _turns.ChooseCommand(BattleCommand.Escape))
                }, 118f);
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
            int n = units.Count;
            float cardH = n >= 5 ? 70f : 100f;
            float gap = n >= 5 ? 6f : 10f;
            float total = n * cardH + Mathf.Max(0, n - 1) * gap;
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(280, Mathf.Max(360f, total + 16f));
            float y = total * 0.5f - cardH * 0.5f;
            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];
                var card = MakePanel(root, "Card" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(260, cardH));
                var img = card.GetComponent<Image>();
                img.color = unit != null && unit == _turns.CurrentActor
                    ? new Color(0.18f, 0.22f, 0.16f, 0.92f)
                    : new Color(0.08f, 0.09f, 0.11f, 0.82f);

                string lv = "";
                if (unit != null && unit.isPlayer)
                {
                    PartyMember member = PartyManager.Ensure().FindByDefinition(unit.definition);
                    if (member != null)
                        lv = "  Lv" + member.level;
                }

                string title = unit == null ? "-" : unit.ShortName + "  " + ThaiElement(unit.Element) + lv;
                var name = MakeText(card, "N", 14, TextAnchor.MiddleLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(240, 22));
                name.text = title;
                name.color = unit != null && unit.IsAlive ? Color.white : new Color(1f, 0.4f, 0.4f);

                if (unit != null)
                {
                    float hpY = n >= 5 ? -34f : -40f;
                    float spY = n >= 5 ? -54f : -68f;
                    MakeBar(card, "HP", new Vector2(0, hpY), unit.currentHp, unit.stats.hp, new Color(0.75f, 0.22f, 0.22f), "HP " + unit.currentHp + "/" + unit.stats.hp);
                    MakeBar(card, "SP", new Vector2(0, spY), unit.currentSp, Mathf.Max(1, unit.stats.sp), new Color(0.22f, 0.45f, 0.82f), "SP " + unit.currentSp + "/" + unit.stats.sp);
                    if (unit.isDefending)
                    {
                        var d = MakeText(card, "D", 11, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -16), new Vector2(80, 18));
                        d.text = "ป้องกัน";
                        d.color = new Color(0.6f, 0.95f, 0.7f);
                    }
                }

                y -= cardH + gap;
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

        void LayoutButtons(RectTransform parent, BtnSpec[] buttons, float width = 140f)
        {
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
