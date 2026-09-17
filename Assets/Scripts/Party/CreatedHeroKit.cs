using UnityEngine;

namespace TsOnline
{
    /// <summary>Skill refs for the created lead. Lives under Resources so player builds include them.</summary>
    [CreateAssetMenu(fileName = "CreatedHeroKit", menuName = "TS Online/Data/Created Hero Kit", order = 11)]
    public class CreatedHeroKit : ScriptableObject
    {
        public SkillDefinition basicStrike;
        public SkillDefinition stoneFist;
        public SkillDefinition mend;
        public SkillDefinition torchSlash;
        public SkillDefinition windClaw;

        public SkillDefinition ExtraFor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth: return stoneFist;
                case ElementType.Water: return mend;
                case ElementType.Fire: return torchSlash;
                case ElementType.Wind: return windClaw;
                default: return null;
            }
        }
    }
}
