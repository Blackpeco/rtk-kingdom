namespace TsOnline
{
    /// <summary>
    /// Inputs for <see cref="DamageCalculator"/>. Skill numbers come from ScriptableObjects, not combat scripts.
    /// </summary>
    public readonly struct DamageRequest
    {
        public readonly float Atk;
        public readonly float IntStat;
        public readonly float Def;
        public readonly float SkillPower;
        public readonly DamageKind Kind;
        public readonly SkillCategory Category;
        public readonly ElementType SkillElement;
        public readonly ElementType UnitElement;
        public readonly ElementType TargetElement;
        public readonly bool IsNormalAttack;
        public readonly float Mastery;
        public readonly float Resist;
        public readonly bool IsCrit;

        public DamageRequest(
            float atk,
            float intStat,
            float def,
            float skillPower,
            DamageKind kind,
            SkillCategory category,
            ElementType skillElement,
            ElementType unitElement,
            ElementType targetElement,
            bool isNormalAttack,
            float mastery,
            float resist,
            bool isCrit)
        {
            Atk = atk;
            IntStat = intStat;
            Def = def;
            SkillPower = skillPower;
            Kind = kind;
            Category = category;
            SkillElement = skillElement;
            UnitElement = unitElement;
            TargetElement = targetElement;
            IsNormalAttack = isNormalAttack;
            Mastery = mastery;
            Resist = resist;
            IsCrit = isCrit;
        }

        public static DamageRequest FromSkill(
            UnitStats attacker,
            float defenderDef,
            SkillDefinition skill,
            ElementType targetElement,
            float mastery,
            float resist,
            bool isCrit)
        {
            if (skill == null)
                throw new System.ArgumentNullException("skill");

            return new DamageRequest(
                attacker.atk,
                attacker.intel,
                defenderDef,
                skill.power,
                skill.damageKind,
                skill.category,
                skill.element,
                ElementType.None,
                targetElement,
                false,
                mastery,
                resist,
                isCrit);
        }

        public static DamageRequest NormalAttack(
            UnitStats attacker,
            float defenderDef,
            ElementType unitElement,
            ElementType targetElement,
            float mastery,
            float resist,
            bool isCrit,
            float skillPower = 1f)
        {
            return new DamageRequest(
                attacker.atk,
                attacker.intel,
                defenderDef,
                skillPower,
                DamageKind.Physical,
                SkillCategory.Attack,
                ElementType.None,
                unitElement,
                targetElement,
                true,
                mastery,
                resist,
                isCrit);
        }
    }
}
