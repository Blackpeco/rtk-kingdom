using UnityEngine;

namespace TsOnline
{
    /// <summary>
    /// Session handoff World → Battle → World. Survives LoadScene without a DontDestroy object.
    /// </summary>
    public static class EncounterContext
    {
        public static bool HasPending;
        public static bool ShouldReturnToWorld;
        public static string ReturnScene = "World";
        public static Vector3 ReturnPosition;
        public static UnitDefinition[] PlayerParty;
        public static UnitDefinition[] Enemies;
        public static string EncounterName;
        public static BattleEndKind LastEnd = BattleEndKind.None;

        public static void Begin(
            UnitDefinition[] party,
            UnitDefinition[] enemies,
            Vector3 returnPosition,
            string encounterName)
        {
            PlayerParty = party;
            Enemies = enemies;
            ReturnPosition = returnPosition;
            EncounterName = encounterName;
            ReturnScene = "World";
            HasPending = true;
            ShouldReturnToWorld = true;
        }

        /// <summary>World finished applying the return spawn. Clears both pending flags so a later Play on Battle.unity is a sandbox 3v3.</summary>
        public static void MarkReturned()
        {
            HasPending = false;
            ShouldReturnToWorld = false;
        }

        public static void ClearReturn()
        {
            ShouldReturnToWorld = false;
        }

        public static void ResetSession()
        {
            HasPending = false;
            ShouldReturnToWorld = false;
            ReturnScene = "World";
            ReturnPosition = Vector3.zero;
            PlayerParty = null;
            Enemies = null;
            EncounterName = null;
            LastEnd = BattleEndKind.None;
        }
    }

    public enum BattleEndKind
    {
        None = 0,
        Win = 1,
        Lose = 2,
        Escape = 3
    }
}
