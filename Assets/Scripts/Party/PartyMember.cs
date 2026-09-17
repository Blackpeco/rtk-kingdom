using System;
using UnityEngine;

namespace TsOnline
{
    [Serializable]
    public class PartyMember
    {
        public UnitDefinition definition;
        public int level = 1;
        public int exp;
        public int unspentPoints;
        public UnitStats bonus;
        public int currentHp;
        public int currentSp;
        public bool unlocked = true;
        public bool isCreatedLead;

        public string Id
        {
            get { return definition != null ? definition.id : ""; }
        }

        public bool IsCreatedLead
        {
            get { return isCreatedLead || Id == CreatedHero.Id; }
        }

        public string ShortName
        {
            get { return definition != null ? definition.displayName : "?"; }
        }

        public string ThaiName
        {
            get { return definition != null ? definition.displayNameThai : ""; }
        }

        public ElementType Element
        {
            get { return definition != null ? definition.element : ElementType.None; }
        }

        public UnitStats EffectiveStats
        {
            get
            {
                UnitStats b = definition != null ? definition.baseStats : new UnitStats(1, 0, 1, 1, 0, 1);
                return new UnitStats(
                    b.hp + bonus.hp,
                    b.sp + bonus.sp,
                    b.atk + bonus.atk,
                    b.intel + bonus.intel,
                    b.def + bonus.def,
                    b.agi + bonus.agi);
            }
        }

        public static PartyMember FromDefinition(UnitDefinition def)
        {
            var m = new PartyMember();
            m.definition = def;
            m.level = 1;
            m.exp = 0;
            m.unspentPoints = 0;
            m.bonus = new UnitStats(0, 0, 0, 0, 0, 0);
            m.unlocked = true;
            if (def != null)
            {
                m.currentHp = def.baseStats.hp;
                m.currentSp = def.baseStats.sp;
            }

            return m;
        }

        public void ClampVitals()
        {
            UnitStats s = EffectiveStats;
            if (currentHp > s.hp)
                currentHp = s.hp;
            if (currentHp < 0)
                currentHp = 0;
            if (currentSp > s.sp)
                currentSp = s.sp;
            if (currentSp < 0)
                currentSp = 0;
        }

        public void SyncFromBattle(BattleUnit unit)
        {
            if (unit == null)
                return;
            currentHp = unit.currentHp;
            currentSp = unit.currentSp;
            ClampVitals();
        }

        public void ApplyToBattle(BattleUnit unit)
        {
            if (unit == null)
                return;
            UnitStats s = EffectiveStats;
            unit.stats = s;
            unit.currentHp = currentHp > 0 ? Mathf.Min(currentHp, s.hp) : 1;
            unit.currentSp = Mathf.Clamp(currentSp, 0, s.sp);
        }
    }
}
