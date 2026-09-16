using UnityEngine;

namespace TsOnline
{
    [CreateAssetMenu(fileName = "Unit", menuName = "TS Online/Data/Unit Definition", order = 2)]
    public class UnitDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string displayNameThai;
        public ElementType element;
        public UnitStats baseStats;
        public SkillDefinition[] startingSkills;
        public bool isGeneral;
        public bool isMonster;
    }
}
