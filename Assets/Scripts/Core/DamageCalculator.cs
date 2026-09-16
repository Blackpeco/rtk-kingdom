namespace TsOnline
{
    /// <summary>
    /// Injectable RNG for Editor / Play Mode tests. Maps to UnityEngine.Random.Range when omitted.
    /// </summary>
    public delegate float RandomRange(float minInclusive, float maxInclusive);

    /// <summary>
    /// Pure damage pipeline. Physical: ATK * Power - DEF * 0.5. Magical: INT * Power - DEF * 0.25.
    /// Final = max(1, Base) * E * (1+M) * (1-R) * Crit * Random(0.95, 1.05).
    /// </summary>
    public sealed class DamageCalculator
    {
        public const float PhysicalDefFactor = 0.5f;
        public const float MagicalDefFactor = 0.25f;
        public const float RandomMin = 0.95f;
        public const float RandomMax = 1.05f;
        public const float CritMultiplier = 1.5f;
        public const float NoCritMultiplier = 1.0f;
        public const float MinimumBase = 1f;

        readonly RandomRange _rng;

        public DamageCalculator(RandomRange rng = null)
        {
            _rng = rng ?? UnityRandom;
        }

        public static DamageCalculator Deterministic(float roll = 1.0f)
        {
            return new DamageCalculator((min, max) => roll);
        }

        public DamageResult Calculate(DamageRequest request)
        {
            float rawBase;
            if (request.Kind == DamageKind.Magical)
                rawBase = request.IntStat * request.SkillPower - request.Def * MagicalDefFactor;
            else
                rawBase = request.Atk * request.SkillPower - request.Def * PhysicalDefFactor;

            float baseDamage = rawBase > MinimumBase ? rawBase : MinimumBase;

            bool usesE = ElementSystem.SkillUsesElementMultiplier(request.Category);
            ElementType compareElement = request.IsNormalAttack
                ? request.UnitElement
                : request.SkillElement;

            float e;
            if (!usesE)
                e = ElementSystem.SkillNeutralMultiplier;
            else if (request.IsNormalAttack)
                e = ElementSystem.GetNormalAttackElementMultiplier(request.UnitElement, request.TargetElement);
            else
                e = ElementSystem.GetSkillElementMultiplier(request.SkillElement, request.TargetElement);

            float m = ElementSystem.ClampMastery(request.Mastery);
            float masteryFactor = 1f + m;

            float r = ElementSystem.ClampResist(request.Resist);
            float resistFactor = 1f - r;

            float critFactor = request.IsCrit ? CritMultiplier : NoCritMultiplier;
            float randomFactor = _rng(RandomMin, RandomMax);

            float finalDamage = baseDamage * e * masteryFactor * resistFactor * critFactor * randomFactor;

            bool advantage = usesE && ElementSystem.IsAdvantage(compareElement, request.TargetElement);
            bool disadvantage = usesE && ElementSystem.IsDisadvantage(compareElement, request.TargetElement);

            return new DamageResult(
                baseDamage,
                e,
                masteryFactor,
                resistFactor,
                critFactor,
                randomFactor,
                finalDamage,
                advantage,
                disadvantage,
                usesE,
                request.IsNormalAttack);
        }

        static float UnityRandom(float minInclusive, float maxInclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxInclusive);
        }
    }
}
