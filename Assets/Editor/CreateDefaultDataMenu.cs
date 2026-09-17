using System.IO;
using UnityEditor;
using UnityEngine;

namespace TsOnline.EditorTools
{
    /// <summary>
    /// Regenerates sample ScriptableObjects under Assets/Data if you prefer creating them inside Unity.
    /// Safe to re-run: overwrites the known default asset paths.
    /// </summary>
    public static class CreateDefaultDataMenu
    {
        const string MenuPath = "Tools/TS Online/Create Default Data Assets";

        [MenuItem(MenuPath, priority = 20)]
        public static void CreateDefaults()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder("Assets/Data/Elements");
            EnsureFolder("Assets/Data/Skills");
            EnsureFolder("Assets/Data/Generals");
            EnsureFolder("Assets/Data/Monsters");
            EnsureFolder("Assets/Data/Items");

            ElementDefinition earth = WriteElement("Earth", "earth", "Earth", "ดิน", ElementType.Earth,
                new Color(0.72f, 0.53f, 0.24f), "Rock and armor. ข่ม Water. Opposite of Fire.");
            ElementDefinition water = WriteElement("Water", "water", "Water", "น้ำ", ElementType.Water,
                new Color(0.22f, 0.52f, 0.82f), "Heal and flood. ข่ม Fire. Opposite of Wind.");
            ElementDefinition fire = WriteElement("Fire", "fire", "Fire", "ไฟ", ElementType.Fire,
                new Color(0.86f, 0.24f, 0.18f), "Burst and burn. ข่ม Wind. Opposite of Earth.");
            ElementDefinition wind = WriteElement("Wind", "wind", "Wind", "ลม", ElementType.Wind,
                new Color(0.40f, 0.82f, 0.55f), "Speed and cuts. ข่ม Earth. Opposite of Water.");

            int icons = 0;
            if (AssignIcon(earth, "Assets/Art/Icons/earth.png")) icons++;
            if (AssignIcon(water, "Assets/Art/Icons/water.png")) icons++;
            if (AssignIcon(fire, "Assets/Art/Icons/fire.png")) icons++;
            if (AssignIcon(wind, "Assets/Art/Icons/wind.png")) icons++;
            Debug.Log("[TS Online] Reassigned " + icons + "/4 element icons (run this after first import if icons are missing).");

