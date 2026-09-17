using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TsOnline
{
    /// <summary>
    /// Session party (max 5) + unlocked roster of the 6 sample generals.
    /// DontDestroyOnLoad so HP/SP/level survive World ↔ Battle.
    /// </summary>
    public class PartyManager : MonoBehaviour
    {
        public const int MaxParty = 5;

        public static PartyManager Instance { get; private set; }

        public readonly List<PartyMember> Roster = new List<PartyMember>();
        public readonly List<PartyMember> Party = new List<PartyMember>();

        public int LastExpAwarded;
        public int LastLevelsGained;
        public string LastRewardSummary = "";

        static readonly string[] DefaultPartyIds = { "zhao_yun", "guan_yu", "zhuge_liang" };

        public static PartyManager Ensure()
        {
            if (Instance != null)
                return Instance;

            var go = new GameObject("PartyManager");
            DontDestroyOnLoad(go);
            return go.AddComponent<PartyManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (Roster.Count == 0)
                BuildDefaultRoster();
        }

        public void BuildDefaultRoster()
        {
            Roster.Clear();
            Party.Clear();
            UnitDefinition[] gens = LoadGenerals();
            for (int i = 0; i < gens.Length; i++)
            {
                if (gens[i] == null)
                    continue;
                Roster.Add(PartyMember.FromDefinition(gens[i]));
            }

            for (int i = 0; i < DefaultPartyIds.Length; i++)
            {
                PartyMember found = FindById(DefaultPartyIds[i]);
                if (found != null && !InParty(found))
                    Party.Add(found);
            }

            if (Party.Count == 0)
            {
                int take = Mathf.Min(3, Roster.Count);
                for (int i = 0; i < take; i++)
                    Party.Add(Roster[i]);
            }
        }

        public PartyMember FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            for (int i = 0; i < Roster.Count; i++)
            {
                if (Roster[i] != null && Roster[i].Id == id)
                    return Roster[i];
            }

            return null;
        }

        public UnitDefinition[] ActiveDefinitions()
        {
            var list = new List<UnitDefinition>();
            for (int i = 0; i < Party.Count; i++)
            {
                if (Party[i] != null && Party[i].definition != null)
                    list.Add(Party[i].definition);
            }

            return list.ToArray();
        }

        public PartyMember FindByDefinition(UnitDefinition def)
        {
            if (def == null)
                return null;
            for (int i = 0; i < Roster.Count; i++)
            {
                if (Roster[i] != null && Roster[i].definition == def)
                    return Roster[i];
            }

            for (int i = 0; i < Roster.Count; i++)
            {
                if (Roster[i] != null && Roster[i].definition != null && Roster[i].definition.id == def.id)
                    return Roster[i];
            }

            return null;
        }

        public bool InParty(PartyMember member)
        {
            return Party.Contains(member);
        }

        public bool TryAddToParty(PartyMember member)
        {
            if (member == null || !member.unlocked || InParty(member))
                return false;
            if (Party.Count >= MaxParty)
                return false;
            Party.Add(member);
            return true;
        }

        public bool TryRemoveFromParty(PartyMember member)
        {
            if (Party.Count <= 1)
                return false;
            return Party.Remove(member);
        }

        public void MoveParty(int index, int delta)
        {
            int dest = index + delta;
            if (index < 0 || dest < 0 || index >= Party.Count || dest >= Party.Count)
                return;
            PartyMember tmp = Party[index];
            Party[index] = Party[dest];
            Party[dest] = tmp;
        }

        public int AwardWinExp(UnitDefinition[] defeated)
        {
            int pot = ExpLevelSystem.TotalReward(defeated);
            LastExpAwarded = pot;
            int levels = 0;
            var leveled = new List<string>();
            for (int i = 0; i < Party.Count; i++)
            {
                int gained = ExpLevelSystem.GrantExp(Party[i], pot);
                levels += gained;
                if (gained > 0)
                    leveled.Add(Party[i].ShortName + " +" + gained + " Lv");
            }

            LastLevelsGained = levels;
            if (pot <= 0)
                LastRewardSummary = "ไม่ได้ EXP";
            else if (leveled.Count > 0)
                LastRewardSummary = "ได้ EXP " + pot + " — " + string.Join(", ", leveled.ToArray());
            else
                LastRewardSummary = "ได้ EXP " + pot + " (ยังไม่เลเวลอัพ)";
            return levels;
        }

        public void WriteBackFromBattle(IList<BattleUnit> units)
        {
            if (units == null)
                return;
            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit u = units[i];
                if (u == null || !u.isPlayer)
                    continue;
                PartyMember m = FindByDefinition(u.definition);
                if (m != null)
                    m.SyncFromBattle(u);
            }
        }

        public int PendingPoints()
        {
            int n = 0;
            for (int i = 0; i < Roster.Count; i++)
            {
                if (Roster[i] != null)
                    n += Roster[i].unspentPoints;
            }

            return n;
        }

        public void AwardFlatExp(int amount)
        {
            if (amount <= 0)
                return;
            int levels = 0;
            for (int i = 0; i < Party.Count; i++)
                levels += ExpLevelSystem.GrantExp(Party[i], amount);
            LastExpAwarded = amount;
            LastLevelsGained = levels;
            LastRewardSummary = "ได้ EXP " + amount + (levels > 0 ? " — มีคนเลเวลอัพ" : "");
        }

        public SavedMember[] ExportRoster()
        {
            var list = new SavedMember[Roster.Count];
            for (int i = 0; i < Roster.Count; i++)
                list[i] = ToSaved(Roster[i]);
            return list;
        }

        public string[] ExportPartyOrder()
        {
            var ids = new string[Party.Count];
            for (int i = 0; i < Party.Count; i++)
                ids[i] = Party[i] != null ? Party[i].Id : "";
            return ids;
        }

        public void ApplySave(SavedMember[] saved, string[] order)
        {
            if (Roster.Count == 0)
                BuildDefaultRoster();
            if (saved != null)
            {
                for (int i = 0; i < saved.Length; i++)
                {
                    if (saved[i] == null)
                        continue;
                    PartyMember m = FindById(saved[i].id);
                    if (m != null)
                        FromSaved(m, saved[i]);
                }
            }

            Party.Clear();
            if (order != null)
            {
                for (int i = 0; i < order.Length && Party.Count < MaxParty; i++)
                {
                    PartyMember m = FindById(order[i]);
                    if (m != null && !InParty(m))
                        Party.Add(m);
                }
            }

            if (Party.Count == 0)
            {
                for (int i = 0; i < DefaultPartyIds.Length; i++)
                {
                    PartyMember found = FindById(DefaultPartyIds[i]);
                    if (found != null && !InParty(found))
                        Party.Add(found);
                }
            }
        }

        static SavedMember ToSaved(PartyMember m)
        {
            var s = new SavedMember();
            if (m == null)
                return s;
            s.id = m.Id;
            s.level = m.level;
            s.exp = m.exp;
            s.unspentPoints = m.unspentPoints;
            s.currentHp = m.currentHp;
            s.currentSp = m.currentSp;
            s.bonusHp = m.bonus.hp;
            s.bonusSp = m.bonus.sp;
            s.bonusAtk = m.bonus.atk;
            s.bonusIntel = m.bonus.intel;
            s.bonusDef = m.bonus.def;
            s.bonusAgi = m.bonus.agi;
            s.unlocked = m.unlocked;
            return s;
        }

        static void FromSaved(PartyMember m, SavedMember s)
        {
            m.level = Mathf.Max(1, s.level);
            m.exp = Mathf.Max(0, s.exp);
            m.unspentPoints = Mathf.Max(0, s.unspentPoints);
            m.bonus = new UnitStats(s.bonusHp, s.bonusSp, s.bonusAtk, s.bonusIntel, s.bonusDef, s.bonusAgi);
            m.currentHp = s.currentHp;
            m.currentSp = s.currentSp;
            m.unlocked = s.unlocked;
            m.ClampVitals();
        }

        static UnitDefinition[] LoadGenerals()
        {
            string[] paths =
            {
                "Assets/Data/Generals/ZhaoYun.asset",
                "Assets/Data/Generals/GuanYu.asset",
                "Assets/Data/Generals/LuBu.asset",
                "Assets/Data/Generals/ZhangFei.asset",
                "Assets/Data/Generals/ZhugeLiang.asset",
                "Assets/Data/Generals/YangXiu.asset"
            };
            var list = new UnitDefinition[paths.Length];
#if UNITY_EDITOR
            for (int i = 0; i < paths.Length; i++)
                list[i] = AssetDatabase.LoadAssetAtPath<UnitDefinition>(paths[i]);
#endif
            if (list[0] != null)
                return list;

            BattleTestConfig cfg = Resources.Load<BattleTestConfig>("Battle/DefaultEncounter");
            if (cfg != null && cfg.playerParty != null)
                return cfg.playerParty;
            return list;
        }
    }
}
