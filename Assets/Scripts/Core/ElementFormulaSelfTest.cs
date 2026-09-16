using System;
using System.Globalization;
using System.Text;

namespace TsOnline
{
    /// <summary>
    /// Shared PASS/FAIL checks for Editor window and Play Mode smoke test.
    /// </summary>
    public static class ElementFormulaSelfTest
    {
        public const float Epsilon = 0.0001f;

        public static string RunAndFormat()
        {
            var log = new StringBuilder();
            int failed;
            int passed = RunAll(line => log.AppendLine(line), out failed);
            log.AppendLine();
            log.AppendFormat(
                CultureInfo.InvariantCulture,
                "Element formula self-test: {0} passed, {1} failed. {2}",
                passed,
                failed,
                failed == 0 ? "PASS" : "FAIL");
            return log.ToString();
        }

        public static int RunAll(Action<string> log, out int failed)
        {
            int passed = 0;
            failed = 0;

            log("[TS Online] === Skill E multipliers (ข่ม 1.25 / แพ้ 0.80 / else 1.00) ===");
            Check(log, ref passed, ref failed, "Earth vs Water (ข่ม)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Earth, ElementType.Water), 1.25f);
            Check(log, ref passed, ref failed, "Water vs Fire (ข่ม)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Water, ElementType.Fire), 1.25f);
            Check(log, ref passed, ref failed, "Fire vs Wind (ข่ม)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Fire, ElementType.Wind), 1.25f);
            Check(log, ref passed, ref failed, "Wind vs Earth (ข่ม)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Wind, ElementType.Earth), 1.25f);

            Check(log, ref passed, ref failed, "Water vs Earth (แพ้)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Water, ElementType.Earth), 0.80f);
            Check(log, ref passed, ref failed, "Fire vs Water (แพ้)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Fire, ElementType.Water), 0.80f);
            Check(log, ref passed, ref failed, "Wind vs Fire (แพ้)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Wind, ElementType.Fire), 0.80f);
            Check(log, ref passed, ref failed, "Earth vs Wind (แพ้)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Earth, ElementType.Wind), 0.80f);

            Check(log, ref passed, ref failed, "Earth vs Fire (opposite)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Earth, ElementType.Fire), 1.00f);
            Check(log, ref passed, ref failed, "Fire vs Earth (opposite)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Fire, ElementType.Earth), 1.00f);
            Check(log, ref passed, ref failed, "Water vs Wind (opposite)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Water, ElementType.Wind), 1.00f);
            Check(log, ref passed, ref failed, "Wind vs Water (opposite)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Wind, ElementType.Water), 1.00f);

            Check(log, ref passed, ref failed, "Earth vs Earth (same)",
                ElementSystem.GetSkillElementMultiplier(ElementType.Earth, ElementType.Earth), 1.00f);
            Check(log, ref passed, ref failed, "None vs Water",
                ElementSystem.GetSkillElementMultiplier(ElementType.None, ElementType.Water), 1.00f);
            Check(log, ref passed, ref failed, "Fire vs None",
                ElementSystem.GetSkillElementMultiplier(ElementType.Fire, ElementType.None), 1.00f);

            log("[TS Online] === Normal-attack E (half strength 1.12 / 0.90 / 1.00) ===");
            Check(log, ref passed, ref failed, "NA Earth vs Water (ข่ม)",
                ElementSystem.GetNormalAttackElementMultiplier(ElementType.Earth, ElementType.Water), 1.12f);
            Check(log, ref passed, ref failed, "NA Water vs Earth (แพ้)",
                ElementSystem.GetNormalAttackElementMultiplier(ElementType.Water, ElementType.Earth), 0.90f);
            Check(log, ref passed, ref failed, "NA Earth vs Fire (opposite)",
                ElementSystem.GetNormalAttackElementMultiplier(ElementType.Earth, ElementType.Fire), 1.00f);
            Check(log, ref passed, ref failed, "NA Fire vs Fire (same)",
                ElementSystem.GetNormalAttackElementMultiplier(ElementType.Fire, ElementType.Fire), 1.00f);
            Check(log, ref passed, ref failed, "NA Wind vs Earth (ข่ม)",
                ElementSystem.GetNormalAttackElementMultiplier(ElementType.Wind, ElementType.Earth), 1.12f);
            Check(log, ref passed, ref failed, "NA Wind vs Fire (แพ้)",
                ElementSystem.GetNormalAttackElementMultiplier(ElementType.Wind, ElementType.Fire), 0.90f);

            log("[TS Online] === Category skips E (heal / buff / wall / stealth) ===");
            ExpectTrue(log, ref passed, ref failed, "Attack uses E",
                ElementSystem.SkillUsesElementMultiplier(SkillCategory.Attack));
            ExpectFalse(log, ref passed, ref failed, "Heal skips E",
                ElementSystem.SkillUsesElementMultiplier(SkillCategory.Heal));
            ExpectFalse(log, ref passed, ref failed, "Buff skips E",
                ElementSystem.SkillUsesElementMultiplier(SkillCategory.Buff));
            ExpectFalse(log, ref passed, ref failed, "Wall skips E",
                ElementSystem.SkillUsesElementMultiplier(SkillCategory.Wall));
            ExpectFalse(log, ref passed, ref failed, "Stealth skips E",
                ElementSystem.SkillUsesElementMultiplier(SkillCategory.Stealth));

            log("[TS Online] === Mastery / resist / learning ===");
            Check(log, ref passed, ref failed, "Mastery 0 uses", ElementSystem.GetMasteryBonus(0), 0f);
            Check(log, ref passed, ref failed, "Mastery 19 uses", ElementSystem.GetMasteryBonus(19), 0f);
            Check(log, ref passed, ref failed, "Mastery 20 uses", ElementSystem.GetMasteryBonus(20), 0.01f);
            Check(log, ref passed, ref failed, "Mastery 300 uses (cap)", ElementSystem.GetMasteryBonus(300), 0.15f);
            Check(log, ref passed, ref failed, "Mastery 400 uses (still cap)", ElementSystem.GetMasteryBonus(400), 0.15f);
            Check(log, ref passed, ref failed, "Resist 0.25 clamped", ElementSystem.ClampResist(0.25f), 0.20f);
            Check(log, ref passed, ref failed, "Resist -0.1 clamped", ElementSystem.ClampResist(-0.1f), 0f);

            ExpectTrue(log, ref passed, ref failed, "Earth can learn Earth",
                ElementSystem.CanLearnSkill(ElementType.Earth, ElementType.Earth));
            ExpectTrue(log, ref passed, ref failed, "Earth can learn Water (adjacent)",
                ElementSystem.CanLearnSkill(ElementType.Earth, ElementType.Water));
            ExpectTrue(log, ref passed, ref failed, "Earth can learn Wind (adjacent)",
                ElementSystem.CanLearnSkill(ElementType.Earth, ElementType.Wind));
            ExpectFalse(log, ref passed, ref failed, "Earth cannot learn Fire (opposite)",
                ElementSystem.CanLearnSkill(ElementType.Earth, ElementType.Fire));
            ExpectFalse(log, ref passed, ref failed, "Water cannot learn Wind (opposite)",
                ElementSystem.CanLearnSkill(ElementType.Water, ElementType.Wind));
            Check(log, ref passed, ref failed, "Own learn cost",
                ElementSystem.GetLearnCostMultiplier(ElementType.Earth, ElementType.Earth), 1.00f);
            Check(log, ref passed, ref failed, "Adjacent learn cost",
                ElementSystem.GetLearnCostMultiplier(ElementType.Earth, ElementType.Water), 1.50f);
            Check(log, ref passed, ref failed, "Opposite learn cost sentinel",
                ElementSystem.GetLearnCostMultiplier(ElementType.Earth, ElementType.Fire), ElementSystem.CannotLearnCost);
            Check(log, ref passed, ref failed, "Water vs Wind learn cost sentinel",
                ElementSystem.GetLearnCostMultiplier(ElementType.Water, ElementType.Wind), ElementSystem.CannotLearnCost);

            log("[TS Online] === Wind skip: evaluate while active, then tick 1-turn duration ===");
            bool skipped;
            ElementStatusKind after;
            int turnsAfter;
            StatusTurnFlow.SimulateWindTurnStart(ElementStatusKind.WindSkip, 1, 0f, out skipped, out after, out turnsAfter);
            ExpectTrue(log, ref passed, ref failed, "Wind skip roll 0.00 skips while status active", skipped);
            ExpectTrue(log, ref passed, ref failed, "1-turn Wind expires after that check", after == ElementStatusKind.None);
            StatusTurnFlow.SimulateWindTurnStart(ElementStatusKind.WindSkip, 1, 0.99f, out skipped, out after, out turnsAfter);
            ExpectFalse(log, ref passed, ref failed, "Wind skip roll 0.99 does not skip", skipped);
            ExpectTrue(log, ref passed, ref failed, "1-turn Wind still expires after no-skip roll", after == ElementStatusKind.None);
            StatusTurnFlow.SimulateWindTurnStart(ElementStatusKind.None, 0, 0f, out skipped, out after, out turnsAfter);
            ExpectFalse(log, ref passed, ref failed, "No status: no skip even with roll 0", skipped);
            StatusTurnFlow.ForceNextSkipRoll = null;

            log("[TS Online] === Sample Final damage (RNG fixed at 1.00 mid) ===");
            var calc = DamageCalculator.Deterministic(1.0f);

            // Physical: Base = 100 * 1.0 - 40 * 0.5 = 80
            // E = 1.25 (Earth skill vs Water), M=0, R=0, crit=1, rng=1 → 100
            var skillAdv = new DamageRequest(
                100f, 0f, 40f, 1.0f,
                DamageKind.Physical, SkillCategory.Attack,
                ElementType.Earth, ElementType.Earth, ElementType.Water,
                false, 0f, 0f, false);
            DamageResult adv = calc.Calculate(skillAdv);
            Check(log, ref passed, ref failed, "Sample Base (phys 100 ATK, 1.0 pwr, 40 DEF)", adv.BaseDamage, 80f);
            Check(log, ref passed, ref failed, "Sample E (Earth skill vs Water)", adv.ElementMultiplier, 1.25f);
            Check(log, ref passed, ref failed, "Sample Final (80 * 1.25)", adv.FinalDamage, 100f);
            ExpectTrue(log, ref passed, ref failed, "Sample reports advantage", adv.IsAdvantage);

            var heal = new DamageRequest(
                100f, 80f, 0f, 1.0f,
                DamageKind.Magical, SkillCategory.Heal,
                ElementType.Earth, ElementType.Earth, ElementType.Water,
                false, 0f, 0f, false);
            DamageResult healResult = calc.Calculate(heal);
            Check(log, ref passed, ref failed, "Heal E stays 1.00 (even Earth vs Water)", healResult.ElementMultiplier, 1.00f);
            Check(log, ref passed, ref failed, "Heal Final (INT 80 * 1, no E)", healResult.FinalDamage, 80f);
            ExpectFalse(log, ref passed, ref failed, "Heal does not report advantage", healResult.IsAdvantage);

            var na = new DamageRequest(
                100f, 0f, 40f, 1.0f,
                DamageKind.Physical, SkillCategory.Attack,
                ElementType.None, ElementType.Earth, ElementType.Water,
                true, 0f, 0f, false);
            DamageResult naResult = calc.Calculate(na);
            Check(log, ref passed, ref failed, "NA Final (80 * 1.12)", naResult.FinalDamage, 89.6f);

            var crit = new DamageRequest(
                100f, 0f, 40f, 1.0f,
                DamageKind.Physical, SkillCategory.Attack,
                ElementType.Earth, ElementType.Earth, ElementType.Water,
                false, 0f, 0f, true);
            DamageResult critResult = calc.Calculate(crit);
            Check(log, ref passed, ref failed, "Crit Final (80 * 1.25 * 1.5)", critResult.FinalDamage, 150f);

            // M=0.50 must clamp to 0.15 → 80 * 1.25 * 1.15 = 115
            var overMastery = new DamageRequest(
                100f, 0f, 40f, 1.0f,
                DamageKind.Physical, SkillCategory.Attack,
                ElementType.Earth, ElementType.Earth, ElementType.Water,
                false, 0.50f, 0f, false);
            DamageResult overM = calc.Calculate(overMastery);
            Check(log, ref passed, ref failed, "M 0.50 clamped to 0.15 (factor)", overM.MasteryFactor, 1.15f);
            Check(log, ref passed, ref failed, "Final with clamped M (80 * 1.25 * 1.15)", overM.FinalDamage, 115f);

            // Magic: Base = 100 * 1.0 - 40 * 0.25 = 90; Water vs Fire ข่ม E=1.25 → 112.5
            var magic = new DamageRequest(
                0f, 100f, 40f, 1.0f,
                DamageKind.Magical, SkillCategory.Attack,
                ElementType.Water, ElementType.Water, ElementType.Fire,
                false, 0f, 0f, false);
            DamageResult mag = calc.Calculate(magic);
            Check(log, ref passed, ref failed, "Magic Base (INT 100, pwr 1, DEF 40)", mag.BaseDamage, 90f);
            Check(log, ref passed, ref failed, "Magic E (Water vs Fire)", mag.ElementMultiplier, 1.25f);
            Check(log, ref passed, ref failed, "Magic Final (90 * 1.25)", mag.FinalDamage, 112.5f);

            return passed;
        }

        static void Check(Action<string> log, ref int passed, ref int failed, string name, float actual, float expected)
        {
            if (AlmostEqual(actual, expected))
            {
                passed++;
                log(string.Format(CultureInfo.InvariantCulture, "  PASS  {0}: {1}", name, actual));
            }
            else
            {
                failed++;
                log(string.Format(CultureInfo.InvariantCulture,
                    "  FAIL  {0}: expected {1}, got {2}", name, expected, actual));
            }
        }

        static void ExpectTrue(Action<string> log, ref int passed, ref int failed, string name, bool actual)
        {
            if (actual)
            {
                passed++;
                log("  PASS  " + name);
            }
            else
            {
                failed++;
                log("  FAIL  " + name + ": expected true");
            }
        }

        static void ExpectFalse(Action<string> log, ref int passed, ref int failed, string name, bool actual)
        {
            if (!actual)
            {
                passed++;
                log("  PASS  " + name);
            }
            else
            {
                failed++;
                log("  FAIL  " + name + ": expected false");
            }
        }

        static bool AlmostEqual(float a, float b)
        {
            float d = a - b;
            if (d < 0f)
                d = -d;
            return d <= Epsilon;
        }
    }
}
