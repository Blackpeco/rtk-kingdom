using System;

namespace TsOnline
{
    /// <summary>
    /// Turn-start status rules. Wind skip is decided BEFORE duration ticks
    /// so a 1-turn Wind status still gets exactly one skip roll.
    /// </summary>
    public static class StatusTurnFlow
    {
        /// <summary>
        /// When set, the next <see cref="ShouldSkipTurn"/> uses this 0–1 roll instead of RNG, then clears.
        /// 0 forces a skip if Wind is active. Used by QA / self-test.
        /// </summary>
        public static float? ForceNextSkipRoll;

        public static bool ShouldSkipTurn(ElementStatusKind status, Func<float> rng01)
        {
            if (status != ElementStatusKind.WindSkip)
                return false;

            float roll;
            if (ForceNextSkipRoll.HasValue)
            {
                roll = ForceNextSkipRoll.Value;
                ForceNextSkipRoll = null;
            }
            else if (rng01 != null)
            {
                roll = rng01();
            }
            else
            {
                roll = UnityEngine.Random.value;
            }

            return roll < StatusEffectSystem.WindSkipChance;
        }

        /// <summary>
        /// Pure helper for tests: skip-check against a still-active status, then expire 1-turn Wind.
        /// </summary>
        public static void SimulateWindTurnStart(
            ElementStatusKind before,
            int turnsBefore,
            float forcedRoll,
            out bool skipped,
            out ElementStatusKind after,
            out int turnsAfter)
        {
            ForceNextSkipRoll = forcedRoll;
            skipped = ShouldSkipTurn(before, null);
            ForceNextSkipRoll = null;
            after = before;
            turnsAfter = turnsBefore;
            if (after == ElementStatusKind.None)
                return;
            turnsAfter--;
            if (turnsAfter <= 0)
            {
                after = ElementStatusKind.None;
                turnsAfter = 0;
            }
        }
    }
}