            SkillDefinition galeSlash = WriteSkill("GaleSlash", "gale_slash", "Gale Slash", "พายุดาบ",
                1.15f, DamageKind.Physical, ElementType.Wind, SkillCategory.Attack, 8, TargetingFlags.AllEnemies);
            SkillDefinition greenDragon = WriteSkill("GreenDragonStrike", "green_dragon_strike", "Green Dragon Strike", "มังกรเขียว",
                1.45f, DamageKind.Physical, ElementType.Wind, SkillCategory.Attack, 10, TargetingFlags.SingleEnemy);
            SkillDefinition skyPiercer = WriteSkill("SkyPiercer", "sky_piercer", "Sky Piercer", "ทวนทะลุฟ้า",
                1.70f, DamageKind.Physical, ElementType.Fire, SkillCategory.Attack, 12, TargetingFlags.SingleEnemy);
            SkillDefinition serpentSweep = WriteSkill("SerpentSpearSweep", "serpent_spear_sweep", "Serpent Spear Sweep", "งูทวนกวาด",
                1.20f, DamageKind.Physical, ElementType.Fire, SkillCategory.Attack, 9, TargetingFlags.AllEnemies);
            SkillDefinition floodBolt = WriteSkill("FloodBolt", "flood_bolt", "Flood Bolt", "สายฟ้าน้ำ",
                1.35f, DamageKind.Magical, ElementType.Water, SkillCategory.Attack, 10, TargetingFlags.SingleEnemy);
            SkillDefinition southWindHeal = WriteSkill("SouthWindHeal", "south_wind_heal", "South Wind Heal", "ลมใต้ฟื้นชีพ",
                1.00f, DamageKind.Magical, ElementType.Water, SkillCategory.Heal, 12, TargetingFlags.SingleAlly);
            SkillDefinition rockslide = WriteSkill("Rockslide", "rockslide", "Rockslide", "หินถล่ม",
                1.30f, DamageKind.Magical, ElementType.Earth, SkillCategory.Attack, 11, TargetingFlags.AllEnemies);
            SkillDefinition earthenWall = WriteSkill("EarthenWall", "earthen_wall", "Earthen Wall", "กำแพงดิน",
                0.00f, DamageKind.Magical, ElementType.Earth, SkillCategory.Wall, 10, TargetingFlags.AllAllies);
            SkillDefinition windClaw = WriteSkill("WindClaw", "wind_claw", "Wind Claw", "กรงเล็บลม",
                1.05f, DamageKind.Physical, ElementType.Wind, SkillCategory.Attack, 4, TargetingFlags.SingleEnemy);
            SkillDefinition torchSlash = WriteSkill("TorchSlash", "torch_slash", "Torch Slash", "ดาบคบเพลิง",
                1.10f, DamageKind.Physical, ElementType.Fire, SkillCategory.Attack, 5, TargetingFlags.SingleEnemy);
            SkillDefinition bogSpit = WriteSkill("BogSpit", "bog_spit", "Bog Spit", "น้ำลายบึง",
                1.10f, DamageKind.Magical, ElementType.Water, SkillCategory.Attack, 5, TargetingFlags.SingleEnemy);
            SkillDefinition stoneFist = WriteSkill("StoneFist", "stone_fist", "Stone Fist", "หมัดหิน",
                1.00f, DamageKind.Physical, ElementType.Earth, SkillCategory.Attack, 4, TargetingFlags.SingleEnemy);
            SkillDefinition divePeck = WriteSkill("DivePeck", "dive_peck", "Dive Peck", "จิกพุ่ง",
                1.00f, DamageKind.Physical, ElementType.Wind, SkillCategory.Attack, 3, TargetingFlags.SingleEnemy);
            SkillDefinition basicStrike = WriteSkill("BasicStrike", "basic_strike", "Basic Strike (skill)", "สกิลไร้ธาตุ",
                1.00f, DamageKind.Physical, ElementType.None, SkillCategory.Attack, 0, TargetingFlags.SingleEnemy,
                "None-element SKILL (E=1.00). True normal attacks must use DamageRequest.NormalAttack so unit element applies half-strength E (1.12 / 0.90 / 1.00).");
            SkillDefinition mend = WriteSkill("Mend", "mend", "Mend", "รักษา",
                0.90f, DamageKind.Magical, ElementType.None, SkillCategory.Heal, 6, TargetingFlags.SingleAlly);

            WriteGeneral("ZhaoYun", "zhao_yun", "Zhao Yun", "จูล่ง", ElementType.Wind,
                new UnitStats(110, 40, 88, 35, 42, 95), galeSlash, basicStrike);
            WriteGeneral("GuanYu", "guan_yu", "Guan Yu", "กวนอู", ElementType.Wind,
                new UnitStats(125, 35, 98, 30, 55, 70), greenDragon, basicStrike);
            WriteGeneral("LuBu", "lu_bu", "Lu Bu", "ลิโป้", ElementType.Fire,
                new UnitStats(130, 30, 110, 25, 48, 80), skyPiercer, basicStrike);
            WriteGeneral("ZhangFei", "zhang_fei", "Zhang Fei", "เตียวหุย", ElementType.Fire,
                new UnitStats(140, 30, 92, 22, 58, 55), serpentSweep, basicStrike);
            WriteGeneral("ZhugeLiang", "zhuge_liang", "Zhuge Liang", "ขงเบ้ง", ElementType.Water,
                new UnitStats(85, 90, 28, 105, 38, 60), floodBolt, southWindHeal, mend);
            WriteGeneral("YangXiu", "yang_xiu", "Yang Xiu", "หยางซิว", ElementType.Earth,
                new UnitStats(95, 80, 32, 92, 50, 52), rockslide, earthenWall);

