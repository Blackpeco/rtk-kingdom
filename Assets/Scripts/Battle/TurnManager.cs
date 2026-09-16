using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TsOnline
{
    public enum BattleState
    {
        Idle = 0,
        AwaitingCommand = 1,
        AwaitingSkill = 2,
        AwaitingTarget = 3,
        Resolving = 4,
        Ended = 5
    }

    /// <summary>
    /// AGI-descending turn cycle. Player: Attack / Skill / Item / Defend / Escape. Enemy: attack weakest.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public const float EscapeChance = 0.30f;
        public const float EnemyThinkSeconds = 0.45f;

        public readonly List<BattleUnit> PlayerUnits = new List<BattleUnit>();
        public readonly List<BattleUnit> EnemyUnits = new List<BattleUnit>();

        public BattleState State { get; private set; }
        public BattleUnit CurrentActor { get; private set; }
        public BattleCommand PendingCommand { get; private set; }
        public SkillDefinition PendingSkill { get; private set; }
        public string Banner { get; private set; }
        public bool PlayerWon { get; private set; }
        public bool Escaped { get; private set; }

        public DamageCalculator Calculator = new DamageCalculator();

        public event Action OnChanged;
        public event Action<string> OnLog;
        public event Action<BattleUnit, string, Color> OnPopup;

        readonly List<BattleUnit> _round = new List<BattleUnit>();
        int _roundIndex;
        int _roundNumber;

        public IReadOnlyList<BattleUnit> AllLiving
        {
            get { return CollectLiving(); }
        }

        public void Setup(IList<BattleUnit> players, IList<BattleUnit> enemies)
        {
            PlayerUnits.Clear();
            EnemyUnits.Clear();
            if (players != null)
                PlayerUnits.AddRange(players);
            if (enemies != null)
                EnemyUnits.AddRange(enemies);
            State = BattleState.Idle;
            CurrentActor = null;
            PendingCommand = BattleCommand.None;
            PendingSkill = null;
            PlayerWon = false;
            Escaped = false;
            _roundNumber = 0;
        }

        public void BeginBattle()
        {
            Log("===== เริ่มทดสอบ 3v3 =====");
            StartNextRound();
        }

        public void ChooseCommand(BattleCommand command)
        {
            if (State != BattleState.AwaitingCommand || CurrentActor == null || !CurrentActor.isPlayer)
                return;

            PendingCommand = command;
            PendingSkill = null;

            if (command == BattleCommand.Defend)
            {
                CurrentActor.isDefending = true;
                Log(CurrentActor.ShortName + " ตั้งการ์ด (รับดาเมจครึ่งหนึ่งครั้งถัดไป)");
                EndCurrentTurn();
                return;
            }

            if (command == BattleCommand.Item)
            {
                Log("ไอเทมยังไม่พร้อม (Step 2 stub)");
                EndCurrentTurn();
                return;
            }

            if (command == BattleCommand.Escape)
            {
                ResolveEscape();
                return;
            }

            if (command == BattleCommand.Skill)
            {
                State = BattleState.AwaitingSkill;
                Banner = "เลือกสกิลของ " + CurrentActor.ShortName;
                Notify();
                return;
            }

            if (command == BattleCommand.Attack)
            {
                var targets = Living(EnemyUnits);
                if (targets.Count == 1)
                {
                    ResolveAttack(CurrentActor, targets[0]);
                    return;
                }

                State = BattleState.AwaitingTarget;
                Banner = "เลือกเป้าหมายโจมตีปกติ";
                Notify();
            }
        }

        public void ChooseSkill(SkillDefinition skill)
        {
            if (State != BattleState.AwaitingSkill || CurrentActor == null)
                return;
            if (!SkillSystem.CanUse(CurrentActor, skill))
            {
                Log("ใช้สกิลไม่ได้ (SP หรือข้อมูลไม่ครบ)");
                Notify();
                return;
            }

            PendingSkill = skill;
            PendingCommand = BattleCommand.Skill;

            if (SkillSystem.IsAoE(skill))
            {
                BattleUnit[] group = SkillSystem.TargetsAllies(skill)
                    ? Living(PlayerUnits).ToArray()
                    : Living(EnemyUnits).ToArray();
                ResolveSkill(CurrentActor, skill, group);
                return;
            }

            List<BattleUnit> options = SkillSystem.TargetsAllies(skill)
                ? Living(PlayerUnits)
                : Living(EnemyUnits);

            if (options.Count == 0)
            {
                Log("ไม่มีเป้าหมาย");
                State = BattleState.AwaitingCommand;
                Banner = "ตาของ " + CurrentActor.ShortName;
                Notify();
                return;
            }

            if (options.Count == 1)
            {
                ResolveSkill(CurrentActor, skill, new[] { options[0] });
                return;
            }

            State = BattleState.AwaitingTarget;
            Banner = "เลือกเป้าหมายสำหรับ " + skill.displayName;
            Notify();
        }

        public void ChooseTarget(BattleUnit target)
        {
            if (State != BattleState.AwaitingTarget || CurrentActor == null || target == null || !target.IsAlive)
                return;

            if (PendingCommand == BattleCommand.Attack)
            {
                if (target.isPlayer)
                    return;
                ResolveAttack(CurrentActor, target);
                return;
            }

            if (PendingCommand == BattleCommand.Skill && PendingSkill != null)
            {
                bool wantAlly = SkillSystem.TargetsAllies(PendingSkill);
                if (wantAlly != target.isPlayer)
                    return;
                ResolveSkill(CurrentActor, PendingSkill, new[] { target });
            }
        }

        public void CancelToCommands()
        {
            if (CurrentActor == null || !CurrentActor.isPlayer)
                return;
            if (State != BattleState.AwaitingSkill && State != BattleState.AwaitingTarget)
                return;
            PendingCommand = BattleCommand.None;
            PendingSkill = null;
            State = BattleState.AwaitingCommand;
            Banner = "ตาของ " + CurrentActor.ShortName + "  (AGI " + CurrentActor.TurnAgi + ")";
            Notify();
        }

        public List<BattleUnit> CurrentTargetOptions()
        {
            if (State != BattleState.AwaitingTarget || CurrentActor == null)
                return new List<BattleUnit>();
            if (PendingCommand == BattleCommand.Attack)
                return Living(EnemyUnits);
            if (PendingSkill != null && SkillSystem.TargetsAllies(PendingSkill))
                return Living(PlayerUnits);
            return Living(EnemyUnits);
        }

        void StartNextRound()
        {
            if (CheckEnd())
                return;

            _roundNumber++;
            _round.Clear();
            _round.AddRange(Living(PlayerUnits));
            _round.AddRange(Living(EnemyUnits));
            _round.Sort(CompareAgi);
            _roundIndex = 0;
            Log("--- รอบที่ " + _roundNumber + " (เรียง AGI) ---");
            AdvanceToNextActor();
        }

        void AdvanceToNextActor()
        {
            while (_roundIndex < _round.Count)
            {
                BattleUnit unit = _round[_roundIndex];
                _roundIndex++;
                if (unit == null || !unit.IsAlive)
                    continue;

                CurrentActor = unit;
                unit.TickStatusAtTurnStart(Log);
                if (!unit.IsAlive)
                {
                    if (CheckEnd())
                        return;
                    continue;
                }

                if (unit.RollSkipTurn())
                {
                    Log(unit.ShortName + " เสียเทิร์นจากสถานะลม");
                    continue;
                }

                unit.isDefending = false;
                PendingCommand = BattleCommand.None;
                PendingSkill = null;

                if (unit.isPlayer)
                {
                    State = BattleState.AwaitingCommand;
                    Banner = "ตาของ " + unit.ShortName + "  (AGI " + unit.TurnAgi + ")";
                    Notify();
                    return;
                }

                State = BattleState.Resolving;
                Banner = unit.ShortName + " กำลังตัดสินใจ…";
                Notify();
                StartCoroutine(EnemyAct(unit));
                return;
            }

            StartNextRound();
        }

        IEnumerator EnemyAct(BattleUnit actor)
        {
            yield return new WaitForSeconds(EnemyThinkSeconds);
            if (State == BattleState.Ended)
                yield break;

            BattleUnit victim = Weakest(Living(PlayerUnits));
            if (victim == null)
            {
                CheckEnd();
                yield break;
            }

            ResolveAttack(actor, victim);
        }

        void ResolveAttack(BattleUnit actor, BattleUnit target)
        {
            State = BattleState.Resolving;
            var request = DamageRequest.NormalAttack(actor.stats, target.stats.def, actor.Element, target.Element, 0f, 0f, false);
            DamageResult result = Calculator.Calculate(request);
            int amount = Mathf.Max(1, Mathf.RoundToInt(result.FinalDamage));
            if (target.isDefending)
            {
                amount = Mathf.Max(1, Mathf.RoundToInt(amount * SkillSystem.DefendDamageFactor));
                target.isDefending = false;
            }

            target.TakeDamage(amount);
            string tag = AdvantageTag(result);
            Log(actor.ShortName + " โจมตีปกติ " + target.ShortName + " → " + amount + "  " + tag
                + "  E=" + result.ElementMultiplier.ToString("0.00"));
            Popup(target, amount + (result.IsAdvantage ? "  ข่มธาตุ!" : result.IsDisadvantage ? "  ธาตุต้าน" : ""), AdvantageColor(result));
            EndCurrentTurn();
        }

        void ResolveSkill(BattleUnit actor, SkillDefinition skill, BattleUnit[] targets)
        {
            State = BattleState.Resolving;
            SkillResolveResult resolved = SkillSystem.Resolve(actor, targets, skill, Calculator);
            if (!resolved.Success)
            {
                Log(resolved.FailReason);
                State = BattleState.AwaitingCommand;
                Banner = "ตาของ " + actor.ShortName;
                Notify();
                return;
            }

            for (int i = 0; i < resolved.Hits.Length; i++)
            {
                SkillHit hit = resolved.Hits[i];
                if (hit.Target == null)
                    continue;
                string tag = AdvantageTag(hit.Result);
                if (skill.category == SkillCategory.Heal)
                {
                    Log(actor.ShortName + " รักษา " + hit.Target.ShortName + " +" + hit.AppliedAmount + " HP  (E="
                        + hit.Result.ElementMultiplier.ToString("0.00") + ")");
                    Popup(hit.Target, "+" + hit.AppliedAmount, new Color(0.45f, 0.95f, 0.55f));
                }
                else if (skill.category == SkillCategory.Buff || skill.category == SkillCategory.Wall || skill.category == SkillCategory.Stealth)
                {
                    Log(actor.ShortName + " ใช้ " + skill.displayName + " กับ " + hit.Target.ShortName + " (ไม่มีตัวคูณธาตุ)");
                    Popup(hit.Target, skill.displayName, Color.cyan);
                }
                else
                {
                    Log(actor.ShortName + " " + skill.displayName + " → " + hit.Target.ShortName + " "
                        + hit.AppliedAmount + "  " + tag + "  E=" + hit.Result.ElementMultiplier.ToString("0.00"));
                    Popup(hit.Target,
                        hit.AppliedAmount + (hit.Result.IsAdvantage ? "  ข่มธาตุ!" : hit.Result.IsDisadvantage ? "  ธาตุต้าน" : ""),
                        AdvantageColor(hit.Result));
                    if (hit.AppliedStatus)
                        Log("  สถานะธาตุติดที่ " + hit.Target.ShortName);
                }
            }

            EndCurrentTurn();
        }

        void ResolveEscape()
        {
            State = BattleState.Resolving;
            if (UnityEngine.Random.value < EscapeChance)
            {
                Escaped = true;
                Banner = "หนีสำเร็จ";
                Log(CurrentActor.ShortName + " หนีจากการต่อสู้");
                Finish(false, true);
                return;
            }

            Log(CurrentActor.ShortName + " หนีไม่สำเร็จ");
            EndCurrentTurn();
        }

        void EndCurrentTurn()
        {
            if (CheckEnd())
                return;
            AdvanceToNextActor();
        }

        bool CheckEnd()
        {
            bool playersAlive = Living(PlayerUnits).Count > 0;
            bool enemiesAlive = Living(EnemyUnits).Count > 0;
            if (playersAlive && enemiesAlive)
                return false;
            if (!playersAlive)
                Finish(false, false);
            else
                Finish(true, false);
            return true;
        }

        void Finish(bool won, bool fled)
        {
            PlayerWon = won;
            Escaped = fled;
            State = BattleState.Ended;
            CurrentActor = null;
            if (fled)
                Banner = "หนีรอด — ทดสอบจบ";
            else
                Banner = won ? "ชนะ — ศัตรูหมด" : "แพ้ — ฝ่ายเราหมด";
            Log("===== " + Banner + " =====");
            Notify();
        }

        static int CompareAgi(BattleUnit a, BattleUnit b)
        {
            int cmp = b.TurnAgi.CompareTo(a.TurnAgi);
            if (cmp != 0)
                return cmp;
            return string.CompareOrdinal(a.ShortName, b.ShortName);
        }

        static List<BattleUnit> Living(List<BattleUnit> side)
        {
            var list = new List<BattleUnit>();
            for (int i = 0; i < side.Count; i++)
            {
                if (side[i] != null && side[i].IsAlive)
                    list.Add(side[i]);
            }

            return list;
        }

        List<BattleUnit> CollectLiving()
        {
            var list = Living(PlayerUnits);
            list.AddRange(Living(EnemyUnits));
            return list;
        }

        static BattleUnit Weakest(List<BattleUnit> living)
        {
            BattleUnit best = null;
            for (int i = 0; i < living.Count; i++)
            {
                if (best == null || living[i].currentHp < best.currentHp)
                    best = living[i];
            }

            return best;
        }

        static string AdvantageTag(DamageResult result)
        {
            if (result.IsAdvantage)
                return "ข่มธาตุ!";
            if (result.IsDisadvantage)
                return "ธาตุต้าน";
            return "";
        }

        static Color AdvantageColor(DamageResult result)
        {
            if (result.IsAdvantage)
                return new Color(0.95f, 0.78f, 0.20f);
            if (result.IsDisadvantage)
                return new Color(0.65f, 0.65f, 0.70f);
            return Color.white;
        }

        void Log(string line)
        {
            Debug.Log("[Battle] " + line);
            if (OnLog != null)
                OnLog(line);
        }

        void Popup(BattleUnit unit, string text, Color color)
        {
            if (OnPopup != null)
                OnPopup(unit, text, color);
        }

        void Notify()
        {
            if (OnChanged != null)
                OnChanged();
        }
    }
}
