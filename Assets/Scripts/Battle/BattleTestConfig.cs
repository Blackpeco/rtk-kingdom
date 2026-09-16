using UnityEngine;

namespace TsOnline
{
    [CreateAssetMenu(fileName = "BattleTestConfig", menuName = "TS Online/Data/Battle Test Config", order = 10)]
    public class BattleTestConfig : ScriptableObject
    {
        public UnitDefinition[] playerParty;
        public UnitDefinition[] enemyParty;
    }
}
