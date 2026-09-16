using System;

namespace TsOnline
{
    /// <summary>
    /// Elemental status rules. Step 2 wires apply-chance; full turn simulation of every effect is still light.
    /// Apply chance 30%. Cannot stack multiple element statuses.
    /// </summary>
    public static class StatusEffectSystem
    {
        public const float DefaultApplyChance = 0.30f;
        public const bool CanStackMultipleElementStatuses = false;

        public const float EarthAgiPenalty = 0.20f;
        public const float WetNextFireHitMultiplier = 1.15f;
        public const float BurnHpPercentPerTurn = 0.04f;
        public const float BurnHealPenalty = 0.30f;
        public const float WindSkipChance = 0.25f;

        public static ElementStatusSpec GetSpec(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth:
                    return new ElementStatusSpec(
                        ElementType.Earth,
                        ElementStatusKind.EarthAgiDown,
                        2,
                        "AGI -20% for 2 turns.",
                        "AGI -20% เป็นเวลา 2 เทิร์น");
                case ElementType.Water:
                    return new ElementStatusSpec(
                        ElementType.Water,
                        ElementStatusKind.Wet,
                        2,
                        "Wet: next Fire hit ×1.15, clears Burn, 2 turns.",
                        "เปียก: โดนไฟครั้งถัดไป ×1.15, ล้างเผาไหม้, 2 เทิร์น");
                case ElementType.Fire:
                    return new ElementStatusSpec(
                        ElementType.Fire,
                        ElementStatusKind.Burn,
                        2,
                        "Burn: 4% HP/turn, healing -30%, 2 turns.",
                        "เผาไหม้: HP 4%/เทิร์น, ฮีล -30%, 2 เทิร์น");
                case ElementType.Wind:
                    return new ElementStatusSpec(
                        ElementType.Wind,
                        ElementStatusKind.WindSkip,
                        1,
                        "25% chance to lose next turn, 1 turn.",
                        "โอกาส 25% เสียเทิร์นถัดไป, 1 เทิร์น");
                default:
                    return new ElementStatusSpec(
                        ElementType.None,
                        ElementStatusKind.None,
                        0,
                        "No elemental status.",
                        "ไม่มีสถานะธาตุ");
            }
        }

        /// <summary>
        /// Rolls apply chance. Refuses to stack a second elemental status.
        /// <paramref name="rng01"/> should return [0,1). Null uses UnityEngine.Random.value.
        /// </summary>
        public static bool TryApply(
            ElementType incomingElement,
            ElementStatusKind currentStatus,
            float applyChance,
            out ElementStatusKind applied,
            Func<float> rng01 = null)
        {
            applied = currentStatus;
            if (incomingElement == ElementType.None)
                return false;

            ElementStatusSpec spec = GetSpec(incomingElement);
            if (spec.Kind == ElementStatusKind.None)
                return false;

            if (currentStatus != ElementStatusKind.None && !CanStackMultipleElementStatuses)
                return false;

            float chance = applyChance;
            if (chance < 0f)
                chance = 0f;
            if (chance > 1f)
                chance = 1f;

            float roll = rng01 != null ? rng01() : UnityEngine.Random.value;
            if (roll >= chance)
                return false;

            applied = spec.Kind;
            return true;
        }
    }
}
