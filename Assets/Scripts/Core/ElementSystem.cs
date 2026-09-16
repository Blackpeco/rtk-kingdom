namespace TsOnline
{
    /// <summary>
    /// Element advantage (ข่ม), opposite pairs, mastery/resist caps, and cross-element skill learning.
    /// Three layers must never be mixed: unit innate, skill element, temporary status on target.
    /// </summary>
    public static class ElementSystem
    {
        public const float SkillAdvantageMultiplier = 1.25f;
        public const float SkillDisadvantageMultiplier = 0.80f;
        public const float SkillNeutralMultiplier = 1.00f;

        public const float NormalAdvantageMultiplier = 1.12f;
        public const float NormalDisadvantageMultiplier = 0.90f;
        public const float NormalNeutralMultiplier = 1.00f;

        public const float MasteryPerTwentyUses = 0.01f;
        public const int UsesPerMasteryStep = 20;
        public const float MasteryCap = 0.15f;
        public const float ResistCap = 0.20f;

        public const float OwnElementLearnCost = 1.00f;
        public const float AdjacentLearnCost = 1.50f;

        static readonly ElementType[] NoAdjacent = new ElementType[0];

        /// <summary>Earth &gt; Water &gt; Fire &gt; Wind &gt; Earth.</summary>
        public static bool IsAdvantage(ElementType attacker, ElementType defender)
        {
            if (attacker == ElementType.None || defender == ElementType.None)
                return false;

            switch (attacker)
            {
                case ElementType.Earth: return defender == ElementType.Water;
                case ElementType.Water: return defender == ElementType.Fire;
                case ElementType.Fire: return defender == ElementType.Wind;
                case ElementType.Wind: return defender == ElementType.Earth;
                default: return false;
            }
        }

        public static bool IsDisadvantage(ElementType attacker, ElementType defender)
        {
            return IsAdvantage(defender, attacker);
        }

        /// <summary>Earth ↔ Fire, Water ↔ Wind. No damage bonus.</summary>
        public static bool IsOpposite(ElementType a, ElementType b)
        {
            if (a == ElementType.None || b == ElementType.None)
                return false;

            return (a == ElementType.Earth && b == ElementType.Fire)
                || (a == ElementType.Fire && b == ElementType.Earth)
                || (a == ElementType.Water && b == ElementType.Wind)
                || (a == ElementType.Wind && b == ElementType.Water);
        }

        public static float GetSkillElementMultiplier(ElementType skillElement, ElementType targetElement)
        {
            if (skillElement == ElementType.None || targetElement == ElementType.None)
                return SkillNeutralMultiplier;
            if (IsAdvantage(skillElement, targetElement))
                return SkillAdvantageMultiplier;
            if (IsDisadvantage(skillElement, targetElement))
                return SkillDisadvantageMultiplier;
            return SkillNeutralMultiplier;
        }

        public static float GetNormalAttackElementMultiplier(ElementType unitElement, ElementType targetElement)
        {
            if (unitElement == ElementType.None || targetElement == ElementType.None)
                return NormalNeutralMultiplier;
            if (IsAdvantage(unitElement, targetElement))
                return NormalAdvantageMultiplier;
            if (IsDisadvantage(unitElement, targetElement))
                return NormalDisadvantageMultiplier;
            return NormalNeutralMultiplier;
        }

        public static bool SkillUsesElementMultiplier(SkillCategory category)
        {
            switch (category)
            {
                case SkillCategory.Heal:
                case SkillCategory.Buff:
                case SkillCategory.Wall:
                case SkillCategory.Stealth:
                    return false;
                default:
                    return true;
            }
        }

        public static bool SkillUsesElementMultiplier(SkillDefinition skill)
        {
            if (skill == null)
                return false;
            return SkillUsesElementMultiplier(skill.category);
        }

        /// <summary>+1% per 20 uses of that element skill, cap 15%.</summary>
        public static float GetMasteryBonus(int usesOfElement)
        {
            if (usesOfElement <= 0)
                return 0f;

            int steps = usesOfElement / UsesPerMasteryStep;
            float bonus = steps * MasteryPerTwentyUses;
            return bonus > MasteryCap ? MasteryCap : bonus;
        }

        /// <summary>Resist from gear, cap 20%. Negatives clamp to 0.</summary>
        public static float ClampResist(float r)
        {
            if (r < 0f)
                return 0f;
            if (r > ResistCap)
                return ResistCap;
            return r;
        }

        public static ElementType[] GetAdjacent(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth:
                    return new[] { ElementType.Water, ElementType.Wind };
                case ElementType.Water:
                    return new[] { ElementType.Earth, ElementType.Fire };
                case ElementType.Fire:
                    return new[] { ElementType.Water, ElementType.Wind };
                case ElementType.Wind:
                    return new[] { ElementType.Fire, ElementType.Earth };
                default:
                    return NoAdjacent;
            }
        }

        public static bool IsAdjacent(ElementType unitElement, ElementType skillElement)
        {
            if (unitElement == ElementType.None || skillElement == ElementType.None)
                return false;

            ElementType[] adjacent = GetAdjacent(unitElement);
            for (int i = 0; i < adjacent.Length; i++)
            {
                if (adjacent[i] == skillElement)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Own element: can learn. Adjacent: can learn. Opposite: cannot.
        /// None on either side is unrestricted (neutral / untyped).
        /// </summary>
        public static bool CanLearnSkill(ElementType unitElement, ElementType skillElement)
        {
            if (skillElement == ElementType.None || unitElement == ElementType.None)
                return true;
            if (unitElement == skillElement)
                return true;
            if (IsOpposite(unitElement, skillElement))
                return false;
            return IsAdjacent(unitElement, skillElement);
        }

        /// <summary>Own / none = ×1.0, adjacent = ×1.5. Opposite returns adjacent cost but <see cref="CanLearnSkill"/> is false.</summary>
        public static float GetLearnCostMultiplier(ElementType unitElement, ElementType skillElement)
        {
            if (skillElement == ElementType.None || unitElement == ElementType.None)
                return OwnElementLearnCost;
            if (unitElement == skillElement)
                return OwnElementLearnCost;
            if (IsAdjacent(unitElement, skillElement))
                return AdjacentLearnCost;
            return AdjacentLearnCost;
        }
    }
}
