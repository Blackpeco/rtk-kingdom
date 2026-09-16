using UnityEngine;

namespace TsOnline
{
    [CreateAssetMenu(fileName = "Skill", menuName = "TS Online/Data/Skill Definition", order = 1)]
    public class SkillDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string displayNameThai;
        [Tooltip("Multiplier in ATK*Power or INT*Power. Not a hardcoded combat constant.")]
        public float power = 1f;
        public DamageKind damageKind = DamageKind.Physical;
        public ElementType element = ElementType.None;
        public SkillCategory category = SkillCategory.Attack;
        public int spCost;
        public TargetingFlags targeting = TargetingFlags.SingleEnemy;
        [Tooltip("When set, overrides the default 30% elemental status apply chance.")]
        public bool overrideStatusApplyChance;
        [Range(0f, 1f)]
        public float statusApplyChance = StatusEffectSystem.DefaultApplyChance;

        public float GetStatusApplyChance()
        {
            return overrideStatusApplyChance
                ? statusApplyChance
                : StatusEffectSystem.DefaultApplyChance;
        }
    }
}