            UnitDefinition wolf = WriteMonster("ForestWolf", "forest_wolf", "Forest Wolf", "หมาป่า", ElementType.Wind,
                new UnitStats(55, 15, 42, 12, 18, 70), 22, windClaw);
            UnitDefinition bandit = WriteMonster("MountainBandit", "mountain_bandit", "Mountain Bandit", "โจรภูเขา", ElementType.Fire,
                new UnitStats(70, 10, 48, 10, 22, 40), 24, torchSlash);
            UnitDefinition frog = WriteMonster("SwampFrog", "swamp_frog", "Swamp Frog", "กบทึง", ElementType.Water,
                new UnitStats(50, 20, 28, 30, 16, 35), 20, bogSpit);
            UnitDefinition golem = WriteMonster("SmallStoneGolem", "small_stone_golem", "Small Stone Golem", "โกเล็มหินเล็ก", ElementType.Earth,
                new UnitStats(90, 10, 35, 8, 40, 15), 28, stoneFist);
            UnitDefinition bird = WriteMonster("RoofBird", "roof_bird", "Roof Bird", "นกหลังคา", ElementType.Wind,
                new UnitStats(40, 12, 30, 14, 12, 85), 16, divePeck);
            UnitDefinition soldier = WriteMonster("LostSoldier", "lost_soldier", "Lost Soldier", "ทหารหลงทาง", ElementType.Fire,
                new UnitStats(65, 12, 40, 16, 24, 38), 22, torchSlash, basicStrike);

            WriteItem("Herb", "herb", "Herb", "สมุนไพร", "Step 1 stub. Restores a little HP later.");

            EncounterTable forest = LoadOrCreate<EncounterTable>("Assets/Data/ForestEdgeEncounters.asset");
            forest.id = "forest_edge";
            forest.displayName = "Forest Edge";
            forest.possibleMonsters = new[] { wolf, bandit, frog, golem, bird, soldier };
            forest.minCount = 1;
            forest.maxCount = 3;
            EditorUtility.SetDirty(forest);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TS Online] Default data assets written under Assets/Data/. Elements, skills, 6 generals, 6 monsters.");
        }

        static ElementDefinition WriteElement(
            string file, string id, string en, string th, ElementType type, Color color, string blurb)
        {
            string path = "Assets/Data/Elements/" + file + ".asset";
            ElementDefinition asset = LoadOrCreate<ElementDefinition>(path);
            asset.id = id;
            asset.displayName = en;
            asset.displayNameThai = th;
            asset.element = type;
            asset.color = color;
            asset.roleBlurb = blurb;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static bool AssignIcon(ElementDefinition element, string iconPath)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (sprite == null)
                return false;
            element.icon = sprite;
            EditorUtility.SetDirty(element);
            return true;
        }

        static SkillDefinition WriteSkill(
            string file, string id, string en, string th,
            float power, DamageKind kind, ElementType element, SkillCategory category,
            int sp, TargetingFlags targeting, string description = null)
        {
            string path = "Assets/Data/Skills/" + file + ".asset";
            SkillDefinition asset = LoadOrCreate<SkillDefinition>(path);
            asset.id = id;
            asset.displayName = en;
            asset.displayNameThai = th;
            asset.description = description ?? string.Empty;
            asset.power = power;
            asset.damageKind = kind;
            asset.element = element;
            asset.category = category;
            asset.spCost = sp;
            asset.targeting = targeting;
            asset.overrideStatusApplyChance = false;
            asset.statusApplyChance = StatusEffectSystem.DefaultApplyChance;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static UnitDefinition WriteGeneral(
            string file, string id, string en, string th, ElementType element, UnitStats stats, params SkillDefinition[] skills)
        {
            return WriteUnit("Assets/Data/Generals/" + file + ".asset", id, en, th, element, stats, true, false, 0, skills);
        }

        static UnitDefinition WriteMonster(
            string file, string id, string en, string th, ElementType element, UnitStats stats, int expReward, params SkillDefinition[] skills)
        {
            return WriteUnit("Assets/Data/Monsters/" + file + ".asset", id, en, th, element, stats, false, true, expReward, skills);
        }

        static UnitDefinition WriteUnit(
            string path, string id, string en, string th, ElementType element, UnitStats stats,
            bool general, bool monster, int expReward, params SkillDefinition[] skills)
        {
            UnitDefinition asset = LoadOrCreate<UnitDefinition>(path);
            asset.id = id;
            asset.displayName = en;
            asset.displayNameThai = th;
            asset.element = element;
            asset.baseStats = stats;
            asset.startingSkills = skills;
            asset.isGeneral = general;
            asset.isMonster = monster;
            asset.expReward = expReward;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static ItemDefinition WriteItem(string file, string id, string en, string th, string desc)
        {
            string path = "Assets/Data/Items/" + file + ".asset";
            ItemDefinition asset = LoadOrCreate<ItemDefinition>(path);
            asset.id = id;
            asset.displayName = en;
            asset.displayNameThai = th;
            asset.description = desc;
            asset.stackLimit = 99;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
                return existing;

            T created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
