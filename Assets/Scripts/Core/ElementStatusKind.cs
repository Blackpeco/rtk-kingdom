namespace TsOnline
{
    /// <summary>
    /// Data/types for Step 1. Runtime application is stubbed in StatusEffectSystem.
    /// Element statuses cannot stack with each other.
    /// </summary>
    public enum ElementStatusKind
    {
        None = 0,
        /// <summary>Earth: AGI -20% for 2 turns.</summary>
        EarthAgiDown = 1,
        /// <summary>Water: Wet — next Fire hit ×1.15, clears burn, 2 turns.</summary>
        Wet = 2,
        /// <summary>Fire: Burn 4% HP/turn, heal -30%, 2 turns.</summary>
        Burn = 3,
        /// <summary>Wind: 25% chance to lose next turn, 1 turn.</summary>
        WindSkip = 4
    }
}
