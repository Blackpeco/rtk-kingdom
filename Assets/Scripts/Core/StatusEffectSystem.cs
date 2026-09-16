namespace TsOnline
{
    /// <summary>
    /// Stub for Step 1. Encodes elemental status rules as data; does not simulate turns.
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
        /// Step 1 stub. Always returns false — StatusEffectSystem is not simulated yet.
        /// </summary>
        public static bool TryApply(
            ElementType incomingElement,
            ElementStatusKind currentStatus,
            float applyChance,
            out ElementStatusKind applied)
        {
            applied = currentStatus;
            return false;
        }
    }
}
