using UnityEngine;

namespace TsOnline
{
    /// <summary>Shim — encounters read the live PartyManager roster (max 5).</summary>
    public static class WorldParty
    {
        public static UnitDefinition[] Members
        {
            get { return PartyManager.Ensure().ActiveDefinitions(); }
            set { }
        }

        public static UnitDefinition[] GetOrLoad()
        {
            return PartyManager.Ensure().ActiveDefinitions();
        }
    }
}
