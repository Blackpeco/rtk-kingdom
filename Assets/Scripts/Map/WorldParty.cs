using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TsOnline
{
    /// <summary>Minimal session party — three Step 2 generals. No recruit UI.</summary>
    public static class WorldParty
    {
        public static UnitDefinition[] Members;

        public static UnitDefinition[] GetOrLoad()
        {
            if (HasMembers())
                return Members;

#if UNITY_EDITOR
            Members = new[]
            {
                AssetDatabase.LoadAssetAtPath<UnitDefinition>("Assets/Data/Generals/ZhaoYun.asset"),
                AssetDatabase.LoadAssetAtPath<UnitDefinition>("Assets/Data/Generals/GuanYu.asset"),
                AssetDatabase.LoadAssetAtPath<UnitDefinition>("Assets/Data/Generals/ZhugeLiang.asset")
            };
            if (HasMembers())
                return Members;
#endif
            BattleTestConfig cfg = Resources.Load<BattleTestConfig>("Battle/DefaultEncounter");
            if (cfg != null && cfg.playerParty != null)
                Members = cfg.playerParty;
            return Members;
        }

        static bool HasMembers()
        {
            if (Members == null || Members.Length == 0)
                return false;
            for (int i = 0; i < Members.Length; i++)
            {
                if (Members[i] != null)
                    return true;
            }

            return false;
        }
    }
}
