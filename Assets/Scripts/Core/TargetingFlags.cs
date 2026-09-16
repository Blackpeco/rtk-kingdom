using System;

namespace TsOnline
{
    [Flags]
    public enum TargetingFlags
    {
        None = 0,
        Self = 1 << 0,
        SingleAlly = 1 << 1,
        AllAllies = 1 << 2,
        SingleEnemy = 1 << 3,
        AllEnemies = 1 << 4
    }
}
