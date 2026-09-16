using UnityEngine;

namespace TsOnline
{
    /// <summary>Stub encounter list for Step 1. World map is out of scope.</summary>
    [CreateAssetMenu(fileName = "EncounterTable", menuName = "TS Online/Data/Encounter Table", order = 4)]
    public class EncounterTable : ScriptableObject
    {
        public string id;
        public string displayName;
        public UnitDefinition[] possibleMonsters;
        public int minCount = 1;
        public int maxCount = 3;
    }
}
