namespace TsOnline
{
    /// <summary>EXP curve and manual stat-point allocation. 3 points per level.</summary>
    public static class ExpLevelSystem
    {
        public const int MaxLevel = 20;
        public const int PointsPerLevel = 3;
        public const int HpPerPoint = 8;
        public const int SpPerPoint = 4;
        public const int CombatPerPoint = 2;

        public enum StatKind
        {
            Hp,
            Sp,
            Atk,
            Intel,
            Def,
            Agi
        }

        public static int ExpToNext(int level)
        {
            if (level < 1)
                level = 1;
            return 20 + level * 15;
        }

        public static int ExpRewardOf(UnitDefinition unit)
        {
            if (unit == null)
                return 0;
            return unit.expReward > 0 ? unit.expReward : (unit.isMonster ? 25 : 0);
        }

        public static int TotalReward(UnitDefinition[] enemies)
        {
            int sum = 0;
            if (enemies == null)
                return 0;
            for (int i = 0; i < enemies.Length; i++)
                sum += ExpRewardOf(enemies[i]);
            return sum;
        }

        /// <summary>Returns how many levels were gained.</summary>
        public static int GrantExp(PartyMember member, int amount)
        {
            if (member == null || amount <= 0)
                return 0;
            if (member.level >= MaxLevel)
                return 0;

            member.exp += amount;
            int gained = 0;
            while (member.level < MaxLevel && member.exp >= ExpToNext(member.level))
            {
                member.exp -= ExpToNext(member.level);
                member.level++;
                member.unspentPoints += PointsPerLevel;
                gained++;
            }

            if (member.level >= MaxLevel)
                member.exp = 0;
            return gained;
        }

        public static bool SpendPoint(PartyMember member, StatKind stat)
        {
            if (member == null || member.unspentPoints <= 0)
                return false;

            UnitStats b = member.bonus;
            switch (stat)
            {
                case StatKind.Hp:
                    b.hp += HpPerPoint;
                    member.currentHp += HpPerPoint;
                    break;
                case StatKind.Sp:
                    b.sp += SpPerPoint;
                    member.currentSp += SpPerPoint;
                    break;
                case StatKind.Atk:
                    b.atk += CombatPerPoint;
                    break;
                case StatKind.Intel:
                    b.intel += CombatPerPoint;
                    break;
                case StatKind.Def:
                    b.def += CombatPerPoint;
                    break;
                case StatKind.Agi:
                    b.agi += CombatPerPoint;
                    break;
                default:
                    return false;
            }

            member.bonus = b;
            member.unspentPoints--;
            member.ClampVitals();
            return true;
        }
    }
}
