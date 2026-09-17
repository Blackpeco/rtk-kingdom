using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TsOnline
{
    /// <summary>Runtime player lead created on CharacterCreate. Not one of the 6 generals.</summary>
    public static class CreatedHero
    {
        public const string Id = "created_player";

        public static UnitStats StatsFor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth:
                    return new UnitStats(130, 35, 55, 30, 70, 40);
                case ElementType.Water:
                    return new UnitStats(90, 95, 40, 90, 40, 50);
                case ElementType.Fire:
                    return new UnitStats(95, 40, 100, 35, 28, 60);
                case ElementType.Wind:
                    return new UnitStats(95, 50, 70, 40, 38, 100);
                default:
                    return new UnitStats(100, 40, 60, 40, 40, 50);
            }
        }

        public static string RoleBlurb(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth:
                    return "ดิน — แทงค์ / คุม  (HP·DEF สูง)";
                case ElementType.Water:
                    return "น้ำ — ฮีล / บัฟ  (SP·INT สูง)";
                case ElementType.Fire:
                    return "ไฟ — ดาเมจ  (ATK สูง, DEF ต่ำ)";
                case ElementType.Wind:
                    return "ลม — ความเร็ว / สตัน  (AGI สูง)";
                default:
                    return "";
            }
        }

        public static string Thai(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth: return "ดิน";
                case ElementType.Water: return "น้ำ";
                case ElementType.Fire: return "ไฟ";
                case ElementType.Wind: return "ลม";
                default: return "-";
            }
        }

        public static string English(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth: return "Earth";
                case ElementType.Water: return "Water";
                case ElementType.Fire: return "Fire";
                case ElementType.Wind: return "Wind";
                default: return "-";
            }
        }

        public static Color ColorOf(ElementType element)
        {
            switch (element)
            {
                case ElementType.Earth: return new Color(0.72f, 0.53f, 0.24f);
                case ElementType.Water: return new Color(0.22f, 0.52f, 0.82f);
                case ElementType.Fire: return new Color(0.86f, 0.24f, 0.18f);
                case ElementType.Wind: return new Color(0.40f, 0.82f, 0.55f);
                default: return Color.gray;
            }
        }

        public static UnitDefinition CreateDefinition(string playerName, ElementType element)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "ผู้กล้า";
            var def = ScriptableObject.CreateInstance<UnitDefinition>();
            def.name = "CreatedPlayer";
            def.id = Id;
            def.displayName = playerName.Trim();
            def.displayNameThai = def.displayName;
            def.element = element;
            def.baseStats = StatsFor(element);
            def.isGeneral = true;
            def.isMonster = false;
            def.startingSkills = LoadStartingSkills(element);
            return def;
        }

        public static PartyMember Make(string playerName, ElementType element)
        {
            PartyMember m = PartyMember.FromDefinition(CreateDefinition(playerName, element));
            m.isCreatedLead = true;
            return m;
        }

        public static void ApplyIdentity(PartyMember member, string playerName, ElementType element)
        {
            if (member == null)
                return;
            if (member.definition == null || member.definition.id != Id)
                member.definition = CreateDefinition(playerName, element);
            else
            {
                if (!string.IsNullOrWhiteSpace(playerName))
                {
                    member.definition.displayName = playerName.Trim();
                    member.definition.displayNameThai = member.definition.displayName;
                }

                member.definition.element = element;
                member.definition.baseStats = StatsFor(element);
                member.definition.startingSkills = LoadStartingSkills(element);
            }

            member.isCreatedLead = true;
            member.ClampVitals();
        }

        static SkillDefinition[] LoadStartingSkills(ElementType element)
        {
            SkillDefinition basic = LoadSkill("Assets/Data/Skills/BasicStrike.asset");
            string extraPath;
            switch (element)
            {
                case ElementType.Earth:
                    extraPath = "Assets/Data/Skills/StoneFist.asset";
                    break;
                case ElementType.Water:
                    extraPath = "Assets/Data/Skills/Mend.asset";
                    break;
                case ElementType.Fire:
                    extraPath = "Assets/Data/Skills/TorchSlash.asset";
                    break;
                default:
                    extraPath = "Assets/Data/Skills/WindClaw.asset";
                    break;
            }

            SkillDefinition extra = LoadSkill(extraPath);
            if (basic != null && extra != null)
                return new[] { extra, basic };
            if (extra != null)
                return new[] { extra };
            if (basic != null)
                return new[] { basic };
            return new SkillDefinition[0];
        }

        static SkillDefinition LoadSkill(string path)
        {
#if UNITY_EDITOR
            SkillDefinition fromEditor = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
            if (fromEditor != null)
                return fromEditor;
#endif
            return null;
        }
    }
}
