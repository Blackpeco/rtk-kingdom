namespace TsOnline
{
    public readonly struct DamageResult
    {
        public readonly float BaseDamage;
        public readonly float ElementMultiplier;
        public readonly float MasteryFactor;
        public readonly float ResistFactor;
        public readonly float CritFactor;
        public readonly float RandomFactor;
        public readonly float FinalDamage;
        public readonly bool IsAdvantage;
        public readonly bool IsDisadvantage;
        public readonly bool UsedElementMultiplier;
        public readonly bool IsNormalAttack;

        public DamageResult(
            float baseDamage,
            float elementMultiplier,
            float masteryFactor,
            float resistFactor,
            float critFactor,
            float randomFactor,
            float finalDamage,
            bool isAdvantage,
            bool isDisadvantage,
            bool usedElementMultiplier,
            bool isNormalAttack)
        {
            BaseDamage = baseDamage;
            ElementMultiplier = elementMultiplier;
            MasteryFactor = masteryFactor;
            ResistFactor = resistFactor;
            CritFactor = critFactor;
            RandomFactor = randomFactor;
            FinalDamage = finalDamage;
            IsAdvantage = isAdvantage;
            IsDisadvantage = isDisadvantage;
            UsedElementMultiplier = usedElementMultiplier;
            IsNormalAttack = isNormalAttack;
        }
    }
}
