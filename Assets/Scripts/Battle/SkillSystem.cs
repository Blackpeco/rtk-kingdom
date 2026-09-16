using System;
using UnityEngine;

namespace TsOnline
{
    /// <summary>
    /// Resolves a <see cref="SkillDefinition"/>: spend SP, run DamageCalculator, heal HP, skip E for support cats.
    /// </summary>
    public static class SkillSystem
    {
        public const float DefendDamageFactor = 0.5f;

        public static bool CanUse(BattleUnit user, SkillDefinition skill)
        {
            if (user == null || !user.IsAlive || skill == null)
                return false;
            return user.currentSp >= skill.spCost;
        }

        public static bool IsSupport(SkillDefinition skill)
        {
            if (skill == null)
                return false;
            return !ElementSystem.SkillUsesElementMultiplier(skill.category);
        }

        public static bool TargetsAllies(SkillDefinition skill)
        {
            if (skill == null)
                return false;
            TargetingFlags t = skill.targeting;
            return (t & (TargetingFlags.Self | TargetingFlags.SingleAlly | TargetingFlags.AllAllies)) != 0
                && (t & (TargetingFlags.SingleEnemy | TargetingFlags.AllEnemies)) == 0;
        }

        public static bool IsAoE(SkillDefinition skill)
        {
            if (skill == null)
                return false;
            return (skill.targeting & (TargetingFlags.AllEnemies | TargetingFlags.AllAllies)) != 0;
        }

        public static SkillResolveResult Resolve(
            BattleUnit user,
            BattleUnit[] targets,
            SkillDefinition skill,
            DamageCalculator calc,
            Func<float> rng01 = null)
        {
            if (user == null || !user.IsAlive)
                return SkillResolveResult.Fail("ผู้ใช้หมดสติ");
            if (skill == null)
                return SkillResolveResult.Fail("ไม่มีสกิล");
            if (targets == null || targets.Length == 0)
                return SkillResolveResult.Fail("ไม่มีเป้าหมาย");
            if (!user.TrySpendSp(skill.spCost))
                return SkillResolveResult.Fail("SP ไม่พอ");

            if (calc == null)
                calc = new DamageCalculator();

            var hits = new SkillHit[targets.Length];
            for (int i = 0; i < targets.Length; i++)
                hits[i] = ApplyToTarget(user, targets[i], skill, calc, rng01);

            return new SkillResolveResult(true, null, skill, hits);
        }

        static SkillHit ApplyToTarget(
            BattleUnit user,
            BattleUnit target,
            SkillDefinition skill,
            DamageCalculator calc,
            Func<float> rng01)
        {
            if (target == null)
                return new SkillHit(null, default(DamageResult), 0, false);

            var request = new DamageRequest(
                user.stats.atk,
                user.stats.intel,
                target.stats.def,
                skill.power,
                skill.damageKind,
                skill.category,
                skill.element,
                user.Element,
                target.Element,
                false,
                0f,
                0f,
                false);

            DamageResult result = calc.Calculate(request);
            int amount = Mathf.Max(1, Mathf.RoundToInt(result.FinalDamage));
            bool appliedStatus = false;

            if (skill.category == SkillCategory.Heal)
            {
                target.HealHp(amount);
                return new SkillHit(target, result, amount, false);
            }

            if (skill.category == SkillCategory.Buff
                || skill.category == SkillCategory.Wall
                || skill.category == SkillCategory.Stealth)
            {
                return new SkillHit(target, result, 0, false);
            }

            if (target.isDefending)
            {
                amount = Mathf.Max(1, Mathf.RoundToInt(amount * DefendDamageFactor));
                target.isDefending = false;
            }

            if (target.status == ElementStatusKind.Wet
                && skill.element == ElementType.Fire
                && skill.category == SkillCategory.Attack)
            {
                amount = Mathf.Max(1, Mathf.RoundToInt(amount * StatusEffectSystem.WetNextFireHitMultiplier));
                target.status = ElementStatusKind.None;
                target.statusTurnsLeft = 0;
            }

            target.TakeDamage(amount);

            if (skill.element != ElementType.None && target.IsAlive)
            {
                ElementStatusKind next;
                if (StatusEffectSystem.TryApply(
                    skill.element,
                    target.status,
                    skill.GetStatusApplyChance(),
                    out next,
                    rng01))
                {
                    ElementStatusSpec spec = StatusEffectSystem.GetSpec(skill.element);
                    target.ApplyStatus(next, spec.DurationTurns);
                    appliedStatus = true;
                }
            }

            return new SkillHit(target, result, amount, appliedStatus);
        }
    }
}
