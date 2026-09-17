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
            public bool emphasis;
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
                bool emphasis = !string.IsNullOrEmpty(text)
                    && (text.IndexOf("ข่มธาตุ") >= 0 || text.IndexOf("ธาตุต้าน") >= 0);
                _popups.Add(new Popup
                {
                    world = unit.transform.position + Vector3.up * 0.75f,
                    text = text,
                    color = color,
                    age = 0f,
                    emphasis = emphasis
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
                p.world += Vector3.up * Time.deltaTime * (p.emphasis ? 0.75f : 0.6f);
                _popups[i] = p;
                if (p.age > (p.emphasis ? 1.85f : 1.3f))
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

        public void OpenLevelUp(System.Action afterDone)
        {
            _showLevelUp = true;
            _afterLevelUp = afterDone;
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
            for (int i = 0; i < _popups.Count; i++)
            {
                Popup p = _popups[i];
                Vector3 screen = Camera.main.WorldToScreenPoint(p.world);
                if (screen.z < 0f)
                    continue;
                float y = Screen.height - screen.y;
                float w = p.emphasis ? 280f : 200f;
                float h = p.emphasis ? 48f : 38f;
                var rect = new Rect(screen.x - w * 0.5f, y - h * 0.5f, w, h);
                if (p.emphasis)
                    UiTheme.DrawFill(rect, new Color(0f, 0f, 0f, 0.62f));
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = p.emphasis ? 30 : 24,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                style.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
                GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), p.text, style);
                style.normal.textColor = p.color;
                GUI.Label(rect, p.text, style);
            }
        }

        void DrawAutoToggles()
        {
            if (_auto == null)
                return;
            float x = Screen.width - 320f;
            UiTheme.DrawPanel(new Rect(x, 8, 308, 132));
            GUI.Label(new Rect(x + 12, 12, 284, 26), "Auto Battle", UiTheme.Title());
            var toggle = new GUIStyle(GUI.skin.toggle) { fontSize = 16 };
            toggle.normal.textColor = Color.white;
            toggle.onNormal.textColor = Color.white;
            _auto.AutoAttack = GUI.Toggle(new Rect(x + 14, 44, 140, 26), _auto.AutoAttack, "Auto Attack", toggle);
            _auto.AutoHeal = GUI.Toggle(new Rect(x + 164, 44, 130, 26), _auto.AutoHeal, "Auto Heal", toggle);
            GUI.Label(new Rect(x + 14, 74, 170, 26), "Heal ถ้า HP < " + Mathf.RoundToInt(_auto.HealThresholdPercent) + "%", UiTheme.Body());
            if (GUI.Button(new Rect(x + 196, 72, 44, 30), "−", UiTheme.Button()))
                _auto.HealThresholdPercent = Mathf.Max(10f, _auto.HealThresholdPercent - 10f);
            if (GUI.Button(new Rect(x + 246, 72, 44, 30), "+", UiTheme.Button()))
                _auto.HealThresholdPercent = Mathf.Min(100f, _auto.HealThresholdPercent + 10f);
            GUI.Label(new Rect(x + 14, 104, 284, 26),
                "ปาโต้เยา รักษาเหลือ " + PatoyoHelper.ChargesLeft + "/" + PatoyoHelper.MaxCharges
                + "   สมุนไพร ×" + InventoryService.CountOf(InventoryService.HerbId),
                UiTheme.Hint());
        }

        void DrawLevelUp()
        {
            PartyManager pm = PartyManager.Ensure();
            float w = Mathf.Min(680f, Screen.width - 40f);
            float h = 320f;
            var box = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.5f - h * 0.5f, w, h);
            UiTheme.DrawPanel(box);
            GUI.Label(new Rect(box.x + 16, box.y + 10, w - 32, 28),
                string.IsNullOrEmpty(pm.LastRewardSummary) ? "เลเวลอัพ — แจกแต้มสถานะ" : pm.LastRewardSummary,
                UiTheme.Title());

            if (pm.Party.Count == 0)
            {
                if (GUI.Button(new Rect(box.x + w * 0.5f - 80, box.y + h - 52, 160, 40), ContinueLabel(), UiTheme.Button()))
                    FinishLevelUp();
                return;
            }

            _levelPick = Mathf.Clamp(_levelPick, 0, pm.Party.Count - 1);
            float tabX = box.x + 16;
            for (int i = 0; i < pm.Party.Count; i++)
            {
                PartyMember tab = pm.Party[i];
                string t = tab.ShortName + (tab.unspentPoints > 0 ? " +" + tab.unspentPoints : "");
                if (GUI.Button(new Rect(tabX, box.y + 44, 118, 32), t, UiTheme.Button()))
                    _levelPick = i;
                tabX += 122;
            }

            PartyMember m = pm.Party[_levelPick];
            UnitStats s = m.EffectiveStats;
            GUI.Label(new Rect(box.x + 16, box.y + 86, w - 32, 24),
                m.ShortName + " / " + m.ThaiName + "  Lv " + m.level
                + "  EXP " + m.exp + "/" + ExpLevelSystem.ExpToNext(m.level)
                + "  แต้ม " + m.unspentPoints, UiTheme.Body());
            GUI.Label(new Rect(box.x + 16, box.y + 112, w - 32, 24),
                "HP " + m.currentHp + "/" + s.hp + "   SP " + m.currentSp + "/" + s.sp
                + "   ATK " + s.atk + "  INT " + s.intel + "  DEF " + s.def + "  AGI " + s.agi,
                UiTheme.Body());

            if (m.unspentPoints > 0)
            {
                DrawAllocBtn(m, box.x + 16, box.y + 146, "HP +" + ExpLevelSystem.HpPerPoint, ExpLevelSystem.StatKind.Hp);
                DrawAllocBtn(m, box.x + 124, box.y + 146, "SP +" + ExpLevelSystem.SpPerPoint, ExpLevelSystem.StatKind.Sp);
                DrawAllocBtn(m, box.x + 232, box.y + 146, "ATK +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Atk);
                DrawAllocBtn(m, box.x + 340, box.y + 146, "INT +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Intel);
                DrawAllocBtn(m, box.x + 448, box.y + 146, "DEF +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Def);
                DrawAllocBtn(m, box.x + 556, box.y + 146, "AGI +" + ExpLevelSystem.CombatPerPoint, ExpLevelSystem.StatKind.Agi);
            }
            else
            {
                GUI.Label(new Rect(box.x + 16, box.y + 150, w - 32, 28),
                    "ขุนพลนี้ไม่มีแต้มเหลือ — เลือกแท็บอื่นหรือดำเนินการต่อ", UiTheme.Hint());
            }

            if (GUI.Button(new Rect(box.x + w * 0.5f - 90, box.y + h - 52, 180, 40), ContinueLabel(), UiTheme.Button()))
                FinishLevelUp();
        }

        static void DrawAllocBtn(PartyMember m, float x, float y, string label, ExpLevelSystem.StatKind stat)
        {
            if (GUI.Button(new Rect(x, y, 100, 36), label, UiTheme.Button()))
                ExpLevelSystem.SpendPoint(m, stat);
        }

        string ContinueLabel()
        {
            return _afterLevelUp != null ? "กลับโลก" : "ดำเนินการต่อ";
        }

        void FinishLevelUp()
        {
            _showLevelUp = false;
            SaveService.Save(EncounterContext.ShouldReturnToWorld
                ? EncounterContext.ReturnPosition
                : (Vector3?)null);
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

            _banner = MakeText(root.transform, "Banner", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -32), new Vector2(980, 44));
            _banner.fontStyle = FontStyle.Bold;

            _logText = MakeText(root.transform, "Log", 14, TextAnchor.LowerLeft, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 132), new Vector2(1000, 100));
            _logText.color = new Color(0.88f, 0.90f, 0.93f);

            _partyHud = MakePanel(root.transform, "PartyHud", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(168, 10), new Vector2(312, 400));
            _partyHud.GetComponent<Image>().color = new Color(0.06f, 0.10f, 0.14f, 0.78f);
            _enemyHud = MakePanel(root.transform, "EnemyHud", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-168, 10), new Vector2(312, 400));
            _enemyHud.GetComponent<Image>().color = new Color(0.16f, 0.07f, 0.07f, 0.78f);

            _commands = MakePanel(root.transform, "Commands", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 58), new Vector2(1080, 88));
            _skills = MakePanel(root.transform, "Skills", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 58), new Vector2(1140, 88));
            _targets = MakePanel(root.transform, "Targets", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 58), new Vector2(1140, 88));
        }

        void Rebuild()
        {
            if (_turns == null || _banner == null)
                return;

            _banner.text = _turns.Banner ?? "";
            _banner.color = _turns.State == BattleState.Ended
                ? new Color(1f, 0.85f, 0.4f)
                : Color.white;

            RefreshHud(_partyHud, _turns.PlayerUnits, true);
            RefreshHud(_enemyHud, _turns.EnemyUnits, false);
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
                    Btn("โจมตีปกติ", new Color(0.62f, 0.28f, 0.22f), () => _turns.ChooseCommand(BattleCommand.Attack)),
                    Btn("สกิล", new Color(0.20f, 0.40f, 0.70f), () => _turns.ChooseCommand(BattleCommand.Skill)),
                    Btn("ปาโต้เยา", patoyoOk ? new Color(0.78f, 0.30f, 0.50f) : new Color(0.22f, 0.2f, 0.22f),
                        () => _turns.ChooseCommand(BattleCommand.Patoyo)),
                    Btn(herbOk ? "สมุนไพร ×" + InventoryService.CountOf(InventoryService.HerbId) : "ไอเทม",
                        herbOk ? new Color(0.32f, 0.52f, 0.22f) : new Color(0.28f, 0.28f, 0.22f),
                        () => _turns.ChooseCommand(BattleCommand.Item)),
                    Btn("ป้องกัน", new Color(0.22f, 0.50f, 0.36f), () => _turns.ChooseCommand(BattleCommand.Defend)),
                    Btn("หนี", new Color(0.30f, 0.30f, 0.34f), () => _turns.ChooseCommand(BattleCommand.Escape))
                }, 148f, 16f);
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
                    Color col = ok ? new Color(0.20f, 0.42f, 0.72f) : new Color(0.2f, 0.2f, 0.22f);
                    SkillDefinition captured = skill;
                    list.Add(Btn(label, col, () =>
                    {
                        if (ok)
                            _turns.ChooseSkill(captured);
                    }));
                }

                list.Add(Btn("ยกเลิก", new Color(0.3f, 0.3f, 0.32f), () => _turns.CancelToCommands()));
                LayoutButtons(_skills, list.ToArray(), 168f, 14f);
            }
            else if (_turns.State == BattleState.AwaitingTarget)
            {
                _targets.gameObject.SetActive(true);
                var list = new List<BtnSpec>();
                List<BattleUnit> options = _turns.CurrentTargetOptions();
                for (int i = 0; i < options.Count; i++)
                {
                    BattleUnit unit = options[i];
                    list.Add(Btn(unit.ShortName + "  HP " + unit.currentHp, unit.ElementColor() * 0.85f, () => _turns.ChooseTarget(unit)));
                }

                list.Add(Btn("ยกเลิก", new Color(0.3f, 0.3f, 0.32f), () => _turns.CancelToCommands()));
                LayoutButtons(_targets, list.ToArray(), 168f, 14f);
            }
        }

        void RefreshHud(RectTransform root, List<BattleUnit> units, bool playerSide)
        {
            ClearKids(root);
            var header = MakeText(root, "Side", 16, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -16), new Vector2(280, 24));
            header.text = playerSide ? "ฝ่ายเรา" : "ศัตรู";
            header.fontStyle = FontStyle.Bold;
            header.color = playerSide ? new Color(0.70f, 0.88f, 1f) : new Color(1f, 0.72f, 0.68f);

            int n = units.Count;
            float cardH = n >= 5 ? 74f : 104f;
            float gap = n >= 5 ? 8f : 12f;
            float total = n * cardH + Mathf.Max(0, n - 1) * gap;
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(312, Mathf.Max(400f, total + 44f));
            float y = total * 0.5f - cardH * 0.5f - 8f;
            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];
                var card = MakePanel(root, "Card" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(292, cardH));
                var img = card.GetComponent<Image>();
                img.color = unit != null && unit == _turns.CurrentActor
                    ? new Color(0.20f, 0.28f, 0.16f, 0.95f)
                    : playerSide
                        ? new Color(0.08f, 0.11f, 0.15f, 0.90f)
                        : new Color(0.16f, 0.08f, 0.08f, 0.90f);

                if (unit != null)
                {
                    var chip = MakePanel(card, "El", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18, -16), new Vector2(16, 16));
                    chip.GetComponent<Image>().color = unit.ElementColor();
                }

                string lv = "";
                if (unit != null && unit.isPlayer)
                {
                    PartyMember member = PartyManager.Ensure().FindByDefinition(unit.definition);
                    if (member != null)
                        lv = "  Lv" + member.level;
                }

                string title = unit == null ? "-" : unit.ShortName + "  " + ThaiElement(unit.Element) + lv;
                var name = MakeText(card, "N", 16, TextAnchor.MiddleLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(12, -16), new Vector2(250, 24));
                name.text = title;
                name.color = unit != null && unit.IsAlive ? Color.white : new Color(1f, 0.4f, 0.4f);

                if (unit != null)
                {
                    float hpY = n >= 5 ? -38f : -44f;
                    float spY = n >= 5 ? -58f : -72f;
                    MakeBar(card, "HP", new Vector2(0, hpY), unit.currentHp, unit.stats.hp, UiTheme.Hp, "HP " + unit.currentHp + "/" + unit.stats.hp, 22f);
                    MakeBar(card, "SP", new Vector2(0, spY), unit.currentSp, Mathf.Max(1, unit.stats.sp), UiTheme.Sp, "SP " + unit.currentSp + "/" + unit.stats.sp, 16f);
                    if (unit.isDefending)
                    {
                        var d = MakeText(card, "D", 12, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -16), new Vector2(80, 18));
                        d.text = "ป้องกัน";
                        d.color = new Color(0.6f, 0.95f, 0.7f);
                    }
                }

                y -= cardH + gap;
            }
        }

        void MakeBar(Transform parent, string id, Vector2 pos, int current, int max, Color fill, string label, float height)
        {
            var bg = MakePanel((RectTransform)parent, id, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), pos, new Vector2(260, height));
            bg.GetComponent<Image>().color = id == "HP" ? UiTheme.HpBack : UiTheme.SpBack;
            float pct = max <= 0 ? 0f : Mathf.Clamp01(current / (float)max);
            var bar = MakePanel(bg, "Fill", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2((260f * pct) * 0.5f, 0), new Vector2(260f * pct, height));
            bar.GetComponent<Image>().color = fill;
            var t = MakeText(bg, "L", 12, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, height));
            t.text = label;
            t.fontStyle = FontStyle.Bold;
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

        void LayoutButtons(RectTransform parent, BtnSpec[] buttons, float width = 148f, float gap = 16f)
        {
            float total = buttons.Length * width + (buttons.Length - 1) * gap;
            float x = -total * 0.5f + width * 0.5f;
            for (int i = 0; i < buttons.Length; i++)
            {
                Button b = MakeButton(parent, buttons[i].Label, buttons[i].Color, buttons[i].Click);
                var rt = b.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x, 0);
                rt.sizeDelta = new Vector2(width, 60);
                x += width + gap;
            }
        }

        Button MakeButton(Transform parent, string label, Color color, UnityEngine.Events.UnityAction click)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(148, 60);
            go.GetComponent<Image>().color = color;
            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = color * 1.15f;
            colors.pressedColor = color * 0.8f;
            btn.colors = colors;
            btn.onClick.AddListener(click);
            var text = MakeText(go.transform, "T", 17, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(148, 60));
            text.text = label;
            text.fontStyle = FontStyle.Bold;
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
            go.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f, 0.62f);
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
            return CreatedHero.Thai(e);
        }
    }
}
