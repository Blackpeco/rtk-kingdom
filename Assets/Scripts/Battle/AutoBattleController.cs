using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TsOnline
{
    /// <summary>
    /// TS-style auto: Attack the lowest-HP enemy, or Heal an ally under the HP% threshold.
    /// Toggle off (or pick a command) to interrupt before the short think delay fires.
    /// </summary>
    public class AutoBattleController : MonoBehaviour
    {
        public const float DefaultHealThresholdPercent = 40f;
        public const float ThinkDelay = 0.35f;

        public bool AutoAttack;
        public bool AutoHeal;
        public float HealThresholdPercent = DefaultHealThresholdPercent;

        TurnManager _turns;
        Coroutine _pending;

        public void Bind(TurnManager turns)
        {
            _turns = turns;
            _turns.OnChanged += HandleChanged;
        }

        void OnDestroy()
        {
            if (_turns != null)
                _turns.OnChanged -= HandleChanged;
        }

        void HandleChanged()
        {
            if (_pending != null)
            {
                StopCoroutine(_pending);
                _pending = null;
            }

            if (_turns == null || _turns.State != BattleState.AwaitingCommand)
                return;
            if (_turns.CurrentActor == null || !_turns.CurrentActor.isPlayer)
                return;
            if (!AutoAttack && !AutoHeal)
                return;

            _pending = StartCoroutine(ActSoon());
        }

        IEnumerator ActSoon()
        {
            yield return new WaitForSeconds(ThinkDelay);
            _pending = null;
            TryAct();
        }

        public void TryAct()
        {
            if (_turns == null || _turns.State != BattleState.AwaitingCommand)
                return;
            BattleUnit actor = _turns.CurrentActor;
            if (actor == null || !actor.isPlayer || !actor.IsAlive)
                return;

            if (AutoHeal && TryHeal(actor))
                return;

            if (AutoAttack)
                TryAttack();
        }

        bool TryHeal(BattleUnit actor)
        {
            SkillDefinition heal = FindHealSkill(actor);
            BattleUnit wounded = LowestAllyBelowThreshold(_turns.PlayerUnits, HealThresholdPercent);
            if (heal == null || wounded == null)
                return false;

            _turns.ChooseCommand(BattleCommand.Skill);
            if (_turns.State != BattleState.AwaitingSkill)
                return _turns.State != BattleState.AwaitingCommand;

            _turns.ChooseSkill(heal);
            if (_turns.State == BattleState.AwaitingTarget)
                _turns.ChooseTarget(wounded);
            return true;
        }

        bool TryAttack()
        {
            _turns.ChooseCommand(BattleCommand.Attack);
            if (_turns.State != BattleState.AwaitingTarget)
                return true;

            BattleUnit target = LowestHp(_turns.EnemyUnits);
            if (target == null)
                return false;
            _turns.ChooseTarget(target);
            return true;
        }

        public static SkillDefinition FindHealSkill(BattleUnit unit)
        {
            if (unit == null)
                return null;
            SkillDefinition best = null;
            SkillDefinition[] skills = unit.Skills;
            for (int i = 0; i < skills.Length; i++)
            {
                SkillDefinition skill = skills[i];
                if (skill == null || skill.category != SkillCategory.Heal)
                    continue;
                if (!SkillSystem.CanUse(unit, skill))
                    continue;
                if (best == null || skill.spCost < best.spCost)
                    best = skill;
            }

            return best;
        }

        public static BattleUnit LowestAllyBelowThreshold(IList<BattleUnit> units, float thresholdPercent)
        {
            BattleUnit best = null;
            float bestPct = 2f;
            if (units == null)
                return null;
            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];
                if (unit == null || !unit.IsAlive)
                    continue;
                float pct = unit.stats.hp <= 0 ? 1f : unit.currentHp / (float)unit.stats.hp;
                if (pct * 100f >= thresholdPercent)
                    continue;
                if (pct < bestPct)
                {
                    bestPct = pct;
                    best = unit;
                }
            }

            return best;
        }

        public static BattleUnit LowestHp(IList<BattleUnit> units)
        {
            BattleUnit best = null;
            if (units == null)
                return null;
            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];
                if (unit == null || !unit.IsAlive)
                    continue;
                if (best == null || unit.currentHp < best.currentHp)
                    best = unit;
            }

            return best;
        }
    }
}
